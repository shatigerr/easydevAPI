using System;
using System.Collections.Generic;

namespace easydev.Models;

public partial class Log
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public long? IdUser { get; set; }

    public long? IdProject { get; set; }

    public string? Query { get; set; }

    public double? RequestDuration { get; set; }


    public virtual Project? IdProjectNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
