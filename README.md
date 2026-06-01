# Unit Conversion API

ASP.NET Core 9 Web API for converting values across length, mass, and temperature.

## Architecture

```text
UnitConversion.sln
src/
  UnitConversion.Domain/          # Core records + contracts
  UnitConversion.Infrastructure/  # Registry + converter + service
  UnitConversion.Api/             # Controllers + startup wiring
tests/
  UnitConversion.Api.Tests/       # Unit + integration tests
```

## Core Idea

All conversions use one affine model:

base = value * factor + offset

This keeps logic generic and makes adding units a data change instead of code rewrites.

## Run

```powershell
$env:Path += ';C:\Program Files\dotnet'
dotnet restore
dotnet run --project src\UnitConversion.Api
```

Swagger (Development):

- http://localhost:5269/swagger

## Test

```powershell
dotnet test UnitConversion.sln -nologo
dotnet test UnitConversion.sln --collect:"XPlat Code Coverage" -nologo
```

## API Endpoints

1. POST /api/conversions
2. GET /api/conversions?value=100&from=m&to=ft
3. GET /api/categories
4. GET /api/units?category=mass
5. GET /health

## Behavior Highlights

1. Case-insensitive unit resolution.
2. Alias support (for example celcius -> celsius, fahrenheight -> fahrenheit).
3. Canonical unit ids returned in successful responses.
4. RFC 7807 ProblemDetails for errors.

## Design Decisions and Trade-offs

1. Single affine conversion model
  - Decision: use one formula for all supported categories (`base = value * factor + offset`).
  - Trade-off: perfect for linear and affine scales, but non-linear domains would require a separate converter strategy.

2. Registry abstraction for scalability
  - Decision: keep unit data behind `IUnitRegistry` with an in-memory implementation for challenge speed.
  - Trade-off: in-memory is fastest to deliver but not ideal for large operational datasets; swapping to JSON/DB is a planned evolution path.

3. Explicit aliases instead of fuzzy matching
  - Decision: support common typo and regional aliases as explicit mappings.
  - Trade-off: deterministic behavior and low risk of false positives, but alias lists must be curated over time.

4. Result-type error flow
  - Decision: use service outcomes (`ConversionOutcome`) for expected business failures.
  - Trade-off: clearer control flow and testability than exceptions, with slightly more mapping code in the controller.

5. Precision strategy
  - Decision: use `double` for conversion math and round finite outputs to 10 decimals in service.
  - Trade-off: avoids noisy floating-point artifacts in API responses while retaining practical precision; raw scientific precision is intentionally not the API contract.

6. Thin API layer
  - Decision: keep controllers focused on HTTP and move business logic to infrastructure service/converter.
  - Trade-off: more classes up front, but much easier testing and future replacement of internal components.

## Scope Notes

This challenge version intentionally focuses on API design, conversion logic, and tests.
Docker and CI/CD are future enhancements.
