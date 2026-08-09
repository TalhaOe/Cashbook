using System.Security.Claims;
using Cashbook.Data;
using Cashbook.Models;
using Cashbook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cashbook.Controllers
{
    [Authorize]
    public class CashEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CashEntries
        public async Task<IActionResult> Index()
        {
            string userId = GetCurrentUserId();

            var cashEntries = await _context.CashEntries
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToListAsync();

            decimal runningBalance = 0;
            decimal totalIncome = 0;
            decimal totalExpenses = 0;

            var rows = new List<CashEntryRowViewModel>();

            foreach (var entry in cashEntries)
            {
                if (entry.Type == EntryType.Income)
                {
                    totalIncome += entry.Amount;
                    runningBalance += entry.Amount;
                }
                else
                {
                    totalExpenses += entry.Amount;
                    runningBalance -= entry.Amount;
                }

                rows.Add(new CashEntryRowViewModel
                {
                    Entry = entry,
                    RunningBalance = runningBalance
                });
            }

            var viewModel = new CashBookIndexViewModel
            {
                Entries = rows,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = runningBalance
            };

            return View(viewModel);
        }

        // GET: CashEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = GetCurrentUserId();

            var cashEntry = await _context.CashEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (cashEntry == null)
            {
                return NotFound();
            }

            return View(cashEntry);
        }

        // GET: CashEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CashEntries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Date,Description,Amount,Type")]  
             CashEntry cashEntry)
        {
            string userId = GetCurrentUserId();

            cashEntry.UserId = userId;
            cashEntry.CreatedAt = DateTime.Now;
            cashEntry.ReceiptNumber = await GenerateReceiptNumber(userId, cashEntry.Date);

            ModelState.Remove(nameof(CashEntry.UserId));
            ModelState.Remove(nameof(CashEntry.User));
            ModelState.Remove(nameof(CashEntry.CreatedAt));
            ModelState.Remove(nameof(CashEntry.ReceiptNumber));

            if (ModelState.IsValid)
            {
                bool balanceWouldBecomeNegative =
                    await WouldBalanceBecomeNegative(userId, cashEntry);

                if (balanceWouldBecomeNegative)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Die Buchung kann nicht gespeichert werden, da der Kassenbestand dadurch negativ werden würde.");

                    return View(cashEntry);
                }

                _context.CashEntries.Add(cashEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(cashEntry);
        }

        // GET: CashEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = GetCurrentUserId();

            var cashEntry = await _context.CashEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (cashEntry == null)
            {
                return NotFound();
            }

            if (cashEntry.InvoiceId.HasValue)
            {
                return RedirectToAction(
                    "Details",
                    "Invoices",
                    new { id = cashEntry.InvoiceId.Value });
            }

            return View(cashEntry);
        }

        // POST: CashEntries/Edit/5
        // POST: CashEntries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Date,ReceiptNumber,Description,Amount,Type")]
             CashEntry postedEntry)
        {
            if (id != postedEntry.Id)
            {
                return NotFound();
            }

            string userId = GetCurrentUserId();

            var cashEntry = await _context.CashEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (cashEntry == null)
            {
                return NotFound();
            }

            if (cashEntry.InvoiceId.HasValue)
            {
                return BadRequest(
                    "Rechnungsbuchungen können nicht direkt bearbeitet werden.");
            }

            ModelState.Remove(nameof(CashEntry.UserId));
            ModelState.Remove(nameof(CashEntry.User));
            ModelState.Remove(nameof(CashEntry.CreatedAt));

            if (ModelState.IsValid)
            {
                var candidateEntry = new CashEntry
                {
                    Id = cashEntry.Id,
                    Date = postedEntry.Date,
                    ReceiptNumber = cashEntry.ReceiptNumber,
                    Description = postedEntry.Description,
                    Amount = postedEntry.Amount,
                    Type = postedEntry.Type,
                    CreatedAt = cashEntry.CreatedAt,
                    UserId = cashEntry.UserId
                };

                bool balanceWouldBecomeNegative =
                    await WouldBalanceBecomeNegative(
                        userId,
                        candidateEntry,
                        cashEntry.Id);

                if (balanceWouldBecomeNegative)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Die Änderung kann nicht gespeichert werden, da der Kassenbestand dadurch negativ werden würde.");

                    return View(postedEntry);
                }

                cashEntry.Date = postedEntry.Date;
                cashEntry.Description = postedEntry.Description;
                cashEntry.Amount = postedEntry.Amount;
                cashEntry.Type = postedEntry.Type;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(postedEntry);
        }

        // GET: CashEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = GetCurrentUserId();

            var cashEntry = await _context.CashEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (cashEntry == null)
            {
                return NotFound();
            }

            if (cashEntry.InvoiceId.HasValue)
            {
                return RedirectToAction(
                    "Details",
                    "Invoices",
                    new { id = cashEntry.InvoiceId.Value });
            }

            return View(cashEntry);
        }

        // POST: CashEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = GetCurrentUserId();

            var cashEntry = await _context.CashEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (cashEntry == null)
            {
                return NotFound();
            }

            if (cashEntry.InvoiceId.HasValue)
            {
                return BadRequest(
                    "Rechnungsbuchungen können nicht direkt gelöscht werden.");
            }

            _context.CashEntries.Remove(cashEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException(
                    "The current user could not be determined.");
        }

        private async Task<string> GenerateReceiptNumber(string userId, DateTime date)
        {
            int year = date.Year;

            string prefix = $"{year}-";

            var lastReceiptNumber = await _context.CashEntries
                .Where(x =>
                    x.UserId == userId &&
                    x.Date.Year == year &&
                    x.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(x => x.ReceiptNumber)
                .Select(x => x.ReceiptNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastReceiptNumber))
            {
                string numberPart = lastReceiptNumber.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{year}-{nextNumber:D4}";
        }

        private async Task<bool> WouldBalanceBecomeNegative(
        string userId,
         CashEntry candidateEntry,
         int? ignoreEntryId = null)
        {
            var entries = await _context.CashEntries
                .Where(x =>
                    x.UserId == userId &&
                    (!ignoreEntryId.HasValue || x.Id != ignoreEntryId.Value))
                .ToListAsync();

            entries.Add(candidateEntry);

            var orderedEntries = entries
            .OrderBy(x => x.Date)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();

            decimal balance = 0;

            foreach (var entry in orderedEntries)
            {
                if (entry.Type == EntryType.Income)
                {
                    balance += entry.Amount;
                }
                else
                {
                    balance -= entry.Amount;
                }

                if (balance < 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

