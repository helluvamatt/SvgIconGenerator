using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SvgIconGenerator;

internal static class SvgUtils
{
    public static List<IconInfo> LoadIcons(SourceProductionContext context, ImmutableArray<AdditionalText> svgFiles, string? globPattern, string? projectDirectory)
    {
        List<IconInfo> icons = [];

        foreach (AdditionalText file in svgFiles)
        {
            // Filter by glob pattern if specified
            if (globPattern is not null)
            {
                string pathToMatch = file.Path;

                // If pattern is relative and we have a project directory, make the path relative
                bool isAbsolutePattern = Path.IsPathRooted(globPattern);
                if (!isAbsolutePattern && projectDirectory is not null && Path.IsPathRooted(file.Path))
                {
                    pathToMatch = PathUtils.GetRelativePath(projectDirectory, file.Path);
                }

                if (!GlobUtils.MatchesGlobPattern(pathToMatch, globPattern)) continue;
            }

            try
            {
                string? fileName = Path.GetFileNameWithoutExtension(file.Path);
                string propertyName = StringUtils.ConvertToPascalCase(fileName);
                SourceText? text = file.GetText(context.CancellationToken);
                if (text is null) continue;

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
}
