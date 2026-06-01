using Microsoft.AspNetCore.Mvc;
using UnitConversion.Infrastructure.Conversion;
using UnitConversion.Infrastructure.Registry;
using UnitConversion.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var pd = new ValidationProblemDetails(ctx.ModelState)
            {
                Type = "https://httpstatuses.io/400",
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Instance = ctx.HttpContext.Request.Path,
            };
            pd.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(pd) { ContentTypes = { "application/problem+json" } };
        };
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "Unit Conversion API",
        Version = "v1",
        Description = "Convert numerical values between units of measurement.",
    });
});

builder.Services.AddSingleton<IUnitRegistry, InMemoryUnitRegistry>();
builder.Services.AddSingleton<IUnitConverter, AffineConverter>();
builder.Services.AddScoped<IConversionService, ConversionService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Unit Conversion API v1");
        o.RoutePrefix = "swagger";
    });
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health")
   .WithTags("Health");

app.MapControllers();

app.Run();

/// <summary>Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can boot the app in tests.</summary>
public partial class Program;
