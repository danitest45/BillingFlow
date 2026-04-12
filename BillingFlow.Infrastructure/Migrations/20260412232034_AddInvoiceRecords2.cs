using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceRecords2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceRecords_Clients_ClientId1",
                table: "InvoiceRecords");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceRecords_ClientId1",
                table: "InvoiceRecords");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "InvoiceRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "InvoiceRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_ClientId1",
                table: "InvoiceRecords",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceRecords_Clients_ClientId1",
                table: "InvoiceRecords",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
