using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SvgIconGenerator;

internal static class SvgUtils
{
    public static List<IconInfo> LoadIcons(SourceProductionContext context, ImmutableArray<AdditionalText> svgFiles, string? globPattern)
    {
        List<IconInfo> icons = [];

        foreach (AdditionalText file in svgFiles)
        {
            // Filter by glob pattern if specified
            if (globPattern is not null && !MatchesGlobPattern(file.Path, globPattern))
                continue;

            try
            {
                string? fileName = Path.GetFileNameWithoutExtension(file.Path);
                string propertyName = StringUtils.ConvertToPascalCase(fileName);
                SourceText? text = file.GetText(context.CancellationToken);
                if (text is null)
                    continue;

                string svgContent = text.ToString();

                // Parse SVG content
                XDocument doc = XDocument.Parse(svgContent);
                XElement svgElement = doc.Root!;

                // Extract all attributes except xmlns and class
                Dictionary<string, string> attributes = svgElement.Attributes()
                    .Where(a => a.Name.LocalName is not "xmlns" and not "class")
                    .ToDictionary(a => a.Name.LocalName, a => a.Value);

                // Get inner XML content without redundant xmlns attributes
                string innerContent = svgElement.GetInnerXml();

                icons.Add(new IconInfo(
                    propertyName,
                    fileName,
                    attributes,
                    innerContent
                ));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.SvgParseError, Location.None, file.Path, ex.Message));
            }
        }

        return icons;
    }

    private static bool MatchesGlobPattern(string filePath, string globPattern)
    {
        // Normalize paths
        string normalizedPath = filePath.Replace('\\', '/');
        string normalizedPattern = globPattern.Replace('\\', '/');

        // Simple glob pattern matching
        // Supports: *, **, and literal paths
        string[] patternParts = normalizedPattern.Split('/');
        string[] pathParts = normalizedPath.Split('/');

        int patternIndex = 0;
        int pathIndex = 0;

        while (patternIndex < patternParts.Length && pathIndex < pathParts.Length)
        {
            string patternPart = patternParts[patternIndex];

            if (patternPart == "**")
            {
                // ** matches zero or more path segments
                if (patternIndex == patternParts.Length - 1)
                    return true; // ** at end matches everything

                // Try to match the rest of the pattern
                for (int i = pathIndex; i < pathParts.Length; i++)
                {
                    string remainingPath = string.Join("/", pathParts.Skip(i));
                    string remainingPattern = string.Join("/", patternParts.Skip(patternIndex + 1));
                    if (MatchesGlobPattern(remainingPath, remainingPattern))
                        return true;
                }
                return false;
            }
            else if (MatchesWildcard(pathParts[pathIndex], patternPart))
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
