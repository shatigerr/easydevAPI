using System.ComponentModel.DataAnnotations.Schema;

namespace easydev.Models
{
    public class TableDB
    {
        public long id { get; set; }
        public long iddatabase { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        [NotMapped]
        public List<ColumnDB> columnsDB { get; set; }
    }
}
