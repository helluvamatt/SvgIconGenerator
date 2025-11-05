using NUnit.Framework;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class PathUtilsTests
{
    [TestCase("/Users/test/project", "/Users/test/project/icons/home.svg", "icons/home.svg", TestName = "ChildPath")]
    [TestCase("/Users/test/project", "/Users/test/project/src/assets/icons/home.svg", "src/assets/icons/home.svg", TestName = "DeepChildPath")]
    [TestCase("/Users/test/project", "/Users/test/project", ".", TestName = "SamePath")]
    [TestCase("/Users/test/project", "/var/www/html/icons/home.svg", "../../../var/www/html/icons/home.svg", TestName = "UnrelatedPath")]
    [TestCase("/Users/test/project/src/feature-a/feature-a-project", "/var/www/html/icons/home.svg", "../../../../../../var/www/html/icons/home.svg", TestName = "UnrelatedPathDeeper")]
    [TestCase("/Users/test/project/src", "/Users/test/project/icons/home.svg", "../icons/home.svg", TestName = "UpwardPath")]
    [TestCase("C:\\Users\\test\\project", "C:\\Users\\test\\project\\icons\\home.svg", "icons/home.svg", TestName = "WithBackslashes")]
    [TestCase("C:/Users/test/project", "C:\\Users\\test\\project\\icons\\home.svg", "icons/home.svg", TestName = "MixedSlashes")]
    [TestCase("/Users/Test/Project", "/users/test/project/icons/home.svg", "icons/home.svg", TestName = "CaseInsensitive")]
    [TestCase("/Users/test/project/", "/Users/test/project/icons/home.svg", "icons/home.svg", TestName = "TrailingSlashInBase")]
    [TestCase("/Users/test/project", "/Users/test/project/file.svg", "file.svg", TestName = "SingleLevelChild")]
    [TestCase(".", "./icons/home.svg", "icons/home.svg", TestName = "RelativePathsConvertsToAbsoluteFirst")]
    [TestCase("/Users/test/project/subfolder", "/Users/test/project/icons/home.svg", "../icons/home.svg", TestName = "SiblingPath")]
    [TestCase("/Users/test//project", "/Users/test/project/icons/home.svg", "icons/home.svg", TestName = "EmptyPathSegments")]
    [TestCase("C:\\Projects\\MyApp", "D:\\Assets\\icons\\home.svg", "../../../D:/Assets/icons/home.svg", TestName = "DifferentDrives")]
    public void GetRelativePath_VariousScenarios_ReturnsExpectedResult(string basePath, string targetPath, string expectedResult)
    {
        // Act
        string result = PathUtils.GetRelativePath(basePath, targetPath);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
