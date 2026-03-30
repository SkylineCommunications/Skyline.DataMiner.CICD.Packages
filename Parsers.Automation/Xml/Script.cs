namespace Skyline.DataMiner.CICD.Parsers.Automation.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Represents a script of an Automation script solution.
    /// </summary>
    public class Script
    {
        private readonly IList<ScriptExe> _scriptExes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Script"/> class.
        /// </summary>
        /// <param name="document">The Automation script document.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is <see langword="null"/>.</exception>
        public Script(XmlDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            _scriptExes = new List<ScriptExe>();

            LoadScript();
        }

        /// <summary>
        /// Gets the script document.
        /// </summary>
        /// <value>The script document.</value>
        public XmlDocument Document { get; }

        /// <summary>
        /// Gets the name of the script.
        /// </summary>
        /// <value>The name of the script.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Exe blocks of the Automation script.
        /// </summary>
        /// <vamie>The Exe blocks of the Automation script.</vamie>
        public IEnumerable<ScriptExe> ScriptExes => _scriptExes;

        /// <summary>
        /// Loads the specified Automation script.
        /// </summary>
        /// <param name="path">The file path of the Automation script.</param>
        /// <returns>The loaded script.</returns>
        /// <exception cref="System.IO.FileNotFoundException">The specified Automation script was not found.</exception>
        public static Script Load(string path)
        {
            if (!FileSystem.Instance.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException("The Automation script file '" + path + "' could not be found.", path);
            }

            var document = XmlDocument.Load(path);
            return new Script(document);
        }

        /// <summary>
        /// Determines whether the specified file is an Automation script.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <returns><c>true</c> if the specified file is an Automation script; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <see langword="null"/> or whitespace.</exception>
        public static bool IsAutomationScriptFile(string path, ILogCollector logCollector = null)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));
            }

            try
            {
                var xmlContent = FileSystem.Instance.File.ReadAllText(path);
                var doc = XDocument.Parse(xmlContent);
                return String.Equals(doc.Root?.Name?.LocalName, "DMSScript", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                logCollector?.ReportError("XDocument failed to Load with exception: " + e);
                return false;
            }
        }

        private void LoadScript()
        {
            var dmsScript = Document?.Element["DMSScript"];
            if (dmsScript == null)
            {
                return;
            }

            Name = dmsScript.Element["Name"]?.InnerText?.Trim();

            var exes = dmsScript.Element["Script"]?.Elements["Exe"];
            if (exes == null)
            {
                return;
            }

            foreach (var exe in exes)
            {
                var scriptExe = new ScriptExe(exe);
                _scriptExes.Add(scriptExe);
            }
        }
    }
}