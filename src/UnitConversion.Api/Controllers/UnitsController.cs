using Microsoft.AspNetCore.Mvc;
using UnitConversion.Domain.Contracts;
using UnitConversion.Infrastructure.Registry;

namespace UnitConversion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class UnitsController : ControllerBase
{
    private readonly IUnitRegistry _registry;

    public UnitsController(IUnitRegistry registry) => _registry = registry;

    /// <summary>List all known units, optionally filtered by category id.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UnitDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UnitDto>> Get([FromQuery] string? category = null)
    {
        var units = _registry.GetUnits(category)
            .Select(u => new UnitDto(u.Id, u.Symbol, u.Name, u.CategoryId));
        return Ok(units);
    }
}
