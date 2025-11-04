using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SvgIconGenerator;

internal static class InjectedSource
{
    public const string Namespace = "SvgIconGenerator";

    public static class IconDto
    {
        private const string Name = nameof(IconDto);
        public const string FullName = $"{Namespace}.{Name}";
        public const string Hint = $"{Name}.g.cs";
        public static SourceText Source => GetInjectedSource(nameof(IconDto));
    }

    public static class GenerateIconsAttribute
    {
        private const string Name = nameof(GenerateIconsAttribute);
        public const string FullName = $"{Namespace}.{Name}";
        public const string Hint = $"{Name}.g.cs";
        public static SourceText Source => GetInjectedSource(nameof(GenerateIconsAttribute));
    }

    private static SourceText GetInjectedSource(string name)
    {
        string resourceName = $"{typeof(InjectedSource).Namespace}.Injected.{name}.cs";
        using Stream stream = typeof(InjectedSource).Assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        return SourceText.From(content, Encoding.UTF8);
    }
}
