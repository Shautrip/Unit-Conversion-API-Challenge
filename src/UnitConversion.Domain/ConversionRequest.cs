using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Domain.Contracts;

/// <summary>Body for <c>POST /api/conversions</c>.</summary>
public sealed class ConversionRequest
{
    /// <summary>Numerical value to convert.</summary>
    [Required]
    public double Value { get; init; }

    /// <summary>Source unit id or symbol (case-insensitive), e.g. <c>m</c>, <c>celsius</c>.</summary>
    [Required, StringLength(32, MinimumLength = 1)]
    public string FromUnit { get; init; } = string.Empty;

    /// <summary>Target unit id or symbol (case-insensitive).</summary>
    [Required, StringLength(32, MinimumLength = 1)]
    public string ToUnit { get; init; } = string.Empty;
}
