using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstallVibe.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Guides",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "Guides",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "Guides");
        }
    }
}
