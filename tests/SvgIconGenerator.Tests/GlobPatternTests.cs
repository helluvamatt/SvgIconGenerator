using NUnit.Framework;
using SvgIconGenerator.Tests.TestHelpers;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class GlobPatternTests
{
    private string testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "GlobPatternTests", Guid.NewGuid().ToString());
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
    public void GlobPattern_WithForwardSlashes_MatchesFiles()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        string subfolder = Path.Combine(iconsFolder, "subfolder");
        Directory.CreateDirectory(subfolder);

        File.WriteAllText(Path.Combine(iconsFolder, "home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(subfolder, "settings.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("icons/subfolder/*.svg")]
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
            Assert.That(generatedCode, Does.Contain("Settings"), "Should contain icon from subfolder");
            Assert.That(generatedCode, Does.Not.Contain("Home"), "Should not contain icon from parent folder");
        }
    }

    [Test]
    public void GlobPattern_WithBackslashes_MatchesFiles()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        string subfolder = Path.Combine(iconsFolder, "subfolder");
        Directory.CreateDirectory(subfolder);

        File.WriteAllText(Path.Combine(iconsFolder, "home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(subfolder, "settings.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("icons\\subfolder\\*.svg")]
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
            Assert.That(generatedCode, Does.Contain("Settings"), "Should contain icon from subfolder");
            Assert.That(generatedCode, Does.Not.Contain("Home"), "Should not contain icon from parent folder");
        }
    }

    [Test]
    public void GlobPattern_WithDoubleAsterisk_MatchesNestedFiles()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        string subfolder = Path.Combine(iconsFolder, "subfolder");
        string deepFolder = Path.Combine(subfolder, "deep");
        Directory.CreateDirectory(deepFolder);

        File.WriteAllText(Path.Combine(iconsFolder, "home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(subfolder, "settings.svg"), TestSvgFiles.ComplexIcon);
        File.WriteAllText(Path.Combine(deepFolder, "advanced.svg"), TestSvgFiles.IconWithMultipleAttributes);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("icons/**/*.svg")]
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
            Assert.That(generatedCode, Does.Contain("Home"), "Should contain icon from root");
            Assert.That(generatedCode, Does.Contain("Settings"), "Should contain icon from subfolder");
            Assert.That(generatedCode, Does.Contain("Advanced"), "Should contain icon from deep folder");
        }
    }

    [Test]
    public void GlobPattern_WithWildcardInMiddle_MatchesFiles()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        string folder1 = Path.Combine(iconsFolder, "set1");
        string folder2 = Path.Combine(iconsFolder, "set2");
        Directory.CreateDirectory(folder1);
        Directory.CreateDirectory(folder2);

        File.WriteAllText(Path.Combine(folder1, "icon1.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(folder2, "icon2.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("icons/set*/*.svg")]
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
            Assert.That(generatedCode, Does.Contain("Icon1"), "Should contain icon from set1");
            Assert.That(generatedCode, Does.Contain("Icon2"), "Should contain icon from set2");
        }
    }

    [Test]
    public void GlobPattern_OnlyFileName_MatchesAnyPath()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        string subfolder = Path.Combine(iconsFolder, "subfolder");
        Directory.CreateDirectory(subfolder);

        File.WriteAllText(Path.Combine(iconsFolder, "home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(subfolder, "settings.svg"), TestSvgFiles.ComplexIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("*.svg")]
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
            // With just *.svg, it should match all svg files regardless of path
            Assert.That(generatedCode, Does.Contain("Home"));
            Assert.That(generatedCode, Does.Contain("Settings"));
        }
    }

    [Test]
    public void GlobPattern_AbsolutePath_MatchesFiles()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "home.svg"), TestSvgFiles.SimpleIcon);

        // Use absolute path in glob pattern
        string absolutePattern = Path.Combine(iconsFolder, "*.svg").Replace('\\', '/');

        string sourceCode =
            $$"""
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("{{absolutePattern}}")]
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
            Assert.That(generatedCode, Does.Contain("Home"));
        }
    }
}
