using Cashbook.Models;

namespace Cashbook.Models.ViewModels
{
    public class CashBookIndexViewModel
    {
        public List<CashEntryRowViewModel> Entries { get; set; } = [];

        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal Balance { get; set; }
    }

    public class CashEntryRowViewModel
    {
        public CashEntry Entry { get; set; } = null!;

        public decimal RunningBalance { get; set; }
    }
}