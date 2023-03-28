using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

public class LinqAddRangeMapping : ExistingTargetMapping
{
    private const string LambdaParamName = "x";
    private const string AddRangeName = "AddRange";

    private readonly ITypeMapping _elementMapping;
    private readonly IMethodSymbol? _selectMethod;

    public LinqAddRangeMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        IMethodSymbol? selectMethod)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _selectMethod = selectMethod;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var lambdaParamName = ctx.NameBuilder.New(LambdaParamName);

        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (_selectMethod != null)
        {
            var sourceMapExpression = _elementMapping.Build(ctx.WithSource(lambdaParamName));
            var convertLambda = SimpleLambdaExpression(Parameter(Identifier(lambdaParamName))).WithExpressionBody(sourceMapExpression);
            mappedSource = StaticInvocation(_selectMethod, ctx.Source, convertLambda);
        }
        else
        {
            mappedSource = _elementMapping.Build(ctx);
        }

        var result = ExpressionStatement(Invocation(MemberAccess(target, AddRangeName), mappedSource));

        return new[] { result };
    }
}
