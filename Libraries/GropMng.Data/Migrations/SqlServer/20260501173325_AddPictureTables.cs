using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddPictureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "PlantPhoto");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "PlantPhoto");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DiseasePhoto");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "DiseasePhoto");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "PlantPhoto",
                newName: "PictureId");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PlantPhoto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PictureId",
                table: "Plant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PictureId",
                table: "GardenSpot",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "DiseasePhoto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PictureId",
                table: "DiseasePhoto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Picture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MimeType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SeoFilename = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AltAttribute = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitleAttribute = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsNew = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    VirtualPath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Picture", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantPhoto_PictureId",
                table: "PlantPhoto",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseasePhoto_PictureId",
                table: "DiseasePhoto",
                column: "PictureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Picture");

            migrationBuilder.DropIndex(
                name: "IX_PlantPhoto_PictureId",
                table: "PlantPhoto");

            migrationBuilder.DropIndex(
                name: "IX_DiseasePhoto_PictureId",
                table: "DiseasePhoto");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PlantPhoto");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "Plant");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "GardenSpot");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "DiseasePhoto");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "DiseasePhoto");

            migrationBuilder.RenameColumn(
                name: "PictureId",
                table: "PlantPhoto",
                newName: "SortOrder");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "PlantPhoto",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "PlantPhoto",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DiseasePhoto",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "DiseasePhoto",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
