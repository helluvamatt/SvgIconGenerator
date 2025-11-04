using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace SvgIconGenerator.Tests.TestHelpers;

/// <summary>
/// Helper class for testing source generators.
/// </summary>
public static class GeneratorTestHelper
{
    /// <summary>
    /// Runs the IconGenerator on the provided source code and returns the generated output and diagnostics.
    /// </summary>
    public static GeneratorTestResult RunGenerator(string sourceCode, string projectDirectory)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        List<MetadataReference> references = [
            ..AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
        ];

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Add the project directory as an analyzer config option
        TestAnalyzerConfigOptionsProvider analyzerConfigOptionsProvider = new(projectDirectory);

        IconGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            optionsProvider: analyzerConfigOptionsProvider);

        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        return new GeneratorTestResult(
            [.. runResult.GeneratedTrees.Select(t => t.ToString())],
            [.. diagnostics],
            outputCompilation,
            runResult);
    }

    private sealed class TestAnalyzerConfigOptionsProvider(string projectDirectory) : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions options = new(projectDirectory);

        public override AnalyzerConfigOptions GlobalOptions => options;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => options;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => options;
    }

    private sealed class TestAnalyzerConfigOptions(string projectDirectory) : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> options = new()
        {
            ["build_property.MSBuildProjectDirectory"] = projectDirectory
        };

        public override bool TryGetValue(string key, out string value)
        {
            return options.TryGetValue(key, out value!);
        }
    }
}

/// <summary>
/// Result of running a source generator test.
/// </summary>
public record GeneratorTestResult(
    IReadOnlyCollection<string> GeneratedSources,
    IReadOnlyList<Diagnostic> Diagnostics,
    Compilation OutputCompilation,
    GeneratorDriverRunResult RunResult);
