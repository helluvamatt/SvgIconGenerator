using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace SvgIconGenerator;

internal static class SvgUtils
{
    public static List<IconInfo> LoadIcons(SourceProductionContext context, string folderPath)
    {
        List<IconInfo> icons = [];

        if (!Directory.Exists(folderPath))
            return icons;

        string[] svgFiles = Directory.GetFiles(folderPath, "*.svg", SearchOption.TopDirectoryOnly);

        foreach (string filePath in svgFiles)
        {
            try
            {
                string? fileName = Path.GetFileNameWithoutExtension(filePath);
                string propertyName = StringUtils.ConvertToPascalCase(fileName);
                string svgContent = File.ReadAllText(filePath);

                // Parse SVG content
                XDocument doc = XDocument.Parse(svgContent);
                XElement svgElement = doc.Root!;

                // Extract all attributes except xmlns and class
                Dictionary<string, string> attributes = svgElement.Attributes()
                    .Where(a => a.Name.LocalName is not "xmlns" and not "class")
                    .ToDictionary(a => a.Name.LocalName, a => a.Value);

                // Get inner XML content
                using XmlReader reader = svgElement.CreateReader();
                reader.MoveToContent();
                string innerContent = reader.ReadInnerXml();

                icons.Add(new IconInfo(
                    propertyName,
                    fileName,
                    attributes,
                    innerContent
                ));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.SvgParseError, Location.None, filePath, ex.Message));
            }
        }

        return icons;
    }
}
