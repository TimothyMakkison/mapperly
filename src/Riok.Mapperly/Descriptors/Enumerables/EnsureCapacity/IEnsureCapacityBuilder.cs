﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

public interface IEnsureCapacityBuilder
{
    StatementSyntax BuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
