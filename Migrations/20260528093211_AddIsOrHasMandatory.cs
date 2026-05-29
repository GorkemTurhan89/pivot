using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pivot.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOrHasMandatory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrHasMandatory",
                table: "PaymentPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOrHasMandatory",
                table: "PaymentPlans");
        }
    }
}
