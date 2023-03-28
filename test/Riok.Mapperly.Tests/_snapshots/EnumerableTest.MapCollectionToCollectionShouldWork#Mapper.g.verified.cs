//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(System.Collections.Generic.Stack<A> source, System.Collections.Generic.Queue<B> target)
    {
        target.EnsureCapacity(((System.Collections.ICollection)source).Count + ((System.Collections.ICollection)target).Count);
        foreach (var item in source)
        {
            target.Enqueue(MapToB(item));
        }
    }

    private B MapToB(A source)
    {
        var target = new B();
        target.Value = source.Value;
        return target;
    }
}