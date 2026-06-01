namespace UnitConversion.Domain;

/// <summary>
/// A measurement category (e.g. Length, Mass, Temperature). Every category
/// has a canonical <see cref="BaseUnitId"/> that all conversions go through.
/// </summary>
public sealed record UnitCategory(string Id, string Name, string BaseUnitId);
