using Microsoft.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace SvgIconGenerator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor MissingProjectDirectory = new(
        "ICON001",
        "Project directory not found",
        "Project directory not found. Cannot resolve icon folder paths.",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingIconDirectory = new(
        "ICON002",
        "Missing icon directory",
        "Icon directory not found: {0}",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoIconsFound = new(
        "ICON003",
        "No icons found",
        "No SVG files found in folder: {0}",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorGeneratingIcons = new(
        "ICON004",
        "Error generating icons",
        "Error generating icons for {0}: {1}",
        "IconGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SvgParseError = new(
        "ICON005",
        "Error parsing SVG",
        "Error parsing SVG file {0}: {1}",
        "IconGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
