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
                name: "SyncGitCommits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommitId = table.Column<string>(type: "TEXT", nullable: false),
                    CommitUserName = table.Column<string>(type: "TEXT", nullable: false),
                    CommitTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CommitMessage = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncGitCommits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientRootPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ServerRootPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogHeads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionsFromTag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SyncTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClientID = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogHeads", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncGitCommits");

            migrationBuilder.DropTable(
                name: "SyncLogFiles");

            migrationBuilder.DropTable(
                name: "SyncLogHeads");
        }
    }
}
