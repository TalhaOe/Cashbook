using System.ComponentModel.DataAnnotations;
using Cashbook.Models;

namespace Cashbook.Models.ViewModels
{
    public class InvoiceCreateViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Rechnungsdatum")]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Leistungsdatum")]
        public DateTime ServiceDate { get; set; } = DateTime.Today;


        // Rechnungssteller

        [Required]
        [Display(Name = "Rechnungssteller")]
        public string IssuerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Straße")]
        public string IssuerStreet { get; set; } = string.Empty;

        [Required]
        [Display(Name = "PLZ")]
        public string IssuerPostalCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ort")]
        public string IssuerCity { get; set; } = string.Empty;

        [Display(Name = "UID")]
        public string? IssuerVatIdentificationNumber { get; set; }


        // Rechnungsempfänger

        [Required]
        [Display(Name = "Rechnungsempfänger")]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Straße")]
        public string RecipientStreet { get; set; } = string.Empty;

        [Required]
        [Display(Name = "PLZ")]
        public string RecipientPostalCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ort")]
        public string RecipientCity { get; set; } = string.Empty;

        [Display(Name = "UID")]
        public string? RecipientVatIdentificationNumber { get; set; }

        [Required]
        [Display(Name = "Rechnungsart")]
        public InvoiceType Type { get; set; } = InvoiceType.Outgoing;

        public List<InvoiceItemCreateViewModel> Items { get; set; }
            = new()
            {
                new InvoiceItemCreateViewModel()
            };
    }


    public class InvoiceItemCreateViewModel
    {
        [Required(ErrorMessage = "Eine Beschreibung ist erforderlich.")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 999999999)]
        public decimal Quantity { get; set; } = 1;

        [Range(0, 999999999)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal VatRate { get; set; } = 20;
    }
}