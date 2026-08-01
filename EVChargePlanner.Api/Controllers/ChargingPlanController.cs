using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Domain.Services;
using EVChargePlanner.Infrastructure;

namespace EVChargePlanner.Api.Controllers;

[ApiController]
[Route("api/charging-plan")]
public class ChargingPlanController : ControllerBase
{
    private readonly EVChargePlannerDbContext _context;
    private readonly ChargingPlannerService _plannerService;

    public ChargingPlanController(EVChargePlannerDbContext context, ChargingPlannerService plannerService)
    {
        _context = context;
        _plannerService = plannerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CarChargingPlan>>> GetPlan([FromQuery] string zone = "NO1")
    {
        var cars = await _context.Cars.ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var prices = await _context.PriceRecords
            .Where(p => p.PriceZone == zone && p.TimeStart.Date == today.ToDateTime(TimeOnly.MinValue).Date)
            .ToListAsync();

        if (prices.Count == 0)
        {
            return NotFound("No price data available for today. The background service may not have run yet.");
        }

        var numberOfChargers = await _context.Chargers.CountAsync();
        if (numberOfChargers == 0)
        {
            numberOfChargers = 1;
        }

        var plan = _plannerService.PlanForMultipleCars(cars, prices, numberOfChargers);

        return Ok(plan);
    }
}