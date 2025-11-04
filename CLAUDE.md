# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SvgIconGenerator is a C# Roslyn incremental source generator that automatically generates strongly-typed icon properties from SVG files. It targets netstandard2.0 and is distributed as a NuGet package.

## Build and Development Commands

### Build the project
```bash
dotnet build
```

### Create NuGet package
```bash
dotnet pack
```
The package is automatically generated on build (see `GeneratePackageOnBuild` in the .csproj).

### Clean build artifacts
```bash
dotnet clean
```

## Architecture

### Source Generator Pipeline

The generator follows the incremental generator pattern:

1. **Post-initialization** (`IconGenerator.cs:13-17`): Injects `GenerateIconsAttribute` and `IconDto` into the consuming project
2. **Syntax filtering** (`IconGenerator.cs:19-24`): Finds `static partial` classes with `[GenerateIcons]` attribute
3. **Source generation** (`IconEmitter.cs:17-45`): For each marked class:
   - Resolves project directory from compilation
   - Scans specified folder for SVG files
   - Parses SVG attributes and inner content
   - Generates partial class with `IconDto` properties

### Key Components

**IconGenerator** (`IconGenerator.cs`)
- Entry point implementing `IIncrementalGenerator`
- Filters for `static partial` classes with `[GenerateIcons]` attribute
- Extracts folder path from attribute constructor argument

**IconEmitter** (`IconEmitter.cs`)
- Generates partial class source code
- Creates `IconDto` properties for each SVG file found
- Handles escaping of SVG content for C# string literals

**SvgUtils** (`SvgUtils.cs`)
- Loads SVG files from specified directory (non-recursive)
- Parses SVG XML to extract attributes and inner content
- Excludes `xmlns` and `class` attributes from default attributes
- Converts kebab-case filenames to PascalCase property names

**InjectedSource** (`InjectedSource.cs`)
- Embeds source files from `Injected/` folder as resources
- `GenerateIconsAttribute`: Marks target classes for code generation
- `IconDto`: Record type containing icon name, attributes, and SVG content

### Build Configuration

The project uses MSBuild Directory.Build.props files for shared configuration:

**Root Directory.Build.props:**
- Nullable reference types enabled
- Warnings treated as errors
- Code style enforcement in build
- Analysis mode set to "All"

**src/Directory.Build.props:**
- Package metadata (title, authors, version, tags)
- Repository URL configuration

**SvgIconGenerator.csproj:**
- Targets netstandard2.0 with latest C# language version
- `IsRoslynComponent=true` enables analyzer infrastructure
- `EnforceExtendedAnalyzerRules=true` for stricter analysis
- Packages into `analyzers/dotnet/cs` folder for source generator deployment
- Files in `Injected/` are excluded from compilation but embedded as resources

### Usage Pattern

Consuming projects apply `[GenerateIcons("path/to/icons")]` to a `static partial` class. The generator:
1. Scans the folder for `*.svg` files
2. Parses each SVG's root attributes and inner content
3. Generates properties like `public static readonly IconDto PropertyName = new(...)`
4. Property names are PascalCase conversions of kebab-case filenames

### Diagnostics

- **ICON001**: No SVG files found in specified folder
- **ICON002**: Exception during icon generation for a class
- **ICON003**: Exception parsing individual SVG file
