using Microsoft.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace SvgIconGenerator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor NoIconsFound = new(
        "ICON001",
        "No icons found",
        "No SVG files found matching pattern: {0}. Ensure SVG files are added as AdditionalFiles in your .csproj file.",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SvgParseError = new(
        "ICON002",
        "Error parsing SVG",
        "Error parsing SVG file {0}: {1}",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
