using UnitConversion.Domain.Contracts;

namespace UnitConversion.Infrastructure.Services;

public enum ConversionError
{
    None,
    UnknownFromUnit,
    UnknownToUnit,
    CategoryMismatch,
}

/// <summary>
/// Result of a conversion attempt. Discriminated by <see cref="Error"/>:
/// <see cref="ConversionError.None"/> means <see cref="Response"/> is set.
/// </summary>
public sealed record ConversionOutcome(
    ConversionError Error,
    ConversionResponse? Response = null,
    string? Message = null)
{
    public bool IsSuccess => Error == ConversionError.None;

    public static ConversionOutcome Success(ConversionResponse response) =>
        new(ConversionError.None, response);

    public static ConversionOutcome Failure(ConversionError error, string message) =>
        new(error, null, message);
}
