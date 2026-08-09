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
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            string userId = GetCurrentUserId();

            var invoices = await _context.Invoices
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return View(invoices);
        }


        // GET: Invoices/Create
        public IActionResult Create()
        {
            return View(new InvoiceCreateViewModel());
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = GetCurrentUserId();

            var invoice = await _context.Invoices
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateViewModel model)
        {
            if (model.Items == null || model.Items.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Die Rechnung benötigt mindestens eine Position.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userId = GetCurrentUserId();

            string invoiceNumber =
                await GenerateInvoiceNumber(userId, model.InvoiceDate.Year);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                InvoiceDate = model.InvoiceDate,
                ServiceDate = model.ServiceDate,
                Type = model.Type,

                IssuerName = model.IssuerName,
                IssuerStreet = model.IssuerStreet,
                IssuerPostalCode = model.IssuerPostalCode,
                IssuerCity = model.IssuerCity,
                IssuerVatIdentificationNumber =
                    model.IssuerVatIdentificationNumber,

                RecipientName = model.RecipientName,
                RecipientStreet = model.RecipientStreet,
                RecipientPostalCode = model.RecipientPostalCode,
                RecipientCity = model.RecipientCity,
                RecipientVatIdentificationNumber =
                    model.RecipientVatIdentificationNumber,

                UserId = userId,
                CreatedAt = DateTime.Now,
                Status = InvoiceStatus.Open
            };

            decimal totalNet = 0;
            decimal totalVat = 0;

            foreach (var item in model.Items!)
            {
                decimal net =
                    Math.Round(
                        item.Quantity * item.UnitPrice,
                        2,
                        MidpointRounding.AwayFromZero);

                decimal vat =
                    Math.Round(
                        net * item.VatRate / 100m,
                        2,
                        MidpointRounding.AwayFromZero);

                totalNet += net;
                totalVat += vat;

                invoice.Items.Add(new InvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    VatRate = item.VatRate
                });
            }

            invoice.NetAmount = totalNet;
            invoice.VatAmount = totalVat;
            invoice.GrossAmount = totalNet + totalVat;

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            string userId = GetCurrentUserId();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            // Offene Rechnung:
            // keine Kassenbewegung vorhanden -> nur Status ändern
            if (invoice.Status == InvoiceStatus.Open)
            {
                invoice.Status = InvoiceStatus.Cancelled;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id });
            }

            // Bezahlte Rechnung:
            // Gegenbuchung im Kassenbuch erzeugen
            if (invoice.Status == InvoiceStatus.Paid)
            {
                var reversalEntry = new CashEntry
                {
                    Date = DateTime.Today,
                    Description = $"Storno Rechnung {invoice.InvoiceNumber}",
                    Amount = invoice.GrossAmount,

                    Type = invoice.Type == InvoiceType.Outgoing
                        ? EntryType.Expense
                        : EntryType.Income,

                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    InvoiceId = invoice.Id
                };

                reversalEntry.ReceiptNumber =
                    await GenerateCashEntryReceiptNumber(
                        userId,
                        reversalEntry.Date);

                // Bei einer stornierten Ausgangsrechnung entsteht eine Ausgabe.
                // Diese darf den Kassenbestand nicht negativ machen.
                if (reversalEntry.Type == EntryType.Expense)
                {
                    bool balanceWouldBecomeNegative =
                        await WouldCashBalanceBecomeNegative(
                            userId,
                            reversalEntry);

                    if (balanceWouldBecomeNegative)
                    {
                        TempData["ErrorMessage"] =
                            "Die Rechnung kann nicht storniert werden, " +
                            "da die Gegenbuchung den Kassenbestand negativ machen würde.";

                        return RedirectToAction(nameof(Details), new { id });
                    }
                }

                invoice.Status = InvoiceStatus.Cancelled;

                _context.CashEntries.Add(reversalEntry);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            string userId = GetCurrentUserId();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != InvoiceStatus.Open)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var cashEntry = new CashEntry
            {
                Date = DateTime.Today,
                Description = $"Zahlung Rechnung {invoice.InvoiceNumber}",
                Amount = invoice.GrossAmount,
                Type = invoice.Type == InvoiceType.Outgoing
                    ? EntryType.Income
                    : EntryType.Expense,
                CreatedAt = DateTime.Now,
                UserId = userId,
                InvoiceId = invoice.Id
            };

            cashEntry.ReceiptNumber =
                await GenerateCashEntryReceiptNumber(
                    userId,
                    cashEntry.Date);

            if (cashEntry.Type == EntryType.Expense)
            {
                bool balanceWouldBecomeNegative =
                    await WouldCashBalanceBecomeNegative(
                        userId,
                        cashEntry);

                if (balanceWouldBecomeNegative)
                {
                    TempData["ErrorMessage"] =
                        "Die Rechnung kann nicht als bezahlt markiert werden, " +
                        "da der Kassenbestand dadurch negativ werden würde.";

                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.Now;

            _context.CashEntries.Add(cashEntry);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<string> GenerateCashEntryReceiptNumber(
          string userId,
            DateTime date)
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
                string numberPart =
                    lastReceiptNumber.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{year}-{nextNumber:D4}";
        }

        private async Task<bool> WouldCashBalanceBecomeNegative(
        string userId,
          CashEntry candidateEntry)
        {
            var entries = await _context.CashEntries
                .Where(x => x.UserId == userId)
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

        private async Task<string> GenerateInvoiceNumber(
            string userId,
            int year)
        {
            string prefix = $"RE-{year}-";

            var lastInvoiceNumber = await _context.Invoices
                .Where(x =>
                    x.UserId == userId &&
                    x.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(x => x.InvoiceNumber)
                .Select(x => x.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastInvoiceNumber))
            {
                string numberPart =
                    lastInvoiceNumber.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }


        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException(
                    "Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }
    }
}