using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UnitConversion.Domain.Contracts;
using UnitConversion.Infrastructure.Conversion;
using UnitConversion.Infrastructure.Registry;
using UnitConversion.Infrastructure.Services;
using Xunit;

namespace UnitConversion.Api.Tests.Services;

public class ConversionServiceTests
{
    private static ConversionService NewService() => new(
        new InMemoryUnitRegistry(),
        new AffineConverter(),
        NullLogger<ConversionService>.Instance);

    [Fact]
    public void Success_returns_expected_response()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 100, FromUnit = "C", ToUnit = "F" });

        outcome.IsSuccess.Should().BeTrue();
        outcome.Response!.Result.Should().BeApproximately(212d, 1e-9);
        outcome.Response.Category.Should().Be("temperature");
    }

    [Fact]
    public void Resolves_units_by_id_case_insensitively()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 1, FromUnit = "KILOGRAM", ToUnit = "pound" });

        outcome.IsSuccess.Should().BeTrue();
        outcome.Response!.Result.Should().BeApproximately(2.204_622_621_848_776d, 1e-9);
    }

    [Fact]
    public void Resolves_aliases_and_returns_canonical_ids()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 39.8d, FromUnit = "celcius", ToUnit = "fahrenheight" });

        outcome.IsSuccess.Should().BeTrue();
        outcome.Response!.FromUnit.Should().Be("celsius");
        outcome.Response.ToUnit.Should().Be("fahrenheit");
        outcome.Response.Result.Should().Be(103.64d);
    }

    [Theory]
    [InlineData(0d, 32d)]      // raw double would be 31.999999999999986
    [InlineData(100d, 212d)]
    [InlineData(-40d, -40d)]
    public void Result_is_rounded_to_suppress_floating_point_noise(double celsius, double expectedFahrenheit)
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = celsius, FromUnit = "C", ToUnit = "F" });

        outcome.IsSuccess.Should().BeTrue();
        outcome.Response!.Result.Should().Be(expectedFahrenheit);
    }

    [Fact]
    public void Positive_infinity_is_preserved()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = double.PositiveInfinity, FromUnit = "C", ToUnit = "F" });

        outcome.IsSuccess.Should().BeTrue();
        outcome.Response!.Result.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void NaN_is_preserved()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = double.NaN, FromUnit = "C", ToUnit = "F" });

        outcome.IsSuccess.Should().BeTrue();
        double.IsNaN(outcome.Response!.Result).Should().BeTrue();
    }

    [Fact]
    public void Unknown_from_unit_returns_failure()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 1, FromUnit = "banana", ToUnit = "m" });

        outcome.IsSuccess.Should().BeFalse();
        outcome.Error.Should().Be(ConversionError.UnknownFromUnit);
    }

    [Fact]
    public void Unknown_to_unit_returns_failure()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "banana" });

        outcome.IsSuccess.Should().BeFalse();
        outcome.Error.Should().Be(ConversionError.UnknownToUnit);
    }

    [Fact]
    public void Cross_category_returns_failure()
    {
        var outcome = NewService().Convert(new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "C" });

        outcome.IsSuccess.Should().BeFalse();
        outcome.Error.Should().Be(ConversionError.CategoryMismatch);
    }
}
