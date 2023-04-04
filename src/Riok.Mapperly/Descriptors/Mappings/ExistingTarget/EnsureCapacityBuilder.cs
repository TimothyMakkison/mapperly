using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

public class EnsureCapacityInfo
{
    public string? SourceAccessor { get; set; }
    public string TargetAccessor { get; set; } = null!;
    public TypeSyntax? CollectionType { get; set; }
    public TypeSyntax? ReadonlyCollectionType { get; set; }
}

public static class EnsureCapacityBuilder
{
    private const string EnsureCapacityName = "EnsureCapacity";

    public static bool CanEnsureCapacity(ITypeSymbol sourceType, ITypeSymbol targetType, WellKnownTypes types, out EnsureCapacityInfo? state)
    {
        state = null;

        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        // if EnsureCapacity is not available then return false.
        if (capacityMethod is null)
        {
            return false;
        }

        // if target does not have a count then return false.
        if (!TryGetNonEnumeratedCount(targetType, types, out var targetSizeProperty))
        {
            return false;
        }

        // if source does not have a count/length then get information to get the non enumerated count at runtime.
        if (!TryGetNonEnumeratedCount(sourceType, types, out var sourceSizeProperty))
        {
            var collectionType = ParseTypeName(types.ICollectionT.Construct((sourceType as INamedTypeSymbol)!.TypeArguments.ToArray()).ToDisplayString());

            var readonlyCollectionType = ParseTypeName(types.IReadOnlyCollectionT.Construct((sourceType as INamedTypeSymbol)!.TypeArguments.ToArray()).ToDisplayString());

            state = new EnsureCapacityInfo()
            {
                TargetAccessor = targetSizeProperty,
                CollectionType = collectionType,
                ReadonlyCollectionType = readonlyCollectionType
            };
            return true;
        }

        state = new EnsureCapacityInfo()
        {
            SourceAccessor = sourceSizeProperty,
            TargetAccessor = targetSizeProperty
        };

        return true;
    }

    public static bool TryBuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target, EnsureCapacityInfo? state, [NotNullWhen(true)] out StatementSyntax? expression)
    {
        expression = null;
        if (state is null)
        {
            return false;
        }

        // if source accessor is avaailable then create simple EnsureCapacity call
        if (state.SourceAccessor is not null)
        {
            expression = EnsureCapacityStatement(target, MemberAccess(target, state.TargetAccessor), MemberAccess(ctx.Source, state.SourceAccessor));
            return true;
        }

        // generate check for runtime type, calling EnsureCapacity if it is a collection/readonlyCollection
        // this is used if source is IEnumerable
        var targetCount = MemberAccess(target, state.TargetAccessor);

        var ifCollection = IfIsTypeEnsureCapacityStatement("collection", state.CollectionType!, ctx, target, targetCount);

        var ifReadonlyCollection = IfIsTypeEnsureCapacityStatement("readonlyCollection", state.ReadonlyCollectionType!, ctx, target, targetCount);

        expression = ifCollection.WithElse(ElseClause(ifReadonlyCollection)); ;
        return true;
    }

    private static ExpressionStatementSyntax EnsureCapacityStatement(ExpressionSyntax target, ExpressionSyntax targetCount, ExpressionSyntax sourceCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }

    private static IfStatementSyntax IfIsTypeEnsureCapacityStatement(string identifier, TypeSyntax type, TypeMappingBuildContext ctx, ExpressionSyntax source, MemberAccessExpressionSyntax sourceCount)
    {
        var asCollection = Identifier(ctx.NameBuilder.New(identifier));
        var collectionCount = MemberAccess(IdentifierName(asCollection), "Count");

        var singleVariableDeclaration = DeclarationPattern(type, SingleVariableDesignation(asCollection));
        var isExpression = IsPatternExpression(source, singleVariableDeclaration);

        var ifIsTypeStatement = IfStatement(isExpression, Block(EnsureCapacityStatement(source, sourceCount, collectionCount)));
        return ifIsTypeStatement;
    }

    private static bool TryGetNonEnumeratedCount(ITypeSymbol value, WellKnownTypes types, [NotNullWhen(true)] out string? expression)
    {
        if (value.IsArrayType())
        {
            expression = "Length";
            return true;
        }
        if (value.HasImplicitInterfaceProperty(types.ICollectionT, "Count") || value.HasImplicitInterfaceProperty(types.IReadOnlyCollectionT, "Count"))
        {
            expression = "Count";
            return true;
        }

        expression = null;
        return false;
    }
}
