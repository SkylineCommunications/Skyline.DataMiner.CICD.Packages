namespace Skyline.DataMiner.CICD.Parsers.Automation.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio;

    /// <summary>
    /// Represents an Automation script solution.
    /// </summary>
    public class AutomationScriptSolution : Solution
    {
        private readonly ICollection<(Script Script, SolutionFolder Folder)> _scripts = new List<(Script, SolutionFolder)>();
        private readonly IFileSystem _fileSystem = FileSystem.Instance;
        private readonly ILogCollector logCollector;

        private AutomationScriptSolution(string solutionPath, ILogCollector logCollector) : base(solutionPath)
        {
            if (!_fileSystem.File.Exists(solutionPath))
            {
                throw new System.IO.FileNotFoundException("Could not find solution file: " + solutionPath);
            }

            this.logCollector = logCollector;
            LoadScripts();
        }

        /// <summary>
        /// Gets the scripts of the Automation script solution.
        /// </summary>
        /// <value>The scripts of the Automation script solution.</value>
        public IEnumerable<(Script Script, SolutionFolder Folder)> Scripts => _scripts;

        /// <summary>
        /// Loads the specified Automation script solution.
        /// </summary>
        /// <param name="solutionPath">The Automation script solution file path.</param>
        /// <param name="logCollector">Log collector.</param>
        /// <returns>The loaded Automation script solution.</returns>
        /// <exception cref="ParserException">Could not find 'Scripts' folder in root of solution.</exception>
        public static AutomationScriptSolution Load(string solutionPath, ILogCollector logCollector = null)
        {
            return new AutomationScriptSolution(solutionPath, logCollector);
        }

        private void LoadScripts()
        {
            var scriptsFolder = GetSubFolder("Scripts");
            if (scriptsFolder == null)
            {
                throw new ParserException("Could not find 'Scripts' folder in root of solution.");
            }

            foreach (var folder in scriptsFolder.GetDescendantFolders())
            {
                logCollector?.ReportStatus("Loading Scripts from Descendant Folder: " + folder.Name);

                foreach (var file in folder.Files)
                {
                    if (!String.Equals(_fileSystem.Path.GetExtension(file.FileName), ".xml", StringComparison.OrdinalIgnoreCase)
                        || !Script.IsAutomationScriptFile(file.AbsolutePath, logCollector))
                    {
                        continue;
                    }

                    try
                    {
                        var script = Script.Load(file.AbsolutePath);
                        _scripts.Add((script, folder));
                    }
                    catch (Exception e)
                    {
                        logCollector?.ReportError("Exception Loading Scripts Checking File: " + e);
                    }
                }
            }
        }
    }
}