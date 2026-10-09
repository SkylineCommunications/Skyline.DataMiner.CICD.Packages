namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using Skyline.DataMiner.CICD.Parsers.Protocol.Xml.EdgeScripts;
    using Skyline.DataMiner.CICD.Parsers.Protocol.Xml.QActions;

    /// <summary>
    /// Represents a connector solution.
    /// </summary>
    public class ProtocolSolution : Solution
    {
        private readonly ILogCollector logCollector;

        private ProtocolSolution(string solutionPath, ILogCollector logCollector) : base(solutionPath)
        {
            this.logCollector = logCollector;

            LoadProtocol();
            LoadQActions();
            LoadScripts();
        }

        /// <summary>
        /// Gets the protocol XML document.
        /// </summary>
        /// <value>The protocol XML document.</value>
        public XmlDocument ProtocolDocument { get; private set; }

        /// <summary>
        /// Gets the QAction projects.
        /// </summary>
        /// <value>The QAction projects.</value>
        public ICollection<QAction> QActions { get; private set; }

        /// <summary>
        /// Gets the connector script projects declared under
        /// <c>&lt;Protocol&gt;&lt;Edge&gt;&lt;Scripts&gt;</c> in protocol.xml.
        /// </summary>
        /// <value>The script projects.</value>
        public ICollection<ProtocolScript> Scripts { get; private set; }

        /// <summary>
        /// Parses the specified connector solution.
        /// </summary>
        /// <param name="solutionPath">The connector solution file path.</param>
        /// <param name="logCollector">Log collector.</param>
        /// <returns>The parsed solution.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="solutionPath"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="solutionPath"/> is empty or does not exist.</exception>
        /// <exception cref="FileNotFoundException">The specified solution path does not exist. -or-
        /// Could not find protocol.xml file in 'Solution Items' folder. -or-
        /// Could not find project. -or-
        /// Could not find QAction content file.</exception>
        /// <exception cref="ParserException">Could not find folder 'Solution Items' in solution. -or-
        /// Could not find project -or-
        /// Main code file could not be found in QAction. -or-
        /// Could not find a 'ConnectorScript_*' folder whose 'manifest.json' 'project.id' matches a declared script's guid.</exception>
        /// <exception cref="Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest.InvalidManifestException">
        /// A declared script's 'manifest.json' is missing, malformed, or fails validation.</exception>
        /// <exception cref="DirectoryNotFoundException">Could not find folder for QAction.</exception>
        public new static ProtocolSolution Load(string solutionPath, ILogCollector logCollector = null)
        {
            if (solutionPath == null) throw new ArgumentNullException(nameof(solutionPath));
            if (String.IsNullOrWhiteSpace(solutionPath)) throw new ArgumentNullException(nameof(solutionPath), "Solution path is invalid.");

            if (!FileSystem.Instance.File.Exists(solutionPath)) throw new FileNotFoundException($"The specified solution path '{solutionPath}' does not exist.");

            return new ProtocolSolution(solutionPath, logCollector);
        }
        
        private void LoadProtocol()
        {
            logCollector?.ReportDebug("Load protocol");
            var solutionItems = GetSubFolder("Solution Items");
            if (solutionItems == null)
            {
                throw new ParserException("Could not find folder 'Solution Items' in solution.");
            }

            var protocolFile = solutionItems.Files.FirstOrDefault(f => String.Equals(f.FileName, "protocol.xml", StringComparison.OrdinalIgnoreCase));

            if (protocolFile != null && FileSystem.Instance.File.Exists(protocolFile.AbsolutePath))
            {
                ProtocolDocument = XmlDocument.Load(protocolFile.AbsolutePath);
            }
            else
            {
                throw new FileNotFoundException("Could not find protocol.xml file in 'Solution Items' folder");
            }
        }

        private void LoadQActions()
        {
            logCollector?.ReportDebug("Load QActions");
            var qactions = new List<QAction>();

            var xmlQActions = ProtocolDocument?.Element["Protocol"]?.Element["QActions"]?.Elements["QAction"];
            if (xmlQActions != null)
            {
                foreach (var xmlQA in xmlQActions)
                {
                    var qa = new QAction(xmlQA);
                    int id = qa.Id;

                    if (String.Equals(qa.Encoding, "csharp", StringComparison.OrdinalIgnoreCase))
                    {
                        qa = LoadQAction(id);
                    }

                    qactions.Add(qa);
                }
            }

            QActions = qactions;
        }

        private QAction LoadQAction(int qactionId)
        {
            string projectName = $"QAction_{qactionId}";

            var projectInSolution = Projects.FirstOrDefault(p => String.Equals(p.Name, projectName));
            if (projectInSolution == null)
            {
                throw new ParserException($"Could not find project with name '{projectName}'");
            }

            Project project = LoadProject(projectInSolution);

            var files = new List<QActionCodeFile>();

            foreach (var file in project.Files)
            {
                string fileName = file.Name;

                if (!String.Equals(FileSystem.Instance.Path.GetExtension(fileName), ".cs", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(FileSystem.Instance.Path.GetFileName(fileName), "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                files.Add(new QActionCodeFile(fileName, file.Content));
            }

            List<string> dllImports = ExtractDllImports(project).ToList();

            return new QAction(qactionId, files, dllImports);
        }

        private IEnumerable<string> ExtractDllImports(Project project)
        {
            foreach (var r in project.References)
            {
                string dll = r.GetDllName();

                if (!String.IsNullOrWhiteSpace(dll))
                {
                    yield return dll;
                }
            }

            foreach (var r in project.ProjectReferences)
            {
                var m = QAction.RegexExtractQActionID.Match(r.Name);
                if (m.Success)
                {
                    // this is a referenced QAction
                    int id = Convert.ToInt32(m.Groups["id"].Value);
                    string dll = $"[ProtocolName].[ProtocolVersion].QAction.{id}.dll";
                    yield return dll;
                }
            }
        }

        private void LoadScripts()
        {
            logCollector?.ReportDebug("Load Scripts");
            var scripts = new List<ProtocolScript>();

            var xmlScripts = ProtocolDocument?.Element["Protocol"]?.Element["Edge"]?.Element["Scripts"]?.Elements["Script"];
            if (xmlScripts == null)
            {
                Scripts = scripts;
                return;
            }

            var seenGuids = new HashSet<Guid>();

            foreach (var xmlScript in xmlScripts)
            {
                var edgeScript = new EdgeScript(xmlScript);

                if (!seenGuids.Add(edgeScript.Guid))
                {
                    throw new ParserException($"Duplicate guid '{edgeScript.Guid}' found on script '{edgeScript.Id}' in 'Edge/Scripts'. Each declared script must have a unique guid.");
                }

                scripts.Add(LoadScript(edgeScript));
            }

            Scripts = scripts;
        }

        private ProtocolScript LoadScript(EdgeScript edgeScript)
        {
            var matchingProjectFolders = new List<string>();

            foreach (var candidateFolder in FileSystem.Instance.Directory.EnumerateDirectories(SolutionDirectory))
            {
                string folderName = FileSystem.Instance.Path.GetFileName(candidateFolder);
                if (!folderName.StartsWith("ConnectorScript_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string manifestPath = FileSystem.Instance.Path.Combine(candidateFolder, "manifest.json");
                if (!FileSystem.Instance.File.Exists(manifestPath))
                {
                    continue;
                }

                Manifest manifest;
                try
                {
                    manifest = Manifest.Parse(FileSystem.Instance.File.ReadAllText(manifestPath));
                }
                catch (Exception)
                {
                    // Invalid manifest.json in an unrelated folder; keep looking for a match.
                    continue;
                }

                if (manifest?.Project != null && manifest.Project.Id == edgeScript.Guid)
                {
                    matchingProjectFolders.Add(candidateFolder);
                }
            }

            if (matchingProjectFolders.Count == 0)
            {
                throw new ParserException($"Could not find a 'ConnectorScript_*' folder in '{SolutionDirectory}' with a 'manifest.json' 'project.id' matching guid '{edgeScript.Guid}' declared for script '{edgeScript.Id}'.");
            }

            if (matchingProjectFolders.Count > 1)
            {
                throw new ParserException($"Multiple 'ConnectorScript_*' folders in '{SolutionDirectory}' have a 'manifest.json' 'project.id' matching guid '{edgeScript.Guid}' declared for script '{edgeScript.Id}': {String.Join(", ", matchingProjectFolders)}.");
            }

            string projectDirectory = matchingProjectFolders[0];
            string requirementsFilePath = FileSystem.Instance.Path.Combine(projectDirectory, "requirements.txt");

            // Fully load and validate the manifest now that the single matching folder has been found.
            Manifest validatedManifest = ManifestLoader.LoadAndValidate(projectDirectory);
            return new ProtocolScript(edgeScript, projectDirectory, requirementsFilePath, validatedManifest);
        }
    }
}
