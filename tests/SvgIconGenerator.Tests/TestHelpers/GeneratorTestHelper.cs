using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
        // Auto-discover SVG files in the icons directory
        string iconsFolder = Path.Combine(projectDirectory, "icons");
        List<TestAdditionalText> additionalFiles = [];

        if (Directory.Exists(iconsFolder))
        {
            string[] svgFiles = Directory.GetFiles(iconsFolder, "*.svg", SearchOption.AllDirectories);
            foreach (string svgFile in svgFiles)
            {
                string content = File.ReadAllText(svgFile);
                additionalFiles.Add(new TestAdditionalText(svgFile, content));
            }
        }

        return RunGenerator(sourceCode, projectDirectory, additionalFiles);
    }

    /// <summary>
    /// Runs the IconGenerator on the provided source code with explicit additional files.
    /// </summary>
    public static GeneratorTestResult RunGenerator(string sourceCode, string projectDirectory, IEnumerable<TestAdditionalText> additionalFiles)
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
            optionsProvider: analyzerConfigOptionsProvider,
            additionalTexts: additionalFiles.ToImmutableArray<AdditionalText>());

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
/// Test implementation of AdditionalText for unit testing.
/// </summary>
public sealed class TestAdditionalText : AdditionalText
{
    private readonly SourceText sourceText;

    public TestAdditionalText(string path, string text)
    {
        Path = path;
        sourceText = SourceText.From(text);
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
}

/// <summary>
/// Result of running a source generator test.
/// </summary>
public record GeneratorTestResult(
    IReadOnlyCollection<string> GeneratedSources,
    IReadOnlyList<Diagnostic> Diagnostics,
    Compilation OutputCompilation,
    GeneratorDriverRunResult RunResult);
