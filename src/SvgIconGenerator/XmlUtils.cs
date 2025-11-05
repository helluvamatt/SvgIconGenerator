using System.Text;
using System.Xml.Linq;

namespace SvgIconGenerator;

internal static class XmlUtils
{
    public static string GetInnerXml(this XElement svgElement)
    {
        // Build the inner content manually by converting each child element to a string
        // and removing the namespace from the elements
        StringBuilder sb = new();
        foreach (XNode node in svgElement.Nodes())
        {
            switch (node)
            {
                case XText text:
                    sb.Append(text.Value);
                    break;
                case XComment comment:
                    sb.Append(comment);
                    break;
                case XElement element:
                {
                    // Remove the namespace and serialize
                    XElement cleanElement = RemoveNamespace(element);
                    sb.Append(cleanElement.ToString(SaveOptions.DisableFormatting));
                    break;
                }
            }
        }

        return sb.ToString();
    }

    public static XElement RemoveNamespace(XElement element)
    {
        // Create a new element without namespace
        XElement newElement = new(element.Name.LocalName);

        // Copy attributes without namespace declarations
        foreach (XAttribute attr in element.Attributes())
        {
            if (attr.IsNamespaceDeclaration) continue;
            newElement.Add(new XAttribute(attr.Name.LocalName, attr.Value));
        }

        // Recursively process child nodes
        foreach (XNode node in element.Nodes())
        {
            if (node is XElement childElement)
            {
                newElement.Add(RemoveNamespace(childElement));
            }
            else
            {
                newElement.Add(node);
            }
        }

        return newElement;
    }
}
