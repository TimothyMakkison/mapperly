using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables;

public class EnsureCapacityHelper
{
    private const string EnsureCapacityName = "EnsureCapacity";
    private const string CountMethodName = nameof(ICollection<object>.Count);
    private const string LengthMethodName = nameof(Array.Length);
    private const string TryGetNonEnumeratedCountMethodName = "TryGetNonEnumeratedCount";

    public static IEnsureCapacityBuilder? TryCreateEnsureCapacityBuilder(ITypeSymbol sourceType, ITypeSymbol targetType, WellKnownTypes types)
    {
        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x => x.Parameters.Length == 1
            && x.Parameters[0].Type.SpecialType == SpecialType.System_Int32
            && x.ReturnType.SpecialType == SpecialType.System_Int32
            && !x.IsStatic);

        // if EnsureCapacity is not available then return false
        if (capacityMethod is null)
            return null;

        // if target does not have a count then return false
        if (!TryGetNonEnumeratedCount(targetType, types, out var targetSizeProperty))
            return null;

        // if target and source count are known then create a simple EnsureCapacity statement
        if (TryGetNonEnumeratedCount(sourceType, types, out var sourceSizeProperty))
            return new EnsureCapacityBuilderSimple(targetSizeProperty, sourceSizeProperty);

        sourceType.ImplementsGeneric(types.IEnumerableT, out var iEnumerable);

        // does the version of .net have TryGetNonEnumeratedCount
        var nonEnumeratedCountMethod = types.Enumerable.GetMembers(TryGetNonEnumeratedCountMethodName)
        .OfType<IMethodSymbol>()
        .FirstOrDefault(x => x.ReturnType.SpecialType == SpecialType.System_Boolean && x.IsStatic && x.Parameters.Length == 2 && x.IsGenericMethod);

        // if source does not have a count and GetNonEnumeratedCount is available then EnusreCapacity if count is available
        if (nonEnumeratedCountMethod is not null)
        {
            var typedNonEnumeratedCount = nonEnumeratedCountMethod.Construct(iEnumerable!.TypeArguments.ToArray());
            return new EnsureCapacityBuilderNonEnumerated(targetSizeProperty, typedNonEnumeratedCount);
        }

        // if source does not have a count and GetNonEnumeratedCount is not available in the current version then try to get lengths at runtime
        var collectionType = types.ICollectionT.Construct(iEnumerable!.TypeArguments.ToArray());

        var readonlyCollectionType = types.IReadOnlyCollectionT.Construct(iEnumerable.TypeArguments.ToArray());

        return new EnsureCapacityBuilderIsType(targetSizeProperty, collectionType, readonlyCollectionType);
    }

    private static bool TryGetNonEnumeratedCount(ITypeSymbol value, WellKnownTypes types, [NotNullWhen(true)] out string? expression)
    {
        if (value.IsArrayType())
        {
            expression = LengthMethodName;
            return true;
        }
        if (value.HasImplicitInterfaceProperty(types.ICollectionT, CountMethodName) || value.HasImplicitInterfaceProperty(types.IReadOnlyCollectionT, CountMethodName))
        {
            expression = CountMethodName;
            return true;
        }

        expression = null;
        return false;
    }

    public static ExpressionStatementSyntax EnsureCapacityStatement(ExpressionSyntax target, ExpressionSyntax sourceCount, ExpressionSyntax targetCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }
}
