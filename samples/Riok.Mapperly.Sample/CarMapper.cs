using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);
}

[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    [MapDerivedType(typeof(Audi), typeof(AudiDto))]
    [MapDerivedType<Porsche, PorscheDto>()]
    //[MapDerivedType(typeof(Porsche), typeof(PorscheDto))]
    // highlight-end
    public static partial BaseCarDto MapCar(BaseCar source);
}

public abstract class BaseCar { }

public class Audi : BaseCar { }

public class Porsche : BaseCar
{
    public int Value { get; set; }
}

public abstract class BaseCarDto { }

public class AudiDto : BaseCarDto { }

public class PorscheDto : BaseCarDto
{
    public int Value { get; set; }
}
