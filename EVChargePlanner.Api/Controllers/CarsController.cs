using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EVChargePlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly EVChargePlannerDbContext _context;

    public CarsController(EVChargePlannerDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Car>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var cars = await _context.Cars.Where(c => c.UserId == userId).ToListAsync();
        return Ok(cars);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var car = await _context.Cars.Where(c => c.UserId == userId).ToListAsync();
        if (car == null) return NotFound();
        return Ok(car);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Car>> Create(Car car)
    {
        car.UserId = GetCurrentUserId();
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Car car)
    {
        var userId = GetCurrentUserId();
        var existing = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (existing == null) return NotFound();

        existing.Name = car.Name;
        existing.BatteryCapacityKWh = car.BatteryCapacityKWh;
        existing.MaxChargingPowerKW = car.MaxChargingPowerKW;
        existing.ModelLabel = car.ModelLabel;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (car == null) return NotFound();
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [ApiController]
    [Route("api/car-models")]
    [Authorize]
    public class CarModelsController : ControllerBase
    {
        private readonly EVChargePlannerDbContext _context;

        public CarModelsController(EVChargePlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var models = await _context.CarModels
                .OrderBy(m => m.Brand)
                .ThenBy(m => m.Model)
                .ToListAsync();

            return Ok(models);
        }
    }
}