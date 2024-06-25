using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteServer.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServerFullPath",
                table: "syncLogFiles",
                newName: "ServerRootPath");

            migrationBuilder.RenameColumn(
                name: "ClientFullPath",
                table: "syncLogFiles",
                newName: "ClientRootPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServerRootPath",
                table: "syncLogFiles",
                newName: "ServerFullPath");

            migrationBuilder.RenameColumn(
                name: "ClientRootPath",
                table: "syncLogFiles",
                newName: "ClientFullPath");
        }
    }
}
