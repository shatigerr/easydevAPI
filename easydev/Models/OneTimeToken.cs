﻿using System;
using System.Collections.Generic;

namespace easydev.Models;

public partial class OneTimeToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string RelatesTo { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User1 User { get; set; } = null!;
}
