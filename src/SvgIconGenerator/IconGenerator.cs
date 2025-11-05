using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

        // Get compilation options to access MSBuild properties
        IncrementalValueProvider<AnalyzerConfigOptionsProvider> configOptions = context.AnalyzerConfigOptionsProvider;

        // Get all additional files (SVG files should be added via <AdditionalFiles Include="..." />)
        IncrementalValuesProvider<AdditionalText> svgFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase));

        // Find all classes with [GenerateIcons] attribute
        IncrementalValuesProvider<ClassInfo> classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                InjectedSource.GenerateIconsAttribute.FullName,
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => new ClassInfo(ctx.TargetSymbol, ctx.TargetNode.GetLocation(), ctx.Attributes));

        // Combine additional files and class declarations to produce the final output
        IncrementalValueProvider<(ImmutableArray<AdditionalText> Left, ImmutableArray<ClassInfo> Right)> sources = svgFiles.Collect().Combine(classDeclarations.Collect());

        // Combine with configuration and generate icon classes
        IncrementalValueProvider<(AnalyzerConfigOptionsProvider Left, (ImmutableArray<AdditionalText> Left, ImmutableArray<ClassInfo> Right) Right)> combined = configOptions.Combine(sources);
        context.RegisterSourceOutput(combined, static (spc, source) => IconEmitter.Execute(source.Left, source.Right.Left, source.Right.Right, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
            && classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)
            && classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
    }
}
