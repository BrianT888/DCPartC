using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCPartC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    clientId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ip = table.Column<string>(type: "TEXT", nullable: false),
                    port = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.clientId);
                });

            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    jobId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    clientId = table.Column<int>(type: "INTEGER", nullable: false),
                    jobName = table.Column<string>(type: "TEXT", nullable: false),
                    jobDescription = table.Column<string>(type: "TEXT", nullable: false),
                    jobStatus = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.jobId);
                    table.ForeignKey(
                        name: "FK_Job_Client_clientId",
                        column: x => x.clientId,
                        principalTable: "Client",
                        principalColumn: "clientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Job_clientId",
                table: "Job",
                column: "clientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Job");

            migrationBuilder.DropTable(
                name: "Client");
        }
    }
}
