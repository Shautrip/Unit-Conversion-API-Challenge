using FluentAssertions;
using UnitConversion.Infrastructure.Registry;
using Xunit;

namespace UnitConversion.Api.Tests.Registry;

public class InMemoryUnitRegistryTests
{
    private readonly InMemoryUnitRegistry _sut = new();

    [Fact]
    public void Categories_contain_expected_base_units()
    {
        var cats = _sut.GetCategories();
        cats.Should().Contain(c => c.Id == "length" && c.BaseUnitId == "meter");
        cats.Should().Contain(c => c.Id == "mass" && c.BaseUnitId == "kilogram");
        cats.Should().Contain(c => c.Id == "temperature" && c.BaseUnitId == "kelvin");
    }

    [Fact]
    public void Filter_by_category_returns_only_matching_units()
    {
        var units = _sut.GetUnits("temperature");
        units.Should().OnlyContain(u => u.CategoryId == "temperature");
        units.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Theory]
    [InlineData("meter")]
    [InlineData("METER")]
    [InlineData("m")]
    public void TryGetUnit_resolves_by_id_or_symbol_case_insensitive(string key)
    {
        _sut.TryGetUnit(key, out var unit).Should().BeTrue();
        unit.Id.Should().Be("meter");
    }

    [Fact]
    public void TryGetUnit_returns_false_for_unknown()
    {
        _sut.TryGetUnit("nope", out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("celcius",    "celsius")]    // common misspelling
    [InlineData("Celcius",    "celsius")]    // case-insensitive alias
    [InlineData("centigrade", "celsius")]    // historical name
    [InlineData("farenheit",  "fahrenheit")]
    [InlineData("FAHRENHIET", "fahrenheit")]
    [InlineData("fahrenheight", "fahrenheit")]
    // British spellings
    [InlineData("metre",       "meter")]
    [InlineData("kilometre",   "kilometer")]
    [InlineData("centimetre",  "centimeter")]
    [InlineData("millimetre",  "millimeter")]
    [InlineData("kilogramme",  "kilogram")]
    [InlineData("gramme",      "gram")]
    [InlineData("milligramme", "milligram")]
    // Plurals
    [InlineData("meters",   "meter")]
    [InlineData("feet",     "foot")]
    [InlineData("inches",   "inch")]
    [InlineData("pounds",   "pound")]
    [InlineData("kilograms","kilogram")]
    // Colloquial
    [InlineData("kilo",     "kilogram")]
    [InlineData("kilos",    "kilogram")]
    [InlineData("lbs",      "pound")]
    [InlineData("ton",      "tonne")]
    [InlineData("tons",     "tonne")]
    public void TryGetUnit_resolves_aliases(string alias, string expectedId)
    {
        _sut.TryGetUnit(alias, out var unit).Should().BeTrue();
        unit.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Base_unit_in_each_category_has_factor_one_and_zero_offset()
    {
        foreach (var c in _sut.GetCategories())
        {
            _sut.TryGetUnit(c.BaseUnitId, out var baseUnit).Should().BeTrue();
            baseUnit.Factor.Should().Be(1d);
            baseUnit.Offset.Should().Be(0d);
        }
    }
}
