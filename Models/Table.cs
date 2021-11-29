using System.Collections.Generic;

namespace DBMS_Web_Relational.Models {
    public class Table {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Row> Rows { get; set; } = new List<Row>();
        public List<Column> Columns { get; set; } = new List<Column>();

        public Table() { }

        public Table(int id, string name) {
            Id = id;
            Name = name;
        }
    }
}
