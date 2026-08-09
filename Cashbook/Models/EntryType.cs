using System.ComponentModel.DataAnnotations;

namespace Cashbook.Models
{
    public enum EntryType
    {
        [Display(Name = "Einnahme")]
        Income = 1,

        [Display(Name = "Ausgabe")]
        Expense = 2
    }
}