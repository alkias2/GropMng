using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Logging;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Data.DbContext;

/// <summary>
/// Defines the Entity Framework Core context represented by GropContext.
/// Exposes mapped sets and configuration required for database interaction.
/// </summary>
public class GropContext : Microsoft.EntityFrameworkCore.DbContext
{
    public GropContext(DbContextOptions<GropContext> options) : base(options)
    {
    }

    public DbSet<AppLog> AppLogs => Set<AppLog>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppLog>(entity =>
        {
            entity.ToTable("AppLog");
            entity.HasKey(e => e.Id).HasName("PK_AppLogs");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Level).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.StackTrace);
            entity.Property(e => e.ExceptionType).HasMaxLength(500);
            entity.Property(e => e.EventId);
            entity.Property(e => e.RequestPath).HasMaxLength(2000);
            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime2(7)")
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Setting");
            entity.HasKey(e => e.Id).HasName("PK_Setting");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Value).IsRequired();
        });
    }
}
