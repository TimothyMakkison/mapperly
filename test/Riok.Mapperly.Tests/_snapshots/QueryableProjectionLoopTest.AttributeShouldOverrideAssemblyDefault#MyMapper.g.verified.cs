﻿//HintName: MyMapper.g.cs
// <auto-generated />
#nullable enable
public partial class MyMapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    partial global::System.Linq.IQueryable<global::B> Map(global::System.Linq.IQueryable<global::A> src)
    {
#nullable disable
        return System.Linq.Queryable.Select(src, x => new global::B()
        {
            Parent = x.Parent != null ? new global::B()
            {
                Parent = x.Parent.Parent != null ? new global::B()
                {
                    Parent = x.Parent.Parent.Parent != null ? new global::B()
                    {
                        Parent = x.Parent.Parent.Parent.Parent != null ? new global::B()
                        {
                            Parent = x.Parent.Parent.Parent.Parent.Parent != null ? default : default,
                        } : default,
                    } : default,
                } : default,
            } : default,
        });
#nullable enable
    }
}