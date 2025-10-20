using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCPartC.Migrations
{
    /// <inheritdoc />
    public partial class ClientJobsCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "jobsFinished",
                table: "Client",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jobsFinished",
                table: "Client");
        }
    }
}
