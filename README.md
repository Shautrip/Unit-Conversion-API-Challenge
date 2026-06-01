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

## Scope Notes

This challenge version intentionally focuses on API design, conversion logic, and tests.
Docker and CI/CD are future enhancements, not required challenge scope.
