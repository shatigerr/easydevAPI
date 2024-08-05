using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace easydev.Models;

public partial class User
{
    public long Id { get; set; }


    public string Name { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public short? Activated { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
