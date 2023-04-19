# Derived types and interfaces

Mapperly supports interfaces and base types as mapping sources and targets,
but Mapperly needs to know which derived types exist.
This can be configured with the `MapDerivedTypeAttribute`:

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    [MapDerivedType(typeof(Audi), typeof(AudiDto))]
    [MapDerivedType(typeof(Porsche), typeof(PorscheDto))]
    // highlight-end
    public static partial CarDto MapCar(Car source);
}

abstract class Car {}
class Audi : Car {}
class Porsche : Car {}

abstract class CarDto {}
class AudiDto : CarDto {}
class PorscheDto : CarDto {}
```

Mapperly implements a type switch with an arm for each source type.
All source types provided to the `MapDerivedTypeAttribute`
need to implement or extend the type of the mapping method parameter.
All target types provided to the `MapDerivedTypeAttribute`
need to implement or extend the mapping method return type.
Each source type has to be unique but multiple source types can be mapped to the same target type.
