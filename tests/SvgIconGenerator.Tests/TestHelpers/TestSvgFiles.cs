namespace SvgIconGenerator.Tests.TestHelpers;

/// <summary>
/// Contains sample SVG file content for testing.
/// </summary>
public static class TestSvgFiles
{
    public const string SimpleIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
        </svg>
        """;

    public const string IconWithMultipleAttributes = """
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32" fill="red" stroke="blue" stroke-width="3" stroke-linecap="round">
          <path d="M10 10 L20 20"/>
        </svg>
        """;

    public const string IconWithClassAttribute = """
        <svg xmlns="http://www.w3.org/2000/svg" class="icon-class" width="24" height="24" viewBox="0 0 24 24">
          <rect x="5" y="5" width="14" height="14"/>
        </svg>
        """;

    public const string InvalidSvg = """
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
          <circle cx="12" cy="12" r="10"/>
        </svg>
        """;

    public const string ComplexIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
          <polyline points="9 22 9 12 15 12 15 22"></polyline>
        </svg>
        """;
}
