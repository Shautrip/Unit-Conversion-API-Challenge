using Microsoft.AspNetCore.Mvc;
using UnitConversion.Domain.Contracts;
using UnitConversion.Infrastructure.Registry;

namespace UnitConversion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IUnitRegistry _registry;

    public CategoriesController(IUnitRegistry registry) => _registry = registry;

    /// <summary>List supported measurement categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<CategoryDto>> Get()
    {
        var categories = _registry.GetCategories()
            .Select(c => new CategoryDto(c.Id, c.Name, c.BaseUnitId));
        return Ok(categories);
    }
}
