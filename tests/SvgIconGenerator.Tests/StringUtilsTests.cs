using NUnit.Framework;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class StringUtilsTests
{
    [Test]
    [TestCase("icon-home", "IconHome")]
    [TestCase("icon-settings", "IconSettings")]
    [TestCase("arrow-down", "ArrowDown")]
    [TestCase("simple", "Simple")]
    [TestCase("multi-word-name", "MultiWordName")]
    public void ConvertToPascalCase_SimpleKebabCase_ReturnsCorrectPascalCase(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("arrow-down-0-1", "ArrowDown0_1")]
    [TestCase("icon-1-2-3", "Icon1_2_3")]
    [TestCase("test-5-end", "Test5_end")]
    public void ConvertToPascalCase_DigitsAfterHyphen_UsesUnderscore(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("arrow-down-01", "ArrowDown01")]
    [TestCase("icon-2x", "Icon2x")]
    [TestCase("test-123abc", "Test123abc")]
    public void ConvertToPascalCase_DigitsFollowedByLetters_NoUnderscore(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", "")]
    [TestCase("a", "A")]
    [TestCase("abc", "Abc")]
    public void ConvertToPascalCase_EdgeCases_HandlesCorrectly(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("123", "123")]
    [TestCase("1-2", "1_2")]
    [TestCase("123-456", "123_456")]
    public void ConvertToPascalCase_OnlyDigits_HandlesCorrectly(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("UPPER-CASE", "UPPERCASE")]
    [TestCase("MiXeD-CaSe", "MiXeDCaSe")]
    public void ConvertToPascalCase_UppercaseInput_PreservesCase(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("icon-", "Icon")]
    [TestCase("-icon", "Icon")]
    [TestCase("--icon", "Icon")]
    [TestCase("icon--home", "IconHome")]
    public void ConvertToPascalCase_TrailingOrLeadingHyphens_HandlesCorrectly(string input, string expected)
    {
        string result = StringUtils.ConvertToPascalCase(input);
        Assert.That(result, Is.EqualTo(expected));
    }
}
