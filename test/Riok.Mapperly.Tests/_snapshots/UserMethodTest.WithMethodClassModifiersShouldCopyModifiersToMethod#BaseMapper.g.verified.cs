﻿//HintName: BaseMapper.g.cs
// <auto-generated />
#nullable enable
public partial class BaseMapper
{
    public virtual partial global::B AToB(global::A source)
    {
        var target = new global::B();
        target.Value = source.Value.ToString();
        target.Value2 = IntToShort(source.Value2);
        return target;
    }

    public partial short IntToShort(int value)
    {
        return (short)value;
    }
}