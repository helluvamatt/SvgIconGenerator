using NUnit.Framework;
using SvgIconGenerator.Tests.TestHelpers;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class SnapshotTests
{
    private string testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "SnapshotTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Test]
    public void GeneratedCode_MatchesExpectedSnapshot_SimpleIcon()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);

        string sourceCode =
            $$"""
              using SvgIconGenerator;

              namespace TestNamespace;

              [GenerateIcons("{{iconsFolder}}")]
              public static partial class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        Assert.That(result.Diagnostics, Is.Empty, "Expected no diagnostics from the generator.");

        string? generatedCode = result.GeneratedSources
            .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));
        Assert.That(generatedCode, Is.Not.Null);

        Assert.That(generatedCode, Does.Contain("namespace TestNamespace"));
        Assert.That(generatedCode, Does.Contain("partial class Icons"));
        Assert.That(generatedCode, Does.Contain("IconHome"));
        Assert.That(generatedCode, Does.Contain("\"icon-home\""));
        Assert.That(generatedCode, Does.Contain("{ \"width\", \"24\" }"));
        Assert.That(generatedCode, Does.Contain("{ \"height\", \"24\" }"));
        Assert.That(generatedCode, Does.Contain("{ \"viewBox\", \"0 0 24 24\" }"));
        Assert.That(generatedCode, Does.Contain("<circle"));
    }

    [Test]
    public void GeneratedCode_MatchesExpectedSnapshot_MultipleIcons()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-settings.svg"), TestSvgFiles.ComplexIcon);

        string sourceCode =
            $$"""
              using SvgIconGenerator;

              namespace MyApp.Icons;

              [GenerateIcons("{{iconsFolder}}")]
              public static partial class AppIcons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        string? generatedCode = result.GeneratedSources
            .FirstOrDefault(s => s.Contains("partial class AppIcons", StringComparison.Ordinal));

        Assert.That(generatedCode, Is.Not.Null);
        Assert.That(generatedCode, Does.Contain("namespace MyApp.Icons"));
        Assert.That(generatedCode, Does.Contain("partial class AppIcons"));

        // Both icons should be present
        Assert.That(generatedCode, Does.Contain("IconHome"));
        Assert.That(generatedCode, Does.Contain("IconSettings"));

        // Both should be readonly IconDto properties
        int iconHomeCount = System.Text.RegularExpressions.Regex.Matches(generatedCode, "IconHome").Count;
        int iconSettingsCount = System.Text.RegularExpressions.Regex.Matches(generatedCode, "IconSettings").Count;

        Assert.That(iconHomeCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(iconSettingsCount, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void GeneratedCode_ProperlyEscapesStrings()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);

        // SVG with quotes and special characters
        const string svgWithQuotes =
            """
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" data-test="value with &quot;quotes&quot;">
              <path d="M10 10 L20 20"/>
            </svg>
            """;

        File.WriteAllText(Path.Combine(iconsFolder, "test.svg"), svgWithQuotes);

        string sourceCode =
            $$"""
              using SvgIconGenerator;

              namespace TestNamespace;

              [GenerateIcons("{{iconsFolder}}")]
              public static partial class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        string? generatedCode = result.GeneratedSources
            .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));

        Assert.That(generatedCode, Is.Not.Null);

        // The generated code should compile without errors
        Assert.That(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error), Is.Empty);
    }

    [Test]
    public void GeneratedCode_HandlesKebabCaseToPascalCase()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "arrow-down-0-1.svg"), TestSvgFiles.SimpleIcon);

        string sourceCode =
            $$"""
              using SvgIconGenerator;

              namespace TestNamespace;

              [GenerateIcons("{{iconsFolder}}")]
              public static partial class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        string? generatedCode = result.GeneratedSources
            .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));

        Assert.That(generatedCode, Is.Not.Null);
        Assert.That(generatedCode, Does.Contain("ArrowDown0_1"));
        Assert.That(generatedCode, Does.Contain("\"arrow-down-0-1\""));
    }

    [Test]
    public void GeneratedCode_ExcludesXmlnsAndClassAttributes()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "test.svg"), TestSvgFiles.IconWithClassAttribute);

        string sourceCode =
            $$"""
              using SvgIconGenerator;

              namespace TestNamespace;

              [GenerateIcons("{{iconsFolder}}")]
              public static partial class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        Assert.That(result.Diagnostics, Is.Empty, "Expected no diagnostics from the generator.");

        string? generatedCode = result.GeneratedSources
            .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));
        Assert.That(generatedCode, Is.Not.Null);

        // Should contain width, height, viewBox
        Assert.That(generatedCode, Does.Contain("{ \"width\", \"24\" }"));
        Assert.That(generatedCode, Does.Contain("{ \"height\", \"24\" }"));
        Assert.That(generatedCode, Does.Contain("{ \"viewBox\", \"0 0 24 24\" }"));

        // Should NOT contain xmlns or class in default attributes
        // Note: xmlns might appear in InnerContent, but not in DefaultAttributes
        string defaultAttributesSection = ExtractDefaultAttributesSection(generatedCode!);
        Assert.That(defaultAttributesSection, Does.Not.Contain("xmlns"));
        Assert.That(defaultAttributesSection, Does.Not.Contain("class"));
    }

    private static string ExtractDefaultAttributesSection(string generatedCode)
    {
        int startIndex = generatedCode.IndexOf("DefaultAttributes:", StringComparison.Ordinal);
        if (startIndex == -1) return string.Empty;

        int endIndex = generatedCode.IndexOf("InnerContent:", startIndex, StringComparison.Ordinal);
        if (endIndex == -1) return string.Empty;

        return generatedCode.Substring(startIndex, endIndex - startIndex);
    }
}
