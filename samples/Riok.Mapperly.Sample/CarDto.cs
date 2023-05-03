namespace Riok.Mapperly.Sample;

public class CarDto
{
    public string Name { get; set; } = string.Empty;

    public int NumberOfSeats { get; set; }

    public CarColorDto Color { get; set; }

    public ProducerDto? Producer { get; set; }

    public List<TireDto>? Tires { get; set; }
}

// Intentionally use different numeric values for demonstration purposes
public enum CarColorDto
{
    Yellow = 1,
    Green = 2,
    Black = 3,
    Blue = 4,
}

// The manufacturer, but named differently for demonstration purposes
public class ProducerDto
{
    public int Id { get; set; }
    public string UniqueId { get; set; }

    public string Name { get; set; }
}

public class TireDto
{
    public string Description { get; set; } = string.Empty;
}
