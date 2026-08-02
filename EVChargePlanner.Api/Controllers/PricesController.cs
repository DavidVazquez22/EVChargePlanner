using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Infrastructure;

namespace EVChargePlanner.Api.Controllers;

[ApiController]
[Route("api/prices")]
[Authorize]
public class PricesController : ControllerBase
{
    private readonly EVChargePlannerDbContext _context;

    public PricesController(EVChargePlannerDbContext context)
    {
        _context = context;
    }

    [HttpGet("today")]
    public async Task<ActionResult<List<PriceRecord>>> GetToday([FromQuery] string zone = "NO1")
    {
        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);

        var prices = await _context.PriceRecords
            .Where(p => p.PriceZone == zone && p.TimeStart.Date == today.Date)
            .OrderBy(p => p.TimeStart)
            .ToListAsync();

        return Ok(prices);
    }
}