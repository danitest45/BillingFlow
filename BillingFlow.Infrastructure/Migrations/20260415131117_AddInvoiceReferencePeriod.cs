using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceReferencePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceRecords_ClientId",
                table: "InvoiceRecords");

            migrationBuilder.AddColumn<int>(
                name: "ReferenceMonth",
                table: "InvoiceRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceYear",
                table: "InvoiceRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "InvoiceRecords"
                SET "ReferenceYear" = EXTRACT(YEAR FROM "DueDate")::int,
                    "ReferenceMonth" = EXTRACT(MONTH FROM "DueDate")::int
            """);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_ClientId_ReferenceYear_ReferenceMonth",
                table: "InvoiceRecords",
                columns: new[] { "ClientId", "ReferenceYear", "ReferenceMonth" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceRecords_ClientId_ReferenceYear_ReferenceMonth",
                table: "InvoiceRecords");

            migrationBuilder.DropColumn(
                name: "ReferenceMonth",
                table: "InvoiceRecords");

            migrationBuilder.DropColumn(
                name: "ReferenceYear",
                table: "InvoiceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_ClientId",
                table: "InvoiceRecords",
                column: "ClientId");
        }
    }
}
