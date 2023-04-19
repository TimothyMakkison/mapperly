using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Roslyn representation of <see cref="Riok.Mapperly.Abstractions.MapDerivedTypeAttribute"/>
/// (use <see cref="ITypeSymbol"/> instead of <see cref="Type"/>).
/// Keep in sync with <see cref="Riok.Mapperly.Abstractions.MapDerivedTypeAttribute"/>
/// </summary>
/// <param name="SourceType">The source type of the derived type mapping.</param>
/// <param name="TargetType">The target type of the derived type mapping.</param>
public record MapDerivedType(ITypeSymbol SourceType, ITypeSymbol TargetType);
