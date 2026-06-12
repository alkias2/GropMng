using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.AI;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Domain.Garden.Preferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GropMng.Data.DbContext;

public partial class GropContext
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<GardenSpot> GardenSpots => Set<GardenSpot>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<SoilMix> SoilMixes => Set<SoilMix>();
    public DbSet<Container> Containers => Set<Container>();
    public DbSet<PlantInstance> PlantInstances => Set<PlantInstance>();
    public DbSet<WateringSchedule> WateringSchedules => Set<WateringSchedule>();
    public DbSet<Fertilizer> Fertilizers => Set<Fertilizer>();
    public DbSet<FertilizingSchedule> FertilizingSchedules => Set<FertilizingSchedule>();
    public DbSet<PlantPhoto> PlantPhotos => Set<PlantPhoto>();
    public DbSet<PlantNote> PlantNotes => Set<PlantNote>();
    public DbSet<DiseaseKnowledge> DiseaseKnowledges => Set<DiseaseKnowledge>();
    public DbSet<DiseaseKnowledgePhoto> DiseaseKnowledgePhotos => Set<DiseaseKnowledgePhoto>();
    public DbSet<DiseaseKnowledgePlant> DiseaseKnowledgePlants => Set<DiseaseKnowledgePlant>();
    public DbSet<PlantProblemRecord> PlantProblemRecords => Set<PlantProblemRecord>();
    public DbSet<PlantProblemSchedule> PlantProblemSchedules => Set<PlantProblemSchedule>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
    public DbSet<AIQueryTemplate> AIQueryTemplates => Set<AIQueryTemplate>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<SoilIngredient> SoilIngredients => Set<SoilIngredient>();
    public DbSet<SoilMixIngredient> SoilMixIngredients => Set<SoilMixIngredient>();
    public DbSet<WateringLog> WateringLogs => Set<WateringLog>();
    public DbSet<FertilizingLog> FertilizingLogs => Set<FertilizingLog>();
    public DbSet<RepottingLog> RepottingLogs => Set<RepottingLog>();
    public DbSet<ActionSkip> ActionSkips => Set<ActionSkip>();

    partial void ConfigureGardenDomain(ModelBuilder modelBuilder)
    {
        ConfigureLocation(modelBuilder.Entity<Location>());
        ConfigureGardenSpot(modelBuilder.Entity<GardenSpot>());
        ConfigurePlant(modelBuilder.Entity<Plant>());
        ConfigureSoilMix(modelBuilder.Entity<SoilMix>());
        ConfigureContainer(modelBuilder.Entity<Container>());
        ConfigurePlantInstance(modelBuilder.Entity<PlantInstance>());
        ConfigureWateringSchedule(modelBuilder.Entity<WateringSchedule>());
        ConfigureFertilizer(modelBuilder.Entity<Fertilizer>());
        ConfigureFertilizingSchedule(modelBuilder.Entity<FertilizingSchedule>());
        ConfigurePlantPhoto(modelBuilder.Entity<PlantPhoto>());
        ConfigurePlantNote(modelBuilder.Entity<PlantNote>());
        ConfigureDiseaseKnowledge(modelBuilder.Entity<DiseaseKnowledge>());
        ConfigureDiseaseKnowledgePhoto(modelBuilder.Entity<DiseaseKnowledgePhoto>());
        ConfigureDiseaseKnowledgePlant(modelBuilder.Entity<DiseaseKnowledgePlant>());
        ConfigurePlantProblemRecord(modelBuilder.Entity<PlantProblemRecord>());
        ConfigurePlantProblemSchedule(modelBuilder.Entity<PlantProblemSchedule>());
        ConfigureAdminNotification(modelBuilder.Entity<AdminNotification>());
        ConfigureAiQueryTemplate(modelBuilder.Entity<AIQueryTemplate>());
        ConfigureUserPreference(modelBuilder.Entity<UserPreference>());
        ConfigureSoilIngredient(modelBuilder.Entity<SoilIngredient>());
        ConfigureSoilMixIngredient(modelBuilder.Entity<SoilMixIngredient>());
        ConfigureWateringLog(modelBuilder.Entity<WateringLog>());
        ConfigureFertilizingLog(modelBuilder.Entity<FertilizingLog>());
        ConfigureRepottingLog(modelBuilder.Entity<RepottingLog>());
        ConfigureActionSkip(modelBuilder.Entity<ActionSkip>());
    }

    private static void ConfigureLocation(EntityTypeBuilder<Location> entity)
    {
        entity.ToTable("Location");
        entity.HasKey(e => e.Id).HasName("PK_Location");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.City).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Greece").IsRequired();
        entity.Property(e => e.Latitude).HasPrecision(10, 7);
        entity.Property(e => e.Longitude).HasPrecision(10, 7);
        entity.Property(e => e.ClimateZone).HasMaxLength(50);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Location_OwnerId");

        entity.HasMany(e => e.GardenSpots)
            .WithOne(e => e.Location)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_GardenSpot_Location");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Location_Owner");
    }

    private static void ConfigureGardenSpot(EntityTypeBuilder<GardenSpot> entity)
    {
        entity.ToTable("GardenSpot", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_GardenSpot_SunHours", "[SunHoursPerDay] IS NULL OR [SunHoursPerDay] BETWEEN 0 AND 24");
            tableBuilder.HasCheckConstraint("CK_GardenSpot_Orientation", BuildEnumConstraintSql<GardenOrientation>("Orientation", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_GardenSpot_CoverType", BuildEnumConstraintSql<GardenCoverType>("CoverType", isNullable: true));
        });

        entity.HasKey(e => e.Id).HasName("PK_GardenSpot");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Orientation).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.CoverType).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.Surroundings).HasMaxLength(500);
        entity.Property(e => e.Notes);
        entity.Property(e => e.PictureId).HasDefaultValue(0);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_GardenSpot_OwnerId");
        entity.HasIndex(e => e.LocationId).HasDatabaseName("IX_GardenSpot_LocationId");
        entity.HasIndex(e => new { e.LocationId, e.Name })
            .IsUnique()
            .HasDatabaseName("UQ_GardenSpot_LocationId_Name")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_GardenSpot_Owner");
    }

    private static void ConfigurePlant(EntityTypeBuilder<Plant> entity)
    {
        entity.ToTable("Plant", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Plant_Category", BuildEnumConstraintSql<PlantCategory>("Category"));
            tableBuilder.HasCheckConstraint("CK_Plant_GrowthType", BuildEnumConstraintSql<PlantGrowthType>("GrowthType", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_Plant_SunRequirement", BuildEnumConstraintSql<PlantSunRequirement>("SunRequirement", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_Plant_WaterRequirement", BuildEnumConstraintSql<PlantWaterRequirement>("WaterRequirement", isNullable: true));
        });

        entity.HasKey(e => e.Id).HasName("PK_Plant");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.CommonName).HasMaxLength(200).IsRequired();
        entity.Property(e => e.ScientificName).HasMaxLength(300).IsRequired();
        entity.Property(e => e.Family).HasMaxLength(200);
        entity.Property(e => e.Category).HasMaxLength(50).HasStorageEnumConversion().HasDefaultValue(PlantCategory.Other).HasSentinel(PlantCategory.Other);
        entity.Property(e => e.GrowthType).HasMaxLength(30).HasNullableStorageEnumConversion();
        entity.Property(e => e.SunRequirement).HasMaxLength(30).HasNullableStorageEnumConversion();
        entity.Property(e => e.WaterRequirement).HasMaxLength(20).HasNullableStorageEnumConversion();
        entity.Property(e => e.MinTempCelsius).HasPrecision(5, 1);
        entity.Property(e => e.MaxTempCelsius).HasPrecision(5, 1);
        entity.Property(e => e.IsEdible).HasDefaultValue(false);
        entity.Property(e => e.IsMedicinal).HasDefaultValue(false);
        entity.Property(e => e.IsToxic).HasDefaultValue(false);
        entity.Property(e => e.GeneralNotes);
        entity.Property(e => e.PictureId).HasDefaultValue(0);

        entity.HasIndex(e => e.CommonName).HasDatabaseName("IX_Plant_CommonName");
        entity.HasIndex(e => e.ScientificName).HasDatabaseName("IX_Plant_ScientificName");
        entity.HasIndex(e => e.ScientificName)
            .IsUnique()
            .HasDatabaseName("UQ_Plant_ScientificName")
            .HasFilter("[IsDeleted] = 0");
    }

    private static void ConfigureSoilMix(EntityTypeBuilder<SoilMix> entity)
    {
        entity.ToTable("SoilMix", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SoilMix_PhMin", "[PhMin] IS NULL OR [PhMin] BETWEEN 0 AND 14");
            tableBuilder.HasCheckConstraint("CK_SoilMix_PhMax", "[PhMax] IS NULL OR [PhMax] BETWEEN 0 AND 14");
            tableBuilder.HasCheckConstraint("CK_SoilMix_PhRange", "[PhMin] IS NULL OR [PhMax] IS NULL OR [PhMin] <= [PhMax]");
            tableBuilder.HasCheckConstraint("CK_SoilMix_Texture", BuildEnumConstraintSql<SoilTextureType>("Texture", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_SoilMix_Drainage", BuildEnumConstraintSql<SoilDrainageType>("Drainage", isNullable: true));
        });

        entity.HasKey(e => e.Id).HasName("PK_SoilMix");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Composition);
        entity.Property(e => e.PhMin).HasPrecision(4, 2);
        entity.Property(e => e.PhMax).HasPrecision(4, 2);
        entity.Property(e => e.Texture).HasMaxLength(30).HasNullableStorageEnumConversion();
        entity.Property(e => e.Drainage).HasMaxLength(20).HasNullableStorageEnumConversion();
        entity.Property(e => e.Notes);
    }

    private static void ConfigureContainer(EntityTypeBuilder<Container> entity)
    {
        entity.ToTable("Container", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Container_ContainerType", BuildEnumConstraintSql<GardenContainerType>("ContainerType"));
            tableBuilder.HasCheckConstraint("CK_Container_Dimensions",
                "[BaseCircumferenceCm] IS NOT NULL OR [RimCircumferenceCm] IS NOT NULL OR [HeightCm] IS NOT NULL OR [LengthCm] IS NOT NULL OR [WidthCm] IS NOT NULL OR [VolumeL] IS NOT NULL");
        });

        entity.HasKey(e => e.Id).HasName("PK_Container");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.ContainerType).HasMaxLength(30).HasStorageEnumConversion().HasDefaultValue(GardenContainerType.Pot);
        entity.Property(e => e.Material).HasMaxLength(100);
        entity.Property(e => e.BaseCircumferenceCm).HasPrecision(8, 2);
        entity.Property(e => e.RimCircumferenceCm).HasPrecision(8, 2);
        entity.Property(e => e.HeightCm).HasPrecision(8, 2);
        entity.Property(e => e.LengthCm).HasPrecision(8, 2);
        entity.Property(e => e.WidthCm).HasPrecision(8, 2);
        entity.Property(e => e.VolumeL).HasPrecision(8, 2);
        entity.Property(e => e.Color).HasMaxLength(50);
        entity.Property(e => e.HasDrainageHole).HasDefaultValue(true);
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Container_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).IsUnique().HasDatabaseName("UX_Container_PlantInstanceId");

        entity.HasOne(e => e.PlantInstance)
            .WithOne(e => e.Container)
            .HasForeignKey<Container>(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Container_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Container_Owner");
    }

    private static void ConfigurePlantInstance(EntityTypeBuilder<PlantInstance> entity)
    {
        entity.ToTable("PlantInstance", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PlantInstance_SizeCategory", BuildEnumConstraintSql<PlantSizeCategory>("SizeCategory", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_PlantInstance_HealthStatus", BuildEnumConstraintSql<PlantHealthStatus>("HealthStatus"));
        });

        entity.HasKey(e => e.Id).HasName("PK_PlantInstance");
        ConfigureAuditableEntity(entity);

        entity.Ignore(e => e.AgeYears);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.GardenSpotId).HasColumnName("SpotId");
        entity.Property(e => e.Nickname).HasMaxLength(200);
        entity.Property(e => e.PlantedDate).HasColumnType("date");
        entity.Property(e => e.SizeCategory).HasMaxLength(20).HasNullableStorageEnumConversion();
        entity.Property(e => e.HeightCm).HasPrecision(8, 1);
        entity.Property(e => e.SpreadCm).HasPrecision(8, 1);
        entity.Property(e => e.HealthStatus).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(PlantHealthStatus.Good).HasSentinel(PlantHealthStatus.Good);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantInstance_OwnerId");
        entity.HasIndex(e => e.PlantId).HasDatabaseName("IX_PlantInstance_PlantId");
        entity.HasIndex(e => e.GardenSpotId).HasDatabaseName("IX_PlantInstance_SpotId");
        entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_PlantInstance_IsActive");

        entity.HasOne(e => e.Plant)
            .WithMany(e => e.PlantInstances)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantInstance_Plant");

        entity.HasOne(e => e.GardenSpot)
            .WithMany(e => e.PlantInstances)
            .HasForeignKey(e => e.GardenSpotId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantInstance_GardenSpot");

        // ContainerId on PlantInstance is the inverse side; the FK lives on Container.PlantInstanceId
        entity.Ignore(e => e.ContainerId);

        entity.HasOne(e => e.SoilMix)
            .WithMany(e => e.PlantInstances)
            .HasForeignKey(e => e.SoilMixId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_PlantInstance_SoilMix");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantInstance_Owner");
    }

    private static void ConfigureDiseaseKnowledge(EntityTypeBuilder<DiseaseKnowledge> entity)
    {
        entity.ToTable("DiseaseKnowledge");
        entity.HasKey(e => e.Id).HasName("PK_DiseaseKnowledge");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.CommonName).HasMaxLength(300).IsRequired();
        entity.Property(e => e.ScientificName).HasMaxLength(300);
        entity.Property(e => e.Description).IsRequired();
        entity.Property(e => e.TreatmentGuidelines).IsRequired();

        entity.HasIndex(e => e.CommonName)
            .IsUnique()
            .HasDatabaseName("UQ_DiseaseKnowledge_CommonName")
            .HasFilter("[IsDeleted] = 0");
    }

    private static void ConfigureDiseaseKnowledgePhoto(EntityTypeBuilder<DiseaseKnowledgePhoto> entity)
    {
        entity.ToTable("DiseaseKnowledgePhoto");
        entity.HasKey(e => e.Id).HasName("PK_DiseaseKnowledgePhoto");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.PictureId).HasDefaultValue(0);
        entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
        entity.Property(e => e.Caption).HasMaxLength(500);

        entity.HasIndex(e => e.DiseaseKnowledgeId).HasDatabaseName("IX_DiseaseKnowledgePhoto_DiseaseKnowledgeId");
        entity.HasIndex(e => e.PictureId).HasDatabaseName("IX_DiseaseKnowledgePhoto_PictureId");

        entity.HasOne(e => e.DiseaseKnowledge)
            .WithMany(e => e.Photos)
            .HasForeignKey(e => e.DiseaseKnowledgeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DiseaseKnowledgePhoto_DiseaseKnowledge");
    }

    private static void ConfigureDiseaseKnowledgePlant(EntityTypeBuilder<DiseaseKnowledgePlant> entity)
    {
        entity.ToTable("DiseaseKnowledgePlant");
        entity.HasKey(e => e.Id).HasName("PK_DiseaseKnowledgePlant");
        ConfigureAuditableEntity(entity);

        entity.HasIndex(e => e.DiseaseKnowledgeId).HasDatabaseName("IX_DiseaseKnowledgePlant_DiseaseKnowledgeId");
        entity.HasIndex(e => e.PlantId).HasDatabaseName("IX_DiseaseKnowledgePlant_PlantId");
        entity.HasIndex(e => new { e.DiseaseKnowledgeId, e.PlantId })
            .IsUnique()
            .HasDatabaseName("UQ_DiseaseKnowledgePlant")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne(e => e.DiseaseKnowledge)
            .WithMany(e => e.PlantLinks)
            .HasForeignKey(e => e.DiseaseKnowledgeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DiseaseKnowledgePlant_DiseaseKnowledge");

        entity.HasOne(e => e.Plant)
            .WithMany()
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_DiseaseKnowledgePlant_Plant");
    }

    private static void ConfigurePlantProblemRecord(EntityTypeBuilder<PlantProblemRecord> entity)
    {
        entity.ToTable("PlantProblemRecord", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PlantProblemRecord_Severity", BuildEnumConstraintSql<Severity>("Severity"));
            tableBuilder.HasCheckConstraint("CK_PlantProblemRecord_ProblemStatus", BuildEnumConstraintSql<ProblemStatus>("ProblemStatus"));
            tableBuilder.HasCheckConstraint("CK_PlantProblemRecord_InfoSource", BuildEnumConstraintSql<InfoSource>("InfoSource"));
        });

        entity.HasKey(e => e.Id).HasName("PK_PlantProblemRecord");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.DiseaseKnowledgeId).IsRequired(false);
        entity.Property(e => e.ProblemName).HasMaxLength(300).IsRequired();
        entity.Property(e => e.DetectedDate).HasColumnType("date").HasDefaultValueSql("CAST(SYSUTCDATETIME() AS date)");
