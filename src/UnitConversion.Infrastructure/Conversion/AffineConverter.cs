using UnitConversion.Domain;

namespace UnitConversion.Infrastructure.Conversion;

/// <summary>
/// Generic affine converter: <c>base = value * factor + offset</c>.
/// Handles both linear (offset == 0, e.g. length, mass) and affine
/// (offset != 0, e.g. temperature) units uniformly.
/// </summary>
public sealed class AffineConverter : IUnitConverter
{
    public double Convert(double value, Unit from, Unit to)
    {
        if (!string.Equals(from.CategoryId, to.CategoryId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Units '{from.Id}' and '{to.Id}' belong to different categories.");
        }

        var inBase = value * from.Factor + from.Offset;
        return (inBase - to.Offset) / to.Factor;
    }
}
