using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllergyScan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAllergyFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Flag",
                table: "Allergies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flag",
                table: "Allergies");
        }
    }
}
