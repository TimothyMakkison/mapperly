using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    private readonly ITypeMapping _elementMapping;
    private readonly string _insertMethodName;

    public ForEachAddEnumerableExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        string insertMethodName)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _insertMethodName = insertMethodName;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);
        var convertedSourceItemExpression = _elementMapping.Build(ctx.WithSource(loopItemVariableName));
        var addMethod = MemberAccess(target, _insertMethodName);

        return new StatementSyntax[]
        {
            ForEachStatement(
                VarIdentifier,
                Identifier(loopItemVariableName),
                ctx.Source,
                Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression))))
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
