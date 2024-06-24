using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "syncLogFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientFullPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ServerFullPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_syncLogFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "syncLogHeads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommitID = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SyncTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClientID = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_syncLogHeads", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "syncLogFiles");

            migrationBuilder.DropTable(
                name: "syncLogHeads");
        }
    }
}
