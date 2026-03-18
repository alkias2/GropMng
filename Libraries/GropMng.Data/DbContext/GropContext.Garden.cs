using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.AI;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
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
    public DbSet<Disease> Diseases => Set<Disease>();
    public DbSet<Pesticide> Pesticides => Set<Pesticide>();
    public DbSet<DiseaseRemedyLink> DiseaseRemedyLinks => Set<DiseaseRemedyLink>();
    public DbSet<PlantDiseaseRecord> PlantDiseaseRecords => Set<PlantDiseaseRecord>();
    public DbSet<DiseasePhoto> DiseasePhotos => Set<DiseasePhoto>();
    public DbSet<AIQueryTemplate> AIQueryTemplates => Set<AIQueryTemplate>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

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
        ConfigureDisease(modelBuilder.Entity<Disease>());
        ConfigurePesticide(modelBuilder.Entity<Pesticide>());
        ConfigureDiseaseRemedyLink(modelBuilder.Entity<DiseaseRemedyLink>());
        ConfigurePlantDiseaseRecord(modelBuilder.Entity<PlantDiseaseRecord>());
        ConfigureDiseasePhoto(modelBuilder.Entity<DiseasePhoto>());
        ConfigureAiQueryTemplate(modelBuilder.Entity<AIQueryTemplate>());
        ConfigureUserPreference(modelBuilder.Entity<UserPreference>());
    }

    private static void ConfigureLocation(EntityTypeBuilder<Location> entity)
    {
        entity.ToTable("Location");
        entity.HasKey(e => e.Id).HasName("PK_Location");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
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

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Orientation).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.CoverType).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.Surroundings).HasMaxLength(500);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_GardenSpot_OwnerId");
        entity.HasIndex(e => e.LocationId).HasDatabaseName("IX_GardenSpot_LocationId");
        entity.HasIndex(e => new { e.LocationId, e.Name })
            .IsUnique()
            .HasDatabaseName("UQ_GardenSpot_LocationId_Name")
            .HasFilter("[IsDeleted] = 0");
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
            tableBuilder.HasCheckConstraint("CK_Container_Dimensions", "[DiameterCm] IS NOT NULL OR [LengthCm] IS NOT NULL OR [WidthCm] IS NOT NULL OR [DepthCm] IS NOT NULL OR [VolumeL] IS NOT NULL");
        });

        entity.HasKey(e => e.Id).HasName("PK_Container");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.ContainerType).HasMaxLength(30).HasStorageEnumConversion().HasDefaultValue(GardenContainerType.Pot);
        entity.Property(e => e.Material).HasMaxLength(100);
        entity.Property(e => e.LengthCm).HasPrecision(8, 2);
        entity.Property(e => e.WidthCm).HasPrecision(8, 2);
        entity.Property(e => e.DepthCm).HasPrecision(8, 2);
        entity.Property(e => e.DiameterCm).HasPrecision(8, 2);
        entity.Property(e => e.VolumeL).HasPrecision(8, 2);
        entity.Property(e => e.Color).HasMaxLength(50);
        entity.Property(e => e.HasDrainageHole).HasDefaultValue(true);
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Container_OwnerId");
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

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
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

        entity.HasOne(e => e.Container)
            .WithMany(e => e.PlantInstances)
            .HasForeignKey(e => e.ContainerId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_PlantInstance_Container");

        entity.HasOne(e => e.SoilMix)
            .WithMany(e => e.PlantInstances)
            .HasForeignKey(e => e.SoilMixId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_PlantInstance_SoilMix");
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

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
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

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.Season).HasMaxLength(20).HasStorageEnumConversion();
        entity.Property(e => e.FrequencyDays).HasDefaultValue((byte)14);
        entity.Property(e => e.Quantity).HasPrecision(8, 3);
        entity.Property(e => e.Unit).HasMaxLength(10).HasNullableStorageEnumConversion().HasDefaultValue(FertilizerQuantityUnit.Gram);
        entity.Property(e => e.Notes).HasMaxLength(500);

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
    }

    private static void ConfigurePlantPhoto(EntityTypeBuilder<PlantPhoto> entity)
    {
        entity.ToTable("PlantPhoto");
        entity.HasKey(e => e.Id).HasName("PK_PlantPhoto");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
        entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
        entity.Property(e => e.TakenDate).HasColumnType("date").HasDefaultValueSql("CAST(SYSUTCDATETIME() AS date)");
        entity.Property(e => e.Caption).HasMaxLength(500);
        entity.Property(e => e.SortOrder).HasDefaultValue(0);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantPhoto_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_PlantPhoto_InstanceId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.Photos)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantPhoto_PlantInstance");
    }

    private static void ConfigurePlantNote(EntityTypeBuilder<PlantNote> entity)
    {
        entity.ToTable("PlantNote");
        entity.HasKey(e => e.Id).HasName("PK_PlantNote");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
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
    }

    private static void ConfigureDisease(EntityTypeBuilder<Disease> entity)
    {
        entity.ToTable("Disease", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Disease_DiseaseType", BuildEnumConstraintSql<PlantDiseaseType>("DiseaseType"));
        });

        entity.HasKey(e => e.Id).HasName("PK_Disease");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
        entity.Property(e => e.DiseaseType).HasMaxLength(50).HasStorageEnumConversion().HasDefaultValue(PlantDiseaseType.Other).HasSentinel(PlantDiseaseType.Other);
        entity.Property(e => e.Symptoms);
        entity.Property(e => e.PreventionNotes);
        entity.Property(e => e.AffectedParts).HasMaxLength(200);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Disease_Name")
            .HasFilter("[IsDeleted] = 0");
    }

    private static void ConfigurePesticide(EntityTypeBuilder<Pesticide> entity)
    {
        entity.ToTable("Pesticide", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Pesticide_PesticideType", BuildEnumConstraintSql<PesticideKind>("PesticideType", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_Pesticide_ApplicationMethod", BuildEnumConstraintSql<PesticideApplicationMethod>("ApplicationMethod", isNullable: true));
        });

        entity.HasKey(e => e.Id).HasName("PK_Pesticide");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
        entity.Property(e => e.Brand).HasMaxLength(200);
        entity.Property(e => e.ActiveIngredient).HasMaxLength(300);
        entity.Property(e => e.PesticideType).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.ApplicationMethod).HasMaxLength(50).HasNullableStorageEnumConversion();
        entity.Property(e => e.IsOrganic).HasDefaultValue(false);
        entity.Property(e => e.SafetyNotes);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.Name).HasDatabaseName("IX_Pesticide_Name");
    }

    private static void ConfigureDiseaseRemedyLink(EntityTypeBuilder<DiseaseRemedyLink> entity)
    {
        entity.ToTable("DiseaseRemedyLink", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_DiseaseRemedyLink_TreatmentType", BuildEnumConstraintSql<RemedyTreatmentType>("TreatmentType"));
        });

        entity.HasKey(e => e.Id).HasName("PK_DiseaseRemedyLink");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.TreatmentType).HasMaxLength(20).HasStorageEnumConversion().HasDefaultValue(RemedyTreatmentType.Curative).HasSentinel(RemedyTreatmentType.Curative);
        entity.Property(e => e.Dosage).HasMaxLength(200);
        entity.Property(e => e.Frequency).HasMaxLength(200);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.DiseaseId).HasDatabaseName("IX_DiseaseRemedyLink_DiseaseId");
        entity.HasIndex(e => e.PesticideId).HasDatabaseName("IX_DiseaseRemedyLink_PesticideId");
        entity.HasIndex(e => new { e.DiseaseId, e.PesticideId, e.TreatmentType })
            .IsUnique()
            .HasDatabaseName("UQ_DiseaseRemedyLink")
            .HasFilter("[IsDeleted] = 0");

        entity.HasOne(e => e.Disease)
            .WithMany(e => e.RemedyLinks)
            .HasForeignKey(e => e.DiseaseId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DiseaseRemedyLink_Disease");

        entity.HasOne(e => e.Pesticide)
            .WithMany(e => e.RemedyLinks)
            .HasForeignKey(e => e.PesticideId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_DiseaseRemedyLink_Pesticide");
    }

    private static void ConfigurePlantDiseaseRecord(EntityTypeBuilder<PlantDiseaseRecord> entity)
    {
        entity.ToTable("PlantDiseaseRecord", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PlantDiseaseRecord_Severity", BuildEnumConstraintSql<PlantDiseaseSeverity>("Severity", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_PlantDiseaseRecord_Outcome", BuildEnumConstraintSql<PlantDiseaseOutcome>("Outcome", isNullable: true));
            tableBuilder.HasCheckConstraint("CK_PlantDiseaseRecord_ResolvedDate", "[ResolvedDate] IS NULL OR [ResolvedDate] >= [DetectedDate]");
        });

        entity.HasKey(e => e.Id).HasName("PK_PlantDiseaseRecord");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.PlantInstanceId).HasColumnName("InstanceId");
        entity.Property(e => e.DetectedDate).HasColumnType("date").HasDefaultValueSql("CAST(SYSUTCDATETIME() AS date)");
        entity.Property(e => e.ResolvedDate).HasColumnType("date");
        entity.Property(e => e.Severity).HasMaxLength(20).HasNullableStorageEnumConversion().HasDefaultValue(PlantDiseaseSeverity.Moderate);
        entity.Property(e => e.TreatmentUsed);
        entity.Property(e => e.Outcome).HasMaxLength(20).HasNullableStorageEnumConversion().HasDefaultValue(PlantDiseaseOutcome.Ongoing);
        entity.Property(e => e.Notes);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_PlantDiseaseRecord_OwnerId");
        entity.HasIndex(e => e.PlantInstanceId).HasDatabaseName("IX_PlantDiseaseRecord_InstanceId");
        entity.HasIndex(e => e.DiseaseId).HasDatabaseName("IX_PlantDiseaseRecord_DiseaseId");

        entity.HasOne(e => e.PlantInstance)
            .WithMany(e => e.DiseaseRecords)
            .HasForeignKey(e => e.PlantInstanceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PlantDiseaseRecord_PlantInstance");

        entity.HasOne(e => e.Disease)
            .WithMany(e => e.PlantDiseaseRecords)
            .HasForeignKey(e => e.DiseaseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PlantDiseaseRecord_Disease");
    }

    private static void ConfigureDiseasePhoto(EntityTypeBuilder<DiseasePhoto> entity)
    {
        entity.ToTable("DiseasePhoto");
        entity.HasKey(e => e.Id).HasName("PK_DiseasePhoto");
        ConfigureAuditableEntity(entity);

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.PlantDiseaseRecordId).HasColumnName("RecordId");
        entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
        entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
        entity.Property(e => e.TakenDate).HasColumnType("date").HasDefaultValueSql("CAST(SYSUTCDATETIME() AS date)");
        entity.Property(e => e.Notes).HasMaxLength(500);

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_DiseasePhoto_OwnerId");
        entity.HasIndex(e => e.PlantDiseaseRecordId).HasDatabaseName("IX_DiseasePhoto_RecordId");

        entity.HasOne(e => e.PlantDiseaseRecord)
            .WithMany(e => e.Photos)
            .HasForeignKey(e => e.PlantDiseaseRecordId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DiseasePhoto_PlantDiseaseRecord");
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

        entity.Property(e => e.OwnerId).HasMaxLength(450).IsRequired();
        entity.Property(e => e.LengthUnit).HasMaxLength(5).HasStorageEnumConversion().HasDefaultValue(LengthUnitType.Centimetre);
        entity.Property(e => e.VolumeUnit).HasMaxLength(5).HasStorageEnumConversion().HasDefaultValue(VolumeUnitType.Litre);
        entity.Property(e => e.TemperatureUnit).HasMaxLength(2).HasStorageEnumConversion().HasDefaultValue(TemperatureUnitType.Celsius);
        entity.Property(e => e.DefaultLanguage).HasMaxLength(10).HasStorageEnumConversion().HasDefaultValue(SupportedLanguage.Greek);

        entity.HasIndex(e => e.OwnerId)
            .IsUnique()
            .HasDatabaseName("UQ_UserPreference_OwnerId")
            .HasFilter("[IsDeleted] = 0");
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