entity.Property(e => e.Severity).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(Severity.Medium).HasSentinel(Severity.Low);
        entity.Property(e => e.ProblemStatus).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(ProblemStatus.Active);
        entity.Property(e => e.InfoSource).HasMaxLength(30).HasStorageEnumConversion().HasDefaultValue(InfoSource.OwnKnowledge);
        entity.Property(e => e.Notes);
        entity.Property(e => e.ResolvedDate).HasColumnType("date");
        entity.Property(e => e.NotifyAdmin).HasDefaultValue(false);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantProblemRecord_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_PlantProblemRecord_InstanceId");
        entity.HasIndex(e => e.DiseaseKnowledgeId).HasDatabaseName("IX_PlantProblemRecord_DiseaseKnowledgeId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.ProblemRecords)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantProblemRecord_PlantInstance");

        entity.HasOne(e => e.DiseaseKnowledge)
            .WithMany()
            .HasForeignKey(e => e.DiseaseKnowledgeId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_PlantProblemRecord_DiseaseKnowledge");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantProblemRecord_Owner");
    }

    private static void ConfigurePlantProblemSchedule(EntityTypeBuilder<PlantProblemSchedule> entity)
    {
        entity.ToTable("PlantProblemSchedule", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PlantProblemSchedule_FrequencyUnit", BuildEnumConstraintSql<ScheduleFrequencyUnit>("FrequencyUnit"));
            tableBuilder.HasCheckConstraint("CK_PlantProblemSchedule_ScheduleStatus", BuildEnumConstraintSql<ScheduleStatus>("ScheduleStatus"));
            tableBuilder.HasCheckConstraint("CK_PlantProblemSchedule_FrequencyValue", "[FrequencyValue] > 0");
        });

        entity.HasKey(e => e.Id).HasName("PK_PlantProblemSchedule");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantProblemRecordId).HasColumnName("RecordId");
        entity.Property(e => e.ActionName).HasMaxLength(300).IsRequired();
        entity.Property(e => e.FrequencyValue).HasDefaultValue(7);
        entity.Property(e => e.FrequencyUnit).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(ScheduleFrequencyUnit.Days);
        entity.Property(e => e.DosageNotes).HasMaxLength(500);
        entity.Property(e => e.StartDate).HasColumnType("date").IsRequired();
        entity.Property(e => e.NextDueDate).HasColumnType("date").IsRequired();
        entity.Property(e => e.ScheduleStatus).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(ScheduleStatus.Active);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantProblemSchedule_OwnerId");
        entity.HasIndex(e => e.PlantProblemRecordId).HasDatabaseName("IX_PlantProblemSchedule_RecordId");
        entity.HasIndex(e => e.NextDueDate).HasDatabaseName("IX_PlantProblemSchedule_NextDueDate");

        entity.HasOne(e => e.PlantProblemRecord)
            .WithMany(e => e.Schedules)
            .HasForeignKey(e => e.PlantProblemRecordId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantProblemSchedule_PlantProblemRecord");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantProblemSchedule_Owner");
    }

    private static void ConfigureAdminNotification(EntityTypeBuilder<AdminNotification> entity)
    {
        entity.ToTable("AdminNotification");
        entity.HasKey(e => e.Id).HasName("PK_AdminNotification");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.ProblemName).HasMaxLength(300).IsRequired();
        entity.Property(e => e.IsResolved).HasDefaultValue(false);
        entity.Property(e => e.ResolvedAtUtc).HasColumnType("datetime2(7)");

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_AdminNotification_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_AdminNotification_InstanceId");
        entity.HasIndex(e => e.IsResolved).HasDatabaseName("IX_AdminNotification_IsResolved");
        entity.HasIndex(e => e.DiseaseKnowledgeId).HasDatabaseName("IX_AdminNotification_DiseaseKnowledgeId");

        entity.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_AdminNotification_Owner");

        entity.HasOne(e => e.PlantInstance)
            .WithMany()
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_AdminNotification_PlantInstance");

        entity.HasOne(e => e.DiseaseKnowledge)
            .WithMany()
            .HasForeignKey(e => e.DiseaseKnowledgeId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_AdminNotification_DiseaseKnowledge");
    }

    private static void ConfigureWateringSchedule(EntityTypeBuilder<WateringSchedule> entity)
    {
        entity.ToTable("WateringSchedule", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_WateringSchedule_Season", BuildEnumConstraintSql<GardenSeason>("Season"));
            tableBuilder.HasCheckConstraint("CK_WateringSchedule_TimeOfDay", BuildEnumConstraintSql<GardenTimeOfDay>("TimeOfDay", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_WateringSchedule_FrequencyDays", "[FrequencyDays] > 0");
        });

        entity.HasKey(e => e.Id).HasName("PK_WateringSchedule");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.Season).HasMaxLength(20).HasStorageEnumConversion();
        entity.Property(e => e.FrequencyDays).HasDefaultValue((byte)3);
        entity.Property(e => e.WaterAmountL).HasPrecision(6, 2);
        entity.Property(e => e.TimeOfDay).HasMaxLength(20).HasNullableStorageEnumConversion().HasDefaultValue(GardenTimeOfDay.Morning);
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_WateringSchedule_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_WateringSchedule_InstanceId");
        entity.HasIndex(e => new { e.PlantInstanceId, e.Season })
            .IsUnique()
            .HasDatabaseName("UQ_WateringSchedule_InstanceSeason")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.WateringSchedules)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WateringSchedule_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WateringSchedule_Owner");
    }

    private static void ConfigureFertilizer(EntityTypeBuilder<Fertilizer> entity)
    {
        entity.ToTable("Fertilizer", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Fertilizer_FertilizerType", BuildEnumConstraintSql<FertilizerKind>("FertilizerType", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_Fertilizer_ApplicationMethod", BuildEnumConstraintSql<FertilizerApplicationMethod>("ApplicationMethod", isNullable: true));
        });

        entity.HasKey(e => e.Id).HasName("PK_Fertilizer");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Brand).HasMaxLength(200);
        entity.Property(e => e.FertilizerType).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.NpkRatio).HasMaxLength(30);
        entity.Property(e => e.ApplicationMethod).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.IsOrganic).HasDefaultValue(false);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.Name).HasDatabaseName("IX_Fertilizer_Name");
    }

    private static void ConfigureFertilizingSchedule(EntityTypeBuilder<FertilizingSchedule> entity)
    {
        entity.ToTable("FertilizingSchedule", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_FertilizingSchedule_Season", BuildEnumConstraintSql<GardenSeason>("Season"));
            tableBuilder.HasCheckConstraint("CK_FertilizingSchedule_Unit", BuildEnumConstraintSql<FertilizerQuantityUnit>("Unit", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_FertilizingSchedule_FrequencyDays", "[FrequencyDays] > 0");
            tableBuilder.HasCheckConstraint("CK_FertilizingSchedule_Quantity", "[Quantity] IS NULL OR [Quantity] >= 0");
        });

        entity.HasKey(e => e.Id).HasName("PK_FertilizingSchedule");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.Season).HasMaxLength(20).HasStorageEnumConversion();
        entity.Property(e => e.FrequencyDays).HasDefaultValue((byte)14);
        entity.Property(e => e.Quantity).HasPrecision(8, 3);
        entity.Property(e => e.Unit).HasMaxLength(10).HasNullableStorageEnumConversion().HasDefaultValue(FertilizerQuantityUnit.Gram);
        entity.Property(e => e.Notes).HasMaxLength(500);
        entity.Property(e => e.DilutionInstructions).HasMaxLength(50);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_FertilizingSchedule_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_FertilizingSchedule_InstanceId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.FertilizingSchedules)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_FertilizingSchedule_PlantInstance");

        entity.HasOne(e => e.Fertilizer)
            .WithMany(e => e.FertilizingSchedules)
            .HasForeignKey(e => e.FertilizerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FertilizingSchedule_Fertilizer");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FertilizingSchedule_Owner");
    }

    private static void ConfigurePlantPhoto(EntityTypeBuilder<PlantPhoto> entity)
    {
        entity.ToTable("PlantPhoto");
        entity.HasKey(e => e.Id).HasName("PK_PlantPhoto");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.PictureId).HasDefaultValue(0);
        entity.Property(e => e.TakenDate).HasColumnType("date").HasDefaultValueSql("CAST(SYSUTCDATETIME() AS date)");
        entity.Property(e => e.Caption).HasMaxLength(500);
        entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantPhoto_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_PlantPhoto_InstanceId");
        entity.HasIndex(e => e.PictureId).HasDatabaseName("IX_PlantPhoto_PictureId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.Photos)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantPhoto_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantPhoto_Owner");
    }

    private static void ConfigurePlantNote(EntityTypeBuilder<PlantNote> entity)
    {
        entity.ToTable("PlantNote");
        entity.HasKey(e => e.Id).HasName("PK_PlantNote");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.Title).HasMaxLength(300);
        entity.Property(e => e.RichHtmlContent).IsRequired();
        entity.Property(e => e.Tags).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantNote_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_PlantNote_InstanceId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.NotesEntries)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantNote_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantNote_Owner");
    }

    private static void ConfigureAiQueryTemplate(EntityTypeBuilder<AIQueryTemplate> entity)
    {
        entity.ToTable("AIQueryTemplate", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_AIQueryTemplate_Scenario", BuildEnumConstraintSql<AiQueryScenario>("Scenario"));
            tableBuilder.HasCheckConstraint("CK_AIQueryTemplate_Language", BuildEnumConstraintSql<SupportedLanguage>("Language"));
        });

        entity.HasKey(e => e.Id).HasName("PK_AIQueryTemplate");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Scenario).HasMaxLength(50).HasStorageEnumConversion();
        entity.Property(e => e.Language).HasMaxLength(10).HasStorageEnumConversion().HasDefaultValue(SupportedLanguage.Greek);
        entity.Property(e => e.PromptTemplate).IsRequired();
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.SortOrder).HasDefaultValue(0);

        entity.HasIndex(e => new { e.TemplateName, e.Language })
            .IsUnique()
            .HasDatabaseName("UQ_AIQueryTemplate_Name_Language")
            .HasFilter("[IsDeleted] = 0");
    }

    private static void ConfigureUserPreference(EntityTypeBuilder<UserPreference> entity)
    {
        entity.ToTable("UserPreference", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_UserPreference_LengthUnit", BuildEnumConstraintSql<LengthUnitType>("LengthUnit"));
            tableBuilder.HasCheckConstraint("CK_UserPreference_VolumeUnit", BuildEnumConstraintSql<VolumeUnitType>("VolumeUnit"));
            tableBuilder.HasCheckConstraint("CK_UserPreference_TemperatureUnit", BuildEnumConstraintSql<TemperatureUnitType>("TemperatureUnit"));
            tableBuilder.HasCheckConstraint("CK_UserPreference_DefaultLanguage", BuildEnumConstraintSql<SupportedLanguage>("DefaultLanguage"));
        });

        entity.HasKey(e => e.Id).HasName("PK_UserPreference");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.LengthUnit).HasMaxLength(5).HasStorageEnumConversion().HasDefaultValue(LengthUnitType.Centimetre);
        entity.Property(e => e.VolumeUnit).HasMaxLength(5).HasStorageEnumConversion().HasDefaultValue(VolumeUnitType.Litre);
        entity.Property(e => e.TemperatureUnit).HasMaxLength(2).HasStorageEnumConversion().HasDefaultValue(TemperatureUnitType.Celsius);
        entity.Property(e => e.DefaultLanguage).HasMaxLength(10).HasStorageEnumConversion().HasDefaultValue(SupportedLanguage.Greek);

        entity.HasIndex(e => e.OwnerId)
            .IsUnique()
            .HasDatabaseName("UQ_UserPreference_OwnerId")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UserPreference_Owner");
    }

    private static void ConfigureAuditableEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : AuditableEntity
    {
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CreatedAtUtc)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(e => e.UpdatedAtUtc)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        entity.Property(e => e.DeletedAtUtc).HasColumnType("datetime2(7)");
    }

    private static string BuildEnumConstraintSql<TEnum>(string columnName, bool isNullable = false)
        where TEnum : struct, Enum
    {
        var allowedValues = string.Join(", ", EnumStorageValueExtensions.GetStorageValues<TEnum>().Select(value => $"N'{value.Replace("'", "''")}'"));
        var predicate = $"[{columnName}] IN ({allowedValues})";

        return isNullable
            ? $"[{columnName}] IS NULL OR {predicate}"
            : predicate;
    }

    private static void ConfigureSoilIngredient(EntityTypeBuilder<SoilIngredient> entity)
    {
        entity.ToTable("SoilIngredient");
        entity.HasKey(e => e.Id).HasName("PK_SoilIngredient");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Description).HasMaxLength(500);

        entity.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("UQ_SoilIngredient_Name")
            .HasFilter("[IsDeleted] = 0");
    }

    private static void ConfigureSoilMixIngredient(EntityTypeBuilder<SoilMixIngredient> entity)
    {
        entity.ToTable("SoilMixIngredient", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SoilMixIngredient_Percentage", "[PercentageByVolume] BETWEEN 0 AND 100");
        });

        entity.HasKey(e => e.Id).HasName("PK_SoilMixIngredient");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.SoilMixId).IsRequired();
        entity.Property(e => e.SoilIngredientId).IsRequired();
        entity.Property(e => e.PercentageByVolume).HasPrecision(5, 2).IsRequired();
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.SoilMixId).HasDatabaseName("IX_SoilMixIngredient_SoilMixId");
        entity.HasIndex(e => new { e.SoilMixId, e.SoilIngredientId })
            .IsUnique()
            .HasDatabaseName("UQ_SoilMixIngredient_Mix_Ingredient")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne(e => e.SoilMix)
            .WithMany(e => e.Ingredients)
            .HasForeignKey(e => e.SoilMixId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SoilMixIngredient_SoilMix");

        entity.HasOne(e => e.SoilIngredient)
            .WithMany(e => e.SoilMixIngredients)
            .HasForeignKey(e => e.SoilIngredientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SoilMixIngredient_SoilIngredient");
    }

    private static void ConfigureWateringLog(EntityTypeBuilder<WateringLog> entity)
    {
        entity.ToTable("WateringLog");
        entity.HasKey(e => e.Id).HasName("PK_WateringLog");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.WateredAtUtc).HasColumnType("datetime2(7)").IsRequired();
        entity.Property(e => e.WaterAmountL).HasPrecision(6, 2);
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_WateringLog_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_WateringLog_InstanceId");
        entity.HasIndex(e => e.WateredAtUtc).HasDatabaseName("IX_WateringLog_WateredAtUtc");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.WateringLogs)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WateringLog_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WateringLog_Owner");
    }

    private static void ConfigureFertilizingLog(EntityTypeBuilder<FertilizingLog> entity)
    {
        entity.ToTable("FertilizingLog", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_FertilizingLog_Unit", BuildEnumConstraintSql<FertilizerQuantityUnit>("Unit", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_FertilizingLog_Quantity", "[Quantity] IS NULL OR [Quantity] >= 0");
        });

        entity.HasKey(e => e.Id).HasName("PK_FertilizingLog");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.AppliedAtUtc).HasColumnType("datetime2(7)").IsRequired();
        entity.Property(e => e.Quantity).HasPrecision(8, 3);
        entity.Property(e => e.Unit).HasMaxLength(10).HasNullableStorageEnumConversion();
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_FertilizingLog_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_FertilizingLog_InstanceId");
        entity.HasIndex(e => e.AppliedAtUtc).HasDatabaseName("IX_FertilizingLog_AppliedAtUtc");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.FertilizingLogs)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_FertilizingLog_PlantInstance");

        entity.HasOne(e => e.Fertilizer)
            .WithMany()
            .HasForeignKey(e => e.FertilizerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FertilizingLog_Fertilizer");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FertilizingLog_Owner");
    }

    private static void ConfigureRepottingLog(EntityTypeBuilder<RepottingLog> entity)
    {
        entity.ToTable("RepottingLog");
        entity.HasKey(e => e.Id).HasName("PK_RepottingLog");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.RepottedAtUtc).HasColumnType("datetime2(7)").IsRequired();
        entity.Property(e => e.SoilMixChanged).HasDefaultValue(false);
        entity.Property(e => e.ContainerChanged).HasDefaultValue(false);
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_RepottingLog_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_RepottingLog_InstanceId");
        entity.HasIndex(e => e.RepottedAtUtc).HasDatabaseName("IX_RepottingLog_RepottedAtUtc");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.RepottingLogs)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RepottingLog_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RepottingLog_Owner");
    }

    private static void ConfigureActionSkip(EntityTypeBuilder<ActionSkip> entity)
    {
        entity.ToTable("ActionSkip", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ActionSkip_ActionType", "[ActionType] IN (0, 1)");
        });

        entity.HasKey(e => e.Id).HasName("PK_ActionSkip");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.ActionType).IsRequired();
        entity.Property(e => e.SkippedAtUtc).HasColumnType("datetime2(7)").IsRequired();
        entity.Property(e => e.ActiveUntilDate).HasColumnType("date").IsRequired();

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_ActionSkip_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_ActionSkip_InstanceId");
        entity.HasIndex(e => e.ActiveUntilDate).HasDatabaseName("IX_ActionSkip_ActiveUntilDate");
        entity.HasIndex(new[] { nameof(ActionSkip.OwnerId), nameof(ActionSkip.PlantInstanceId), nameof(ActionSkip.ActionType), nameof(ActionSkip.ActiveUntilDate) })
              .HasDatabaseName("IX_ActionSkip_Owner_Instance_Type_Until");

        entity.HasOne(e => e.PlantInstance)
            .WithMany()
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ActionSkip_PlantInstance");

        entity.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .HasPrincipalKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ActionSkip_Owner");
    }
}

internal static class EnumPropertyBuilderExtensions
{
    public static PropertyBuilder<TEnum> HasStorageEnumConversion<TEnum>(this PropertyBuilder<TEnum> propertyBuilder)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasConversion(
            enumValue => enumValue.ToStorageValue(),
            storageValue => EnumStorageValueExtensions.FromStorageValue<TEnum>(storageValue));
    }

    public static PropertyBuilder<TEnum?> HasNullableStorageEnumConversion<TEnum>(this PropertyBuilder<TEnum?> propertyBuilder)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasConversion(
            enumValue => enumValue.HasValue ? enumValue.Value.ToStorageValue() : null,
            storageValue => string.IsNullOrWhiteSpace(storageValue) ? null : EnumStorageValueExtensions.FromStorageValue<TEnum>(storageValue));
    }
}