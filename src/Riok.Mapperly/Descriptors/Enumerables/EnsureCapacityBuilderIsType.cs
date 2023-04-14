using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables;

public class EnsureCapacityBuilderIsType : IEnsureCapacityBuilder
{
    private const string CollectionName = "collection";
    private const string ReadonlyCollectionName = "readonlyCollection";
    private const string CountMethodName = nameof(ICollection<object>.Count);

    private readonly string _targetAccessor;
    private readonly INamedTypeSymbol _collectionType;
    private readonly INamedTypeSymbol _readonlyCollectionType;

    public EnsureCapacityBuilderIsType(string targetAccessor, INamedTypeSymbol collectionType, INamedTypeSymbol readonlyCollectionType)
    {
        _targetAccessor = targetAccessor;
        _collectionType = collectionType;
        _readonlyCollectionType = readonlyCollectionType;
    }

    public StatementSyntax BuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var targetCount = MemberAccess(target, _targetAccessor);
        var collectionSyntaxType = FullyQualifiedIdentifier(_collectionType);

        var readonlyCollectionSyntaxType = FullyQualifiedIdentifier(_readonlyCollectionType);

        var ifCollection = IfIsTypeEnsureCapacityStatement(CollectionName, collectionSyntaxType!, ctx, target, targetCount);

        var ifReadonlyCollection = IfIsTypeEnsureCapacityStatement(ReadonlyCollectionName, readonlyCollectionSyntaxType!, ctx, target, targetCount);

        return ifCollection.WithElse(ElseClause(ifReadonlyCollection));
    }

    private static IfStatementSyntax IfIsTypeEnsureCapacityStatement(string identifier, TypeSyntax type, TypeMappingBuildContext ctx, ExpressionSyntax target, MemberAccessExpressionSyntax targetCount)
    {
        var asCollection = Identifier(ctx.NameBuilder.New(identifier));
        var collectionCount = MemberAccess(IdentifierName(asCollection), CountMethodName);

        var singleVariableDeclaration = DeclarationPattern(type, SingleVariableDesignation(asCollection));
        var isExpression = IsPatternExpression(ctx.Source, singleVariableDeclaration);

        var ifIsTypeStatement = IfStatement(isExpression, Block(EnsureCapacityHelper.EnsureCapacityStatement(target, collectionCount, targetCount)));
        return ifIsTypeStatement;
    }
}
