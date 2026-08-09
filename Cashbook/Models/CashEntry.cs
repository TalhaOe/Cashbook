using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cashbook.Models
{
    public class CashEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Das Datum ist erforderlich.")]
        [Display(Name = "Datum")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Die Belegnummer ist erforderlich.")]
        [StringLength(50)]
        [Display(Name = "Belegnummer")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die Beschreibung ist erforderlich.")]
        [StringLength(500)]
        [Display(Name = "Beschreibung")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Der Betrag ist erforderlich.")]
        [Range(0.01, 999999999.99,
            ErrorMessage = "Der Betrag muss größer als 0 sein.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Betrag")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Der Buchungstyp ist erforderlich.")]
        [Display(Name = "Typ")]
        public EntryType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        public int? InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }
    }
}