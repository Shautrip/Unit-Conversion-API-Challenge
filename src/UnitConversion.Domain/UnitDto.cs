namespace UnitConversion.Domain.Contracts;

public sealed record UnitDto(string Id, string Symbol, string Name, string Category);

public sealed record CategoryDto(string Id, string Name, string BaseUnit);
