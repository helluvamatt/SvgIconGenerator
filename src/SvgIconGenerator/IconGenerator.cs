using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SvgIconGenerator;

[Generator]
public class IconGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute and DTO sources that will be injected into the consuming project
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(InjectedSource.GenerateIconsAttribute.Hint, InjectedSource.GenerateIconsAttribute.Source);
            ctx.AddSource(InjectedSource.IconDto.Hint, InjectedSource.IconDto.Source);
        });

        IncrementalValueProvider<string?> projectDirectory = context.AnalyzerConfigOptionsProvider
            .Select(static (ctx, _) => ctx.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out string? info) ? info : null);

        // Find all classes with [GenerateIcons] attribute
        IncrementalValuesProvider<ClassInfo> classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                InjectedSource.GenerateIconsAttribute.FullName,
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => new ClassInfo(ctx.TargetSymbol, ctx.TargetNode.GetLocation(), ctx.Attributes));

        // Generate icon classes
        IncrementalValueProvider<(string? Left, ImmutableArray<ClassInfo> Right)> combined = projectDirectory.Combine(classDeclarations.Collect());
        context.RegisterSourceOutput(combined, static (spc, source) => IconEmitter.Execute(source.Left, source.Right, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
            && classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)
            && classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
    }
}
