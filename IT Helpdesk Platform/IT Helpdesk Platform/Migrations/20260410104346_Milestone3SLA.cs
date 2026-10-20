using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IT_Helpdesk_Platform.Migrations
{
    /// <inheritdoc />
    public partial class Milestone3SLA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Tickets");
        }
    }
}
