using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pivot.Migrations
{
    /// <inheritdoc />
    public partial class AddMainAddOnLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MainAddOnLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MainPlanId = table.Column<int>(type: "int", nullable: false),
                    AddOnPlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainAddOnLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainAddOnLinks_PaymentPlans_AddOnPlanId",
                        column: x => x.AddOnPlanId,
                        principalTable: "PaymentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MainAddOnLinks_PaymentPlans_MainPlanId",
                        column: x => x.MainPlanId,
                        principalTable: "PaymentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MainAddOnLinks_AddOnPlanId",
                table: "MainAddOnLinks",
                column: "AddOnPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MainAddOnLinks_MainPlanId_AddOnPlanId",
                table: "MainAddOnLinks",
                columns: new[] { "MainPlanId", "AddOnPlanId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainAddOnLinks");
        }
    }
}
