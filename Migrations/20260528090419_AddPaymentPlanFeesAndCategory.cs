using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pivot.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPlanFeesAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PaymentPlans",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdditional",
                table: "PaymentPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MembershipFee",
                table: "PaymentPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotedFee",
                table: "PaymentPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RegistrationFee",
                table: "PaymentPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "IsAdditional",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "MembershipFee",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "PromotedFee",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "RegistrationFee",
                table: "PaymentPlans");
        }
    }
}
