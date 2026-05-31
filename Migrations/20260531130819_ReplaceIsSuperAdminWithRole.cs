using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pivot.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsSuperAdminWithRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Önce Role kolonunu ekle (default değer mevcut tüm satırlar için).
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "users",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "SalesRep")
                .Annotation("MySql:CharSet", "utf8mb4");

            // 2) Eski IsSuperAdmin=1 olan kullanıcıları SuperAdmin rolüne taşı.
            migrationBuilder.Sql("UPDATE users SET Role = 'SuperAdmin' WHERE IsSuperAdmin = 1;");

            // 3) Eski boolean kolonu kaldır.
            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                table: "users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE users SET IsSuperAdmin = 1 WHERE Role = 'SuperAdmin';");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");
        }
    }
}
