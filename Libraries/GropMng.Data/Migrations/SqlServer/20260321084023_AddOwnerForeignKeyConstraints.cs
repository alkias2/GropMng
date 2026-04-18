using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddOwnerForeignKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Owner_OwnerId",
                table: "Owner",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Container_Owner",
                table: "Container",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiseasePhoto_Owner",
                table: "DiseasePhoto",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizingSchedule_Owner",
                table: "FertilizingSchedule",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GardenSpot_Owner",
                table: "GardenSpot",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Owner",
                table: "Location",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantDiseaseRecord_Owner",
                table: "PlantDiseaseRecord",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantInstance_Owner",
                table: "PlantInstance",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantNote_Owner",
                table: "PlantNote",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantPhoto_Owner",
                table: "PlantPhoto",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreference_Owner",
                table: "UserPreference",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WateringSchedule_Owner",
                table: "WateringSchedule",
                column: "OwnerId",
                principalTable: "Owner",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Container_Owner",
                table: "Container");

            migrationBuilder.DropForeignKey(
                name: "FK_DiseasePhoto_Owner",
                table: "DiseasePhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_FertilizingSchedule_Owner",
                table: "FertilizingSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_GardenSpot_Owner",
                table: "GardenSpot");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_Owner",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantDiseaseRecord_Owner",
                table: "PlantDiseaseRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantInstance_Owner",
                table: "PlantInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantNote_Owner",
                table: "PlantNote");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantPhoto_Owner",
                table: "PlantPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreference_Owner",
                table: "UserPreference");

            migrationBuilder.DropForeignKey(
                name: "FK_WateringSchedule_Owner",
                table: "WateringSchedule");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Owner_OwnerId",
                table: "Owner");
        }
    }
}
