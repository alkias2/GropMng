using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddDiseaseKnowledgeIdToAdminNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiseaseKnowledgeId",
                table: "AdminNotification",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotification_DiseaseKnowledgeId",
                table: "AdminNotification",
                column: "DiseaseKnowledgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotification_DiseaseKnowledge",
                table: "AdminNotification",
                column: "DiseaseKnowledgeId",
                principalTable: "DiseaseKnowledge",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotification_DiseaseKnowledge",
                table: "AdminNotification");

            migrationBuilder.DropIndex(
                name: "IX_AdminNotification_DiseaseKnowledgeId",
                table: "AdminNotification");

            migrationBuilder.DropColumn(
                name: "DiseaseKnowledgeId",
                table: "AdminNotification");
        }
    }
}
