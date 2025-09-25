namespace Skyline.DataMiner.CICD.Assemblers.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Common.NuGet;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    using EditXml = Skyline.DataMiner.CICD.Parsers.Common.XmlEdit;

    /// <summary>
    /// Automation script builder.
    /// </summary>
    public class AutomationScriptBuilder
    {
        private static readonly Regex RegexProjectPlaceholder = new Regex(@"\[Project:(?<projectName>.*)\]", RegexOptions.Compiled & RegexOptions.IgnoreCase);
        private static readonly HashSet<string> NetFramework481ReferenceAssemblies = new HashSet<string>(new[] { "Accessibility.dll", "CustomMarshalers.dll", "ISymWrapper.dll", "Microsoft.Activities.Build.dll", "Microsoft.Build.Conversion.v4.0.dll", "Microsoft.Build.dll", "Microsoft.Build.Engine.dll", "Microsoft.Build.Framework.dll", "Microsoft.Build.Tasks.v4.0.dll", "Microsoft.Build.Utilities.v4.0.dll", "Microsoft.CSharp.dll", "Microsoft.JScript.dll", "Microsoft.VisualBasic.Compatibility.Data.dll", "Microsoft.VisualBasic.Compatibility.dll", "Microsoft.VisualBasic.dll", "Microsoft.VisualC.dll", "Microsoft.VisualC.STLCLR.dll", "Microsoft.Win32.Primitives.dll", "mscorlib.dll", "netstandard.dll", "PresentationBuildTasks.dll", "PresentationCore.dll", "PresentationFramework.Aero.dll", "PresentationFramework.Aero2.dll", "PresentationFramework.AeroLite.dll", "PresentationFramework.Classic.dll", "PresentationFramework.dll", "PresentationFramework.Luna.dll", "PresentationFramework.Royale.dll", "ReachFramework.dll", "sysglobl.dll", "System.Activities.Core.Presentation.dll", "System.Activities.dll", "System.Activities.DurableInstancing.dll", "System.Activities.Presentation.dll", "System.AddIn.Contract.dll", "System.AddIn.dll", "System.AppContext.dll", "System.Collections.Concurrent.dll", "System.Collections.dll", "System.Collections.NonGeneric.dll", "System.Collections.Specialized.dll", "System.ComponentModel.Annotations.dll", "System.ComponentModel.Composition.dll", "System.ComponentModel.Composition.Registration.dll", "System.ComponentModel.DataAnnotations.dll", "System.ComponentModel.dll", "System.ComponentModel.EventBasedAsync.dll", "System.ComponentModel.Primitives.dll", "System.ComponentModel.TypeConverter.dll", "System.Configuration.dll", "System.Configuration.Install.dll", "System.Console.dll", "System.Core.dll", "System.Data.Common.dll", "System.Data.DataSetExtensions.dll", "System.Data.dll", "System.Data.Entity.Design.dll", "System.Data.Entity.dll", "System.Data.Linq.dll", "System.Data.OracleClient.dll", "System.Data.Services.Client.dll", "System.Data.Services.Design.dll", "System.Data.Services.dll", "System.Data.SqlXml.dll", "System.Deployment.dll", "System.Design.dll", "System.Device.dll", "System.Diagnostics.Contracts.dll", "System.Diagnostics.Debug.dll", "System.Diagnostics.FileVersionInfo.dll", "System.Diagnostics.Process.dll", "System.Diagnostics.StackTrace.dll", "System.Diagnostics.TextWriterTraceListener.dll", "System.Diagnostics.Tools.dll", "System.Diagnostics.TraceSource.dll", "System.Diagnostics.Tracing.dll", "System.DirectoryServices.AccountManagement.dll", "System.DirectoryServices.dll", "System.DirectoryServices.Protocols.dll", "System.dll", "System.Drawing.Design.dll", "System.Drawing.dll", "System.Drawing.Primitives.dll", "System.Dynamic.dll", "System.Dynamic.Runtime.dll", "System.EnterpriseServices.dll", "System.EnterpriseServices.Thunk.dll", "System.EnterpriseServices.Wrapper.dll", "System.Globalization.Calendars.dll", "System.Globalization.dll", "System.Globalization.Extensions.dll", "System.IdentityModel.dll", "System.IdentityModel.Selectors.dll", "System.IdentityModel.Services.dll", "System.IO.Compression.dll", "System.IO.Compression.FileSystem.dll", "System.IO.Compression.ZipFile.dll", "System.IO.dll", "System.IO.FileSystem.dll", "System.IO.FileSystem.DriveInfo.dll", "System.IO.FileSystem.Primitives.dll", "System.IO.FileSystem.Watcher.dll", "System.IO.IsolatedStorage.dll", "System.IO.Log.dll", "System.IO.MemoryMappedFiles.dll", "System.IO.Pipes.dll", "System.IO.UnmanagedMemoryStream.dll", "System.Linq.dll", "System.Linq.Expressions.dll", "System.Linq.Parallel.dll", "System.Linq.Queryable.dll", "System.Management.dll", "System.Management.Instrumentation.dll", "System.Messaging.dll", "System.Net.dll", "System.Net.Http.dll", "System.Net.Http.Rtc.dll", "System.Net.Http.WebRequest.dll", "System.Net.NameResolution.dll", "System.Net.NetworkInformation.dll", "System.Net.Ping.dll", "System.Net.Primitives.dll", "System.Net.Requests.dll", "System.Net.Security.dll", "System.Net.Sockets.dll", "System.Net.WebHeaderCollection.dll", "System.Net.WebSockets.Client.dll", "System.Net.WebSockets.dll", "System.Numerics.dll", "System.ObjectModel.dll", "System.Printing.dll", "System.Reflection.Context.dll", "System.Reflection.dll", "System.Reflection.Emit.dll", "System.Reflection.Emit.ILGeneration.dll", "System.Reflection.Emit.Lightweight.dll", "System.Reflection.Extensions.dll", "System.Reflection.Primitives.dll", "System.Resources.Reader.dll", "System.Resources.ResourceManager.dll", "System.Resources.Writer.dll", "System.Runtime.Caching.dll", "System.Runtime.CompilerServices.VisualC.dll", "System.Runtime.dll", "System.Runtime.DurableInstancing.dll", "System.Runtime.Extensions.dll", "System.Runtime.Handles.dll", "System.Runtime.InteropServices.dll", "System.Runtime.InteropServices.RuntimeInformation.dll", "System.Runtime.InteropServices.WindowsRuntime.dll", "System.Runtime.Numerics.dll", "System.Runtime.Remoting.dll", "System.Runtime.Serialization.dll", "System.Runtime.Serialization.Formatters.dll", "System.Runtime.Serialization.Formatters.Soap.dll", "System.Runtime.Serialization.Json.dll", "System.Runtime.Serialization.Primitives.dll", "System.Runtime.Serialization.Xml.dll", "System.Security.Claims.dll", "System.Security.Cryptography.Algorithms.dll", "System.Security.Cryptography.Csp.dll", "System.Security.Cryptography.Encoding.dll", "System.Security.Cryptography.Primitives.dll", "System.Security.Cryptography.X509Certificates.dll", "System.Security.dll", "System.Security.Principal.dll", "System.Security.SecureString.dll", "System.ServiceModel.Activation.dll", "System.ServiceModel.Activities.dll", "System.ServiceModel.Channels.dll", "System.ServiceModel.Discovery.dll", "System.ServiceModel.dll", "System.ServiceModel.Duplex.dll", "System.ServiceModel.Http.dll", "System.ServiceModel.NetTcp.dll", "System.ServiceModel.Primitives.dll", "System.ServiceModel.Routing.dll", "System.ServiceModel.Security.dll", "System.ServiceModel.Web.dll", "System.ServiceProcess.dll", "System.Speech.dll", "System.Text.Encoding.dll", "System.Text.Encoding.Extensions.dll", "System.Text.RegularExpressions.dll", "System.Threading.dll", "System.Threading.Overlapped.dll", "System.Threading.Tasks.dll", "System.Threading.Tasks.Parallel.dll", "System.Threading.Thread.dll", "System.Threading.ThreadPool.dll", "System.Threading.Timer.dll", "System.Transactions.dll", "System.ValueTuple.dll", "System.Web.Abstractions.dll", "System.Web.ApplicationServices.dll", "System.Web.DataVisualization.Design.dll", "System.Web.DataVisualization.dll", "System.Web.dll", "System.Web.DynamicData.Design.dll", "System.Web.DynamicData.dll", "System.Web.Entity.Design.dll", "System.Web.Entity.dll", "System.Web.Extensions.Design.dll", "System.Web.Extensions.dll", "System.Web.Mobile.dll", "System.Web.RegularExpressions.dll", "System.Web.Routing.dll", "System.Web.Services.dll", "System.Windows.Controls.Ribbon.dll", "System.Windows.dll", "System.Windows.Forms.DataVisualization.Design.dll", "System.Windows.Forms.DataVisualization.dll", "System.Windows.Forms.dll", "System.Windows.Input.Manipulations.dll", "System.Windows.Presentation.dll", "System.Workflow.Activities.dll", "System.Workflow.ComponentModel.dll", "System.Workflow.Runtime.dll", "System.WorkflowServices.dll", "System.Xaml.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Xml.ReaderWriter.dll", "System.Xml.Serialization.dll", "System.Xml.XDocument.dll", "System.Xml.XmlDocument.dll", "System.Xml.XmlSerializer.dll", "System.Xml.XPath.dll", "System.Xml.XPath.XDocument.dll", "UIAutomationClient.dll", "UIAutomationClientsideProviders.dll", "UIAutomationProvider.dll", "UIAutomationTypes.dll", "WindowsBase.dll", "WindowsFormsIntegration.dll", "XamlBuildTask.dll" });


        private readonly IFileSystem _fileSystem = FileSystem.Instance;
        private readonly ILogCollector logCollector;
        private readonly string directoryForNuGetConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptBuilder"/> class.
        /// </summary>
        /// <param name="script">The Automation script.</param>
        /// <param name="projects">The projects corresponding with the C# Exe blocks.</param>
        /// <param name="allScripts">All the scripts in the Automation script solution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="script"/> is <see langword="null"/>.</exception>
        [Obsolete("Use the constructor with the directoryForNuGetConfig so it can take in account the NuGet.config from the solution.")]
        public AutomationScriptBuilder(Script script, IDictionary<string, Project> projects, IEnumerable<Script> allScripts)
            : this(script, projects, allScripts, directoryForNuGetConfig: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptBuilder"/> class.
        /// </summary>
        /// <param name="script">The Automation script.</param>
        /// <param name="projects">The projects corresponding with the C# Exe blocks.</param>
        /// <param name="allScripts">All the scripts in the Automation script solution.</param>
        /// <param name="directoryForNuGetConfig">Directory where the solution is located</param>
        /// <exception cref="ArgumentNullException"><paramref name="script"/> is <see langword="null"/>.</exception>
        public AutomationScriptBuilder(Script script, IDictionary<string, Project> projects, IEnumerable<Script> allScripts, string directoryForNuGetConfig)
        {
            Model = script ?? throw new ArgumentNullException(nameof(script));
            Document = script.Document;
            Projects = projects ?? new Dictionary<string, Project>();

            // ToList as it will be enumerated multiple times later on.
            AllScripts = allScripts.ToList();

            this.directoryForNuGetConfig = directoryForNuGetConfig;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptBuilder"/> class.
        /// </summary>
        /// <param name="script">The Automation script.</param>
        /// <param name="projects">The projects corresponding with the C# Exe blocks.</param>
        /// <param name="allScripts">All the scripts in the Automation script solution.</param>
        /// <param name="logCollector">The log collector</param>
        /// <exception cref="ArgumentNullException"><paramref name="script"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> is <see langword="null"/>.</exception>
        [Obsolete("Use the constructor with the directoryForNuGetConfig so it can take in account the NuGet.config from the solution.")]
        public AutomationScriptBuilder(Script script, IDictionary<string, Project> projects, IEnumerable<Script> allScripts, ILogCollector logCollector)
            : this(script, projects, allScripts, logCollector, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptBuilder"/> class.
        /// </summary>
        /// <param name="script">The Automation script.</param>
        /// <param name="projects">The projects corresponding with the C# Exe blocks.</param>
        /// <param name="allScripts">All the scripts in the Automation script solution.</param>
        /// <param name="logCollector">The log collector</param>
        /// <param name="directoryForNuGetConfig">Directory where the solution is located</param>
        /// <exception cref="ArgumentNullException"><paramref name="script"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> is <see langword="null"/>.</exception>
        public AutomationScriptBuilder(Script script, IDictionary<string, Project> projects, IEnumerable<Script> allScripts, ILogCollector logCollector, string directoryForNuGetConfig)
            : this(script, projects, allScripts, directoryForNuGetConfig)
        {
            this.logCollector = logCollector ?? throw new ArgumentNullException(nameof(logCollector));
        }

        private XmlDocument Document { get; }

        private Script Model { get; }

        private IDictionary<string, Project> Projects { get; }

        private IEnumerable<Script> AllScripts { get; }

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
        /// Will try to find the project name from the placeholder in the xml.
        /// </summary>
        /// <param name="text">Placeholder text.</param>
        /// <param name="projectName">Found project name.</param>
        /// <param name="match">Regex Match.</param>
        /// <returns>True when finding the project name. Otherwise false.</returns>
        public static bool TryFindProjectPlaceholder(string text, out string projectName, out Match match)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                match = null;
                projectName = null;
                return false;
            }

            match = RegexProjectPlaceholder.Match(text);
            if (match.Success)
            {
                projectName = match.Groups["projectName"].Value;
                return true;
            }

            projectName = null;
            return false;
        }

        /// <summary>
        /// Builds the solution.
        /// </summary>
        /// <returns>The build result.</returns>
        /// <exception cref="AggregateException">Aggregation of exception(s) thrown during building.</exception>
        public async Task<BuildResultItems> BuildAsync()
        {
            var xmlEdit = new EditXml.XmlDocument(Document);

            return await BuildExeActionsAsync(xmlEdit).ConfigureAwait(false);
        }

        private static bool IsMainCodeFile(ProjectFile file)
        {
            var content = file.Content;
            return !String.IsNullOrWhiteSpace(content)
                   && content.Contains("class Script")
                   && (content.Contains("void Run(Engine") || content.Contains("void Run(IEngine"));
        }

        private async Task BuildDllImportsAsync(EditXml.XmlElement editExe, Project project, PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems)
        {
            // remove existing references
            foreach (var existing in editExe.Elements["Param"].ToList())
            {
                var typeAttr = existing.Attributes.FirstOrDefault(x => String.Equals(x.Name, "type", StringComparison.OrdinalIgnoreCase));

                if (String.Equals(typeAttr?.Value, "ref"))
                {
                    editExe.Children.Remove(existing);
                }
            }

            NuGetPackageAssemblyData nugetAssemblyData = null;

            // PackageReferences (NuGet packages)
            if (project.PackageReferences != null)
            {
                List<PackageIdentity> packageIdentities = GetPackageIdentities(project.PackageReferences);

                nugetAssemblyData = await ProcessPackageReferences(editExe, project, packageReferenceProcessor, buildResultItems, packageIdentities);
            }

            // Add references from C# project.
            if (project.References != null)
            {
                ProcessReferences(editExe, project, nugetAssemblyData, packageReferenceProcessor, buildResultItems);
            }
        }

        private async Task<NuGetPackageAssemblyData> ProcessPackageReferences(EditXml.XmlElement editExe, Project project, PackageReferenceProcessor packageReferenceProcessor,
            BuildResultItems buildResultItems, IList<PackageIdentity> packageIdentities)
        {
            NuGetPackageAssemblyData nugetAssemblyData = null;

            if (packageIdentities.Count > 0)
            {
                nugetAssemblyData = await packageReferenceProcessor.ProcessAsync(packageIdentities, project.TargetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive).ConfigureAwait(false);
                LogDebug($"NuGetPackageAssemblyData: {nugetAssemblyData}");

                ProcessFrameworkAssemblies(editExe, nugetAssemblyData);
                ProcessLibAssemblies(editExe, buildResultItems, nugetAssemblyData);
            }

            return nugetAssemblyData;
        }

        private void ProcessLibAssemblies(EditXml.XmlElement editExe, BuildResultItems buildResultItems, NuGetPackageAssemblyData nugetAssemblyData)
        {
            HashSet<string> directoriesWithExplicitDllImport = new HashSet<string>();
            Dictionary<string, string> potentialRemainingDirectoryImports = new Dictionary<string, string>();

            if (nugetAssemblyData.DllImportNugetAssemblyReferences.Count > 0)
            {
                // At this point it could be that there are multiple assemblies with the same name (if different NuGet packages contain the same assembly).
                // If this is the case, we select the highest version.
                Dictionary<string, List<PackageAssemblyReference>> assemblies = BuildAssemblyPackageMap(_fileSystem, nugetAssemblyData.DllImportNugetAssemblyReferences);
                AddAssemblyParamReferences(editExe, directoriesWithExplicitDllImport, potentialRemainingDirectoryImports, assemblies);

                foreach (var directoryPath in potentialRemainingDirectoryImports.Keys)
                {
                    if (!directoriesWithExplicitDllImport.Contains(directoryPath))
                    {
                        LogDebug($"ProcessLibAssemblies|Build DllImportDirectoryReferences|Dir: {directoryPath}");
                        nugetAssemblyData.DllImportDirectoryReferences.Add(directoryPath);
                        nugetAssemblyData.DllImportDirectoryReferencesAssembly.Add(directoryPath, potentialRemainingDirectoryImports[directoryPath]);
                    }
                }
            }

            HashSet<string> nugetPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string dir in directoriesWithExplicitDllImport)
            {
                string packageName = dir.Split('\\').First(); // Fixed to windows directory separator

                LogDebug($"ProcessLibAssemblies|Build nugetPackages|Dir: {dir}|PackageName: {packageName}");
                nugetPackages.Add(packageName);
            }

            foreach (string dir in nugetAssemblyData.DllImportDirectoryReferences)
            {
                string packageName = dir.Split('\\').First(); // Fixed to windows directory separator

                LogDebug($"ProcessLibAssemblies|DllImportDirectoryReferences|Check to add|Dir: {dir}|PackageName: {packageName}");
                if (nugetPackages.Contains(packageName))
                {
                    continue;
                }

                if (!ScriptHelper.IsDefaultImportDll(dir))
                {
                    var referenceAssembly = nugetAssemblyData.DllImportDirectoryReferencesAssembly[dir];

                    LogDebug($"ProcessLibAssemblies|DllImportDirectoryReferences|Add Param for {referenceAssembly}");

                    // Add reference to selected item. Only add if no other package with same name has been added already.
                    var fullPathToAssembly = _fileSystem.Path.Combine(@"C:\Skyline DataMiner\ProtocolScripts\DllImport", referenceAssembly);
                    AddParamToXml(editExe, fullPathToAssembly);
                }
            }

            foreach (var libItem in nugetAssemblyData.NugetAssemblies)
            {
                if (buildResultItems.Assemblies.FirstOrDefault(b => b.AssemblyPath == libItem.AssemblyPath) == null)
                {
                    buildResultItems.Assemblies.Add(libItem);
                }
            }
        }

        private void AddAssemblyParamReferences(EditXml.XmlElement editExe, HashSet<string> directoriesWithExplicitDllImport, Dictionary<string, string> potentialRemainingDirectoryImports, Dictionary<string, List<PackageAssemblyReference>> assemblies)
        {
            foreach (var assembly in assemblies.Keys)
            {
                var packagesContainingAssembly = assemblies[assembly];

                if (packagesContainingAssembly.Count == 1)
                {
                    var libItem = packagesContainingAssembly[0];

                    directoriesWithExplicitDllImport.Add(libItem.DllImport.Substring(0, libItem.DllImport.Length - assembly.Length));

                    LogDebug($"AddAssemblyParamReferences|Add Param for {libItem}");

                    // Add reference to assembly.
                    var fullPathToAssembly = GetPathForPackage(libItem);
                    AddParamToXml(editExe, fullPathToAssembly);
                }
                else
                {
                    PackageAssemblyReference selectedItem = SelectMostRecentVersion(packagesContainingAssembly);

                    if (selectedItem == null)
                    {
                        continue;
                    }

                    directoriesWithExplicitDllImport.Add(selectedItem.DllImport.Substring(0, selectedItem.DllImport.Length - assembly.Length));

                    LogDebug($"AddAssemblyParamReferences|Add Param for {selectedItem}");

                    // Add reference to selected item.
                    var fullPathToAssembly = GetPathForPackage(selectedItem);
                    AddParamToXml(editExe, fullPathToAssembly);

                    // Add other items that were not selected as hint directories.
                    AddRemainingHintPathDirectories(potentialRemainingDirectoryImports, assembly, packagesContainingAssembly, selectedItem);
                }
            }

            string GetPathForPackage(PackageAssemblyReference reference)
            {
                string folderLocation;
                if (reference.IsFilesPackage)
                {
                    folderLocation = @"C:\Skyline DataMiner\Files";
                }
                else
                {
                    folderLocation = @"C:\Skyline DataMiner\ProtocolScripts\DllImport";
                }

                return _fileSystem.Path.Combine(folderLocation, reference.DllImport);
            }
        }

        private static void AddRemainingHintPathDirectories(Dictionary<string, string> potentialRemainingDirectoryImports, string assembly, List<PackageAssemblyReference> packagesContainingAssembly, PackageAssemblyReference selectedItem)
        {
            foreach (var libItem in packagesContainingAssembly)
            {
                if (libItem == selectedItem)
                {
                    continue;
                }

                string directoryPath = libItem.DllImport.Substring(0, libItem.DllImport.Length - assembly.Length);

                if (!potentialRemainingDirectoryImports.ContainsKey(directoryPath))
                {
                    potentialRemainingDirectoryImports.Add(directoryPath, libItem.DllImport);
                }
            }
        }

        private static PackageAssemblyReference SelectMostRecentVersion(IList<PackageAssemblyReference> packagesContainingAssembly)
        {
            PackageAssemblyReference selectedItem = null;
            Version mostRecentVersion = null;

            foreach (var libItem in packagesContainingAssembly)
            {
                var version = AssemblyName.GetAssemblyName(libItem.AssemblyPath).Version;

                if (mostRecentVersion == null || version > mostRecentVersion)
                {
                    mostRecentVersion = version;
                    selectedItem = libItem;
                }
            }

            return selectedItem;
        }

        private static Dictionary<string, List<PackageAssemblyReference>> BuildAssemblyPackageMap(IFileSystem fs, IList<PackageAssemblyReference> dllImportNugetAssemblyReferences)
        {
            Dictionary<string, List<PackageAssemblyReference>> assemblies = new Dictionary<string, List<PackageAssemblyReference>>();

            foreach (var libItem in dllImportNugetAssemblyReferences)
            {
                string assemblyName = fs.Path.GetFileName(libItem.DllImport);

                if (assemblies.TryGetValue(assemblyName, out List<PackageAssemblyReference> entries))
                {
                    entries.Add(libItem);
                }
                else
                {
                    assemblies.Add(assemblyName, new List<PackageAssemblyReference> { libItem });
                }
            }

            return assemblies;
        }

        private void ProcessReferences(EditXml.XmlElement editExe, Project project, NuGetPackageAssemblyData nugetAssemblyData,
            PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems)
        {
            foreach (var r in project.References)
            {
                ProcessReference(project, r, editExe, nugetAssemblyData, packageReferenceProcessor, buildResultItems);
            }
        }

        private void ProcessReference(Project project, Reference r, EditXml.XmlElement editExe, NuGetPackageAssemblyData nugetAssemblyData,
            PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems)
        {
            var dllName = r.GetDllName();
            if (ScriptHelper.IsDefaultImportDll(dllName) ||
                (nugetAssemblyData != null && nugetAssemblyData.ProcessedAssemblies.Contains(dllName)))
            {
                return;
            }

            string destinationPath = dllName;
            bool isFiles = false;
            if (r.HintPath?.Contains(packageReferenceProcessor.NuGetRootPath) == true)
            {
                // DLL is from a NuGet but is transitive from precompile or other project reference.
                // These can be ignored.
            }
            else if (ScriptHelper.NeedsFilesPath(dllName))
            {
                destinationPath = _fileSystem.Path.Combine(@"C:\Skyline DataMiner\Files", dllName);
                isFiles = true;
            }
            else if (ScriptHelper.NeedsProtocolScriptsPath(dllName))
            {
                destinationPath = _fileSystem.Path.Combine(@"C:\Skyline DataMiner\ProtocolScripts", dllName);
            }
            else if (!NetFramework481ReferenceAssemblies.Contains(dllName) && !_fileSystem.Path.IsPathRooted(dllName))
            {
                // Always use the DllImport folder (is synced on a cluster)
                destinationPath = _fileSystem.Path.Combine(@"C:\Skyline DataMiner\ProtocolScripts\DllImport", dllName);
            }
            else
            {
                // Do nothing.
            }

            LogDebug($"ProcessReference|Add Param for {destinationPath}");
            AddOrUpdateReferenceInExeBlock(_fileSystem, destinationPath, editExe);

            // If custom DLL
            if (r.HintPath != null && project.ProjectStyle == ProjectStyle.Sdk)
            {
                string dllPath = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(project.ProjectDirectory, r.HintPath));

                buildResultItems.DllAssemblies.Add(new DllAssemblyReference(dllName, dllPath, isFiles));
            }
        }

        private static void AddOrUpdateReferenceInExeBlock(IFileSystem fs, string dll, EditXml.XmlElement editExe)
        {
            if (HasDllImport(fs, editExe, dll, out var existing))
            {
                existing.InnerText = dll;
            }
            else
            {
                AddParamToXml(editExe, dll);
            }
        }

        private void ProcessFrameworkAssemblies(EditXml.XmlElement editExe, NuGetPackageAssemblyData nugetAssemblyData)
        {
            foreach (var frameworkAssembly in nugetAssemblyData.DllImportFrameworkAssemblyReferences)
            {
                if (!ScriptHelper.IsDefaultImportDll(frameworkAssembly))
                {
                    LogDebug($"ProcessFrameworkAssemblies|Add Param for {frameworkAssembly}");

                    // Add reference to .NET framework assembly.
                    AddParamToXml(editExe, frameworkAssembly);
                }
            }
        }

        private static List<PackageIdentity> GetPackageIdentities(IEnumerable<PackageReference> packageReferences)
        {
            var packageIdentities = new List<PackageIdentity>();

            foreach (var packageReference in packageReferences)
            {
                var packageIdentity = new PackageIdentity(packageReference.Name, NuGetVersion.Parse(packageReference.Version));
                packageIdentities.Add(packageIdentity);
            }

            return packageIdentities;
        }

        private static void AddParamToXml(EditXml.XmlElement editExe, string dllPath, string type = "ref")
        {
            var newParam = new EditXml.XmlElement("Param", dllPath);
            newParam.Attributes.Add(new EditXml.XmlAttribute("type", type));
            editExe.Children.Add(newParam);
        }

        private static bool HasDllImport(IFileSystem fs, EditXml.XmlElement editExe, string dll, out EditXml.XmlElement existing)
        {
            foreach (var p in editExe.Elements["param"])
            {
                try
                {
                    if (!String.Equals(p.Attribute["type"]?.Value, "ref") ||
                        !String.Equals(fs.Path.GetFileName(p.InnerText), fs.Path.GetFileName(dll), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    // ignore
                    continue;
                }

                existing = p;
                return true;
            }

            existing = null;
            return false;
        }

        private static bool HasScriptRef(EditXml.XmlElement editExe, string scriptRef)
        {
            foreach (var p in editExe.Elements["param"])
            {
                if (String.Equals(p.Attribute["type"]?.Value, "scriptRef")
                    && String.Equals(p.InnerText, scriptRef))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyCDataContent(ScriptExe exe, EditXml.XmlElement editExe, string newContent)
        {
            EditXml.XmlCDATA editCData = null;

            if (exe.NodeCDATA != null)
            {
                editCData = editExe.TryFindNode(exe.NodeCDATA) as EditXml.XmlCDATA;
            }

            if (editCData == null)
            {
                editCData = new EditXml.XmlCDATA();

                var valueNode = editExe.Element["Value"];
                valueNode.Children.Clear();
                valueNode.Children.Add(editCData);
            }

            editCData.InnerText = newContent;
        }

        private static IList<ProjectFile> GetRelevantCodeFilesSorted(Project project)
        {
            var files = project.Files
                               .Where(x => x.Name.EndsWith(".cs") && !x.Name.EndsWith("AssemblyInfo.cs"))
                               .OrderByDescending(IsMainCodeFile)
                               .ThenByDescending(x => x.Name.StartsWith("script", StringComparison.OrdinalIgnoreCase))
                               .ThenBy(x => x.Name)
                               .ToList();

            return files;
        }

        private void BuildScriptReferences(EditXml.XmlElement editExe, Project project)
        {
            if (project.ProjectReferences == null)
            {
                return;
            }

            foreach (var r in project.ProjectReferences)
            {
                ScriptExe refExe;

                if (r.Name == "AutomationScript_ClassLibrary")
                {
                    refExe = Model.ScriptExes.FirstOrDefault(x => String.Equals(x.LibraryName, "DIS Class Library"));
                }
                else
                {
                    refExe = FindExeBlockReferenceInScript(project, r);
                }

                if (refExe != null)
                {
                    string scriptRef = "[AutomationScriptName]:" + refExe.LibraryName;

                    if (!HasScriptRef(editExe, scriptRef))
                    {
                        AddParamToXml(editExe, scriptRef, "scriptRef");
                    }

                    continue;
                }

                // Find exe from another script
                (string scriptName, ScriptExe scriptExe) = FindExeFromOtherScript(project, r);

                if (scriptExe != null)
                {
                    string scriptRef = scriptName + ":" + scriptExe.LibraryName;

                    if (!HasScriptRef(editExe, scriptRef))
                    {
                        AddParamToXml(editExe, scriptRef, "scriptRef");
                    }
                }
            }
        }

        private (string scriptName, ScriptExe scriptExe) FindExeFromOtherScript(Project project, ProjectReference r)
        {
            // Check if the exe block belongs to another script.
            foreach (var script in AllScripts)
            {
                var referencedProjectName = r.Name;

                foreach (var exe in script.ScriptExes)
                {
                    if (TryFindProjectPlaceholder(exe.Code, out string projectName, out _)
                        && String.Equals(projectName, referencedProjectName))
                    {
                        if (String.IsNullOrWhiteSpace(exe.LibraryName))
                        {
                            throw new AssemblerException(
                                $"Missing 'libraryName' param on exe {exe.Id} (project '{projectName}') which is referenced by project '{project.AssemblyName}'");
                        }

                        return (script.Name, exe);
                    }
                }
            }

            return (null, null);
        }

        private ScriptExe FindExeBlockReferenceInScript(Project project, ProjectReference r)
        {
            ScriptExe refExe = null;

            // check if this is a reference to another Exe block in the same script.
            foreach (var exe in Model.ScriptExes)
            {
                if (TryFindProjectPlaceholder(exe.Code, out string projectName, out _)
                    && String.Equals(projectName, r.Name))
                {
                    if (String.IsNullOrWhiteSpace(exe.LibraryName))
                    {
                        throw new AssemblerException($"Missing 'libraryName' param on exe {exe.Id} (project '{projectName}') which is referenced by project '{project.AssemblyName}'");
                    }

                    refExe = exe;
                    break;
                }
            }

            return refExe;
        }

        private async Task<BuildResultItems> BuildExeActionsAsync(EditXml.XmlDocument xmlEdit)
        {
            PackageReferenceProcessor packageReferenceProcessor;
            if (logCollector == null)
            {
                packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig);
            }
            else
            {
                packageReferenceProcessor = new PackageReferenceProcessor(logCollector, directoryForNuGetConfig);
            }

            BuildResultItems buildResultItems = new BuildResultItems();

            foreach (var exe in Model.ScriptExes)
            {
                if (String.Equals(exe.Type, "csharp", StringComparison.OrdinalIgnoreCase))
                {
                    await BuildExeActionAsync(xmlEdit, exe, packageReferenceProcessor, buildResultItems).ConfigureAwait(false);
                }
            }

            buildResultItems.Document = xmlEdit.GetXml();

            return buildResultItems;
        }

        private async Task BuildExeActionAsync(EditXml.XmlDocument xmlEdit, ScriptExe exe, PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems)
        {
            var editExe = xmlEdit.TryFindNode(exe.Node);
            if (editExe != null && TryFindProjectPlaceholder(exe.Code, out string projectName, out Match match))
            {
                if (!Projects.TryGetValue(projectName, out var project))
                {
                    throw new AssemblerException($"Project with name '{projectName}' could not be found!");
                }

                string combinedCode = CombineProjectFiles(project);

                var newContent = exe.Code.Replace(match.Value, combinedCode);
                ApplyCDataContent(exe, editExe, newContent);

                await BuildDllImportsAsync(editExe, project, packageReferenceProcessor, buildResultItems).ConfigureAwait(false);
                BuildScriptReferences(editExe, project);

                // format
                editExe.Format();
            }
        }

        private void LogDebug(string message)
        {
            logCollector?.ReportDebug(Model?.Name + "|" + message);
        }
    }
}
