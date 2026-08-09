using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cashbook.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; } = null!;

        [Required]
        [StringLength(500)]
        [Display(Name = "Leistung / Artikel")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 999999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Menge")]
        public decimal Quantity { get; set; } = 1;

        [Range(0, 999999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Einzelpreis netto")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "USt. %")]
        public decimal VatRate { get; set; }
    }
}