using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zonit.Extensions.Databases.Examples.Migrations
{
    /// <inheritdoc />
    public partial class Examples_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Zonit",
                table: "Examples.Blog",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Zonit",
                table: "Examples.Blog");
        }
    }
}
