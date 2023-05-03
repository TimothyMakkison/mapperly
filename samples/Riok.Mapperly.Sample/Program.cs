using System.Text.Json;
using Riok.Mapperly.Sample;

var car = new Car
{
    Name = "my car",
    NumberOfSeats = 5,
    Color = CarColor.Blue,
    Manufacturer = new Manufacturer(1, "best manufacturer"),
    Tires =
    {
        new Tire { Description = "front left tire" },
        new Tire { Description = "front right tire" },
        new Tire { Description = "back left tire" },
        new Tire { Description = "back right tire" },
    },
};
var carDto = CarMapper.MapCarToDto(car);

Console.WriteLine("Mapped car to car DTO:");
Console.WriteLine(JsonSerializer.Serialize(carDto, new JsonSerializerOptions { WriteIndented = true }));

//var a = "";

//FullNameOf("Heyall");
//new Mine(nameof(Car.Manufacturer.Name));

//Console.WriteLine(FullNameOf(nameof(System.Net.Http.HttpRequestMessage.Content.Headers)));

//Console.WriteLine(ChooseProperty("Hello","nameof(Hello World)"));

//static string ChooseProperty(string value, string? expression)
//{
//    if (expression is null)
//        return value;
//    var NameOfFunction = "nameof(";
//    var start = expression.IndexOf(NameOfFunction);
//    if (start is -1 or > 0) return value;
//    var end = expression.EndsWith(")");
//    if (!end) return value;

//    var stripped = expression.Substring(7, expression.Length - 1 - NameOfFunction.Length);

//    return stripped;
//}

//[Target(nameof(HttpRequestMessage.Content.Headers))]
//static string FullNameOf(string value, [CallerArgumentExpression(nameof(value))] string fullpath = default!)
//{
//    var attr = MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(TargetAttribute), true)[0];
//    var c = (TargetAttribute)attr;
//    Console.WriteLine("Custom");
//    Console.WriteLine(c.FullPath);

//    // Do some validation here..
//    Console.WriteLine(value);
//    Console.WriteLine(fullpath);
//    //string outputString = fullpath.Substring(fullpath.IndexOf("(") + 1, fullpath.IndexOf(")") - fullpath.IndexOf("(") - 1);

//    return fullpath;
//}

//record Mine
//{
//    public Mine(string path, [CallerArgumentExpression(nameof(path))] string fullpath = default!)
//    {
//        Console.WriteLine(fullpath);
//        Console.WriteLine(path);
//    }
//}

//public class MyClass
//{
//    public object Value { get; set; }
//}

//[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
//public class TargetAttribute : Attribute
//{
//    public TargetAttribute(string target, [CallerArgumentExpression(nameof(target))] string fullpath = default!)
//    {
//        // Do some validation here...

//        // Strip "nameof(" and ")"
//        FullPath = fullpath.Substring(fullpath.IndexOf("(") + 1, fullpath.IndexOf(")") - fullpath.IndexOf("(") - 1);
//    }

//    // Use for reflection or source generator
//    public string FullPath { get; set; }
//}
