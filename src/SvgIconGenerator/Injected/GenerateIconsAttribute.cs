namespace SvgIconGenerator
{
    /// <summary>
    /// Marks a static partial class for icon generation.
    /// The source generator will scan the specified folder for SVG files and generate
    /// static readonly IconDto properties for each icon found.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class GenerateIconsAttribute : global::System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateIconsAttribute"/> class.
        /// </summary>
        /// <param name="iconFolderPath">
        /// The relative or absolute path to the folder containing SVG icon files.
        /// The path is resolved relative to the project directory.
        /// </param>
        public GenerateIconsAttribute(string iconFolderPath)
        {
        }
    }
}
