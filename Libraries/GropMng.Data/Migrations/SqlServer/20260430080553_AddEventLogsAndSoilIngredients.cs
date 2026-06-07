using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddEventLogsAndSoilIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FertilizingLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    FertilizerId = table.Column<int>(type: "int", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizingLog", x => x.Id);
                    table.CheckConstraint("CK_FertilizingLog_Quantity", "[Quantity] IS NULL OR [Quantity] >= 0");
                    table.CheckConstraint("CK_FertilizingLog_Unit", "[Unit] IS NULL OR [Unit] IN (N'g', N'kg', N'ml', N'l', N'tbsp', N'tsp')");
                    table.ForeignKey(
                        name: "FK_FertilizingLog_Fertilizer",
                        column: x => x.FertilizerId,
                        principalTable: "Fertilizer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FertilizingLog_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FertilizingLog_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepottingLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    PreviousContainerId = table.Column<int>(type: "int", nullable: true),
                    NewContainerId = table.Column<int>(type: "int", nullable: true),
                    PreviousSoilMixId = table.Column<int>(type: "int", nullable: true),
                    NewSoilMixId = table.Column<int>(type: "int", nullable: true),
                    RepottedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    SoilMixChanged = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ContainerChanged = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepottingLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepottingLog_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepottingLog_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoilIngredient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilIngredient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WateringLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    WateredAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    WaterAmountL = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WateringLog_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WateringLog_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoilMixIngredient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoilMixId = table.Column<int>(type: "int", nullable: false),
                    SoilIngredientId = table.Column<int>(type: "int", nullable: false),
                    PercentageByVolume = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilMixIngredient", x => x.Id);
                    table.CheckConstraint("CK_SoilMixIngredient_Percentage", "[PercentageByVolume] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_SoilMixIngredient_SoilIngredient",
                        column: x => x.SoilIngredientId,
                        principalTable: "SoilIngredient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoilMixIngredient_SoilMix",
                        column: x => x.SoilMixId,
                        principalTable: "SoilMix",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingLog_AppliedAtUtc",
                table: "FertilizingLog",
                column: "AppliedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingLog_FertilizerId",
                table: "FertilizingLog",
                column: "FertilizerId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingLog_InstanceId",
                table: "FertilizingLog",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizingLog_OwnerId",
                table: "FertilizingLog",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLog_InstanceId",
                table: "RepottingLog",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLog_OwnerId",
                table: "RepottingLog",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLog_RepottedAtUtc",
                table: "RepottingLog",
                column: "RepottedAtUtc");

            migrationBuilder.CreateIndex(
                name: "UQ_SoilIngredient_Name",
                table: "SoilIngredient",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SoilMixIngredient_SoilIngredientId",
                table: "SoilMixIngredient",
                column: "SoilIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_SoilMixIngredient_SoilMixId",
                table: "SoilMixIngredient",
                column: "SoilMixId");

            migrationBuilder.CreateIndex(
                name: "UQ_SoilMixIngredient_Mix_Ingredient",
                table: "SoilMixIngredient",
                columns: new[] { "SoilMixId", "SoilIngredientId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLog_InstanceId",
                table: "WateringLog",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLog_OwnerId",
                table: "WateringLog",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLog_WateredAtUtc",
                table: "WateringLog",
                column: "WateredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FertilizingLog");

            migrationBuilder.DropTable(
                name: "RepottingLog");

            migrationBuilder.DropTable(
                name: "SoilMixIngredient");

            migrationBuilder.DropTable(
                name: "WateringLog");

            migrationBuilder.DropTable(
                name: "SoilIngredient");
        }
    }
}
