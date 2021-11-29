using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DBMS_Web_Relational.Models {
    public class DatabaseManager {
        private const char sep = '$';
        private const char space = '\t';
        private const string _specialChars = "\\/:*?\"<>|";
        private string _connectionStr = @"Server=DESKTOP-7A9LNDR\SQLEXPRESS;Integrated security=SSPI;database=";
        private List<int> _lastIds = new();
        private static DatabaseManager _instance;

        public Database Database { get; set; }

        public static DatabaseManager Instance {
            get {
                if (_instance == null) {
                    _instance = new DatabaseManager();
                }

                return _instance;
            }
        }

        private DatabaseManager() { }

        public static void ExecuteSqlQuery(string query, string connectionStr) {
            var connection = new SqlConnection(connectionStr);
            connection.Open();
            
            var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        public static object ExecuteSqlQueryScalar(string query, string connectionStr) {
            var connection = new SqlConnection(connectionStr);
            connection.Open();

            var command = new SqlCommand(query, connection);
            object res = command.ExecuteScalar();

            connection.Close();
            return res;
        }

        public static List<string> ExecuteSqlQueryWithReader(string query, string connectionStr) {
            var res = new List<string>();
            
            var connection = new SqlConnection(connectionStr);
            connection.Open();

            var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            var reader = command.ExecuteReader();
            while (reader.Read()) {
                res.Add(reader.GetString(0));
            }

            return res;
        }

        #region Database methods

        public bool CreateDatabase(string name) {
            if (name.IndexOfAny(_specialChars.ToCharArray()) != -1) {
                return false;
            }

            Database = new Database(name);

            _connectionStr = @"Server=DESKTOP-7A9LNDR\SQLEXPRESS;Integrated security=SSPI;database=";
            ExecuteSqlQuery($"CREATE DATABASE {name}", _connectionStr + "master");
            _connectionStr += name;
            
            return true;
        }

        public void DeleteDatabase() {
            _connectionStr = @"Server=DESKTOP-7A9LNDR\SQLEXPRESS;Integrated security=SSPI;database=master";
            ExecuteSqlQuery($"USE master; DROP DATABASE {Database.Name}", _connectionStr);
            Database = null;
        }

        public List<string> GetDatabaseNames() {
            string query = "SELECT name from sys.databases";

            return ExecuteSqlQueryWithReader(query, _connectionStr).Skip(4).ToList();
        }

        public void OpenDatabase(string name) {
            var queryTables = $"SELECT TABLE_NAME FROM [{name}].INFORMATION_SCHEMA.TABLES";
            _connectionStr += name;
            Database = new Database(name);

            List<string> tableNames = ExecuteSqlQueryWithReader(queryTables, _connectionStr);
            for (int i = 0; i < tableNames.Count; i++) {
                Database.Tables.Add(new Table(i, tableNames[i]));
                _lastIds.Add(0);

                var queryColumnNames = $"SELECT COLUMN_NAME FROM [{name}].INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableNames[i]}'";
                var queryColumnTypes = $"SELECT DATA_TYPE FROM [{name}].INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableNames[i]}'";

                List<string> columnNames = ExecuteSqlQueryWithReader(queryColumnNames, _connectionStr);
                List<string> columnTypes = ExecuteSqlQueryWithReader(queryColumnTypes, _connectionStr);

                for (int j = 0; j < columnNames.Count; j++) {
                    Database.Tables[i].Columns.Add(ColumnFromString(j, columnNames[j], ColumnType(columnTypes[j])));
                }

                Database.Tables[i].Rows = QueryRows(tableNames[i], columnNames.Count, _connectionStr);
            }

        }

        private List<Row> QueryRows(string tableName, int columnsCount, string connectionStr) {
            var res = new List<Row>();
            string query = $"SELECT * FROM {tableName}";

            var connection = new SqlConnection(connectionStr);
            connection.Open();

            var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            var reader = command.ExecuteReader();
            while (reader.Read()) {
                var row = new Row(reader.GetInt32(0));

                for (int i = 0; i < columnsCount; i++) {
                    row.Values.Add(reader.GetValue(i).ToString());
                }

                res.Add(row);
            }

            return res;
        }
        #endregion

        #region Table methods
        public bool AddTable(string name) {
            if (GetTableNames().Contains(name)) {
                return false;
            }

            var table = new Table(Database.Tables.Count, name);
            table.Columns.Add(new IntColumn(0, "id"));
            Database.Tables.Add(table);
            _lastIds.Add(0);

            string query = $"CREATE TABLE {name} (id INT IDENTITY(1,1) PRIMARY KEY)";
            ExecuteSqlQuery(query, _connectionStr);

            return true;
        }

        public Table GetTable(int index) => Database.Tables[index];

        public List<string> GetTableNames() => Database.Tables.Select(t => t.Name).ToList();

        public List<string> GetColumnNames(int tableIndex) => Database.Tables[tableIndex].Columns.Select(c => c.Name).ToList();

        public void EditTable(int tableIndex, string name) {
            var table = GetTable(tableIndex);
            var query = $"EXEC SP_RENAME '{table.Name}', '{name}'";

            table.Name = name;
            ExecuteSqlQuery(query, _connectionStr);
        }

        public void DeleteTable(int index) {
            string query = $"DROP TABLE {Database.Tables[index].Name}";

            Database.Tables.RemoveAt(index);
            ExecuteSqlQuery(query, _connectionStr);
        }
        #endregion

        #region Column methods
        public bool AddColumn(int tableIndex, dynamic column) {
            if (GetColumnNames(tableIndex).Contains(column.Name)) {
                return false;
            }

            var table = Database.Tables[tableIndex];
            string query = $"ALTER TABLE {table.Name} ADD {column.Name} {SqlServerColumnType(column.type)} NULL";

            table.Columns.Add(column);
            ExecuteSqlQuery(query, _connectionStr);

            foreach (var row in table.Rows) {
                row.Values.Add("");
            }

            return true;
        }

        public void DeleteColumn(int tableIndex, int columnIndex) {
            var table = Database.Tables[tableIndex];
            string query = $"ALTER TABLE {table.Name} DROP COLUMN {table.Columns[columnIndex].Name}";

            table.Columns.RemoveAt(columnIndex);
            ExecuteSqlQuery(query, _connectionStr);

            foreach (var row in table.Rows) {
                row.Values.RemoveAt(columnIndex);
            }

            if (table.Columns.Count == 0) {
                table.Rows.Clear();
            }
        }
        #endregion

        #region Row methods
        public bool AddRow(int tableIndex) {
            if (Database == null || Database.Tables.Count == 0 || Database.Tables[tableIndex].Columns.Count == 0) {
                return false;
            }

            int id = ++_lastIds[tableIndex];
            var table = Database.Tables[tableIndex];
            var row = new Row(id);
            string query = $"INSERT INTO {table.Name} (";
            string queryValues = "VALUES (";

            row.Values.Add(id.ToString());
            foreach (var column in table.Columns.Skip(1)) {
                row.Values.Add("");
                query += $"{column.Name}, ";
                queryValues += "NULL, ";
            }
            query = query[0..^2] + ") " + queryValues[0..^2] + ")";
            ExecuteSqlQuery(query, _connectionStr);

            table.Rows.Add(row);
            return true;
        }

        public void DeleteRow(int tableIndex, int rowIndex) {
            var table = Database.Tables[tableIndex];
            string query = $"DELETE FROM {table.Name} WHERE id = {table.Rows[rowIndex].Id}";
            
            table.Rows.RemoveAt(rowIndex);
            ExecuteSqlQuery(query, _connectionStr);
        }
        #endregion

        #region Extra operations
        public bool ChangeCellValue(string value, int tableIndex, int columnIndex, int rowIndex) {
            var table = Database.Tables[tableIndex];
            dynamic column = table.Columns[columnIndex];

            bool isNumber = column.type.Equals("INT") || column.type.Equals("REAL");
            string queryValue = isNumber ? value : $"N'{value}'";

            string query = $"UPDATE {table.Name} SET {column.Name} = {queryValue} WHERE id = {table.Rows[rowIndex].Id}";
            ExecuteSqlQuery(query, _connectionStr);

            if (column.Validate(value)) {
                table.Rows[rowIndex][columnIndex] = value;
                return true;
            }

            return false;
        }

        public Table Project(int tableIndex, int[] columnIndices) {
            if (Database == null || Database.Tables.Count == 0 || Database.Tables[tableIndex].Columns.Count == 0) {
                return null;
            }

            var table = Database.Tables[tableIndex];
            var resRows = new List<Row>();

            foreach (var row in table.Rows) {
                var resRow = new Row(table.Rows.Count);

                foreach (int i in columnIndices) {
                    resRow.Values.Add(row.Values[i]);
                }

                resRows.Add(resRow);
            }

            var resColumns = new List<Column>();
            foreach (int i in columnIndices) {
                resColumns.Add(Database.Tables[tableIndex].Columns[i]);
            }

            return new Table(table.Id, table.Name) {
                Columns = resColumns,
                Rows = resRows
            };
        }
        #endregion

        #region Inner methods

        public static Column ColumnFromString(int id, string name, string type) {
            return type switch {
                "INT" => new IntColumn(id, name),
                "REAL" => new RealColumn(id, name),
                "CHAR" => new CharColumn(id, name),
                "STRING" => new StringColumn(id, name),
                "TEXT FILE" => new TextFileColumn(id, name),
                "INT INTERVAL" => new IntIntervalColumn(id, name),
                _ => null
            };
        }

        public static string SqlServerColumnType(string type) {
            return type switch {
                "INT" => "INT",
                "REAL" => "REAL",
                "CHAR" => "CHAR(1)",
                "STRING" => "NVARCHAR(MAX)",
                "TEXT FILE" => "NTEXT",
                "INT INTERVAL" => "VARCHAR(MAX)",
                _ => null
            };
        }
        
        public static string ColumnType(string sqlServerType) {
            return sqlServerType switch {
                "int" => "INT",
                "real" => "REAL",
                "char" => "CHAR",
                "nvarchar" => "STRING",
                "ntext" => "TEXT FILE",
                "varchar" => "INT INTERVAL",
                _ => null
            };
        }
        #endregion
    }
}
