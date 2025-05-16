namespace easydev.Models
{
    public class SchemaEntry
    {
        public string Title { get; set; }
        public string Type { get; set; }
    }

    public class DatabaseNodeData
    {
        public string Label { get; set; }
        public List<SchemaEntry> Schema { get; set; }
    }

    public class DatabaseNode
    {
        public string Id { get; set; }
        public Position Position { get; set; }
        public string Type { get; set; }
        public DatabaseNodeData Data { get; set; }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Edge
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string SourceHandle { get; set; }
        public string TargetHandle { get; set; }
        public bool Animated { get; set; }
        public EdgeStyle Style { get; set; }
        public string Label { get; set; }
    }

    public class EdgeStyle
    {
        public string Stroke { get; set; }
    }


    public class SchemaResponse
    {
        public List<DatabaseNode> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
    }


}
