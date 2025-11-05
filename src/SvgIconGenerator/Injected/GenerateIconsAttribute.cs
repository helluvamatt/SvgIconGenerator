namespace SvgIconGenerator
{
    /// <summary>
    /// Marks a static partial class for icon generation.
    /// The source generator will scan AdditionalFiles for SVG files matching the specified glob pattern
    /// and generate static readonly IconDto properties for each icon found.
    /// </summary>
    /// <remarks>
    /// SVG files must be added to the project as AdditionalFiles in the .csproj file:
    /// <code>
    /// &lt;ItemGroup&gt;
    ///   &lt;AdditionalFiles Include="icons/**/*.svg" /&gt;
    /// &lt;/ItemGroup&gt;
    /// </code>
    /// </remarks>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class GenerateIconsAttribute : global::System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateIconsAttribute"/> class.
        /// </summary>
        /// <param name="globPattern">
        /// Optional glob pattern to filter SVG files from AdditionalFiles.
        /// If not specified, all SVG files in AdditionalFiles will be included.
        /// Supports * (wildcard) and ** (recursive) patterns.
        /// Example: "node_modules/lucide-static/icons/*.svg"
        /// </param>
        public GenerateIconsAttribute(string? globPattern = null)
        {
        }
    }
}
