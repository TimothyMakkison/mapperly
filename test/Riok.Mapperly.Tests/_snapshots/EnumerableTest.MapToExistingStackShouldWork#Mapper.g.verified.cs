//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(System.Collections.Generic.List<A>? source, System.Collections.Generic.Stack<B> target)
    {
        if (source == null)
            return;
        target.EnsureCapacity(source.Count + target.Count);
        foreach (var item in source)
        {
            target.Push(MapToB(item));
        }
    }

    private B MapToB(A source)
    {
        var target = new B();
        target.Value = source.Value;
        return target;
    }
}