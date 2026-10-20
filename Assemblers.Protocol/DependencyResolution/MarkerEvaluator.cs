namespace Skyline.DataMiner.CICD.Assemblers.Protocol.DependencyResolution
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Evaluates a single PEP 508 environment marker comparison (e.g. <c>sys_platform == "win32"</c>) against a
    /// synthetic per-platform environment, so <c>requirements.txt</c> markers resolve correctly regardless of the
    /// build agent's actual OS (pip itself only evaluates markers against the host it runs on, not the requested
    /// <c>--platform</c>). Compound expressions (<c>and</c>, <c>or</c>, parentheses) are rejected with a clear
    /// error rather than silently mis-evaluated, since they aren't needed here (fixed Python version per connector,
    /// exactly two supported platforms, no "extra" concept).
    /// </summary>
    internal static class MarkerEvaluator
    {
        /// <summary>
        /// Evaluates <paramref name="markerExpression"/> against <paramref name="environment"/>.
        /// </summary>
        /// <param name="markerExpression">The marker expression (the part of a requirement specifier after the <c>;</c>).</param>
        /// <param name="environment">The synthetic marker environment to evaluate against.</param>
        /// <returns><see langword="true"/> if the requirement applies for this environment.</returns>
        /// <exception cref="NotSupportedException">The marker expression is a compound expression (<c>and</c>/<c>or</c>/parentheses), which is not supported.</exception>
        /// <exception cref="FormatException">The marker expression could not be parsed as a single comparison.</exception>
        public static bool Evaluate(string markerExpression, IReadOnlyDictionary<string, string> environment)
        {
            if (String.IsNullOrWhiteSpace(markerExpression))
            {
                return true;
            }

            List<string> tokens = Tokenize(markerExpression);

            foreach (string token in tokens)
            {
                if (token == "(" || token == ")"
                    || String.Equals(token, "and", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(token, "or", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException(
                        $"Compound marker expression '{markerExpression}' is not supported. Only a single comparison " +
                        "(e.g. \"sys_platform == 'win32'\") is supported per requirement; split the requirement across " +
                        "separate lines or simplify the condition.");
                }
            }

            int position = 0;

            string left = ParseValue(tokens, ref position, environment);

            if (position >= tokens.Count)
            {
                throw new FormatException($"Expected a comparison operator in marker expression '{markerExpression}'.");
            }

            string op = tokens[position];
            bool negatedIn = false;

            if (String.Equals(op, "not", StringComparison.OrdinalIgnoreCase))
            {
                position++;
                if (position >= tokens.Count || !String.Equals(tokens[position], "in", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException($"Expected 'in' after 'not' in marker expression '{markerExpression}'.");
                }

                op = "in";
                negatedIn = true;
            }

            position++;

            string right = ParseValue(tokens, ref position, environment);

            if (position != tokens.Count)
            {
                throw new FormatException($"Unexpected token '{tokens[position]}' in marker expression '{markerExpression}'.");
            }

            bool evaluated = EvaluateComparison(op, left, right);
            return negatedIn ? !evaluated : evaluated;
        }

        private static string ParseValue(IReadOnlyList<string> tokens, ref int position, IReadOnlyDictionary<string, string> environment)
        {
            if (position >= tokens.Count)
            {
                throw new FormatException("Unexpected end of marker expression.");
            }

            string token = tokens[position];
            position++;

            if (token.Length >= 2 && (token[0] == '\'' || token[0] == '"') && token[token.Length - 1] == token[0])
            {
                return token.Substring(1, token.Length - 2);
            }

            if (environment.TryGetValue(token, out string value))
            {
                return value;
            }

            // Unknown marker variable (e.g. "extra", which is not relevant outside of extras resolution): treat its
            // value as an empty string so comparisons against it simply don't match, rather than throwing.
            return String.Empty;
        }

        private static bool EvaluateComparison(string op, string left, string right)
        {
            switch (op)
            {
                case "==":
                    return String.Equals(left, right, StringComparison.Ordinal);

                case "!=":
                    return !String.Equals(left, right, StringComparison.Ordinal);

                case "in":
                    return right.IndexOf(left, StringComparison.Ordinal) >= 0;

                case ">=":
                case "<=":
                case ">":
                case "<":
                    return CompareVersions(left, right, op);

                default:
                    throw new FormatException($"Unsupported marker operator '{op}'.");
            }
        }

        private static bool CompareVersions(string left, string right, string op)
        {
            if (!TryParseVersion(left, out Version leftVersion) || !TryParseVersion(right, out Version rightVersion))
            {
                // Not a comparable version-like value: fall back to ordinal string comparison for the relevant operator.
                int stringComparison = String.CompareOrdinal(left, right);
                return CompareUsingOperator(stringComparison, op);
            }

            return CompareUsingOperator(leftVersion.CompareTo(rightVersion), op);
        }

        private static bool CompareUsingOperator(int comparison, string op)
        {
            switch (op)
            {
                case ">=":
                    return comparison >= 0;

                case "<=":
                    return comparison <= 0;

                case ">":
                    return comparison > 0;

                case "<":
                    return comparison < 0;

                default:
                    throw new FormatException($"Unsupported marker operator '{op}'.");
            }
        }

        private static bool TryParseVersion(string value, out Version version)
        {
            // Normalize to at least a "major.minor" shape so bare "3" or "3.14" both parse via System.Version.
            string normalized = value.Trim();

            if (normalized.Length == 0)
            {
                version = null;
                return false;
            }

            if (!Regex.IsMatch(normalized, @"^\d+(\.\d+)*$"))
            {
                version = null;
                return false;
            }

            if (normalized.IndexOf('.') < 0)
            {
                normalized += ".0";
            }

            return Version.TryParse(normalized, out version);
        }

        private static List<string> Tokenize(string markerExpression)
        {
            var tokens = new List<string>();
            int i = 0;

            while (i < markerExpression.Length)
            {
                char c = markerExpression[i];

                if (Char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (c == '(' || c == ')')
                {
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                if (c == '\'' || c == '"')
                {
                    int start = i;
                    i++;
                    while (i < markerExpression.Length && markerExpression[i] != c)
                    {
                        i++;
                    }

                    if (i >= markerExpression.Length)
                    {
                        throw new FormatException($"Unterminated string literal in marker expression '{markerExpression}'.");
                    }

                    i++; // Include the closing quote.
                    tokens.Add(markerExpression.Substring(start, i - start));
                    continue;
                }

                if (c == '=' || c == '!' || c == '<' || c == '>')
                {
                    if (i + 1 < markerExpression.Length && markerExpression[i + 1] == '=')
                    {
                        tokens.Add(markerExpression.Substring(i, 2));
                        i += 2;
                    }
                    else if (c == '<' || c == '>')
                    {
                        tokens.Add(c.ToString());
                        i++;
                    }
                    else
                    {
                        throw new FormatException($"Unexpected character '{c}' in marker expression '{markerExpression}'.");
                    }

                    continue;
                }

                if (c == '~' && i + 1 < markerExpression.Length && markerExpression[i + 1] == '=')
                {
                    // ~= (compatible release): not meaningful for the simple identity/version comparisons we
                    // support here, but tokenize it so a clear FormatException is raised later instead of a
                    // confusing tokenizer failure.
                    tokens.Add("~=");
                    i += 2;
                    continue;
                }

                // Identifier (marker variable name, or one of the "and"/"or"/"in"/"not" keywords).
                int identifierStart = i;
                while (i < markerExpression.Length && (Char.IsLetterOrDigit(markerExpression[i]) || markerExpression[i] == '_' || markerExpression[i] == '.'))
                {
                    i++;
                }

                if (i == identifierStart)
                {
                    throw new FormatException($"Unexpected character '{markerExpression[i]}' in marker expression '{markerExpression}'.");
                }

                tokens.Add(markerExpression.Substring(identifierStart, i - identifierStart));
            }

            return tokens;
        }
    }
}
