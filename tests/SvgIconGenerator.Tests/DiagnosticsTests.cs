using System.Globalization;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SvgIconGenerator.Tests.TestHelpers;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class DiagnosticsTests
{
    private string testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "DiagnosticsTests", Guid.NewGuid().ToString());
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
    public void ICON001_NoSvgFiles_ReportsWarning()
    {
        // Arrange
        string emptyIconsFolder = Path.Combine(testDirectory, "empty-icons");
        Directory.CreateDirectory(emptyIconsFolder);

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
            Assert.That(result.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(result.Diagnostics[0].Id, Is.EqualTo("ICON001"));
            Assert.That(result.Diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
            Assert.That(result.Diagnostics[0].GetMessage(CultureInfo.InvariantCulture), Does.Contain("No SVG files found"));
        }
    }

    [Test]
    public void ICON001_NonExistentFolder_ReportsWarning()
    {
        // Arrange
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
            Assert.That(result.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(result.Diagnostics[0].Id, Is.EqualTo("ICON001"));
            Assert.That(result.Diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        }
    }

    [Test]
    public void ICON001_FolderWithOnlyNonSvgFiles_ReportsWarning()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "readme.txt"), "This is a text file");
        File.WriteAllText(Path.Combine(iconsFolder, "image.png"), "fake png content");

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
            Assert.That(result.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(result.Diagnostics[0].Id, Is.EqualTo("ICON001"));
            Assert.That(result.Diagnostics[0].GetMessage(CultureInfo.InvariantCulture), Does.Contain("No SVG files found"));
        }
    }

    [Test]
    public void ICON002_InvalidSvgFile_ReportsWarning()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "invalid.svg"), TestSvgFiles.InvalidSvg);

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
            Assert.That(result.Diagnostics, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(result.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Id == "ICON002" && d.GetMessage(CultureInfo.InvariantCulture).Contains("invalid.svg", StringComparison.Ordinal)));
        }
    }

    [Test]
    public void ICON002_PartiallyInvalidFiles_GeneratesValidOnesAndReportsErrors()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "valid.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(iconsFolder, "invalid.svg"), TestSvgFiles.InvalidSvg);

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
            Assert.That(result.Diagnostics, Has.Count.EqualTo(1));
            Assert.That(result.Diagnostics[0].Id, Is.EqualTo("ICON002"));

            // Valid icon should still be generated
            string? generatedCode = result.GeneratedSources
                .FirstOrDefault(s => s.Contains("partial class Icons", StringComparison.Ordinal));

            Assert.That(generatedCode, Is.Not.Null);
            Assert.That(generatedCode, Does.Contain("Valid"));
        }
    }

    [Test]
    public void ICON002_CorruptedSvgContent_ReportsWarning()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "corrupted.svg"), "This is not valid XML at all!");

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
            Assert.That(result.Diagnostics, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(result.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Id == "ICON002" && d.GetMessage(CultureInfo.InvariantCulture).Contains("corrupted.svg", StringComparison.Ordinal)));
        }
    }
}
