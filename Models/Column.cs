using System.Globalization;
using System.IO;

namespace DBMS_Web_Relational.Models {
    public abstract class Column {
        public readonly string type = "";
        public int Id { get; set; }
        public string Name { get; set; }

        public Column(int id, string name) {
            Id = id;
            Name = name;
        }

        public abstract bool Validate(string value);
    }

    public class IntColumn : Column {
        public new readonly string type = "INT";
        public IntColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) => int.TryParse(value, out _);
    }

    public class RealColumn : Column {
        public new readonly string type = "REAL";
        public RealColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    public class CharColumn : Column {
        public new readonly string type = "CHAR";
        public CharColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) => char.TryParse(value, out _);
    }

    public class StringColumn : Column {
        public new readonly string type = "STRING";
        public StringColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) => true;
    }

    public class TextFileColumn : Column {
        public new readonly string type = "TEXT FILE";
        public TextFileColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) => true;
    }

    public class IntIntervalColumn : Column {
        public new readonly string type = "INT INTERVAL";
        public IntIntervalColumn(int id, string name) : base(id, name) { }

        public override bool Validate(string value) {
            string[] buf = value.Replace(" ", "").Split(',');

            return buf.Length == 2 && int.TryParse(buf[0], out int a) && int.TryParse(buf[1], out int b) && a < b;
        }
    }
}
