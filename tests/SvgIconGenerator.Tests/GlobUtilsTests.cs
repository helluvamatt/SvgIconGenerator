using NUnit.Framework;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class GlobUtilsTests
{
    #region Basic Wildcard Tests

    [Test]
    public void MatchesGlobPattern_ExactMatch_ReturnsTrue()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "icons/home.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_NoMatch_ReturnsFalse()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "icons/settings.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_SingleAsterisk_MatchesFilename()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_SingleAsteriskAtEnd_MatchesExtension()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "icons/home.*";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_SingleAsteriskInMiddle_MatchesPartialName()
    {
        // Arrange
        const string path = "icons/icon-home-large.svg";
        const string pattern = "icons/icon-*-large.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Double Asterisk Tests

    [Test]
    public void MatchesGlobPattern_DoubleAsterisk_MatchesNestedPaths()
    {
        // Arrange
        const string path = "icons/subfolder/deep/home.svg";
        const string pattern = "icons/**/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_DoubleAsterisk_MatchesZeroSegments()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "icons/**/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_DoubleAsteriskAtEnd_MatchesEverything()
    {
        // Arrange
        const string path = "icons/subfolder/deep/nested/home.svg";
        const string pattern = "icons/**";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_DoubleAsteriskInMiddle_MatchesMultipleSegments()
    {
        // Arrange
        const string path = "src/assets/icons/subfolder/home.svg";
        const string pattern = "src/**/home.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Filename-Only Pattern Tests

    [Test]
    public void MatchesGlobPattern_FilenameOnly_MatchesAnyPath()
    {
        // Arrange
        const string path = "icons/subfolder/home.svg";
        const string pattern = "home.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_FilenameOnlyWithWildcard_MatchesAnyPath()
    {
        // Arrange
        const string path = "icons/subfolder/home.svg";
        const string pattern = "*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_FilenameOnlyNoMatch_ReturnsFalse()
    {
        // Arrange
        const string path = "icons/subfolder/home.svg";
        const string pattern = "settings.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Path Normalization Tests

    [Test]
    public void MatchesGlobPattern_BackslashesInPath_NormalizesAndMatches()
    {
        // Arrange
        const string path = "icons\\subfolder\\home.svg";
        const string pattern = "icons/subfolder/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_BackslashesInPattern_NormalizesAndMatches()
    {
        // Arrange
        const string path = "icons/subfolder/home.svg";
        const string pattern = "icons\\subfolder\\*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_MixedSlashes_NormalizesAndMatches()
    {
        // Arrange
        const string path = "icons\\subfolder/home.svg";
        const string pattern = "icons/subfolder\\*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Case Sensitivity Tests

    [Test]
    public void MatchesGlobPattern_CaseInsensitive_Matches()
    {
        // Arrange
        const string path = "Icons/Home.SVG";
        const string pattern = "icons/home.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_CaseInsensitiveWithWildcard_Matches()
    {
        // Arrange
        const string path = "Icons/Home.SVG";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void MatchesGlobPattern_EmptyPattern_NoMatch()
    {
        // Arrange
        const string path = "icons/home.svg";
        const string pattern = "";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_OnlyAsterisk_MatchesFilename()
    {
        // Arrange
        const string path = "home.svg";
        const string pattern = "*";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_OnlyDoubleAsterisk_MatchesAnything()
    {
        // Arrange
        const string path = "icons/subfolder/deep/home.svg";
        const string pattern = "**";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_PathWithoutDirectory_MatchesPattern()
    {
        // Arrange
        const string path = "home.svg";
        const string pattern = "*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Complex Pattern Tests

    [Test]
    public void MatchesGlobPattern_MultipleWildcards_Matches()
    {
        // Arrange
        const string path = "icons/category-home/icon-large.svg";
        const string pattern = "icons/*-*/*-*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_WildcardWithPrefix_Matches()
    {
        // Arrange
        const string path = "icons/icon-home.svg";
        const string pattern = "icons/icon-*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_WildcardWithSuffix_Matches()
    {
        // Arrange
        const string path = "icons/home-icon.svg";
        const string pattern = "icons/*-icon.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_MultipleDirectoryWildcards_Matches()
    {
        // Arrange
        const string path = "src/assets/icons/home.svg";
        const string pattern = "*/*/icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_DoubleAsteriskBetweenSegments_Matches()
    {
        // Arrange
        const string path = "src/components/ui/icons/home.svg";
        const string pattern = "src/**/icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Negative Tests

    [Test]
    public void MatchesGlobPattern_WrongExtension_NoMatch()
    {
        // Arrange
        const string path = "icons/home.png";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_WrongDirectory_NoMatch()
    {
        // Arrange
        const string path = "images/home.svg";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_TooManySegments_NoMatch()
    {
        // Arrange
        const string path = "icons/subfolder/home.svg";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_TooFewSegments_NoMatch()
    {
        // Arrange
        const string path = "home.svg";
        const string pattern = "icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_PrefixMismatch_NoMatch()
    {
        // Arrange
        const string path = "icons/home-icon.svg";
        const string pattern = "icons/icon-*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesGlobPattern_SuffixMismatch_NoMatch()
    {
        // Arrange
        const string path = "icons/icon-home.svg";
        const string pattern = "icons/*-button.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Real-World Scenarios

    [Test]
    public void MatchesGlobPattern_NodeModulesPattern_Matches()
    {
        // Arrange
        const string path = "node_modules/lucide-static/icons/home.svg";
        const string pattern = "node_modules/lucide-static/icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_SharedAssetsPattern_Matches()
    {
        // Arrange
        const string path = "shared/assets/icons/home.svg";
        const string pattern = "shared/**/icons/*.svg";

        // Act
        bool result = GlobUtils.MatchesGlobPattern(path, pattern);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesGlobPattern_MultipleIconSets_Matches()
    {
        // Arrange
        string[] paths =
        [
            "icons/set1/home.svg",
            "icons/set2/settings.svg",
            "icons/set3/user.svg"
        ];
        const string pattern = "icons/set*/*.svg";

        // Act & Assert
        foreach (string path in paths)
        {
            Assert.That(GlobUtils.MatchesGlobPattern(path, pattern), Is.True);
        }
    }

    [Test]
    public void MatchesGlobPattern_AllSvgFiles_Matches()
    {
        // Arrange
        string[] paths =
        [
            "icons/home.svg",
            "images/logo.svg",
            "assets/icon.svg"
        ];
        const string pattern = "*.svg";

        // Act & Assert
        foreach (string path in paths)
        {
            Assert.That(GlobUtils.MatchesGlobPattern(path, pattern), Is.True);
        }
    }

    #endregion
}
