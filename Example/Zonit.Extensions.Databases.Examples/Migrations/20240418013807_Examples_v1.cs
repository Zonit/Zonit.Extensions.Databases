using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zonit.Extensions.Databases.Examples.Migrations
{
    /// <inheritdoc />
    public partial class Examples_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Zonit");

            migrationBuilder.CreateTable(
                name: "Examples.Blog",
                schema: "Zonit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examples.Blog", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Examples.Blog",
                schema: "Zonit");
        }
    }
}
