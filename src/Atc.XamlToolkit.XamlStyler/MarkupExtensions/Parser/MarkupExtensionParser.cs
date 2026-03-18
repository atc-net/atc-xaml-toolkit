#pragma warning disable MA0051 // Method is too long
namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// Hand-written recursive descent parser for XAML markup extensions.
/// Replaces the Irony-based parser from the upstream Xavalon/XamlStyler project.
/// </summary>
public sealed class MarkupExtensionParser
{
    /// <summary>
    /// Attempts to parse a markup extension string into a <see cref="MarkupExtension"/> graph.
    /// </summary>
    /// <param name="sourceText">The markup extension string (e.g., "{Binding Path=Name}").</param>
    /// <param name="graph">The parsed result, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Parser must not throw; returns false on any failure.")]
    public static bool TryParse(
        string sourceText,
        out MarkupExtension? graph)
    {
        graph = null;

        try
        {
            var reader = new TokenReader(sourceText);
            graph = ParseMarkupExtension(reader);
            return graph is not null;
        }
        catch
        {
            return false;
        }
    }

    private static MarkupExtension? ParseMarkupExtension(TokenReader reader)
    {
        reader.SkipWhitespace();

        if (!reader.TryConsume('{'))
        {
            return null;
        }

        reader.SkipWhitespace();

        // Read type name (everything up to whitespace, '}', or end)
        var typeName = reader.ReadTypeName();
        if (string.IsNullOrEmpty(typeName))
        {
            return null;
        }

        reader.SkipWhitespace();

        // If immediately closed, it's a parameterless extension
        if (reader.TryConsume('}'))
        {
            return new MarkupExtension(typeName);
        }

        // Parse arguments
        var arguments = new List<Argument>();
        var isFirst = true;

        while (!reader.IsAtEnd)
        {
            reader.SkipWhitespace();

            if (reader.TryConsume('}'))
            {
                return new MarkupExtension(typeName, arguments.ToArray());
            }

            if (!isFirst)
            {
                if (!reader.TryConsume(','))
                {
                    return null;
                }

                reader.SkipWhitespace();
            }

            isFirst = false;

            var argument = ParseArgument(reader);
            if (argument is null)
            {
                return null;
            }

            arguments.Add(argument);
        }

        return null;
    }

    private static Argument? ParseArgument(TokenReader reader)
    {
        reader.SkipWhitespace();

        // Check for nested markup extension
        if (reader.Peek() == '{')
        {
            var nested = ParseMarkupExtension(reader);
            return nested is not null ? new PositionalArgument(nested) : null;
        }

        // Read a token -- could be a member name (if followed by '=') or a positional value
        var token = reader.ReadValueToken();
        if (token is null)
        {
            return null;
        }

        reader.SkipWhitespace();

        // If followed by '=', this is a named argument
        if (reader.TryConsume('='))
        {
            reader.SkipWhitespace();

            Value? value;
            if (reader.Peek() == '{')
            {
                var nested = ParseMarkupExtension(reader);
                if (nested is null)
                {
                    return null;
                }

                value = nested;
            }
            else
            {
                var valueToken = reader.ReadValueToken();
                if (valueToken is null)
                {
                    return null;
                }

                value = new LiteralValue(valueToken);
            }

            return new NamedArgument(token, value);
        }

        return new PositionalArgument(new LiteralValue(token));
    }

    /// <summary>
    /// Simple character-by-character reader for tokenizing markup extension strings.
    /// </summary>
    private sealed class TokenReader
    {
        private readonly string source;
        private int position;

        public TokenReader(string source)
        {
            this.source = source;
        }

        public bool IsAtEnd => position >= source.Length;

        public char Peek() => IsAtEnd ? '\0' : source[position];

        public void SkipWhitespace()
        {
            while (!IsAtEnd && char.IsWhiteSpace(source[position]))
            {
                position++;
            }
        }

        public bool TryConsume(char expected)
        {
            if (IsAtEnd || source[position] != expected)
            {
                return false;
            }

            position++;
            return true;
        }

        public string ReadTypeName()
        {
            var start = position;
            while (!IsAtEnd)
            {
                var c = source[position];
                if (c == ' ' || c == '}' || c == '{' || c == '\r' || c == '\n' || c == '\t')
                {
                    break;
                }

                position++;
            }

            return source.Substring(start, position - start);
        }

        /// <summary>
        /// Reads a value token: either a quoted string or an unquoted value
        /// terminated by ',', '}', or '=' (for unquoted tokens).
        /// </summary>
        /// <returns>The token value, or null if at end.</returns>
        public string? ReadValueToken()
        {
            if (IsAtEnd)
            {
                return null;
            }

            var c = source[position];

            // Quoted string
            if (c == '\'' || c == '"')
            {
                return ReadQuotedString(c);
            }

            // Escaped literal: {} prefix means literal text
            if (c == '{' && position + 1 < source.Length && source[position + 1] == '}')
            {
                position += 2;
                return ReadUnquotedValue();
            }

            return ReadUnquotedValue();
        }

        private string ReadQuotedString(char quote)
        {
            position++; // skip opening quote
            var sb = new StringBuilder();
            sb.Append(quote); // preserve opening quote
            while (!IsAtEnd)
            {
                var c = source[position];
                if (c == quote)
                {
                    position++; // skip closing quote
                    sb.Append(quote); // preserve closing quote
                    return sb.ToString();
                }

                if (c == '\\')
                {
                    position++;
                    if (!IsAtEnd)
                    {
                        sb.Append(source[position]);
                        position++;
                    }

                    continue;
                }

                sb.Append(c);
                position++;
            }

            sb.Append(quote); // unterminated - still preserve
            return sb.ToString();
        }

        private string ReadUnquotedValue()
        {
            var start = position;
            var braceDepth = 0;
            var lastNonWhitespace = position;

            while (!IsAtEnd)
            {
                var c = source[position];

                switch (c)
                {
                    case '{':
                        braceDepth++;
                        position++;
                        lastNonWhitespace = position;
                        continue;

                    case '}':
                        if (braceDepth > 0)
                        {
                            braceDepth--;
                            position++;
                            lastNonWhitespace = position;
                            continue;
                        }

                        // End of value (closing brace of parent extension)
                        return TrimResult(start, lastNonWhitespace);

                    case ',' or '=':
                        if (braceDepth == 0)
                        {
                            return TrimResult(start, lastNonWhitespace);
                        }

                        position++;
                        lastNonWhitespace = position;
                        continue;

                    case '\\':
                        position++; // skip escape char
                        if (!IsAtEnd)
                        {
                            position++;
                            lastNonWhitespace = position;
                        }

                        continue;

                    default:
                        position++;
                        if (!char.IsWhiteSpace(c))
                        {
                            lastNonWhitespace = position;
                        }

                        continue;
                }
            }

            return TrimResult(start, lastNonWhitespace);
        }

        private string TrimResult(
            int start,
            int lastNonWhitespace)
        {
            var length = lastNonWhitespace - start;
            return length > 0 ? source.Substring(start, length) : string.Empty;
        }
    }
}