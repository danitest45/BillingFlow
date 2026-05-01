using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientBillingCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingCycle",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BillingStartDate",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingStartDate",
                table: "Clients");
        }
    }
}
