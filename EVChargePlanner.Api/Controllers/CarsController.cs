using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Infrastructure;
using Microsoft.AspNetCore.Authorization;

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

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Car>>> GetAll()
    {
        var cars = await _context.Cars.ToListAsync();
        return Ok(cars);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();
        return Ok(car);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Car>> Create(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Car car)
    {
        if (id != car.Id) return BadRequest();
        _context.Entry(car).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _context.Cars.FindAsync(id);
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