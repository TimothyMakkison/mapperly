using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

public class EnsureCapacityBuilderNonEnumerated : IEnsureCapacityBuilder
{
    private readonly string _targetAccessor;
    private readonly IMethodSymbol _getNonEnumeratedMethod;

    public EnsureCapacityBuilderNonEnumerated(string targetAccessor, IMethodSymbol getNonEnumeratedMethod)
    {
        _targetAccessor = targetAccessor;
        _getNonEnumeratedMethod = getNonEnumeratedMethod;
    }

    public StatementSyntax BuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var targetCount = MemberAccess(target, _targetAccessor);

        var countIdentifier = Identifier(ctx.NameBuilder.New("sourceCount"));
        var countIdentifierName = IdentifierName(countIdentifier);

        var enumerableArgument = Argument(ctx.Source);

        var outVarArgument = Argument(
                               DeclarationExpression(
                                   VarIdentifier,
                                   SingleVariableDesignation(countIdentifier)))
            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

        var getNonEnumeratedInvocation = StaticInvocation(_getNonEnumeratedMethod, enumerableArgument, outVarArgument);
        return IfStatement(getNonEnumeratedInvocation, Block(EnsureCapacityHelper.EnsureCapacityStatement(target, countIdentifierName, targetCount)));
    }
}
