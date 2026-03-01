using System;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ServerMetric> ServerMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServerMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("server_metrics_pkey");

            entity.ToTable("server_metrics", "sysmonitor");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CpuUsagePercentage)
                .HasPrecision(5, 2)
                .HasColumnName("cpu_usage_percentage");
            entity.Property(e => e.MemoryUsageMb)
                .HasPrecision(10, 2)
                .HasColumnName("memory_usage_mb");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("recorded_at");
            entity.Property(e => e.ServerName)
                .HasMaxLength(255)
                .HasColumnName("server_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
