using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Domain.Services;
using EVChargePlanner.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace EVChargePlanner.Api.Controllers;
public record CarChargeRequest(int CarId, int CurrentBatteryPercentage, int TargetBatteryPercentage, DateTime? DepartureTime);
public record ChargingPlanRequest(List<CarChargeRequest> Cars, string Zone = "NO1");

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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<List<CarChargingPlan>>> GetPlan(ChargingPlanRequest request)
    {
        var carIds = request.Cars.Select(c => c.CarId).ToList();
        var cars = await _context.Cars.Where(c => carIds.Contains(c.Id)).ToListAsync();

        foreach (var carRequest in request.Cars)
        {
            var car = cars.FirstOrDefault(c => c.Id == carRequest.CarId);
            if (car == null)
            {
                return BadRequest($"Car with id {carRequest.CarId} not found.");
            }

            if (carRequest.CurrentBatteryPercentage < 0 || carRequest.CurrentBatteryPercentage > 100)
            {
                return BadRequest($"{car.Name}: current battery percentage must be between 0 and 100.");
            }

            if (carRequest.TargetBatteryPercentage < 0 || carRequest.TargetBatteryPercentage > 100)
            {
                return BadRequest($"{car.Name}: target battery percentage must be between 0 and 100.");
            }

            if (carRequest.CurrentBatteryPercentage >= carRequest.TargetBatteryPercentage)
            {
                return BadRequest(
                    $"{car.Name}: current battery ({carRequest.CurrentBatteryPercentage}%) must be lower than target ({carRequest.TargetBatteryPercentage}%).");
            }
        }

        var chargeInfos = request.Cars.Select(req => new CarChargeInfo
        {
            Car = cars.First(c => c.Id == req.CarId),
            CurrentBatteryPercentage = req.CurrentBatteryPercentage,
            TargetBatteryPercentage = req.TargetBatteryPercentage,
            DepartureTime = req.DepartureTime
        }).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var prices = await _context.PriceRecords
            .Where(p => p.PriceZone == request.Zone && p.TimeStart.Date == today.ToDateTime(TimeOnly.MinValue).Date)
            .ToListAsync();

        if (prices.Count == 0)
        {
            return NotFound("No price data available for today.");
        }

        var numberOfChargers = await _context.Chargers.CountAsync();
        if (numberOfChargers == 0) numberOfChargers = 1;

        var plan = _plannerService.PlanForMultipleCars(chargeInfos, prices, numberOfChargers);
        return Ok(plan);
    }
}