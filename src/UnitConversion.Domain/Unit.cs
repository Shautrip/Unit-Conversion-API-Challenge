namespace UnitConversion.Domain;

/// <summary>
/// A unit of measurement, defined relative to its category's base unit by an
/// affine transform: <c>base = value * Factor + Offset</c>.
/// For purely linear conversions (length, mass) <see cref="Offset"/> is 0.
/// </summary>
public sealed record Unit(
    string Id,
    string Symbol,
    string Name,
    string CategoryId,
    double Factor,
    double Offset = 0d,
    IReadOnlyList<string>? Aliases = null);
