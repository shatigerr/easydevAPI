using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace easydev.Models;

public partial class Endpoint
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? HttpMethod { get; set; }

    public string? Url { get; set; }

    public string? Query { get; set; }

    public long? IdProject { get; set; }

    public string? Params { get; set; }
    [JsonIgnore]
    public virtual Project? IdProjectNavigation { get; set; }
}