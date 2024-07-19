using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace easydev.Models;

public partial class Database
{
    public long Id { get; set; }

    public string? Dbengine { get; set; }

    public string? Host { get; set; }

    public string? Password { get; set; }

    public string? User { get; set; }

    public string? Database1 { get; set; }

    public string? Port { get; set; }

    [JsonIgnore]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
