using System.ComponentModel.DataAnnotations.Schema;

namespace easydev.Models
{
    public class ColumnDB
    {
        public long id { get; set; }
        public long tableid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int length { get; set; }
        public bool isnullable { get; set; }
        public bool isprimarykey { get; set; }
        
        public bool isforeignkey { get; set; }
        
        public string? referencedTable { get; set; }
        
        public string? referencedColumn { get; set; }
        
        public string? defaultvalue { get; set; }
        
        [NotMapped]
        public bool isNew { get; set; }
        [NotMapped]
        public bool isUpdated { get; set; }
        [NotMapped]
        public bool isDelete { get; set; }

    }
}
