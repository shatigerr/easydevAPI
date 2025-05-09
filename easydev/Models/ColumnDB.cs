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
        public string? defaultvalue { get; set; }

    }
}
