using UnitConversion.Domain.Contracts;
using UnitConversion.Infrastructure.Conversion;
using UnitConversion.Infrastructure.Registry;
using Microsoft.Extensions.Logging;

namespace UnitConversion.Infrastructure.Services;

public sealed class ConversionService : IConversionService
{
    private readonly IUnitRegistry _registry;
    private readonly IUnitConverter _converter;
    private readonly ILogger<ConversionService> _logger;

    public ConversionService(
        IUnitRegistry registry,
        IUnitConverter converter,
        ILogger<ConversionService> logger)
    {
        _registry = registry;
        _converter = converter;
        _logger = logger;
    }

    public ConversionOutcome Convert(ConversionRequest request)
    {
        if (!_registry.TryGetUnit(request.FromUnit, out var from))
        {
            return ConversionOutcome.Failure(
                ConversionError.UnknownFromUnit,
                $"Unknown source unit '{request.FromUnit}'.");
        }

        if (!_registry.TryGetUnit(request.ToUnit, out var to))
        {
            return ConversionOutcome.Failure(
                ConversionError.UnknownToUnit,
                $"Unknown target unit '{request.ToUnit}'.");
        }

        if (!string.Equals(from.CategoryId, to.CategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return ConversionOutcome.Failure(
                ConversionError.CategoryMismatch,
                $"Cannot convert from '{from.Id}' ({from.CategoryId}) to '{to.Id}' ({to.CategoryId}).");
        }

        var raw = _converter.Convert(request.Value, from, to);
        // Round to 10 decimals to suppress IEEE 754 artifacts (e.g. 31.999999999999986 -> 32).
        // 10 places is well beyond the precision of any seeded conversion factor.
        var result = double.IsFinite(raw)
            ? Math.Round(raw, 10, MidpointRounding.ToEven)
            : raw;
        _logger.LogDebug(
            "Converted {Value} {From} -> {Result} {To}",
            request.Value, from.Id, result, to.Id);

        return ConversionOutcome.Success(
            new ConversionResponse(request.Value, from.Id, to.Id, result, from.CategoryId));
    }
}
