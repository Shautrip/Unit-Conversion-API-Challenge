using Microsoft.AspNetCore.Mvc;
using UnitConversion.Domain.Contracts;
using UnitConversion.Infrastructure.Services;

namespace UnitConversion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ConversionsController : ControllerBase
{
    private readonly IConversionService _service;

    public ConversionsController(IConversionService service) => _service = service;

    /// <summary>Convert a value between two units.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ConversionResponse> Post([FromBody] ConversionRequest request) =>
        Map(_service.Convert(request));

    /// <summary>Convenience GET variant: <c>?value=100&amp;from=C&amp;to=F</c>.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ConversionResponse> Get(
        [FromQuery] double value,
        [FromQuery] string from,
        [FromQuery] string to) =>
        Map(_service.Convert(new ConversionRequest { Value = value, FromUnit = from, ToUnit = to }));

    private ActionResult<ConversionResponse> Map(ConversionOutcome outcome)
    {
        if (outcome.IsSuccess)
        {
            return Ok(outcome.Response);
        }

        return outcome.Error switch
        {
            ConversionError.UnknownFromUnit or ConversionError.UnknownToUnit =>
                Problem(
                    title: "Unknown unit",
                    detail: outcome.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    type: "https://httpstatuses.io/404"),
            ConversionError.CategoryMismatch =>
                Problem(
                    title: "Incompatible units",
                    detail: outcome.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "https://httpstatuses.io/400"),
            _ => Problem(
                    title: "Conversion failed",
                    detail: outcome.Message,
                    statusCode: StatusCodes.Status500InternalServerError),
        };
    }
}
