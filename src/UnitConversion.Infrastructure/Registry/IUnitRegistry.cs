using UnitConversion.Domain;

namespace UnitConversion.Infrastructure.Registry;

/// <summary>
/// Read-only catalog of units and categories. The in-memory implementation
/// seeds data at startup; alternative implementations (JSON, database) can be
/// swapped in via DI without affecting callers.
/// </summary>
public interface IUnitRegistry
{
    IReadOnlyCollection<UnitCategory> GetCategories();

    IReadOnlyCollection<Unit> GetUnits(string? categoryId = null);

    /// <summary>Resolve a unit by id (preferred) or symbol. Case-insensitive.</summary>
    bool TryGetUnit(string idOrSymbol, out Unit unit);
}
