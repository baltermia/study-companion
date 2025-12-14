using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyCompanion.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Homework",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Homework");
        }
    }
}
