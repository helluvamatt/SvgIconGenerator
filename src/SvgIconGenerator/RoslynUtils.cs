using Microsoft.CodeAnalysis;

namespace SvgIconGenerator;

internal static class RoslynUtils
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> source) where T : notnull
        => source.Where(t => t is not null)
            .Select((t, _) => t!);
}
