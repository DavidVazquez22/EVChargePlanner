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

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<PriceRecord>>> GetUpcoming([FromQuery] string zone = "NO1")
    {
        var cutoff = DateTime.UtcNow.AddHours(-3);

        var prices = await _context.PriceRecords
            .Where(p => p.PriceZone == zone && p.TimeEnd > cutoff)
            .OrderBy(p => p.TimeStart)
            .ToListAsync();

        return Ok(prices);
    }

    [HttpGet("availability")]
    public async Task<ActionResult> GetAvailability([FromQuery] string zone = "NO1")
    {
        var latest = await _context.PriceRecords
            .Where(p => p.PriceZone == zone)
            .OrderByDescending(p => p.TimeEnd)
            .Select(p => p.TimeEnd)
            .FirstOrDefaultAsync();

        if (latest == default)
        {
            return NotFound("No price data available.");
        }

        return Ok(new { latestAvailable = latest });
    }
}