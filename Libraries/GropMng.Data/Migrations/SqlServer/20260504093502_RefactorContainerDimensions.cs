using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class RefactorContainerDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlantInstance_Container",
                table: "PlantInstance");

            migrationBuilder.DropIndex(
                name: "IX_PlantInstance_ContainerId",
                table: "PlantInstance");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Container_Dimensions",
                table: "Container");

            migrationBuilder.DropColumn(
                name: "ContainerId",
                table: "PlantInstance");

            migrationBuilder.RenameColumn(
                name: "DiameterCm",
                table: "Container",
                newName: "RimCircumferenceCm");

            migrationBuilder.RenameColumn(
                name: "DepthCm",
                table: "Container",
                newName: "HeightCm");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCircumferenceCm",
                table: "Container",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlantInstanceId",
                table: "Container",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Container_PlantInstanceId",
                table: "Container",
                column: "PlantInstanceId",
                unique: true,
                filter: "[PlantInstanceId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Container_Dimensions",
                table: "Container",
                sql: "[BaseCircumferenceCm] IS NOT NULL OR [RimCircumferenceCm] IS NOT NULL OR [HeightCm] IS NOT NULL OR [LengthCm] IS NOT NULL OR [WidthCm] IS NOT NULL OR [VolumeL] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Container_PlantInstance",
                table: "Container",
                column: "PlantInstanceId",
                principalTable: "PlantInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Container_PlantInstance",
                table: "Container");

            migrationBuilder.DropIndex(
                name: "UX_Container_PlantInstanceId",
                table: "Container");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Container_Dimensions",
                table: "Container");

            migrationBuilder.DropColumn(
                name: "BaseCircumferenceCm",
                table: "Container");

            migrationBuilder.DropColumn(
                name: "PlantInstanceId",
                table: "Container");

            migrationBuilder.RenameColumn(
                name: "RimCircumferenceCm",
                table: "Container",
                newName: "DiameterCm");

            migrationBuilder.RenameColumn(
                name: "HeightCm",
                table: "Container",
                newName: "DepthCm");

            migrationBuilder.AddColumn<int>(
                name: "ContainerId",
                table: "PlantInstance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantInstance_ContainerId",
                table: "PlantInstance",
                column: "ContainerId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Container_Dimensions",
                table: "Container",
                sql: "[DiameterCm] IS NOT NULL OR [LengthCm] IS NOT NULL OR [WidthCm] IS NOT NULL OR [DepthCm] IS NOT NULL OR [VolumeL] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PlantInstance_Container",
                table: "PlantInstance",
                column: "ContainerId",
                principalTable: "Container",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
