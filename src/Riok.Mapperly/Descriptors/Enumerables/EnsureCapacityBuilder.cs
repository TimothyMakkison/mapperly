using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables;

public class EnsureCapacityBuilder
{
    private const string EnsureCapacityName = "EnsureCapacity";
    private const string CollectionName = "collection";
    private const string ReadonlyCollectionName = "readonlyCollection";
    private const string CountMethodName = nameof(ICollection<object>.Count);
    private const string LengthMethodName = nameof(Array.Length);

    public readonly string _targetAccessor;
    public readonly string? _sourceAccessor = null;
    private readonly TypeSyntax? _collectionType = null;
    private readonly TypeSyntax? _readonlyCollectionType = null;

    public EnsureCapacityBuilder(string targetAccessor, string? sourceAccessor)
    {
        _targetAccessor = targetAccessor;
        _sourceAccessor = sourceAccessor;
    }

    public EnsureCapacityBuilder(string targetAccessor, TypeSyntax? collectionType, TypeSyntax? readonlyCollectionType)
    {
        _targetAccessor = targetAccessor;
        _collectionType = collectionType;
        _readonlyCollectionType = readonlyCollectionType;
    }

    public static EnsureCapacityBuilder? TryCreateEnsureCapacityBuilder(ITypeSymbol sourceType, ITypeSymbol targetType, WellKnownTypes types)
    {
        var capacityMethod = targetType.GetMembers(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x => x.Parameters.Length == 1
            && x.Parameters[0].Type.SpecialType == SpecialType.System_Int32
            && x.ReturnType.SpecialType == SpecialType.System_Int32
            && !x.IsStatic);

        // if EnsureCapacity is not available then return false.
        if (capacityMethod is null)
        {
            return null;
        }

        // if target does not have a count then return false.
        if (!TryGetNonEnumeratedCount(targetType, types, out var targetSizeProperty))
        {
            return null;
        }

        // if source does not have a count/length then get information to get the non enumerated count at runtime.
        if (!TryGetNonEnumeratedCount(sourceType, types, out var sourceSizeProperty))
        {
            sourceType.ImplementsGeneric(types.IEnumerableT, out var iEnumerable);

            var collectionType = ParseTypeName(types.ICollectionT.Construct(iEnumerable!.TypeArguments.ToArray()).ToDisplayString());

            var readonlyCollectionType = ParseTypeName(types.IReadOnlyCollectionT.Construct(iEnumerable.TypeArguments.ToArray()).ToDisplayString());

            return new EnsureCapacityBuilder(targetSizeProperty, collectionType, readonlyCollectionType);
        }

        return new EnsureCapacityBuilder(targetSizeProperty, sourceSizeProperty);
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

    public StatementSyntax BuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // if source accessor is available then create simple EnsureCapacity call
        if (_sourceAccessor is not null)
        {
            return EnsureCapacityStatement(target, MemberAccess(ctx.Source, _sourceAccessor), MemberAccess(target, _targetAccessor));
        }

        // generate check for runtime type, calling EnsureCapacity if it is a collection/readonlyCollection
        // this is used if source is IEnumerable
        var targetCount = MemberAccess(target, _targetAccessor);

        var ifCollection = IfIsTypeEnsureCapacityStatement(ctx.NameBuilder.New(CollectionName), _collectionType!, ctx, target, targetCount);

        var ifReadonlyCollection = IfIsTypeEnsureCapacityStatement(ctx.NameBuilder.New(ReadonlyCollectionName), _readonlyCollectionType!, ctx, target, targetCount);

        return ifCollection.WithElse(ElseClause(ifReadonlyCollection)); ;
    }

    private static ExpressionStatementSyntax EnsureCapacityStatement(ExpressionSyntax target, ExpressionSyntax sourceCount, ExpressionSyntax targetCount)
    {
        var sumMethod = BinaryExpression(SyntaxKind.AddExpression, sourceCount, targetCount);
        return ExpressionStatement(Invocation(MemberAccess(target, EnsureCapacityName), sumMethod));
    }

    private static IfStatementSyntax IfIsTypeEnsureCapacityStatement(string identifier, TypeSyntax type, TypeMappingBuildContext ctx, ExpressionSyntax target, MemberAccessExpressionSyntax targetCount)
    {
        var asCollection = Identifier(ctx.NameBuilder.New(identifier));
        var collectionCount = MemberAccess(IdentifierName(asCollection), CountMethodName);

        var singleVariableDeclaration = DeclarationPattern(type, SingleVariableDesignation(asCollection));
        var isExpression = IsPatternExpression(ctx.Source, singleVariableDeclaration);

        var ifIsTypeStatement = IfStatement(isExpression, Block(EnsureCapacityStatement(target, collectionCount, targetCount)));
        return ifIsTypeStatement;
    }
}
