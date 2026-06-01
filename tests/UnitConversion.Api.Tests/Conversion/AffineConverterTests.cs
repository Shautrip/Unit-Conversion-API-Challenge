using FluentAssertions;
using UnitConversion.Infrastructure.Conversion;
using UnitConversion.Domain;
using Xunit;

namespace UnitConversion.Api.Tests.Conversion;

public class AffineConverterTests
{
    private readonly AffineConverter _sut = new();

    private static readonly Unit Meter      = new("meter",      "m",  "Meter",      "length", 1d);
    private static readonly Unit Kilometer  = new("kilometer",  "km", "Kilometer",  "length", 1_000d);
    private static readonly Unit Foot       = new("foot",       "ft", "Foot",       "length", 0.3048d);
    private static readonly Unit Mile       = new("mile",       "mi", "Mile",       "length", 1_609.344d);
    private static readonly Unit Kilogram   = new("kilogram",   "kg", "Kilogram",   "mass",   1d);
    private static readonly Unit Pound      = new("pound",      "lb", "Pound",      "mass",   0.453_592_37d);
    private static readonly Unit Kelvin     = new("kelvin",     "K",  "Kelvin",     "temperature", 1d, 0d);
    private static readonly Unit Celsius    = new("celsius",    "C",  "Celsius",    "temperature", 1d, 273.15d);
    private static readonly Unit Fahrenheit = new("fahrenheit", "F",  "Fahrenheit", "temperature", 5d / 9d, 459.67d * 5d / 9d);

    [Theory]
    [InlineData(100d, 328.083_989_501_312)]   // m -> ft
    public void Linear_meters_to_feet(double meters, double expected)
    {
        _sut.Convert(meters, Meter, Foot).Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData(0d, 32d)]
    [InlineData(100d, 212d)]
    [InlineData(-40d, -40d)]                  // famous fixed point
    public void Celsius_to_fahrenheit(double c, double f)
    {
        _sut.Convert(c, Celsius, Fahrenheit).Should().BeApproximately(f, 1e-9);
    }

    [Fact]
    public void Absolute_zero_celsius_is_zero_kelvin()
    {
        _sut.Convert(-273.15d, Celsius, Kelvin).Should().BeApproximately(0d, 1e-9);
    }

    [Fact]
    public void Kilogram_to_pound_known_value()
    {
        _sut.Convert(1d, Kilogram, Pound).Should().BeApproximately(2.204_622_621_848_776d, 1e-9);
    }

    [Theory]
    [MemberData(nameof(RoundTripPairs))]
    public void Round_trip_returns_original_value(Unit a, Unit b, double value)
    {
        var there = _sut.Convert(value, a, b);
        var back = _sut.Convert(there, b, a);
        back.Should().BeApproximately(value, 1e-9);
    }

    public static IEnumerable<object[]> RoundTripPairs()
    {
        yield return new object[] { Meter,    Foot,       123.456d };
        yield return new object[] { Meter,    Mile,       42_195d };
        yield return new object[] { Kilometer, Foot,      3.14d };
        yield return new object[] { Kilogram, Pound,      99.9d };
        yield return new object[] { Celsius,  Fahrenheit, 36.6d };
        yield return new object[] { Kelvin,   Fahrenheit, 300d };
    }

    [Fact]
    public void Throws_when_categories_differ()
    {
        var act = () => _sut.Convert(1d, Meter, Celsius);
        act.Should().Throw<ArgumentException>();
    }
}
