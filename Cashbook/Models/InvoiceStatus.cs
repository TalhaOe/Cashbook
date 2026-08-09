using System.ComponentModel.DataAnnotations;

namespace Cashbook.Models
{
    public enum InvoiceStatus
    {
        [Display(Name = "Offen")]
        Open = 1,

        [Display(Name = "Bezahlt")]
        Paid = 2,

        [Display(Name = "Storniert")]
        Cancelled = 3
    }
}