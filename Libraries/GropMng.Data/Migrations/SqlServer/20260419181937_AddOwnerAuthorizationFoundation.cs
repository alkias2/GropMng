using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GropMng.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddOwnerAuthorizationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Owner",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "Owner",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Owner",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "OwnerPassword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PasswordResetTokenExpiresAtUtc = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerPassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerPassword_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnerRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owner_OwnerRole_Mapping",
                columns: table => new
                {
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    OwnerRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owner_OwnerRole_Mapping", x => new { x.OwnerId, x.OwnerRoleId });
                    table.ForeignKey(
                        name: "FK_Owner_OwnerRole_Mapping_OwnerRole_OwnerRoleId",
                        column: x => x.OwnerRoleId,
                        principalTable: "OwnerRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Owner_OwnerRole_Mapping_Owner_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnerRole_PermissionRecord_Mapping",
                columns: table => new
                {
                    OwnerRoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerRole_PermissionRecord_Mapping", x => new { x.OwnerRoleId, x.PermissionRecordId });
                    table.ForeignKey(
                        name: "FK_OwnerRole_PermissionRecord_Mapping_OwnerRole_OwnerRoleId",
                        column: x => x.OwnerRoleId,
                        principalTable: "OwnerRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnerRole_PermissionRecord_Mapping_PermissionRecord_PermissionRecordId",
                        column: x => x.PermissionRecordId,
                        principalTable: "PermissionRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Owner_Status",
                table: "Owner",
                column: "Status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Owner_Status",
                table: "Owner",
                sql: "[Status] IN (N'PendingActivation', N'Active', N'Inactive')");

            migrationBuilder.CreateIndex(
                name: "IX_Owner_OwnerRole_Mapping_OwnerRoleId",
                table: "Owner_OwnerRole_Mapping",
                column: "OwnerRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerPassword_OwnerId",
                table: "OwnerPassword",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "UQ_OwnerPassword_CurrentPerOwner",
                table: "OwnerPassword",
                columns: new[] { "OwnerId", "IsCurrent" },
                unique: true,
                filter: "[IsCurrent] = 1");

            migrationBuilder.CreateIndex(
                name: "UQ_OwnerRole_Name",
                table: "OwnerRole",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_OwnerRole_SystemName",
                table: "OwnerRole",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnerRole_PermissionRecord_Mapping_PermissionRecordId",
                table: "OwnerRole_PermissionRecord_Mapping",
                column: "PermissionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRecord_Category",
                table: "PermissionRecord",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "UQ_PermissionRecord_SystemName",
                table: "PermissionRecord",
                column: "SystemName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Owner_OwnerRole_Mapping");

            migrationBuilder.DropTable(
                name: "OwnerPassword");

            migrationBuilder.DropTable(
                name: "OwnerRole_PermissionRecord_Mapping");

            migrationBuilder.DropTable(
                name: "OwnerRole");

            migrationBuilder.DropTable(
                name: "PermissionRecord");

            migrationBuilder.DropIndex(
                name: "IX_Owner_Status",
                table: "Owner");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Owner_Status",
                table: "Owner");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Owner");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Owner");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Owner");
        }
    }
}
