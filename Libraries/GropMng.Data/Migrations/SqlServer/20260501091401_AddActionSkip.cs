using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddActionSkip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionSkip",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<byte>(type: "tinyint", nullable: false),
                    SkippedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ActiveUntilDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionSkip", x => x.Id);
                    table.CheckConstraint("CK_ActionSkip_ActionType", "[ActionType] IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_ActionSkip_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActionSkip_PlantInstance",
                        column: x => x.InstanceId,
                        principalTable: "PlantInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionSkip_ActiveUntilDate",
                table: "ActionSkip",
                column: "ActiveUntilDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActionSkip_InstanceId",
                table: "ActionSkip",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionSkip_Owner_Instance_Type_Until",
                table: "ActionSkip",
                columns: new[] { "OwnerId", "InstanceId", "ActionType", "ActiveUntilDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ActionSkip_OwnerId",
                table: "ActionSkip",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionSkip");
        }
    }
}
