using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Domain;
using EVChargePlanner.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace EVChargePlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ChargersController : ControllerBase
{
    private readonly EVChargePlannerDbContext _context;

    public ChargersController(EVChargePlannerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Charger>>> GetAll()
    {
        return Ok(await _context.Chargers.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Charger>> Create(Charger charger)
    {
        _context.Chargers.Add(charger);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), charger);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var charger = await _context.Chargers.FindAsync(id);
        if (charger == null)
        {
            return NotFound();
        }

        _context.Chargers.Remove(charger);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}