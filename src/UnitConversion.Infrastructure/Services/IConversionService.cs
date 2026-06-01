using UnitConversion.Domain.Contracts;

namespace UnitConversion.Infrastructure.Services;

public interface IConversionService
{
    ConversionOutcome Convert(ConversionRequest request);
}
