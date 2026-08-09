using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendInvoiceAddressData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VatIdentificationNumber",
                table: "Invoices",
                newName: "RecipientVatIdentificationNumber");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Invoices",
                newName: "RecipientStreet");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Invoices",
                newName: "RecipientPostalCode");

            migrationBuilder.RenameColumn(
                name: "PartnerName",
                table: "Invoices",
                newName: "RecipientName");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Invoices",
                newName: "RecipientCity");

            migrationBuilder.AddColumn<string>(
                name: "IssuerCity",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IssuerName",
                table: "Invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IssuerPostalCode",
                table: "Invoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IssuerStreet",
                table: "Invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IssuerVatIdentificationNumber",
                table: "Invoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuerCity",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuerName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuerPostalCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuerStreet",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuerVatIdentificationNumber",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "RecipientVatIdentificationNumber",
                table: "Invoices",
                newName: "VatIdentificationNumber");

            migrationBuilder.RenameColumn(
                name: "RecipientStreet",
                table: "Invoices",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "RecipientPostalCode",
                table: "Invoices",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "Invoices",
                newName: "PartnerName");

            migrationBuilder.RenameColumn(
                name: "RecipientCity",
                table: "Invoices",
                newName: "City");
        }
    }
}
