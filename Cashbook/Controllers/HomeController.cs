using System.Security.Claims;
using Cashbook.Data;
using Cashbook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cashbook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string userId = GetCurrentUserId();

            var cashEntries = await _context.CashEntries
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            decimal totalIncome = await _context.CashEntries
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == EntryType.Income)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            decimal totalExpenses = await _context.CashEntries
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == EntryType.Expense)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            decimal balance = totalIncome - totalExpenses;

            int openInvoices = await _context.Invoices
                .CountAsync(x =>
                    x.UserId == userId &&
                    x.Status == InvoiceStatus.Open);

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.Balance = balance;
            ViewBag.OpenInvoices = openInvoices;
            ViewBag.RecentEntries = cashEntries;

            return View();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException(
                    "Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }
    }
}