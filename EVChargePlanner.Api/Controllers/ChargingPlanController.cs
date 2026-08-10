using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Domain.Services;
using EVChargePlanner.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EVChargePlanner.Api.Controllers;

public record CarChargeRequest(int CarId, int CurrentBatteryPercentage, int TargetBatteryPercentage, DateTime? ArrivalTime, DateTime? DepartureTime);
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
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var carIds = request.Cars.Select(c => c.CarId).ToList();
        var cars = await _context.Cars.Where(c => carIds.Contains(c.Id) && c.UserId == userId).ToListAsync();

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

            if (carRequest.ArrivalTime.HasValue && carRequest.DepartureTime.HasValue
                && carRequest.ArrivalTime.Value >= carRequest.DepartureTime.Value)
            {
                return BadRequest($"{car.Name}: arrival time must be before departure time.");
            }
        }

        var chargeInfos = request.Cars.Select(req => new CarChargeInfo
        {
            Car = cars.First(c => c.Id == req.CarId),
            CurrentBatteryPercentage = req.CurrentBatteryPercentage,
            TargetBatteryPercentage = req.TargetBatteryPercentage,
            ArrivalTime = req.ArrivalTime,
            DepartureTime = req.DepartureTime
        }).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.UtcNow;
        var prices = await _context.PriceRecords
            .Where(p => p.PriceZone == request.Zone
                     && p.TimeStart.Date == today.ToDateTime(TimeOnly.MinValue).Date
                     && p.TimeStart >= now.AddHours(-1))
            .OrderBy(p => p.TimeStart)
            .ToListAsync();

        if (prices.Count == 0)
        {
            return NotFound("No price data available for today.");
        }

        var chargers = await _context.Chargers.ToListAsync();
        if (chargers.Count == 0)
        {
            chargers = new List<Charger> { new Charger { Id = 0, Name = "Default", MaxPowerKW = 999 } };
        }

        var todayDate = DateTime.Today;
        var existingSessions = await _context.ChargingSessions
            .Where(s => s.StartTime.Date == todayDate)
            .ToListAsync();

        var plan = _plannerService.PlanForMultipleCars(chargeInfos, prices, chargers, existingSessions);
        return Ok(plan);
    }

    public record ConfirmSessionRequest(int CarId, int ChargerId, DateTime StartTime, DateTime EndTime, decimal EstimatedCost);
    public record ConfirmPlanRequest(List<ConfirmSessionRequest> Sessions);

    [Authorize]
    [HttpPost("confirm")]
    public async Task<ActionResult> ConfirmPlan(ConfirmPlanRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var carIds = request.Sessions.Select(s => s.CarId).ToList();
        var validCarIds = await _context.Cars
            .Where(c => carIds.Contains(c.Id) && c.UserId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var s in request.Sessions)
        {
            if (!validCarIds.Contains(s.CarId))
            {
                return Forbid();
            }

            _context.ChargingSessions.Add(new ChargingSession
            {
                CarId = s.CarId,
                ChargerId = s.ChargerId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                EstimatedCost = s.EstimatedCost
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpGet("today")]
    public async Task<ActionResult> GetTodaySessions()
    {
        var userID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var today = DateTime.Today;

        var sessions = await _context.ChargingSessions
            .Include(s => s.Car)
            .Include(s => s.Charger)
            .Where(s => s.StartTime.Date == today)
            .ToListAsync();

        return Ok(sessions.Select(s => new
        {
            s.Id,
            CarName = s.Car!.Name,
            ChargerName = s.Charger!.Name,
            s.StartTime,
            s.EndTime,
            s.EstimatedCost
        }));
    }

    [Authorize]
    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var session = await _context.ChargingSessions
            .Include(s => s.Car)
            .FirstOrDefaultAsync(s => s.Id == id && s.Car!.UserId == userId);
        
        if (session == null)
        {
            return NotFound();
        }

        _context.ChargingSessions.Remove(session);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}