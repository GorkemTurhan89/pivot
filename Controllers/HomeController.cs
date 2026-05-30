using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models;
using Pivot.Models.Entities;

namespace Pivot.Controllers;

public class HomeController : Controller
{
    private readonly PivotDbContext _db;

    public HomeController(PivotDbContext db)
    {
        _db = db;
    }

    // Ana sayfa: CartDetail dashboard'u (UpdateDate desc, son 50 + lazy "daha fazla").
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet]
    public async Task<IActionResult> GetCarts(int skip = 0, int take = 50, string? status = null)
    {
        if (take < 1) take = 1;
        if (take > 200) take = 200;

        var q = _db.CartDetails.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)
            && Enum.TryParse<CartStatus>(status, true, out var s))
        {
            q = q.Where(c => c.Status == s);
        }

        var items = await q
            .OrderByDescending(c => c.UpdateDate)
            .Skip(skip)
            .Take(take)
            .Select(c => new
            {
                c.Id,
                c.CartGuid,
                MemberName = c.Member != null ? (c.Member.FirstName + " " + c.Member.LastName) : "",
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Nationality = c.Nationality,
                CreateDate = c.CreateDate,
                UpdateDate = c.UpdateDate,
                Status = c.Status.ToString(),
                TotalPaymentPrice = c.TotalPaymentPrice
            })
            .ToListAsync();

        return Json(items);
    }
}
