using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);

    public static partial B MapCarToDto(A car);

    public class A
    { public IEnumerable<int> Value { get; } }

    public class B
    { public Queue<long> Value { get; set; } }
}
