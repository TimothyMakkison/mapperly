using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapperIgnoreSource("Manufacturer.Ident")]
    //[MapperIgnoreTarget("Producer.Ident")]
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);

    [MapProperty(nameof(Source.Manufacturer), nameof(Target.Producer))] // Map property with a different name in the target type
    public static partial Target Map(Source car);
}
