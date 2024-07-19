using System;
using System.Collections.Generic;

namespace easydev.Models;

public partial class User
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
