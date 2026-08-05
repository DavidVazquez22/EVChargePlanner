using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVChargePlanner.Infrastructure;

namespace EVChargePlanner.Api.Controllers;

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