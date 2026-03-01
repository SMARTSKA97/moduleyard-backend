using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ServerMetric
{
    public Guid Id { get; set; }

    public string ServerName { get; set; } = null!;

    public decimal CpuUsagePercentage { get; set; }

    public decimal MemoryUsageMb { get; set; }

    public DateTime RecordedAt { get; set; }
}
