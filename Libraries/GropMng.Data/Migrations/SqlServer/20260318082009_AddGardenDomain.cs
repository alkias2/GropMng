using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddGardenDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIQueryTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Scenario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "el"),
                    PromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIQueryTemplate", x => x.Id);
                    table.CheckConstraint("CK_AIQueryTemplate_Language", "[Language] IN (N'el', N'en')");
                    table.CheckConstraint("CK_AIQueryTemplate_Scenario", "[Scenario] IN (N'Watering', N'Fertilizing', N'Repotting', N'Planting', N'Disease', N'Pruning', N'Pest', N'General')");
                });

            migrationBuilder.CreateTable(
                name: "Container",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ContainerType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pot"),
                    Material = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LengthCm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    WidthCm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    DepthCm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    DiameterCm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    VolumeL = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasDrainageHole = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Container", x => x.Id);
                    table.CheckConstraint("CK_Container_ContainerType", "[ContainerType] IN (N'Pot', N'Bed', N'HangingBasket', N'WindowBox', N'RaisedBed', N'Other')");
                    table.CheckConstraint("CK_Container_Dimensions", "[DiameterCm] IS NOT NULL OR [LengthCm] IS NOT NULL OR [WidthCm] IS NOT NULL OR [DepthCm] IS NOT NULL OR [VolumeL] IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "Disease",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DiseaseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    Symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreventionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedParts = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disease", x => x.Id);
                    table.CheckConstraint("CK_Disease_DiseaseType", "[DiseaseType] IN (N'Fungal', N'Bacterial', N'Viral', N'Pest', N'Deficiency', N'Physiological', N'Other')");
                });

            migrationBuilder.CreateTable(
                name: "Fertilizer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FertilizerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NpkRatio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ApplicationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsOrganic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fertilizer", x => x.Id);
                    table.CheckConstraint("CK_Fertilizer_ApplicationMethod", "[ApplicationMethod] IS NULL OR [ApplicationMethod] IN (N'Soil', N'Foliar', N'Drip', N'Diluted')");
                    table.CheckConstraint("CK_Fertilizer_FertilizerType", "[FertilizerType] IS NULL OR [FertilizerType] IN (N'Organic', N'Chemical', N'Mineral', N'Liquid', N'Granular', N'SlowRelease')");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Greece"),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    ClimateZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pesticide",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActiveIngredient = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PesticideType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApplicationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsOrganic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WithholdingDays = table.Column<byte>(type: "tinyint", nullable: true),
                    SafetyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesticide", x => x.Id);
                    table.CheckConstraint("CK_Pesticide_ApplicationMethod", "[ApplicationMethod] IS NULL OR [ApplicationMethod] IN (N'Spray', N'Soil', N'Drench', N'Granule', N'Systemic')");
                    table.CheckConstraint("CK_Pesticide_PesticideType", "[PesticideType] IS NULL OR [PesticideType] IN (N'Fungicide', N'Insecticide', N'Herbicide', N'Acaricide', N'Bactericide', N'Biostimulant', N'Other')");
                });

            migrationBuilder.CreateTable(
                name: "Plant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScientificName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Family = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    GrowthType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SunRequirement = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WaterRequirement = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MinTempCelsius = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    MaxTempCelsius = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    IsEdible = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsMedicinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsToxic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plant", x => x.Id);
                    table.CheckConstraint("CK_Plant_Category", "[Category] IN (N'Shrub', N'Tree', N'Climber', N'Ornamental', N'Edible', N'Aromatic', N'Succulent', N'Grass', N'Fern', N'Other')");
                    table.CheckConstraint("CK_Plant_GrowthType", "[GrowthType] IS NULL OR [GrowthType] IN (N'Annual', N'Biennial', N'Perennial', N'Bulb')");
                    table.CheckConstraint("CK_Plant_SunRequirement", "[SunRequirement] IS NULL OR [SunRequirement] IN (N'FullSun', N'PartialShade', N'FullShade')");
                    table.CheckConstraint("CK_Plant_WaterRequirement", "[WaterRequirement] IS NULL OR [WaterRequirement] IN (N'Low', N'Moderate', N'High')");
                });

            migrationBuilder.CreateTable(
                name: "SoilMix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhMin = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    PhMax = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    Texture = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Drainage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilMix", x => x.Id);
                    table.CheckConstraint("CK_SoilMix_Drainage", "[Drainage] IS NULL OR [Drainage] IN (N'Poor', N'Moderate', N'Good', N'Excellent')");
                    table.CheckConstraint("CK_SoilMix_PhMax", "[PhMax] IS NULL OR [PhMax] BETWEEN 0 AND 14");
                    table.CheckConstraint("CK_SoilMix_PhMin", "[PhMin] IS NULL OR [PhMin] BETWEEN 0 AND 14");
                    table.CheckConstraint("CK_SoilMix_PhRange", "[PhMin] IS NULL OR [PhMax] IS NULL OR [PhMin] <= [PhMax]");
                    table.CheckConstraint("CK_SoilMix_Texture", "[Texture] IS NULL OR [Texture] IN (N'Sandy', N'Loamy', N'Clay', N'Silty', N'Peaty', N'Chalky')");
                });

            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LengthUnit = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "cm"),
                    VolumeUnit = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "l"),
                    TemperatureUnit = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "C"),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "el"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreference", x => x.Id);
                    table.CheckConstraint("CK_UserPreference_DefaultLanguage", "[DefaultLanguage] IN (N'el', N'en')");
                    table.CheckConstraint("CK_UserPreference_LengthUnit", "[LengthUnit] IN (N'cm', N'in')");
                    table.CheckConstraint("CK_UserPreference_TemperatureUnit", "[TemperatureUnit] IN (N'C', N'F')");
                    table.CheckConstraint("CK_UserPreference_VolumeUnit", "[VolumeUnit] IN (N'l', N'gal')");
                });

            migrationBuilder.CreateTable(
                name: "GardenSpot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoverType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SunHoursPerDay = table.Column<byte>(type: "tinyint", nullable: true),
                    Surroundings = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GardenSpot", x => x.Id);
                    table.CheckConstraint("CK_GardenSpot_CoverType", "[CoverType] IS NULL OR [CoverType] IN (N'Open', N'Covered', N'Semi-covered')");
                    table.CheckConstraint("CK_GardenSpot_Orientation", "[Orientation] IS NULL OR [Orientation] IN (N'N', N'NE', N'E', N'SE', N'S', N'SW', N'W', N'NW')");
                    table.CheckConstraint("CK_GardenSpot_SunHours", "[SunHoursPerDay] IS NULL OR [SunHoursPerDay] BETWEEN 0 AND 24");
                    table.ForeignKey(
                        name: "FK_GardenSpot_Location",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiseaseRemedyLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    PesticideId = table.Column<int>(type: "int", nullable: false),
                    TreatmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Curative"),
                    Dosage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseRemedyLink", x => x.Id);
                    table.CheckConstraint("CK_DiseaseRemedyLink_TreatmentType", "[TreatmentType] IN (N'Preventive', N'Curative')");
                    table.ForeignKey(
                        name: "FK_DiseaseRemedyLink_Disease",
                        column: x => x.DiseaseId,
                        principalTable: "Disease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiseaseRemedyLink_Pesticide",
                        column: x => x.PesticideId,
                        principalTable: "Pesticide",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantInstance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    SpotId = table.Column<int>(type: "int", nullable: false),
                    ContainerId = table.Column<int>(type: "int", nullable: true),
                    SoilMixId = table.Column<int>(type: "int", nullable: true),
                    Nickname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlantedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SizeCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(8,1)", precision: 8, scale: 1, nullable: true),
                    SpreadCm = table.Column<decimal>(type: "decimal(8,1)", precision: 8, scale: 1, nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Good"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantInstance", x => x.Id);
                    table.CheckConstraint("CK_PlantInstance_HealthStatus", "[HealthStatus] IN (N'Excellent', N'Good', N'Fair', N'Poor', N'Critical')");
                    table.CheckConstraint("CK_PlantInstance_SizeCategory", "[SizeCategory] IS NULL OR [SizeCategory] IN (N'Seedling', N'Small', N'Medium', N'Large', N'Mature')");
                    table.ForeignKey(
                        name: "FK_PlantInstance_Container",
                        column: x => x.ContainerId,
                        principalTable: "Container",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlantInstance_GardenSpot",
                        column: x => x.SpotId,
                        principalTable: "GardenSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantInstance_Plant",
                        column: x => x.PlantId,
                        principalTable: "Plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantInstance_SoilMix",
                        column: x => x.SoilMixId,
                        principalTable: "SoilMix",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FertilizingSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    FertilizerId = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrequencyDays = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)14),
                    Quantity = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "g"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizingSchedule", x => x.Id);
                    table.CheckConstraint("CK_FertilizingSchedule_FrequencyDays", "[FrequencyDays] > 0");
                    table.CheckConstraint("CK_FertilizingSchedule_Quantity", "[Quantity] IS NULL OR [Quantity] >= 0");
                    table.CheckConstraint("CK_FertilizingSchedule_Season", "[Season] IN (N'Spring', N'Summer', N'Autumn', N'Winter', N'AllYear')");
                    table.CheckConstraint("CK_FertilizingSchedule_Unit", "[Unit] IS NULL OR [Unit] IN (N'g', N'kg', N'ml', N'l', N'tbsp', N'tsp')");
                    table.ForeignKey(
                        name: "FK_FertilizingSchedule_Fertilizer",
                        column: x => x.FertilizerId,
                        principalTable: "Fertilizer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FertilizingSchedule_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantDiseaseRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    DetectedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    ResolvedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Moderate"),
                    TreatmentUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Ongoing"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantDiseaseRecord", x => x.Id);
                    table.CheckConstraint("CK_PlantDiseaseRecord_Outcome", "[Outcome] IS NULL OR [Outcome] IN (N'Resolved', N'Ongoing', N'Lost', N'Unknown')");
                    table.CheckConstraint("CK_PlantDiseaseRecord_ResolvedDate", "[ResolvedDate] IS NULL OR [ResolvedDate] >= [DetectedDate]");
                    table.CheckConstraint("CK_PlantDiseaseRecord_Severity", "[Severity] IS NULL OR [Severity] IN (N'Mild', N'Moderate', N'Severe', N'Critical')");
                    table.ForeignKey(
                        name: "FK_PlantDiseaseRecord_Disease",
                        column: x => x.DiseaseId,
                        principalTable: "Disease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantDiseaseRecord_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantNote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RichHtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantNote_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TakenDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantPhoto_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WateringSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrequencyDays = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)3),
                    WaterAmountL = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    TimeOfDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Morning"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringSchedule", x => x.Id);
                    table.CheckConstraint("CK_WateringSchedule_FrequencyDays", "[FrequencyDays] > 0");
                    table.CheckConstraint("CK_WateringSchedule_Season", "[Season] IN (N'Spring', N'Summer', N'Autumn', N'Winter', N'AllYear')");
                    table.CheckConstraint("CK_WateringSchedule_TimeOfDay", "[TimeOfDay] IS NULL OR [TimeOfDay] IN (N'Morning', N'Midday', N'Evening', N'Any')");
                    table.ForeignKey(
                        name: "FK_WateringSchedule_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiseasePhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TakenDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseasePhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiseasePhoto_PlantDiseaseRecord",
                        column: x => x.RecordId,
                        principalTable: "PlantDiseaseRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_AIQueryTemplate_Name_Language",
                table: "AIQueryTemplate",
                columns: new[] { "TemplateName", "Language" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Container_OwnerId",
                table: "Container",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_Disease_Name",
                table: "Disease",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DiseasePhoto_OwnerId",
                table: "DiseasePhoto",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseasePhoto_RecordId",
                table: "DiseasePhoto",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseRemedyLink_DiseaseId",
                table: "DiseaseRemedyLink",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseRemedyLink_PesticideId",
                table: "DiseaseRemedyLink",
                column: "PesticideId");

            migrationBuilder.CreateIndex(
                name: "UQ_DiseaseRemedyLink",
                table: "DiseaseRemedyLink",
                columns: new[] { "DiseaseId", "PesticideId", "TreatmentType" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Fertilizer_Name",
                table: "Fertilizer",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingSchedule_FertilizerId",
                table: "FertilizingSchedule",
                column: "FertilizerId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingSchedule_InstanceId",
                table: "FertilizingSchedule",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingSchedule_OwnerId",
                table: "FertilizingSchedule",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GardenSpot_LocationId",
                table: "GardenSpot",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GardenSpot_OwnerId",
                table: "GardenSpot",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_GardenSpot_LocationId_Name",
                table: "GardenSpot",
                columns: new[] { "LocationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Location_OwnerId",
                table: "Location",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pesticide_Name",
                table: "Pesticide",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_CommonName",
                table: "Plant",
                column: "CommonName");

            migrationBuilder.CreateIndex(
                name: "UQ_Plant_ScientificName",
                table: "Plant",
                column: "ScientificName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlantDiseaseRecord_DiseaseId",
                table: "PlantDiseaseRecord",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantDiseaseRecord_InstanceId",
                table: "PlantDiseaseRecord",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantDiseaseRecord_OwnerId",
                table: "PlantDiseaseRecord",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_ContainerId",
                table: "PlantInstance",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_IsActive",
                table: "PlantInstance",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_OwnerId",
                table: "PlantInstance",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_PlantId",
                table: "PlantInstance",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_SoilMixId",
                table: "PlantInstance",
                column: "SoilMixId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_SpotId",
                table: "PlantInstance",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantNote_InstanceId",
                table: "PlantNote",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantNote_OwnerId",
                table: "PlantNote",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantPhoto_InstanceId",
                table: "PlantPhoto",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantPhoto_OwnerId",
                table: "PlantPhoto",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserPreference_OwnerId",
                table: "UserPreference",
                column: "OwnerId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WateringSchedule_InstanceId",
                table: "WateringSchedule",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringSchedule_OwnerId",
                table: "WateringSchedule",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_WateringSchedule_InstanceSeason",
                table: "WateringSchedule",
                columns: new[] { "InstanceId", "Season" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIQueryTemplate");

            migrationBuilder.DropTable(
                name: "DiseasePhoto");

            migrationBuilder.DropTable(
                name: "DiseaseRemedyLink");

            migrationBuilder.DropTable(
                name: "FertilizingSchedule");

            migrationBuilder.DropTable(
                name: "PlantNote");

            migrationBuilder.DropTable(
                name: "PlantPhoto");

            migrationBuilder.DropTable(
                name: "UserPreference");

            migrationBuilder.DropTable(
                name: "WateringSchedule");

            migrationBuilder.DropTable(
                name: "PlantDiseaseRecord");

            migrationBuilder.DropTable(
                name: "Pesticide");

            migrationBuilder.DropTable(
                name: "Fertilizer");

            migrationBuilder.DropTable(
                name: "Disease");

            migrationBuilder.DropTable(
                name: "PlantInstance");

            migrationBuilder.DropTable(
                name: "Container");

            migrationBuilder.DropTable(
                name: "GardenSpot");

            migrationBuilder.DropTable(
                name: "Plant");

            migrationBuilder.DropTable(
                name: "SoilMix");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
