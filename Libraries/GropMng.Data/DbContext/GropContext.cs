using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Domain.Logging;
using GropMng.Core.Domain.Garden.Owners;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Data.DbContext;

/// <summary>
/// Defines the Entity Framework Core context represented by GropContext.
/// Exposes mapped sets and configuration required for database interaction.
/// </summary>
public partial class GropContext : Microsoft.EntityFrameworkCore.DbContext
{
    public GropContext(DbContextOptions<GropContext> options) : base(options)
    {
    }

    public DbSet<AppLog> AppLogs => Set<AppLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<LocaleStringResource> LocaleStringResources => Set<LocaleStringResource>();

    partial void ConfigureGardenDomain(ModelBuilder modelBuilder);

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

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("Language");
            entity.HasKey(e => e.Id).HasName("PK_Language");

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LanguageCulture).HasMaxLength(20).IsRequired();
            entity.Property(e => e.UniqueSeoCode).HasMaxLength(2).IsRequired();
            entity.Property(e => e.FlagImageFileName).HasMaxLength(100);
            entity.Property(e => e.Rtl).HasDefaultValue(false);
            entity.Property(e => e.Published).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedOnUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedOnUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => e.LanguageCulture).IsUnique().HasDatabaseName("UQ_Language_Culture");
            entity.HasIndex(e => e.UniqueSeoCode).IsUnique().HasDatabaseName("UQ_Language_SeoCode");
        });

        modelBuilder.Entity<LocaleStringResource>(entity =>
        {
            entity.ToTable("LocaleStringResource");
            entity.HasKey(e => e.Id).HasName("PK_LocaleStringResource");

            entity.Property(e => e.LanguageId).IsRequired();
            entity.Property(e => e.ResourceName).HasMaxLength(400).IsRequired();
            entity.Property(e => e.ResourceValue).IsRequired();
            entity.Property(e => e.CreatedOnUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedOnUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new { e.LanguageId, e.ResourceName })
                .IsUnique()
                .HasDatabaseName("UQ_LocaleStringResource_LanguageId_ResourceName");

            entity.HasOne(e => e.Language)
                .WithMany(e => e.LocaleStringResources)
                .HasForeignKey(e => e.LanguageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_LocaleStringResource_Language");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.ToTable("Owner");
            entity.HasKey(e => e.Id).HasName("PK_Owner");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAtUtc).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAtUtc).HasColumnType("datetime2(7)");

            entity.HasIndex(e => e.OwnerId).IsUnique().HasDatabaseName("UQ_Owner_OwnerId");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Owner_Email");
        });

        ConfigureGardenDomain(modelBuilder);
    }
}
