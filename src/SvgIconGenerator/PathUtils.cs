namespace SvgIconGenerator;

internal static class PathUtils
{
    /// <summary>
    /// Gets the relative path from one path to another.
    /// This is a netstandard2.0-compatible implementation since Path.GetRelativePath is not available.
    /// </summary>
    /// <param name="relativeTo">The source path (base directory).</param>
    /// <param name="path">The destination path.</param>
    /// <returns>The relative path from relativeTo to path.</returns>
    public static string GetRelativePath(string relativeTo, string path)
    {
        // Normalize paths
        string normalizedRelativeTo = Path.GetFullPath(relativeTo).Replace('\\', '/').TrimEnd('/');
        string normalizedPath = Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');

        // If paths are equal
        if (normalizedPath.Equals(normalizedRelativeTo, StringComparison.OrdinalIgnoreCase))
            return ".";

        // Split into segments, filtering out empty strings (from leading slashes)
        string[] fromParts = normalizedRelativeTo.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
        string[] toParts = normalizedPath.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();

        // Find common prefix length
        int commonLength = 0;
        int minLength = Math.Min(fromParts.Length, toParts.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (!fromParts[i].Equals(toParts[i], StringComparison.OrdinalIgnoreCase))
                break;
            commonLength++;
        }

        // Build relative path
        List<string> relativeParts = [];

        // Add ".." for each segment we need to go up
        for (int i = commonLength; i < fromParts.Length; i++)
        {
            relativeParts.Add("..");
        }

        // Add the remaining segments from the target path
        for (int i = commonLength; i < toParts.Length; i++)
        {
            relativeParts.Add(toParts[i]);
        }

        return string.Join("/", relativeParts);
    }
}
