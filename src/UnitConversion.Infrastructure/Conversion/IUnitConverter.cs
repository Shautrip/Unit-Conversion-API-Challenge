using UnitConversion.Domain;

namespace UnitConversion.Infrastructure.Conversion;

/// <summary>
/// Converts a numerical value between two units that share a category.
/// </summary>
public interface IUnitConverter
{
    double Convert(double value, Unit from, Unit to);
}
