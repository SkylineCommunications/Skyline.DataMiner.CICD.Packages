namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Models.Protocol.Enums;
    using Skyline.DataMiner.CICD.Models.Protocol.Read;
    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    using EditXml = Skyline.DataMiner.CICD.Parsers.Common.XmlEdit;
    using ProtocolDocumentEdit = Skyline.DataMiner.CICD.Models.Protocol.Edit.ProtocolDocumentEdit;
    using QActionsQAction = Skyline.DataMiner.CICD.Models.Protocol.Edit.QActionsQAction;
    using QActions = Skyline.DataMiner.CICD.Models.Protocol.Edit.QActions;

    /// <summary>
    /// Protocol builder.
    /// </summary>
    public class ProtocolBuilder
    {
        private readonly ILogCollector logCollector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolBuilder"/> class using the specified protocol solution.
        /// </summary>
        /// <param name="solution">The protocol solution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="solution"/> is <see langword="null"/>.</exception>
        public ProtocolBuilder(ProtocolSolution solution)
        {
            if (solution == null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            Document = solution.ProtocolDocument;
            Model = new ProtocolModel(Document);

            Projects = new Dictionary<string, Project>();

            foreach (var p in solution.Projects)
            {
                Projects[p.Name] = solution.LoadProject(p);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolBuilder"/> class using the specified protocol solution.
        /// </summary>
        /// <param name="solution">The protocol solution.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <exception cref="ArgumentNullException"><paramref name="solution"/> is <see langword="null"/>.</exception>
        public ProtocolBuilder(ProtocolSolution solution, ILogCollector logCollector)
            : this(solution)
        {
            this.logCollector = logCollector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolBuilder"/> class using the specified protocol solution.
        /// </summary>
        /// <param name="solution">The protocol solution.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <param name="overrideVersion">Version that will be used to override the version in the protocol itself.</param>
        /// <exception cref="ArgumentNullException"><paramref name="solution"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="overrideVersion"/> is <see langword="null"/>, empty or whitespace.</exception>
        public ProtocolBuilder(ProtocolSolution solution, ILogCollector logCollector, string overrideVersion)
            : this(solution, logCollector)
        {
            if (String.IsNullOrWhiteSpace(overrideVersion))
            {
                throw new ArgumentException(nameof(overrideVersion));
            }

            var edit = new EditXml.XmlDocument(solution.ProtocolDocument);
            var version = edit.Root.Element["Version"];
            if (version == null)
            {
                throw new AssemblerException("Protocol does not contain a Version tag.");
            }

            version.InnerText = overrideVersion;

            // Override the Document & Model as it now has the overridden version.
            Document = XmlDocument.Parse(edit.GetXml());
            Model = new ProtocolModel(Document);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolBuilder"/> class using the specified document and projects.
        /// </summary>
        /// <param name="document">The protocol XML document.</param>
        /// <param name="projects">The project dictionary.</param>
        /// <param name="overrideVersion">Version that will be used to override the version in the protocol itself.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/>, or <paramref name="projects"/> is <see langword="null"/>.</exception>
        internal ProtocolBuilder(XmlDocument document, IDictionary<string, Project> projects, string overrideVersion = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (!String.IsNullOrWhiteSpace(overrideVersion))
            {
                var edit = new EditXml.XmlDocument(document);
                var version = edit.Root.Element["Version"];
                if (version == null)
                {
                    throw new AssemblerException("Protocol does not contain a Version tag.");
                }

                version.InnerText = overrideVersion;

                Document = XmlDocument.Parse(edit.GetXml());
            }
            else
            {
                Document = document;
            }

            Model = new ProtocolModel(Document);
            Projects = projects ?? throw new ArgumentNullException(nameof(projects));
        }

        /// <summary>
        /// Gets the XML document of the protocol.
        /// </summary>
        /// <value>The protocol model.</value>
        public XmlDocument Document { get; }

        /// <summary>
        /// Gets the protocol model/
        /// </summary>
        /// <value>The protocol model.</value>
        public IProtocolModel Model { get; }

        /// <summary>
        /// Gets the projects dictionary.
        /// </summary>
        /// <value>The projects dictionary.</value>
        public IDictionary<string, Project> Projects { get; }

        /// <summary>
        /// Builds the protocol assembling the protocol document and projects into the full protocol XML document and assemblies of NuGet packages.
        /// </summary>
        /// <returns>The build result items.</returns>
        /// <exception cref="AssemblerException">Project with name '{projectName}' could not be found -or-
        /// No code files found for QAction -or-
        /// File could not be found in project -or-
        /// Cannot replace QAction, because the target XML node is not empty.</exception>
        public async Task<BuildResultItems> BuildAsync()
        {
            logCollector?.ReportStatus("Building the protocol.");
            var protocolEdit = new ProtocolDocumentEdit(Document, Model);
            return await BuildResultsAsync(protocolEdit).ConfigureAwait(false);
        }

        /// <summary>
        /// Combines all the cs files in the project into a single string.
        /// </summary>
        /// <param name="project">Project.</param>
        /// <returns>String with all the code from the cs files.</returns>
        /// <exception cref="AssemblerException">No code files found in specified project.</exception>
        public static string CombineProjectFiles(Project project)
        {
            var files = GetRelevantCodeFilesSorted(project);

            if (files.Count == 0)
            {
                throw new AssemblerException($"No code files found in project '{project.AssemblyName}'");
            }

            return CSharpCodeCombiner.CombineFiles(files);
        }

        /// <summary>
        /// Retrieves the C# files (.cs) of the QAction project in sorted order.
        /// </summary>
        /// <param name="project">The QAction project.</param>
        /// <returns>The C# files (.cs) of the QAction project in sorted order.</returns>
        /// <remarks>This method excludes any AssemblyInfo.cs file present in the project.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="project"/> is <see langword="null"/>.</exception>
        public static IList<ProjectFile> GetRelevantCodeFilesSorted(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var files = project.Files
                               .Where(x => x.Name.EndsWith(".cs") && !x.Name.EndsWith("AssemblyInfo.cs"))
                               .OrderByDescending(x => x.Name.StartsWith("QAction_"))
                               .ThenBy(x => x.Name)
                               .ToList();

            return files;
        }

        private async Task<BuildResultItems> BuildResultsAsync(ProtocolDocumentEdit protocolEdit)
        {
            BuildResultItems buildResultItems = new BuildResultItems();

            await BuildQActions(protocolEdit, buildResultItems, Model?.Protocol?.Compliancies).ConfigureAwait(false);
            BuildVersionHistoryComment(protocolEdit, Model);
            buildResultItems.Document = protocolEdit.Document.GetXml();

            return buildResultItems;
        }

        private async Task BuildQActions(ProtocolDocumentEdit protocolEdit, BuildResultItems buildResultItems, ICompliancies compliancies)
        {
            logCollector?.ReportDebug("Building QActions");
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            var qactions = protocolEdit.Protocol?.QActions;
            if (qactions != null)
            {
                ReorderQActionsByDependency(qactions);

                foreach (var qa in qactions)
                {
                    await BuildQAction(qa, qactions.Read, packageReferenceProcessor, buildResultItems, compliancies).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Reorders the QActions so that any QAction referenced (via a project reference) by another QAction
        /// is positioned before the referencing QAction, and marks the referenced QAction with the "precompile" option.
        /// If a circular dependency is detected between QActions, the order is left untouched.
        /// </summary>
        /// <param name="qactions">The QActions collection to reorder.</param>
        private void ReorderQActionsByDependency(QActions qactions)
        {
            var qaList = qactions.ToList();

            // Build a dependency map: QAction Id -> set of QAction Ids it references (project references to other QAction projects).
            var dependencies = new Dictionary<uint, HashSet<uint>>();
            var qaById = new Dictionary<uint, QActionsQAction>();

            foreach (var qa in qaList)
            {
                if (qa.Id?.Value == null)
                {
                    continue;
                }

                uint qaId = qa.Id.Value.Value;
                qaById[qaId] = qa;

                string projectName = $"QAction_{qaId}";
                if (!Projects.TryGetValue(projectName, out var project) || project.ProjectReferences == null)
                {
                    continue;
                }

                var referencedIds = new HashSet<uint>();
                foreach (var r in project.ProjectReferences)
                {
                    if (Helper.TryGetQActionId(r, out int refId) && refId != qaId)
                    {
                        referencedIds.Add((uint)refId);
                    }
                }

                if (referencedIds.Count > 0)
                {
                    dependencies[qaId] = referencedIds;
                }
            }

            if (dependencies.Count == 0)
            {
                // No dependencies between QActions, nothing to do.
                return;
            }

            if (HasCircularDependency(dependencies))
            {
                logCollector?.ReportWarning("ProtocolBuilder|ReorderQActionsByDependency|Circular dependency detected between QActions. Skipping reordering.");
                return;
            }

            var referencedIdsFlat = new HashSet<uint>(dependencies.Values.SelectMany(x => x));
            foreach (var referencedId in referencedIdsFlat)
            {
                if (qaById.TryGetValue(referencedId, out var referencedQa))
                {
                    MarkAsPrecompile(referencedQa);
                }
            }

            // Perform a topological sort so that referenced QActions come before the QActions that reference them.
            List<QActionsQAction> sortedQActions = TopologicalSort(qaList, dependencies);

            // Reorder the underlying XML nodes to match the sorted order.
            ApplyOrderToXml(qactions, sortedQActions);
        }

        /// <summary>
        /// Detects whether the dependency graph contains a circular dependency.
        /// </summary>
        /// <param name="dependencies">The dependency graph.</param>
        /// <returns><c>true</c> if the dependency graph contains a circular dependency; otherwise, <c>false</c>.</returns>
        /// <remarks>This should never occur (as Visual Studio does not allow adding a circular project reference) but this is added as an additional safeguard.</remarks>
        private static bool HasCircularDependency(Dictionary<uint, HashSet<uint>> dependencies)
        {
            var visiting = new HashSet<uint>();
            var visited = new HashSet<uint>();

            foreach (var node in dependencies.Keys)
            {
                if (HasCycle(node, dependencies, visiting, visited))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified node is part of a cycle in the dependency graph.
        /// </summary>
        /// <param name="node">The node to check for a cycle.</param>
        /// <param name="dependencies">The dependency graph.</param>
        /// <param name="visiting">The set of nodes currently being visited.</param>
        /// <param name="visited">The set of nodes that have already been visited.</param>
        /// <returns><c>true</c> if the node is part of a cycle; otherwise, <c>false</c>.</returns>
        private static bool HasCycle(uint node, Dictionary<uint, HashSet<uint>> dependencies, HashSet<uint> visiting, HashSet<uint> visited)
        {
            if (visited.Contains(node))
            {
                return false;
            }

            if (visiting.Contains(node))
            {
                return true;
            }

            visiting.Add(node);

            if (dependencies.TryGetValue(node, out var dependencyIds))
            {
                foreach (var dependencyId in dependencyIds)
                {
                    if (HasCycle(dependencyId, dependencies, visiting, visited))
                    {
                        return true;
                    }
                }
            }

            visiting.Remove(node);
            visited.Add(node);

            return false;
        }

        /// <summary>
        /// Performs a topological sort, keeping the original relative order of unrelated QActions as much as possible.
        /// </summary>
        private static List<QActionsQAction> TopologicalSort(List<QActionsQAction> qactions, Dictionary<uint, HashSet<uint>> dependencies)
        {
            var result = new List<QActionsQAction>();
            var visited = new HashSet<uint>();

            foreach (var qa in qactions)
            {
                Visit(qa);
            }

            return result;

            void Visit(QActionsQAction qaction)
            {
                uint? id = qaction.Id?.Value;
                if (id == null || visited.Contains(id.Value))
                {
                    return;
                }

                visited.Add(id.Value);

                if (dependencies.TryGetValue(id.Value, out var dependencyIds))
                {
                    foreach (var dependencyId in dependencyIds)
                    {
                        var depQaction = qactions.FirstOrDefault(q => q.Id?.Value == dependencyId);
                        if (depQaction != null)
                        {
                            Visit(depQaction);
                        }
                    }
                }

                result.Add(qaction);
            }
        }

        /// <summary>
        /// Reorders the QActions collection to match the specified order.
        /// </summary>
        /// <param name="qactions">The QActions collection to reorder (also backs the underlying XML).</param>
        /// <param name="sortedQActions">The QActions in the desired order.</param>
        private static void ApplyOrderToXml(QActions qactions, List<QActionsQAction> sortedQActions)
        {
            if (sortedQActions.Count == 0)
            {
                return;
            }

            qactions.Clear();

            foreach (var qa in sortedQActions)
            {
                qactions.Add(qa);
            }
        }

        /// <summary>
        /// Ensures the specified QAction has the "precompile" option set, preserving any other existing options.
        /// </summary>
        private static void MarkAsPrecompile(QActionsQAction qa)
        {
            const string precompileOption = "precompile";

            if (qa.Options == null || String.IsNullOrWhiteSpace(qa.Options.RawValue))
            {
                qa.Options = precompileOption;
            }
            else if (!qa.Options.RawValue.Split(';').Any(o => String.Equals(o.Trim(), precompileOption, StringComparison.OrdinalIgnoreCase)))
            {
                qa.Options = precompileOption + ";" + qa.Options;
            }
        }

        private async Task BuildQAction(QActionsQAction qa, IQActions allQActions, PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems, ICompliancies compliancies)
        {
            if (qa.Encoding?.Value != EnumQActionEncoding.Csharp)
            {
                // skip JScript etc..
                return;
            }

            var qaId = qa.Id?.Value;

            string projectName = $"QAction_{qaId}";
            if (!Projects.TryGetValue(projectName, out var project))
            {
                throw new AssemblerException($"Project with name '{projectName}' could not be found!");
            }

            // replace code
            var hasQActionCodeChanges = BuildQActionCode(qa, project, qaId);

            // DLL imports
            var hasDllImportChanges = await BuildQActionDllImports(qa, project, allQActions, packageReferenceProcessor, buildResultItems, compliancies).ConfigureAwait(false);

            // format
            if (hasQActionCodeChanges || hasDllImportChanges)
            {
                qa.EditNode.Format();
            }
        }

        private static bool BuildQActionCode(QActionsQAction qa, Project project, uint? qaId)
        {
            bool hasChanges = false;

            string newCode = CombineProjectFiles(project);
            if (!String.IsNullOrWhiteSpace(newCode))
            {
                if (!String.IsNullOrWhiteSpace(qa.Code) && !String.Equals(qa.Code, newCode))
                {
                    throw new AssemblerException($"Cannot replace QAction {qaId}, because the target XML node is not empty!");
                }

                qa.Code = newCode;
                hasChanges = true;
            }

            return hasChanges;
        }

        private static async Task<bool> BuildQActionDllImports(QActionsQAction qa, Project project, IQActions allQActions, PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems, ICompliancies compliancies)
        {
            bool hasChanges = false;

            var dllImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string dllsFolder = null;
            if (project.Path != null)
            {
                dllsFolder = FileSystem.Instance.Path.Combine(FileSystem.Instance.Directory.GetParentDirectory(FileSystem.Instance.Path.GetDirectoryName(project.Path)), @"Dlls\");
            }

            NuGetPackageAssemblyData nugetAssemblyData = await ProcessPackageReferences(project, packageReferenceProcessor, buildResultItems, dllImports);
            ProcessReferences(project, packageReferenceProcessor, compliancies, nugetAssemblyData, dllsFolder, dllImports, buildResultItems);
            ProcessProjectReferences(project, allQActions, dllImports);

            // Edit QAction@dllImport
            if (dllImports.Count > 0)
            {
                qa.DllImport = String.Join(";", dllImports);
                hasChanges = true;
            }

            return hasChanges;
        }

        private static async Task<NuGetPackageAssemblyData> ProcessPackageReferences(Project project, PackageReferenceProcessor packageReferenceProcessor,
            BuildResultItems buildResultItems, HashSet<string> dllImports)
        {
            if (project.PackageReferences == null)
            {
                return null;
            }

            List<PackageIdentity> packageIdentities = GetPackageIdentities(project);

            if (packageIdentities.Count <= 0)
            {
                return null;
            }

            NuGetPackageAssemblyData nugetAssemblyData = await AssemblyFilter.FilterAsync(project.TargetFrameworkMoniker, packageReferenceProcessor, buildResultItems, dllImports, packageIdentities).ConfigureAwait(false);

            return nugetAssemblyData;
        }

        private static List<PackageIdentity> GetPackageIdentities(Project project)
        {
            var packageIdentities = new List<PackageIdentity>();

            foreach (var packageReference in project.PackageReferences)
            {
                var packageIdentity = new PackageIdentity(packageReference.Name, NuGetVersion.Parse(packageReference.Version));
                packageIdentities.Add(packageIdentity);
            }

            return packageIdentities;
        }

        private static void ProcessProjectReferences(Project project, IQActions allQActions, HashSet<string> dllImports)
        {
            if (project.ProjectReferences != null)
            {
                foreach (var r in project.ProjectReferences)
                {
                    if (r.Name == "QAction_ClassLibrary")
                    {
                        // Do nothing (no longer supported).
                    }
                    else if (r.Name == "QAction_Helper")
                    {
                        // Do nothing.
                    }
                    else if (Helper.TryGetQActionId(r, out int id))
                    {
                        var refQA = allQActions.FirstOrDefault(q => q.Id?.Value == id);
                        if (refQA != null)
                        {
                            string dll;

                            var options = refQA.GetOptions();
                            if (!String.IsNullOrWhiteSpace(options?.CustomDllName))
                            {
                                dll = $"[ProtocolName].[ProtocolVersion].{options.CustomDllName}";
                            }
                            else
                            {
                                dll = $"[ProtocolName].[ProtocolVersion].QAction.{id}.dll";
                            }

                            dllImports.Add(dll);
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(r.Name))
                    {
                        dllImports.Add($"{r.Name}.dll");
                    }
                }
            }
        }

        private static void ProcessReferences(Project project, PackageReferenceProcessor packageReferenceProcessor, ICompliancies compliancies,
            NuGetPackageAssemblyData nugetAssemblyData, string dllsFolder, HashSet<string> dllImports, BuildResultItems buildResultItems)
        {
            if (project.References == null)
            {
                return;
            }

            foreach (Reference r in project.References)
            {
                string dllName = r.GetDllName();

                if (IsDllDefaultInQAction(dllName, compliancies) || DevPackHelper.IsDevPackDllReference(r) ||
                    (nugetAssemblyData != null && nugetAssemblyData.ProcessedAssemblies.Contains(dllName)))
                {
                    continue;
                }

                if (r.HintPath?.Contains(packageReferenceProcessor.NuGetRootPath) == true)
                {
                    // DLL is from a NuGet but is transitive from precompile or other project reference.
                    // These can be ignored.
                    continue;
                }

                if (!String.IsNullOrWhiteSpace(r.HintPath) && FileSystem.Instance.Path.IsPathRooted(r.HintPath))
                {
                    string absolutePath = r.HintPath;

                    if (absolutePath.StartsWith(@"C:\Skyline DataMiner\ProtocolScripts\DllImport\"))
                    {
                        dllName = absolutePath.Substring(47);
                    }
                    else if (absolutePath.StartsWith(@"C:\Skyline DataMiner\ProtocolScripts\"))
                    {
                        dllName = absolutePath.Substring(37);
                    }
                    else if (absolutePath.StartsWith(@"C:\Skyline DataMiner\Files\"))
                    {
                        dllName = absolutePath.Substring(27);
                    }
                    else if (dllsFolder != null && absolutePath.StartsWith(dllsFolder))
                    {
                        dllName = absolutePath.Substring(dllsFolder.Length);
                    }
                }

                dllImports.Add(dllName);

                // If custom DLL
                if (!String.IsNullOrWhiteSpace(r.HintPath) && project.ProjectStyle == ProjectStyle.Sdk)
                {
                    string dllPath = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(project.ProjectDirectory, r.HintPath));

                    buildResultItems.DllAssemblies.Add(new DllAssemblyReference(dllName, dllPath));
                }
            }
        }

        private static bool IsDllDefaultInQAction(string dllName, ICompliancies compliancies)
        {
            if (dllName.Equals("System.Xml.dll", StringComparison.OrdinalIgnoreCase))
            {
                string minimumRequiredVersion = compliancies?.MinimumRequiredVersion?.Value ?? String.Empty;
                string[] versionParts = minimumRequiredVersion.Split('.');

                // See RN 19494.
                if (!Int32.TryParse(versionParts[0], out int major) || major >= 10)
                {
                    return true;
                }
            }
            else if (Helper.QActionDefaultImportDLLs.Any(d => String.Equals(d, dllName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private void BuildVersionHistoryComment(ProtocolDocumentEdit protocolEdit, IProtocolModel model)
        {
            logCollector?.ReportDebug("Building version history comment");
            var versionHistory = model.Protocol?.VersionHistory;
            if (versionHistory == null)
            {
                return;
            }

            var xmlEdit = protocolEdit.Document;
            var xmlProtocol = xmlEdit.TryFindNode(model.Protocol.ReadNode);

            var firstXmlComment = xmlEdit.Children.OfType<EditXml.XmlComment>().FirstOrDefault();
            if (firstXmlComment == null || xmlEdit.Children.IndexOf(firstXmlComment) > xmlEdit.Children.IndexOf(xmlProtocol))
            {
                firstXmlComment = new EditXml.XmlComment("");
                xmlEdit.Children.InsertBefore(xmlProtocol, firstXmlComment);
                xmlEdit.Children.InsertBefore(xmlProtocol, new EditXml.XmlText("\r\n"));
            }

            firstXmlComment.InnerText += "\r\n\r\n" + BuildVersionHistoryComment(versionHistory);
        }

        private static string BuildVersionHistoryComment(IVersionHistory versionHistory)
        {
            if (versionHistory is null)
            {
                throw new ArgumentNullException(nameof(versionHistory));
            }

            AsciiTable table = new AsciiTable();

            void appendComment(string datetime, string version, string author, string comment)
            {
                var commentLines = GetLines(comment).ToList();

                table.AddRow(datetime, version, author, commentLines.Count > 0 ? commentLines[0] : "");

                for (int i = 1; i < commentLines.Count; i++)
                {
                    table.AddRow("", "", "", commentLines[i]);
                }
            }

            table.AddRow("DATE", "VERSION", "AUTHOR", "COMMENTS");
            table.AddRow();

            foreach (var branch in versionHistory.Branches.OrEmptyIfNull().OrderBy(x => x.Id?.Value))
            {
                foreach (var systemVersion in branch.SystemVersions.OrEmptyIfNull().OrderBy(x => x.Id?.Value))
                {
                    foreach (var majorVersion in systemVersion.MajorVersions.OrEmptyIfNull().OrderBy(x => x.Id?.Value))
                    {
                        foreach (var minorVersion in majorVersion.MinorVersions.OrEmptyIfNull().OrderBy(x => x.Id?.Value))
                        {
                            string date = minorVersion.Date?.Value?.ToString("dd/MM/yyyy");
                            string version = String.Join(".", branch.Id?.Value, systemVersion.Id?.Value, majorVersion.Id?.Value, minorVersion.Id?.Value);
                            string author = $"{minorVersion.Provider?.Author?.Value}, {minorVersion.Provider?.Company?.Value}";

                            StringBuilder sbComment = new StringBuilder();

                            foreach (var change in minorVersion.Changes)
                            {
                                switch (change)
                                {
                                    case IVersionHistoryBranchesBranchSystemVersionsSystemVersionMajorVersionsMajorVersionMinorVersionsMinorVersionChangesNewFeature nf:
                                        sbComment.AppendLine("NF: " + nf.Value);
                                        break;
                                    case IVersionHistoryBranchesBranchSystemVersionsSystemVersionMajorVersionsMajorVersionMinorVersionsMinorVersionChangesFix f:
                                        sbComment.AppendLine("Fix: " + f.Value);
                                        break;
                                    case IVersionHistoryBranchesBranchSystemVersionsSystemVersionMajorVersionsMajorVersionMinorVersionsMinorVersionChangesChange c:
                                        sbComment.AppendLine("Change: " + c.Value);
                                        break;
                                }
                            }

                            string comment = sbComment.ToString().Trim();

                            appendComment(date, version, author, comment);
                        }
                    }
                }
            }

            return "Revision History (auto generated):\r\n\r\n" + table.ToString();
        }

        private static IEnumerable<string> GetLines(string s)
        {
            StringReader sr = new StringReader(s);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
