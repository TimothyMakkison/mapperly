using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableExistingTargetMapping : ExistingTargetMapping
{
    private const string LoopItemVariableName = "item";
    private const string EnsureCapacityName = "EnsureCapacity";
    private const string CollectionCountName = nameof(ICollection<object>.Count);

    private readonly ITypeMapping _elementMapping;
    private readonly string _insertMethodName;
    private readonly WellKnownTypes _types;

    public ForEachAddEnumerableExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        string insertMethodName,
        WellKnownTypes types)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _insertMethodName = insertMethodName;
        _types = types;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);
        var convertedSourceItemExpression = _elementMapping.Build(ctx.WithSource(loopItemVariableName));
        var addMethod = MemberAccess(target, _insertMethodName);

        var foreachStatement = ForEachStatement(
                    VarIdentifier,
                    Identifier(loopItemVariableName),
                    ctx.Source,
                    Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression))));

        var ensureCapacityStatement = EnsureCapacityStatement(SourceType, TargetType, ctx.Source, target, _types);

        if (ensureCapacityStatement is null)
        {
            return new StatementSyntax[]
            {
                foreachStatement
            };
        }
        return new StatementSyntax[]
        {
            ensureCapacityStatement,
            foreachStatement
        };
    }

    private static ExpressionStatementSyntax? EnsureCapacityStatement(ITypeSymbol sourceType, ITypeSymbol targetType, ExpressionSyntax source, ExpressionSyntax target, WellKnownTypes types)
    {
        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        if (capacityMethod == null)
        {
            return null;
        }

        if (!TryCreateCollectionCount(sourceType, source, types, out var sourceCount))
        {
            return null;
        }
        if (!TryCreateCollectionCount(targetType, target, types, out var targetCount))
        {
            return null;
        }

        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }

    private static bool TryCreateCollectionCount(ITypeSymbol value, ExpressionSyntax valuesSyntax, WellKnownTypes types, [NotNullWhen(true)] out ExpressionSyntax? expression)
    {
        if (value.ImplementsInterface(types.ICollection, out var inter))
        {
            var identifier = IdentifierName(inter.ToDisplayString());
            var cast = ParenthesizedExpression(CastExpression(identifier, valuesSyntax));
            expression = MemberAccess(cast, CollectionCountName);
            return true;
        }
        if (value.ImplementsInterface(types.ICollectionT, out var genInter))
        {
            var identifier = IdentifierName(genInter.ToDisplayString());
            var cast = ParenthesizedExpression(CastExpression(identifier, valuesSyntax));
            expression = MemberAccess(cast, CollectionCountName);
            return true;
        }

        expression = null;
        return false;
    }
}
