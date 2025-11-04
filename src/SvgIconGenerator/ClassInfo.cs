using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SvgIconGenerator;

//internal sealed record ClassInfo(INamespaceSymbol Namespace, string ClassName, string IconFolderPath);
internal sealed record ClassInfo(ISymbol TargetSymbol, Location Location, ImmutableArray<AttributeData> Attributes);
