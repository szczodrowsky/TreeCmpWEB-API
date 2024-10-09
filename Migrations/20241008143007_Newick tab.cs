using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreeCmpWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Newicktab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Newicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comparisionMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    newickFirstString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    newickSecondString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    windowWidth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rootedMetrics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unrootedMetrics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    normalizedDistances = table.Column<bool>(type: "bit", nullable: true),
                    pruneTrees = table.Column<bool>(type: "bit", nullable: true),
                    includeSummary = table.Column<bool>(type: "bit", nullable: true),
                    zeroWeightsAllowed = table.Column<bool>(type: "bit", nullable: true),
                    bifurcationTreesOnly = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newicks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileGeneratedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseFiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Newicks");

            migrationBuilder.DropTable(
                name: "ResponseFiles");
        }
    }
}
