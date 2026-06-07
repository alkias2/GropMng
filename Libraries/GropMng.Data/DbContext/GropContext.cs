using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Domain.Logging;
using GropMng.Core.Domain.Media;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Security;
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
    public DbSet<OwnerRole> OwnerRoles => Set<OwnerRole>();
    public DbSet<OwnerPassword> OwnerPasswords => Set<OwnerPassword>();
    public DbSet<PermissionRecord> PermissionRecords => Set<PermissionRecord>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<LocaleStringResource> LocaleStringResources => Set<LocaleStringResource>();
    public DbSet<Picture> Pictures => Set<Picture>();

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
            entity.ToTable("Owner", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_Owner_Status", BuildEnumConstraintSql<OwnerAccountStatus>("Status"));
            });

            entity.HasKey(e => e.Id).HasName("PK_Owner");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).HasStorageEnumConversion().HasDefaultValue(OwnerAccountStatus.Active).HasSentinel(OwnerAccountStatus.Active);
            entity.Property(e => e.IsEmailConfirmed).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAtUtc).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAtUtc).HasColumnType("datetime2(7)");

            entity.HasIndex(e => e.OwnerId).IsUnique().HasDatabaseName("UQ_Owner_OwnerId");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Owner_Email");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Owner_Status");
        });

        modelBuilder.Entity<OwnerRole>(entity =>
        {
            entity.ToTable("OwnerRole");
            entity.HasKey(e => e.Id).HasName("PK_OwnerRole");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SystemName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSystemRole).HasDefaultValue(true);

            entity.HasIndex(e => e.SystemName).IsUnique().HasDatabaseName("UQ_OwnerRole_SystemName");
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_OwnerRole_Name");

            entity.HasMany(e => e.PermissionRecords)
                .WithMany(e => e.OwnerRoles)
                .UsingEntity<Dictionary<string, object>>(
                    "OwnerRole_PermissionRecord_Mapping",
                    right => right
                        .HasOne<PermissionRecord>()
                        .WithMany()
                        .HasForeignKey("PermissionRecordId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<OwnerRole>()
                        .WithMany()
                        .HasForeignKey("OwnerRoleId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("OwnerRole_PermissionRecord_Mapping");
                        join.HasKey("OwnerRoleId", "PermissionRecordId");
                    });
        });

        modelBuilder.Entity<OwnerPassword>(entity =>
        {
            entity.ToTable("OwnerPassword");
            entity.HasKey(e => e.Id).HasName("PK_OwnerPassword");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.PasswordSalt).HasMaxLength(256);
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2(7)").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.IsCurrent).HasDefaultValue(true);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(256);
            entity.Property(e => e.PasswordResetTokenExpiresAtUtc).HasColumnType("datetime2(7)");

            entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_OwnerPassword_OwnerId");
            entity.HasIndex(e => new { e.OwnerId, e.IsCurrent })
                .HasDatabaseName("UQ_OwnerPassword_CurrentPerOwner")
                .IsUnique()
                .HasFilter("[IsCurrent] = 1");

            entity.HasOne(e => e.Owner)
                .WithMany(e => e.Passwords)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OwnerPassword_Owner");
        });

        modelBuilder.Entity<PermissionRecord>(entity =>
        {
            entity.ToTable("PermissionRecord");
            entity.HasKey(e => e.Id).HasName("PK_PermissionRecord");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SystemName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(200).IsRequired();

            entity.HasIndex(e => e.SystemName).IsUnique().HasDatabaseName("UQ_PermissionRecord_SystemName");
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_PermissionRecord_Category");
        });

        modelBuilder.Entity<Owner>()
            .HasMany(e => e.OwnerRoles)
            .WithMany(e => e.Owners)
            .UsingEntity<Dictionary<string, object>>(
                "Owner_OwnerRole_Mapping",
                right => right
                    .HasOne<OwnerRole>()
                    .WithMany()
                    .HasForeignKey("OwnerRoleId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Owner>()
                    .WithMany()
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("Owner_OwnerRole_Mapping");
                    join.HasKey("OwnerId", "OwnerRoleId");
                });

        ConfigureGardenDomain(modelBuilder);

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("Picture");
            entity.HasKey(e => e.Id).HasName("PK_Picture");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MimeType).HasMaxLength(40).IsRequired();
            entity.Property(e => e.SeoFilename).HasMaxLength(300);
            entity.Property(e => e.AltAttribute).HasMaxLength(300);
            entity.Property(e => e.TitleAttribute).HasMaxLength(300);
            entity.Property(e => e.IsNew).HasDefaultValue(true);
            entity.Property(e => e.VirtualPath).HasMaxLength(2000);
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnType("datetime2(7)")
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
