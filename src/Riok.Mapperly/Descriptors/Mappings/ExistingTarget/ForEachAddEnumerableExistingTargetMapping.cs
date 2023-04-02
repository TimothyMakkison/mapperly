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

    private const string EnsureCapacityName = "EnsureCapacity";

    private static StatementSyntax? EnsureCapacityStatement(ITypeSymbol sourceType, ITypeSymbol targetType, ExpressionSyntax source, ExpressionSyntax target, WellKnownTypes types)
    {
        // If ensure is available
        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        if (capacityMethod == null)
        {
            return null;
        }

        // If dest has does not have non enum exit
        if (!TryGetNonEnumeratedCount(targetType, target, types, out var targetCount))
        {
            return null;
        }

        // If source does not have enum count then create if type then ensure capacity
        if (!TryGetNonEnumeratedCount(sourceType, source, types, out var sourceCount))
        {
            var iden = Identifier("collection");
            var coll = MemberAccess(IdentifierName(iden), "Count");

            return SyntaxFactory.IfStatement(CreateIsPattern(sourceType as INamedTypeSymbol, source, types, iden), ConstrutEnsureCap(target, targetCount, coll));
        }

        // If source has non enume generate fixed constant expression.
        return ConstrutEnsureCap(target, targetCount, sourceCount);
    }

    private static ExpressionStatementSyntax ConstrutEnsureCap(ExpressionSyntax target, ExpressionSyntax targetCount, ExpressionSyntax sourceCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }

    private static bool TryGetNonEnumeratedCount(ITypeSymbol value, ExpressionSyntax valuesSyntax, WellKnownTypes types, [NotNullWhen(true)] out ExpressionSyntax? expression)
    {
        if (value.IsArrayType())
        {
            expression = MemberAccess(valuesSyntax, "Length");
            return true;
        }
        if (value.HasImplicitInterfaceMethod(types.ICollectionT, "Count"))
        {
            expression = MemberAccess(valuesSyntax, "Count");
            return true;
        }

        expression = null;
        return false;
    }

    //private static bool conIf()
    //{

    //}

    private static IsPatternExpressionSyntax CreateIsPattern(INamedTypeSymbol value, ExpressionSyntax valuesSyntax, WellKnownTypes types, SyntaxToken identifier)
    {
        var type = ParseTypeName(types.ICollectionT.Construct(value.TypeArguments.ToArray()).ToDisplayString());
        //var de = SyntaxFactory.VariableDeclaration()

        DeclarationPatternSyntax singleVariableDeclaration = DeclarationPattern(type, SingleVariableDesignation(identifier));

        return IsPatternExpression(valuesSyntax, singleVariableDeclaration);
    }
}
