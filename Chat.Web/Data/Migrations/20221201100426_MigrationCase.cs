using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Web.Data.Migrations
{
    public partial class MigrationCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaseId",
                table: "Messages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CaseDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CaseStartingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CaseCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AgentReporting = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgentJoinDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MiccCaseId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiccCaseGuid = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Option01 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Option02 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Folder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cases_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CaseId",
                table: "Messages",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_AdminId",
                table: "Cases",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_RoomId",
                table: "Cases",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Cases_CaseId",
                table: "Messages",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Cases_CaseId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CaseId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "Messages");
        }
    }
}
