namespace SvgIconGenerator;

internal static class GlobUtils
{
    /// <summary>
    /// Matches a file path against a glob pattern.
    /// Supports wildcards: * (single segment), ** (multiple segments), and ? (single character).
    /// </summary>
    /// <param name="filePath">The file path to test.</param>
    /// <param name="globPattern">The glob pattern to match against.</param>
    /// <returns>True if the file path matches the pattern, false otherwise.</returns>
    public static bool MatchesGlobPattern(string filePath, string globPattern)
    {
        // Normalize paths to use forward slashes
        string normalizedPath = filePath.Replace('\\', '/');
        string normalizedPattern = globPattern.Replace('\\', '/');

        // If pattern is just a filename (no directory separators), match against the filename only
        if (!normalizedPattern.Contains('/'))
        {
            string fileName = normalizedPath.Contains('/')
                ? normalizedPath.Substring(normalizedPath.LastIndexOf('/') + 1)
                : normalizedPath;
            return MatchesWildcard(fileName, normalizedPattern);
        }

        // Split into path segments
        string[] patternParts = normalizedPattern.Split('/');
        string[] pathParts = normalizedPath.Split('/');

        return MatchesPatternParts(pathParts, 0, patternParts, 0);
    }

    /// <summary>
    /// Recursively matches path parts against pattern parts.
    /// </summary>
    private static bool MatchesPatternParts(string[] pathParts, int pathIndex, string[] patternParts, int patternIndex)
    {
        while (patternIndex < patternParts.Length && pathIndex < pathParts.Length)
        {
            string patternPart = patternParts[patternIndex];

            if (patternPart == "**")
            {
                // ** matches zero or more path segments
                if (patternIndex == patternParts.Length - 1)
                    return true; // ** at end matches everything

                // Try to match the rest of the pattern from any position
                for (int i = pathIndex; i <= pathParts.Length; i++)
                {
                    if (MatchesPatternParts(pathParts, i, patternParts, patternIndex + 1))
                        return true;
                }
                return false;
            }

            if (MatchesWildcard(pathParts[pathIndex], patternPart))
            {
                patternIndex++;
                pathIndex++;
            }
            else
            {
                return false;
            }
        }

        return patternIndex == patternParts.Length && pathIndex == pathParts.Length;
    }

    /// <summary>
    /// Matches a text string against a pattern that may contain * wildcards.
    /// </summary>
    private static bool MatchesWildcard(string text, string pattern)
    {
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return text.Equals(pattern, StringComparison.OrdinalIgnoreCase);

        // Simple wildcard matching
        string[] parts = pattern.Split('*');
        int currentIndex = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (string.IsNullOrEmpty(part))
                continue;

            int foundIndex = text.IndexOf(part, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (foundIndex == -1)
                return false;

            if (i == 0 && foundIndex != 0)
                return false; // First part must match at start if no leading *

            currentIndex = foundIndex + part.Length;
        }

        // If pattern doesn't end with *, ensure we matched to the end
        if (!pattern.EndsWith("*", StringComparison.Ordinal) && currentIndex != text.Length)
            return false;

        return true;
    }
}
