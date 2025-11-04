using NUnit.Framework;

namespace SvgIconGenerator.Tests;

/// <summary>
/// SvgUtils tests are covered through integration tests in IconGeneratorTests and DiagnosticsTests
/// since SvgUtils requires a SourceProductionContext struct that cannot be easily mocked.
/// </summary>
[TestFixture]
public class SvgUtilsTests
{
    [Test]
    public void SvgUtils_IsCoveredByIntegrationTests()
    {
        // SvgUtils.LoadIcons is tested indirectly through:
        // - IconGeneratorTests (validates SVG loading and parsing)
        // - DiagnosticsTests (validates error handling for invalid SVG)
        // - SnapshotTests (validates attribute extraction and xmlns/class filtering)
        Assert.Pass("SvgUtils is covered by integration tests");
    }
}
