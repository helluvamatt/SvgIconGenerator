using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SvgIconGenerator;

internal sealed class SourceWriter
{
    private const char IndentationChar = ' ';
    private const int CharsPerIndentation = 4;

    private readonly StringBuilder sb = new();
    private int indentation;

    public int Indentation
    {
        get => indentation;
        set
        {
            if (value < 0)
            {
                Throw();
                static void Throw()
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }

            indentation = value;
        }
    }

    public void PushIndentation() => indentation++;
    public void PopIndentation() => indentation--;

    public void WriteLine(char value)
    {
        AddIndentation();
        sb.Append(value);
        sb.AppendLine();
    }

    public void WriteLine(string text)
    {
        if (indentation == 0)
        {
            sb.AppendLine(text);
            return;
        }

        bool isFinalLine;
        ReadOnlySpan<char> remainingText = text.AsSpan();
        do
        {
            ReadOnlySpan<char> nextLine = GetNextLine(ref remainingText, out isFinalLine);

            AddIndentation();
            sb.Append(nextLine);
            sb.AppendLine();
        } while (!isFinalLine);
    }

    public void WriteLine() => sb.AppendLine();

    public SourceText ToSourceText()
    {
        Debug.Assert(indentation == 0 && sb.Length > 0);
        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    public void Reset()
    {
        sb.Clear();
        indentation = 0;
    }

    private void AddIndentation()
        => sb.Append(IndentationChar, CharsPerIndentation * indentation);

    private static ReadOnlySpan<char> GetNextLine(ref ReadOnlySpan<char> remainingText, out bool isFinalLine)
    {
        if (remainingText.IsEmpty)
        {
            isFinalLine = true;
            return default;
        }

        ReadOnlySpan<char> next;
        ReadOnlySpan<char> rest;

        int lineLength = remainingText.IndexOf('\n');
        if (lineLength == -1)
        {
            lineLength = remainingText.Length;
            isFinalLine = true;
            rest = default;
        }
        else
        {
            rest = remainingText.Slice(lineLength + 1);
            isFinalLine = false;
        }

        if ((uint)lineLength > 0 && remainingText[lineLength - 1] == '\r')
        {
            lineLength--;
        }

        next = remainingText.Slice(0, lineLength);
        remainingText = rest;
        return next;
    }
}
