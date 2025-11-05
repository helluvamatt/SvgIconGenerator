using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SvgIconGenerator.Tests.TestHelpers;

public sealed class TestAdditionalText(string path, string text) : AdditionalText
{
    private readonly SourceText sourceText = SourceText.From(text);

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
}
