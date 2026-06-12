using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class RedesignDiseaseManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiseasePhoto");

            migrationBuilder.DropTable(
                name: "DiseaseRemedyLink");

            migrationBuilder.DropTable(
                name: "PlantDiseaseRecord");

            migrationBuilder.DropTable(
                name: "Pesticide");

            migrationBuilder.DropTable(
                name: "Disease");

            migrationBuilder.CreateTable(
                name: "AdminNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    ProblemName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotification_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotification_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiseaseKnowledge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommonName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ScientificName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentGuidelines = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseKnowledge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiseaseKnowledgePhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseKnowledgeId = table.Column<int>(type: "int", nullable: false),
                    PictureId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseKnowledgePhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiseaseKnowledgePhoto_DiseaseKnowledge",
                        column: x => x.DiseaseKnowledgeId,
                        principalTable: "DiseaseKnowledge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiseaseKnowledgePlant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseKnowledgeId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseKnowledgePlant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiseaseKnowledgePlant_DiseaseKnowledge",
                        column: x => x.DiseaseKnowledgeId,
                        principalTable: "DiseaseKnowledge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiseaseKnowledgePlant_Plant",
                        column: x => x.PlantId,
                        principalTable: "Plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantProblemRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    DiseaseKnowledgeId = table.Column<int>(type: "int", nullable: true),
                    ProblemName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DetectedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    ProblemStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    InfoSource = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "OwnKnowledge"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NotifyAdmin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantProblemRecord", x => x.Id);
                    table.CheckConstraint("CK_PlantProblemRecord_InfoSource", "[InfoSource] IN (N'OwnKnowledge', N'Agronomist', N'AITool', N'Internet', N'Other')");
                    table.CheckConstraint("CK_PlantProblemRecord_ProblemStatus", "[ProblemStatus] IN (N'Active', N'Monitoring', N'Resolved')");
                    table.CheckConstraint("CK_PlantProblemRecord_Severity", "[Severity] IN (N'Low', N'Medium', N'High')");
                    table.ForeignKey(
                        name: "FK_PlantProblemRecord_DiseaseKnowledge",
                        column: x => x.DiseaseKnowledgeId,
                        principalTable: "DiseaseKnowledge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlantProblemRecord_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantProblemRecord_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantProblemSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FrequencyValue = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    FrequencyUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Days"),
                    DosageNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduleStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantProblemSchedule", x => x.Id);
                    table.CheckConstraint("CK_PlantProblemSchedule_FrequencyUnit", "[FrequencyUnit] IN (N'Days', N'Weeks', N'Months')");
                    table.CheckConstraint("CK_PlantProblemSchedule_FrequencyValue", "[FrequencyValue] > 0");
                    table.CheckConstraint("CK_PlantProblemSchedule_ScheduleStatus", "[ScheduleStatus] IN (N'Active', N'Completed', N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_PlantProblemSchedule_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantProblemSchedule_PlantProblemRecord",
                        column: x => x.RecordId,
                        principalTable: "PlantProblemRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotification_InstanceId",
                table: "AdminNotification",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotification_IsResolved",
                table: "AdminNotification",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotification_OwnerId",
                table: "AdminNotification",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_DiseaseKnowledge_CommonName",
                table: "DiseaseKnowledge",
                column: "CommonName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseKnowledgePhoto_DiseaseKnowledgeId",
                table: "DiseaseKnowledgePhoto",
                column: "DiseaseKnowledgeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseKnowledgePhoto_PictureId",
                table: "DiseaseKnowledgePhoto",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseKnowledgePlant_DiseaseKnowledgeId",
                table: "DiseaseKnowledgePlant",
                column: "DiseaseKnowledgeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseKnowledgePlant_PlantId",
                table: "DiseaseKnowledgePlant",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "UQ_DiseaseKnowledgePlant",
                table: "DiseaseKnowledgePlant",
                columns: new[] { "DiseaseKnowledgeId", "PlantId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemRecord_DiseaseKnowledgeId",
                table: "PlantProblemRecord",
                column: "DiseaseKnowledgeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemRecord_InstanceId",
                table: "PlantProblemRecord",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemRecord_OwnerId",
                table: "PlantProblemRecord",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemSchedule_NextDueDate",
                table: "PlantProblemSchedule",
                column: "NextDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemSchedule_OwnerId",
                table: "PlantProblemSchedule",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProblemSchedule_RecordId",
                table: "PlantProblemSchedule",
                column: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotification");

            migrationBuilder.DropTable(
                name: "DiseaseKnowledgePhoto");

            migrationBuilder.DropTable(
                name: "DiseaseKnowledgePlant");

            migrationBuilder.DropTable(
                name: "PlantProblemSchedule");

            migrationBuilder.DropTable(
                name: "PlantProblemRecord");

            migrationBuilder.DropTable(
                name: "DiseaseKnowledge");

            migrationBuilder.CreateTable(
                name: "Disease",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffectedParts = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    DiseaseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreventionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disease", x => x.Id);
                    table.CheckConstraint("CK_Disease_DiseaseType", "[DiseaseType] IN (N'Fungal', N'Bacterial', N'Viral', N'Pest', N'Deficiency', N'Physiological', N'Other')");
                });

            migrationBuilder.CreateTable(
                name: "Pesticide",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActiveIngredient = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ApplicationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsOrganic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PesticideType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SafetyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    WithholdingDays = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesticide", x => x.Id);
                    table.CheckConstraint("CK_Pesticide_ApplicationMethod", "[ApplicationMethod] IS NULL OR [ApplicationMethod] IN (N'Spray', N'Soil', N'Drench', N'Granule', N'Systemic')");
                    table.CheckConstraint("CK_Pesticide_PesticideType", "[PesticideType] IS NULL OR [PesticideType] IN (N'Fungicide', N'Insecticide', N'Herbicide', N'Acaricide', N'Bactericide', N'Biostimulant', N'Other')");
                });

            migrationBuilder.CreateTable(
                name: "PlantDiseaseRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    DetectedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Ongoing"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolvedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Moderate"),
                    TreatmentUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
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
                        name: "FK_PlantDiseaseRecord_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantDiseaseRecord_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
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
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Curative"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
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
                name: "DiseasePhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PictureId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TakenDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseasePhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiseasePhoto_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiseasePhoto_PlantDiseaseRecord",
                        column: x => x.RecordId,
                        principalTable: "PlantDiseaseRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_DiseasePhoto_PictureId",
                table: "DiseasePhoto",
                column: "PictureId");

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
                name: "IX_Pesticide_Name",
                table: "Pesticide",
                column: "Name");

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
        }
    }
}
