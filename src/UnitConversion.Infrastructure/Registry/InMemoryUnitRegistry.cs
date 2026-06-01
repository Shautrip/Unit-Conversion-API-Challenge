using System.Diagnostics.CodeAnalysis;
using UnitConversion.Domain;

namespace UnitConversion.Infrastructure.Registry;

/// <summary>
/// Hardcoded seed of categories and units. To add a unit, append an entry to
/// <see cref="Seed"/>; no other code change is required. To swap to a
/// configuration-backed source, implement <see cref="IUnitRegistry"/> and
/// register it instead of this type in <c>Program.cs</c>.
/// </summary>
public sealed class InMemoryUnitRegistry : IUnitRegistry
{
    private readonly IReadOnlyList<UnitCategory> _categories;
    private readonly IReadOnlyList<Unit> _units;
    private readonly Dictionary<string, Unit> _byId;
    private readonly Dictionary<string, Unit> _bySymbol;

    public InMemoryUnitRegistry()
    {
        (_categories, _units) = Seed();
        _byId = new(StringComparer.OrdinalIgnoreCase);
        _bySymbol = new(StringComparer.OrdinalIgnoreCase);
        foreach (var u in _units)
        {
            _byId.Add(u.Id, u);
            // First registration wins for ambiguous symbols (e.g. "t" → tonne).
            _bySymbol.TryAdd(u.Symbol, u);
            // Aliases share the id bucket; first registration wins.
            if (u.Aliases is not null)
            {
                foreach (var alias in u.Aliases)
                {
                    _byId.TryAdd(alias, u);
                }
            }
        }
    }

    public IReadOnlyCollection<UnitCategory> GetCategories() => _categories;

    public IReadOnlyCollection<Unit> GetUnits(string? categoryId = null) =>
        categoryId is null
            ? _units
            : _units.Where(u => string.Equals(u.CategoryId, categoryId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

    public bool TryGetUnit(string idOrSymbol, [NotNullWhen(true)] out Unit unit)
    {
        if (_byId.TryGetValue(idOrSymbol, out var byId))
        {
            unit = byId;
            return true;
        }

        if (_bySymbol.TryGetValue(idOrSymbol, out var bySymbol))
        {
            unit = bySymbol;
            return true;
        }

        unit = null!;
        return false;
    }

    private static (IReadOnlyList<UnitCategory>, IReadOnlyList<Unit>) Seed()
    {
        var categories = new List<UnitCategory>
        {
            new("length", "Length", "meter"),
            new("mass", "Mass", "kilogram"),
            new("temperature", "Temperature", "kelvin"),
        };

        // Factor = value-of-unit expressed in the base unit (linear).
        // Offset only used for temperature (affine).
        var units = new List<Unit>
        {
            // Length (base: meter)
            new("meter",      "m",  "Meter",      "length", 1d,
                Aliases: new[] { "metre", "meters", "metres" }),
            new("kilometer",  "km", "Kilometer",  "length", 1_000d,
                Aliases: new[] { "kilometre", "kilometers", "kilometres", "klick", "klicks" }),
            new("centimeter", "cm", "Centimeter", "length", 0.01d,
                Aliases: new[] { "centimetre", "centimeters", "centimetres" }),
            new("millimeter", "mm", "Millimeter", "length", 0.001d,
                Aliases: new[] { "millimetre", "millimeters", "millimetres" }),
            new("inch",       "in", "Inch",       "length", 0.0254d,
                Aliases: new[] { "inches", "\"" }),
            new("foot",       "ft", "Foot",       "length", 0.3048d,
                Aliases: new[] { "feet", "'" }),
            new("yard",       "yd", "Yard",       "length", 0.9144d,
                Aliases: new[] { "yards" }),
            new("mile",       "mi", "Mile",       "length", 1_609.344d,
                Aliases: new[] { "miles" }),

            // Mass (base: kilogram)
            new("kilogram",  "kg", "Kilogram",   "mass", 1d,
                Aliases: new[] { "kilogramme", "kilogrammes", "kilograms", "kilo", "kilos" }),
            new("gram",      "g",  "Gram",       "mass", 0.001d,
                Aliases: new[] { "gramme", "grammes", "grams" }),
            new("milligram", "mg", "Milligram",  "mass", 0.000_001d,
                Aliases: new[] { "milligramme", "milligrammes", "milligrams" }),
            new("tonne",     "t",  "Metric ton", "mass", 1_000d,
                Aliases: new[] { "ton", "tons", "tonnes", "metricton", "metric-ton" }),
            new("pound",     "lb", "Pound",      "mass", 0.453_592_37d,
                Aliases: new[] { "pounds", "lbs" }),
            new("ounce",     "oz", "Ounce",      "mass", 0.028_349_523_125d,
                Aliases: new[] { "ounces" }),

            // Temperature (base: kelvin) — affine
            // K = value * factor + offset
            new("kelvin",     "K", "Kelvin",     "temperature", 1d, 0d,
                Aliases: new[] { "kelvins" }),
            new("celsius",    "C", "Celsius",    "temperature", 1d, 273.15d,
                Aliases: new[] { "celcius", "centigrade" }),
            new("fahrenheit", "F", "Fahrenheit", "temperature", 5d / 9d, 459.67d * 5d / 9d,
                Aliases: new[] { "farenheit", "fahrenhiet", "fahrenheight" }),
        };

        return (categories, units);
    }
}
