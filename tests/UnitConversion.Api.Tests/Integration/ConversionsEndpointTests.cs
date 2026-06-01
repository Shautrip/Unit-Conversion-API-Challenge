using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversion.Domain.Contracts;
using Xunit;

namespace UnitConversion.Api.Tests.Integration;

public class ConversionsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ConversionsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_converts_celsius_to_fahrenheit()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions",
            new ConversionRequest { Value = 100, FromUnit = "C", ToUnit = "F" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        body!.Result.Should().BeApproximately(212d, 1e-9);
        body.Category.Should().Be("temperature");
    }

    [Fact]
    public async Task Get_variant_works()
    {
        var response = await _client.GetAsync("/api/conversions?value=100&from=m&to=ft");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        body!.Result.Should().BeApproximately(328.083_989_501_312d, 1e-6);
    }

    [Fact]
    public async Task Post_resolves_aliases_and_returns_canonical_unit_ids()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions",
            new ConversionRequest { Value = 39.8d, FromUnit = "celcius", ToUnit = "fahrenheight" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        body!.FromUnit.Should().Be("celsius");
        body.ToUnit.Should().Be("fahrenheit");
        body.Result.Should().Be(103.64d);
    }

    [Fact]
    public async Task Unknown_unit_returns_404_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions",
            new ConversionRequest { Value = 1, FromUnit = "banana", ToUnit = "m" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Unknown unit");
    }

    [Fact]
    public async Task Category_mismatch_returns_400_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions",
            new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "C" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Incompatible units");
    }

    [Fact]
    public async Task Missing_body_field_returns_400_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions",
            new { value = 1, fromUnit = "m" }); // toUnit missing

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Categories_endpoint_lists_supported_categories()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        categories.Should().NotBeNull();
        categories!.Select(c => c.Id).Should().Contain(["length", "mass", "temperature"]);
    }

    [Fact]
    public async Task Units_endpoint_filters_by_category()
    {
        var units = await _client.GetFromJsonAsync<List<UnitDto>>("/api/units?category=mass");
        units.Should().NotBeNull();
        units!.Should().OnlyContain(u => u.Category == "mass");
    }

    [Fact]
    public async Task Health_endpoint_returns_ok()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
