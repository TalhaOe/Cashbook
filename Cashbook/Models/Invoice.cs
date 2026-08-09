using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cashbook.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Rechnungsnummer")]
        public string InvoiceNumber { get; set; } = string.Empty;

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
        [StringLength(200)]
        [Display(Name = "Rechnungssteller")]
        public string IssuerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Straße")]
        public string IssuerStreet { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "PLZ")]
        public string IssuerPostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Ort")]
        public string IssuerCity { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "UID Rechnungssteller")]
        public string? IssuerVatIdentificationNumber { get; set; }


        [Required]
        [StringLength(200)]
        [Display(Name = "Rechnungsempfänger")]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Straße")]
        public string RecipientStreet { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "PLZ")]
        public string RecipientPostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Ort")]
        public string RecipientCity { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "UID Rechnungsempfänger")]
        public string? RecipientVatIdentificationNumber { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Netto")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Umsatzsteuer")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Brutto")]
        public decimal GrossAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        public ICollection<InvoiceItem> Items { get; set; }
            = new List<InvoiceItem>();

        [Required]
        [Display(Name = "Status")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

        [DataType(DataType.Date)]
        [Display(Name = "Bezahlt am")]
        public DateTime? PaidAt { get; set; }

        [Required]
        [Display(Name = "Rechnungsart")]
        public InvoiceType Type { get; set; }
    }
}