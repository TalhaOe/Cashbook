using System.ComponentModel.DataAnnotations;

namespace Cashbook.Models
{
    public enum InvoiceType
    {
        [Display(Name = "Ausgangsrechnung")]
        Outgoing = 1,

        [Display(Name = "Eingangsrechnung")]
        Incoming = 2
    }
}