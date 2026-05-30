using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pivot.Migrations
{
    /// <inheritdoc />
    public partial class SupplementsAndAccDetailExpand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "AccommodationDetails",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AccommodationDetails",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisplayOption",
                table: "AccommodationDetails",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Required",
                table: "AccommodationDetails",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "AccommodationDetails",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupplementDetails",
                columns: table => new
                {
                    PaymentPlanId = table.Column<int>(type: "int", nullable: false),
                    School = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Campus = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppliesTo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupplementName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Required = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementDetails", x => x.PaymentPlanId);
                    table.ForeignKey(
                        name: "FK_SupplementDetails_PaymentPlans_PaymentPlanId",
                        column: x => x.PaymentPlanId,
                        principalTable: "PaymentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDetails_School_Country_Campus_Type",
                table: "AccommodationDetails",
                columns: new[] { "School", "Country", "Campus", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementDetails_School_Country_Campus_AppliesTo",
                table: "SupplementDetails",
                columns: new[] { "School", "Country", "Campus", "AppliesTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplementDetails");

            migrationBuilder.DropIndex(
                name: "IX_AccommodationDetails_School_Country_Campus_Type",
                table: "AccommodationDetails");

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "AccommodationDetails");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AccommodationDetails");

            migrationBuilder.DropColumn(
                name: "DisplayOption",
                table: "AccommodationDetails");

            migrationBuilder.DropColumn(
                name: "Required",
                table: "AccommodationDetails");

            migrationBuilder.DropColumn(
                name: "School",
                table: "AccommodationDetails");
        }
    }
}
