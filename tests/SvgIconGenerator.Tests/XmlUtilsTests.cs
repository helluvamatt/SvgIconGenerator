using System.Xml.Linq;
using NUnit.Framework;

namespace SvgIconGenerator.Tests;

[TestFixture]
public class XmlUtilsTests
{
    [Test]
    public void GetInnerXml_SimpleElement_ReturnsInnerContent()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10"/>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        Assert.That(result, Does.Not.Contain("xmlns"));
        Assert.That(result, Does.Contain("<circle"));
        Assert.That(result, Does.Contain("cx=\"12\""));
        Assert.That(result, Does.Contain("cy=\"12\""));
        Assert.That(result, Does.Contain("r=\"10\""));
    }

    [Test]
    public void GetInnerXml_MultipleChildren_ReturnsAllChildren()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <path d="M10 10"/>
              <circle cx="5" cy="5" r="3"/>
              <rect x="0" y="0" width="20" height="20"/>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("xmlns"));
            Assert.That(result, Does.Contain("<path"));
            Assert.That(result, Does.Contain("<circle"));
            Assert.That(result, Does.Contain("<rect"));
        }
    }

    [Test]
    public void GetInnerXml_NestedElements_RemovesNamespaceFromAll()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g>
                <path d="M10 10"/>
                <circle cx="5" cy="5" r="3"/>
              </g>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("xmlns"));
            Assert.That(result, Does.Contain("<g>"));
            Assert.That(result, Does.Contain("<path"));
            Assert.That(result, Does.Contain("<circle"));
            Assert.That(result, Does.Contain("</g>"));
        }
    }

    [Test]
    public void GetInnerXml_WithTextContent_PreservesText()
    {
        // Arrange
        const string xml = """
            <div xmlns="http://example.com">
              <p>Hello World</p>
              Some text
              <span>More text</span>
            </div>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("xmlns"));
            Assert.That(result, Does.Contain("Hello World"));
            Assert.That(result, Does.Contain("Some text"));
            Assert.That(result, Does.Contain("More text"));
        }
    }

    [Test]
    public void GetInnerXml_WithComments_PreservesComments()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <!-- This is a comment -->
              <circle cx="12" cy="12" r="10"/>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("<!-- This is a comment -->"));
            Assert.That(result, Does.Contain("<circle"));
        }
    }

    [Test]
    public void GetInnerXml_EmptyElement_ReturnsEmptyString()
    {
        // Arrange
        const string xml = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetInnerXml_SelfClosingChildren_HandlesCorrectly()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <path d="M10 10" />
              <line x1="0" y1="0" x2="10" y2="10" />
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("xmlns"));
            Assert.That(result, Does.Contain("<path"));
            Assert.That(result, Does.Contain("<line"));
        }
    }

    [Test]
    public void RemoveNamespace_SimpleElement_RemovesXmlns()
    {
        // Arrange
        const string xml = """<circle xmlns="http://www.w3.org/2000/svg" cx="12" cy="12" r="10"/>""";
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name.LocalName, Is.EqualTo("circle"));
            Assert.That(result.Name.NamespaceName, Is.Empty);
            Assert.That(result.Attribute("cx")?.Value, Is.EqualTo("12"));
            Assert.That(result.Attribute("cy")?.Value, Is.EqualTo("12"));
            Assert.That(result.Attribute("r")?.Value, Is.EqualTo("10"));
            Assert.That(result.Attributes().Any(a => a.IsNamespaceDeclaration), Is.False);
        }
    }

    [Test]
    public void RemoveNamespace_NestedElements_RemovesNamespaceFromAll()
    {
        // Arrange
        const string xml = """
            <g xmlns="http://www.w3.org/2000/svg">
              <circle cx="5" cy="5" r="3"/>
              <rect x="0" y="0" width="10" height="10"/>
            </g>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name.LocalName, Is.EqualTo("g"));
            Assert.That(result.Name.NamespaceName, Is.Empty);

            XElement? circle = result.Elements().FirstOrDefault(e => e.Name.LocalName == "circle");
            Assert.That(circle, Is.Not.Null);
            Assert.That(circle!.Name.NamespaceName, Is.Empty);

            XElement? rect = result.Elements().FirstOrDefault(e => e.Name.LocalName == "rect");
            Assert.That(rect, Is.Not.Null);
            Assert.That(rect!.Name.NamespaceName, Is.Empty);

            Assert.That(result.Descendants().All(e => !e.Attributes().Any(a => a.IsNamespaceDeclaration)), Is.True);
        }
    }

    [Test]
    public void RemoveNamespace_PreservesAttributes()
    {
        // Arrange
        const string xml = """<path xmlns="http://www.w3.org/2000/svg" d="M10 10 L20 20" stroke="red" fill="blue"/>""";
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attribute("d")?.Value, Is.EqualTo("M10 10 L20 20"));
            Assert.That(result.Attribute("stroke")?.Value, Is.EqualTo("red"));
            Assert.That(result.Attribute("fill")?.Value, Is.EqualTo("blue"));
        }
    }

    [Test]
    public void RemoveNamespace_WithTextContent_PreservesText()
    {
        // Arrange
        const string xml = """<text xmlns="http://www.w3.org/2000/svg">Hello World</text>""";
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        Assert.That(result.Value, Is.EqualTo("Hello World"));
    }

    [Test]
    public void RemoveNamespace_DeeplyNestedElements_RemovesAllNamespaces()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg">
              <g>
                <g>
                  <circle cx="1" cy="1" r="1"/>
                </g>
              </g>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name.NamespaceName, Is.Empty);
            Assert.That(result.Descendants().All(e => string.IsNullOrEmpty(e.Name.NamespaceName)), Is.True);
            Assert.That(result.Descendants().All(e => !e.Attributes().Any(a => a.IsNamespaceDeclaration)), Is.True);
        }
    }

    [Test]
    public void RemoveNamespace_MixedContent_PreservesStructure()
    {
        // Arrange
        const string xml = """
            <div xmlns="http://example.com">
              Text before
              <span>Nested text</span>
              Text after
            </div>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name.LocalName, Is.EqualTo("div"));
            Assert.That(result.Value, Does.Contain("Text before"));
            Assert.That(result.Value, Does.Contain("Nested text"));
            Assert.That(result.Value, Does.Contain("Text after"));

            XElement? span = result.Element("span");
            Assert.That(span, Is.Not.Null);
            Assert.That(span!.Value, Is.EqualTo("Nested text"));
        }
    }

    [Test]
    public void RemoveNamespace_WithMultipleNamespaces_RemovesAll()
    {
        // Arrange
        const string xml = """
            <root xmlns="http://example.com" xmlns:custom="http://custom.com">
              <child custom:attr="value">Content</child>
            </root>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        XElement result = XmlUtils.RemoveNamespace(element);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name.NamespaceName, Is.Empty);
            Assert.That(result.Attributes().Any(a => a.IsNamespaceDeclaration), Is.False);

            XElement? child = result.Element("child");
            Assert.That(child, Is.Not.Null);
            Assert.That(child!.Name.NamespaceName, Is.Empty);
        }
    }

    [Test]
    public void GetInnerXml_RealWorldSvg_RemovesRedundantXmlns()
    {
        // Arrange
        const string xml = """
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
              <path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
              <polyline points="9 22 9 12 15 12 15 22"></polyline>
            </svg>
            """;
        XElement element = XElement.Parse(xml);

        // Act
        string result = element.GetInnerXml();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Should not contain any xmlns declarations
            Assert.That(result, Does.Not.Contain("xmlns=\"http://www.w3.org/2000/svg\""));
            Assert.That(result, Does.Not.Contain("xmlns="));

            // Should contain the actual elements
            Assert.That(result, Does.Contain("<path"));
            Assert.That(result, Does.Contain("<polyline"));
            Assert.That(result, Does.Contain("d=\"m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\""));
            Assert.That(result, Does.Contain("points=\"9 22 9 12 15 12 15 22\""));
        }
    }
}
