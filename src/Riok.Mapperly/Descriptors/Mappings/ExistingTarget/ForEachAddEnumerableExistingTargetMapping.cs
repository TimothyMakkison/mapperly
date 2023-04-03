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

    public record Ens
    {
        public string? SourceAcc { get; set; }
        public string TargetAcc { get; set; } = null!;
        public TypeSyntax? CollectionType { get; set; }
        public TypeSyntax? ReadonlyCollectionType { get; set; }
    }

    private static bool EnsureCapacityStatement(ITypeSymbol sourceType, ITypeSymbol targetType, ExpressionSyntax source, ExpressionSyntax target, WellKnownTypes types, out Ens? state)
    {
        state = null;

        // If ensure capacity is not available then return false.
        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        if (capacityMethod == null)
        {
            return false;
        }

        // If dest has does not have non enum exit
        if (!TryGetNonEnumeratedCount(targetType, types, out var targetSizeProperty))
        {
            return false;
        }

        // If source does not have enum count then create if type then ensure capacity
        if (!TryGetNonEnumeratedCount(sourceType, types, out var sourceSizeProperty))
        {
            var type = ParseTypeName(types.ICollectionT.Construct((sourceType as INamedTypeSymbol)!.TypeArguments.ToArray()).ToDisplayString());

            state = new Ens() { SourceAcc = null, TargetAcc = targetSizeProperty, CollectionType = type };
            return true;
        }

        state = new Ens() { SourceAcc = sourceSizeProperty, TargetAcc = targetSizeProperty };

        return true;
    }

    private static StatementSyntax BuildEnsure(Ens state, TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        if (state.SourceAcc is not null)
        {
            return EnsureCapacityStatement(target, MemberAccess(target, state.TargetAcc), MemberAccess(ctx.Source, state.SourceAcc));
        }

        var asCollection = Identifier(ctx.NameBuilder.New("collection"));
        var collectionCount = MemberAccess(IdentifierName(asCollection), "Count");
        var asReadonlyCollection = Identifier(ctx.NameBuilder.New("readonlyCollection"));
        var readonlyCollectionCount = MemberAccess(IdentifierName(asCollection), "Count");

        var targetCount = MemberAccess(target, state.TargetAcc);

        var ifCollection = IfStatement(CreateIsPattern(state.CollectionType!, ctx.Source, asCollection),
            EnsureCapacityStatement(target, targetCount, collectionCount));

        var ifReadonlyCollection = IfStatement(CreateIsPattern(state.ReadonlyCollectionType!, ctx.Source, asReadonlyCollection),
            EnsureCapacityStatement(target, targetCount, readonlyCollectionCount));

        return ifCollection.WithElse(ElseClause(Token(SyntaxKind.ElseKeyword), ifReadonlyCollection));
    }

    private static ExpressionStatementSyntax EnsureCapacityStatement(ExpressionSyntax target, ExpressionSyntax targetCount, ExpressionSyntax sourceCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }

    private static bool TryGetNonEnumeratedCount(ITypeSymbol value, WellKnownTypes types, [NotNullWhen(true)] out string? expression)
    {
        if (value.IsArrayType())
        {
            expression = "Length";
            return true;
        }
        if (value.HasImplicitInterfaceMethod(types.ICollectionT, "Count") ||
            value.HasImplicitInterfaceMethod(types.IReadOnlyCollectionT, "Count") || value.HasImplicitInterfaceMethod(types.IReadOnlyDictionaryT, "Count"))
        {
            expression = "Count";
            return true;
        }

        expression = null;
        return false;
    }

    private static IsPatternExpressionSyntax CreateIsPattern(TypeSyntax type, ExpressionSyntax valuesSyntax, SyntaxToken identifier)
    {
        var singleVariableDeclaration = DeclarationPattern(type, SingleVariableDesignation(identifier));

        return IsPatternExpression(valuesSyntax, singleVariableDeclaration);
    }
}
