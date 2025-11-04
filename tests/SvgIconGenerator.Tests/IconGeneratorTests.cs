using NUnit.Framework;
using SvgIconGenerator.Tests.TestHelpers;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class IconGeneratorTests
{
    private string testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "IconGeneratorTests", Guid.NewGuid().ToString());
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
    public void Generator_WithValidClass_GeneratesIconProperties()
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
        Assert.That(result.GeneratedSources, Has.Count.GreaterThan(0));

        string generatedCode = string.Join("\n", result.GeneratedSources);
        Assert.That(generatedCode, Does.Contain("partial class Icons"));
        Assert.That(generatedCode, Does.Contain("IconHome"));
        Assert.That(generatedCode, Does.Contain("IconDto"));
    }

    [Test]
    public void Generator_WithMultipleSvgFiles_GeneratesMultipleProperties()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-settings.svg"), TestSvgFiles.ComplexIcon);

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
        string generatedCode = string.Join("\n", result.GeneratedSources);
        Assert.That(generatedCode, Does.Contain("IconHome"));
        Assert.That(generatedCode, Does.Contain("IconSettings"));
    }

    [Test]
    public void Generator_WithNonPartialClass_DoesNotGenerate()
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
              public static class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        List<string> userGeneratedCode = result.GeneratedSources
            .Where(s => s.Contains("partial class Icons", StringComparison.Ordinal))
            .ToList();
        Assert.That(userGeneratedCode, Is.Empty);
    }

    [Test]
    public void Generator_WithNonStaticClass_DoesNotGenerate()
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
              public partial class Icons
              {
              }
              """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        List<string> userGeneratedCode = result.GeneratedSources
            .Where(s => s.Contains("partial class Icons", StringComparison.Ordinal))
            .ToList();
        Assert.That(userGeneratedCode, Is.Empty);
    }

    [Test]
    public void Generator_InjectsAttributeAndDto()
    {
        // Arrange
        const string sourceCode =
            """
            namespace TestNamespace;

            public class Test
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        string generatedCode = string.Join("\n", result.GeneratedSources);
        Assert.That(generatedCode, Does.Contain("GenerateIconsAttribute"));
        Assert.That(generatedCode, Does.Contain("IconDto"));
    }

    [Test]
    public void Generator_WithRelativePath_ResolvesCorrectly()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);

        const string sourceCode =
            """
            using SvgIconGenerator;

            namespace TestNamespace;

            [GenerateIcons("icons")]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        Assert.That(result.Diagnostics, Is.Empty, "Expected no diagnostics from the generator.");
        string generatedCode = string.Join("\n", result.GeneratedSources);
        Assert.That(generatedCode, Does.Contain("IconHome"));
    }

    [Test]
    public void Generator_WithNoAttributes_DoesNotGenerate()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);

        const string sourceCode =
            """
            namespace TestNamespace;

            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        List<string> userGeneratedCode = result.GeneratedSources
            .Where(s => s.Contains("partial class Icons", StringComparison.Ordinal))
            .ToList();
        Assert.That(userGeneratedCode, Is.Empty, "Generator should ignore classes without [GenerateIcons] attribute");
    }

    [Test]
    public void Generator_WithDifferentAttribute_DoesNotGenerate()
    {
        // Arrange
        string iconsFolder = Path.Combine(testDirectory, "icons");
        Directory.CreateDirectory(iconsFolder);
        File.WriteAllText(Path.Combine(iconsFolder, "icon-home.svg"), TestSvgFiles.SimpleIcon);

        const string sourceCode =
            """
            using System;

            namespace TestNamespace;

            [Serializable]
            public static partial class Icons
            {
            }
            """;

        // Act
        GeneratorTestResult result = GeneratorTestHelper.RunGenerator(sourceCode, testDirectory);

        // Assert
        List<string> userGeneratedCode = result.GeneratedSources
            .Where(s => s.Contains("partial class Icons", StringComparison.Ordinal))
            .ToList();
        Assert.That(userGeneratedCode, Is.Empty, "Generator should ignore classes with non-GenerateIcons attributes");
    }
}
