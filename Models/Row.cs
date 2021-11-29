using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMS_Web_Relational.Models {
    public class Row {
        public int Id { get; set; }
        public List<string> Values { get; set; } = new List<string>();

        public Row(int id) {
            Id = id;
        }

        public string this[int i] {
            get => Values[i];
            set => Values[i] = value;
        }
    }
}
