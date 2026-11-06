using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllergyScan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScanAllergyManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Scans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Allergies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ScanAllergies",
                columns: table => new
                {
                    ScanId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllergyId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DetectionFlag = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanAllergies", x => new { x.ScanId, x.AllergyId });
                    table.ForeignKey(
                        name: "FK_ScanAllergies_Allergies_AllergyId",
                        column: x => x.AllergyId,
                        principalTable: "Allergies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScanAllergies_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scans_UserId",
                table: "Scans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_UserId_ScannedAt",
                table: "Scans",
                columns: new[] { "UserId", "ScannedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_UserId",
                table: "Allergies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanAllergies_AllergyId",
                table: "ScanAllergies",
                column: "AllergyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanAllergies_ScanId",
                table: "ScanAllergies",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanAllergies_ScanId_AllergyId",
                table: "ScanAllergies",
                columns: new[] { "ScanId", "AllergyId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScanAllergies_UserId",
                table: "ScanAllergies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanAllergies");

            migrationBuilder.DropIndex(
                name: "IX_Scans_UserId",
                table: "Scans");

            migrationBuilder.DropIndex(
                name: "IX_Scans_UserId_ScannedAt",
                table: "Scans");

            migrationBuilder.DropIndex(
                name: "IX_Allergies_UserId",
                table: "Allergies");

            migrationBuilder.DropIndex(
                name: "IX_Allergies_UserId_Name",
                table: "Allergies");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Scans");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Allergies");
        }
    }
}
