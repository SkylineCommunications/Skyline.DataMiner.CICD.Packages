namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a resolved pip invocation, consisting of an executable and a set of leading arguments that need to be
    /// prepended to any pip command (e.g. <c>python -m pip</c> or just <c>pip</c>).
    /// </summary>
    internal sealed class PipCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipCommand"/> class.
        /// </summary>
        /// <param name="fileName">The executable to start (e.g. <c>python</c> or <c>pip</c>).</param>
        /// <param name="leadingArguments">Arguments that must be prepended to every pip invocation (e.g. <c>-m pip</c>).</param>
        public PipCommand(string fileName, IReadOnlyList<string> leadingArguments)
        {
            FileName = fileName;
            LeadingArguments = leadingArguments;
        }

        /// <summary>
        /// Gets the executable to start.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the arguments that must be prepended to every pip invocation.
        /// </summary>
        public IReadOnlyList<string> LeadingArguments { get; }

        /// <summary>
        /// Creates a <see cref="ProcessStartInfo"/> for invoking pip with the specified arguments.
        /// </summary>
        /// <param name="arguments">The pip arguments (e.g. "download", "-r", "requirements.txt", ...).</param>
        /// <returns>The configured <see cref="ProcessStartInfo"/>.</returns>
        public ProcessStartInfo CreateProcessStartInfo(IEnumerable<string> arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = BuildArgumentString(LeadingArguments.Concat(arguments)),
            };

            return startInfo;
        }

        /// <summary>
        /// Builds a properly quoted command line argument string, since <c>ProcessStartInfo.ArgumentList</c> is not
        /// available on all targeted frameworks (e.g. .NET Framework 4.8).
        /// </summary>
        private static string BuildArgumentString(IEnumerable<string> arguments)
        {
            var builder = new StringBuilder();

            foreach (var argument in arguments)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                AppendQuotedArgument(builder, argument);
            }

            return builder.ToString();
        }

        private static void AppendQuotedArgument(StringBuilder builder, string argument)
        {
            bool needsQuotes = argument.Length == 0 || argument.IndexOfAny(new[] { ' ', '\t', '"' }) >= 0;

            if (!needsQuotes)
            {
                builder.Append(argument);
                return;
            }

            builder.Append('"');

            for (int i = 0; i < argument.Length; i++)
            {
                int backslashCount = 0;
                while (i < argument.Length && argument[i] == '\\')
                {
                    backslashCount++;
                    i++;
                }

                if (i == argument.Length)
                {
                    // Backslashes at the end of the argument must be doubled since they precede the closing quote.
                    builder.Append('\\', backslashCount * 2);
                    break;
                }

                if (argument[i] == '"')
                {
                    // Backslashes preceding a quote must be doubled, and the quote itself escaped.
                    builder.Append('\\', (backslashCount * 2) + 1);
                    builder.Append('"');
                }
                else
                {
                    builder.Append('\\', backslashCount);
                    builder.Append(argument[i]);
                }
            }

            builder.Append('"');
        }
    }
}
