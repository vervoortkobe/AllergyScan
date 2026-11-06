using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllergyScan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AllergyNameUniquePerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Allergies_Name",
                table: "Allergies");

            migrationBuilder.DropIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies");

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies");

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_Name",
                table: "Allergies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies",
                columns: new[] { "UserId", "Name" });
        }
    }
}
