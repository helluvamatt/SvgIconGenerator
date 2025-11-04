// Polyfills for netstandard2.0

// Namespace does not match folder structure (IDE0130)
#pragma warning disable IDE0130
// ReSharper disable CheckNamespace

namespace System.Runtime.CompilerServices
{
    // Required for netstandard2.0 and using records or properties with init setters
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    internal interface IsExternalInit;
}

namespace System.Text
{
    internal static class StringBuilderExtensions
    {
        public static unsafe void Append(this StringBuilder sb, ReadOnlySpan<char> value)
        {
            fixed (char* ptr = value)
            {
                sb.Append(ptr, value.Length);
            }
        }
    }
}

#pragma warning restore IDE0130
