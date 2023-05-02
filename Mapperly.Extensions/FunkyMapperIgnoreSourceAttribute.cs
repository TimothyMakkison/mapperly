using System.Runtime.CompilerServices;
using Riok.Mapperly.Abstractions;

namespace Mapperly.Extensions;

public class FunkyMapperIgnoreSourceAttribute : MapperIgnoreSourceAttribute
{
    private FunkyMapperIgnoreSourceAttribute(string source)
        : base(source) { }

    public FunkyMapperIgnoreSourceAttribute(string source, [CallerArgumentExpression(nameof(source))] string fullpath = default!)
        : base(string.Join(".", fullpath.Split(".").Skip(1))) { }
}
