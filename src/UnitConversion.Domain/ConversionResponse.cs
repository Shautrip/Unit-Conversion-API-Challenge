namespace UnitConversion.Domain.Contracts;

/// <summary>Successful conversion response.</summary>
public sealed record ConversionResponse(
    double Value,
    string FromUnit,
    string ToUnit,
    double Result,
    string Category);
