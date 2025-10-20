using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCPartC.Migrations
{
    /// <inheritdoc />
    public partial class changedjobresult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "Job",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "Job");
        }
    }
}
