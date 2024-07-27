using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace easydev.Models;

public partial class Project
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid? Key { get; set; }

    public long? IdUser { get; set; }

    public long? Iddatabase { get; set; }

    public virtual ICollection<Endpoint> Endpoints { get; set; } = new List<Endpoint>();

    [JsonIgnore]
    public virtual User? IdUserNavigation { get; set; }

    public virtual Database? IddatabaseNavigation { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}
