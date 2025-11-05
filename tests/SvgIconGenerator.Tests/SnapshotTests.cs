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

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        using (Assert.EnterMultipleScope())
        {
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
    }

    [Test]
    public void GeneratedCode_MatchesExpectedSnapshot_MultipleIcons()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-settings.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace MyApp.Icons;

            [GenerateIcons]
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

        using (Assert.EnterMultipleScope())
        {
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

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons]
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

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            string? generatedCode = result.GeneratedSources
                .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));

            Assert.That(generatedCode, Is.Not.Null);
            Assert.That(generatedCode, Does.Contain("ArrowDown0_1"));
            Assert.That(generatedCode, Does.Contain("\"arrow-down-0-1\""));
        }
    }

    [Test]
    public void GeneratedCode_ExcludesXmlnsAndClassAttributes()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "test.svg"), TestSvgFiles.IconWithClassAttribute);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        using (Assert.EnterMultipleScope())
        {
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
    }

    [Test]
    public void GeneratedCode_InnerContent_DoesNotContainRedundantXmlns()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "test.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Diagnostics, Is.Empty, "Expected no diagnostics from the generator.");

            string? generatedCode = result.GeneratedSources
                .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));
            Assert.That(generatedCode, Is.Not.Null);

            // Extract the InnerContent section
            string innerContentSection = ExtractInnerContentSection(generatedCode!);

            // The InnerContent should NOT contain xmlns="http://www.w3.org/2000/svg" on child elements
            // This is a redundant attribute that gets added by XML serialization
            Assert.That(innerContentSection, Does.Not.Contain("xmlns=\\\"http://www.w3.org/2000/svg\\\""), "InnerContent should not contain redundant xmlns attributes on child elements.");
        }
    }

    private static string ExtractDefaultAttributesSection(string generatedCode)
    {
        string[] lines = generatedCode.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        int startLine = Array.FindIndex(lines, l => string.Equals(l, "new global::System.Collections.Generic.Dictionary<string, string> {", StringComparison.Ordinal));
        if (startLine == -1) throw new InvalidOperationException("Could not find the start of DefaultAttributes section.");

        int endLine = Array.FindIndex(lines, startLine, l => string.Equals(l, "},", StringComparison.Ordinal));
        if (endLine == -1) throw new InvalidOperationException("Could not find the end of DefaultAttributes section.");

        return string.Join('\n', lines[startLine..endLine]);
    }

    private static string ExtractInnerContentSection(string generatedCode)
    {
        // Split the generated code into lines
        string[] lines = generatedCode.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Find the line that ends with ');', there should be exactly one
        string[] innerContentLines = [..lines.Where(line => line.EndsWith(");", StringComparison.Ordinal))];
        if (innerContentLines.Length < 1) throw new InvalidOperationException("Expected at least one line ending with ');'.");

        // Remove the trailing ');'
        return innerContentLines[0][..^2].Trim();
    }
}
