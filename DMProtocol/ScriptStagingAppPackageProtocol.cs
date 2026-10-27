namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System.Collections.Generic;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Defers resolving/staging connector scripts until <see cref="CreatePackage()"/>/<see cref="CreatePackage(string)"/>
    /// is called, so the temporary staging directories only exist for the duration of that call.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="CreatePackage()"/>/<see cref="CreatePackage(string)"/> a second time will fail, since
    /// the staging directories are deleted afterward. Build a new instance via
    /// <see cref="ProtocolPackageCreator.Factory"/> instead.
    /// </remarks>
    internal sealed class ScriptStagingAppPackageProtocol : IAppPackageProtocol
    {
        private readonly ILogCollector _logCollector;
        private readonly IAppPackageProtocol _metadataPackage;
        private readonly IAppPackageProtocolBuilder _packageBuilder;
        private readonly ICollection<ConnectorScript> _scripts;

        public ScriptStagingAppPackageProtocol(ILogCollector logCollector, IAppPackageProtocol metadataPackage, IAppPackageProtocolBuilder packageBuilder, ICollection<ConnectorScript> scripts)
        {
            _logCollector = logCollector;
            _metadataPackage = metadataPackage;
            _packageBuilder = packageBuilder;
            _scripts = scripts;
        }

        public string Name => _metadataPackage.Name;

        public string Version => _metadataPackage.Version;

        public string ProtocolPath => _metadataPackage.ProtocolPath;

        public byte[] ProtocolContent => _metadataPackage.ProtocolContent;

        public IReadOnlyCollection<IAppPackageAlarmTemplate> AlarmTemplates => _metadataPackage.AlarmTemplates;

        public IReadOnlyCollection<IAppPackageInformationTemplate> InformationTemplates => _metadataPackage.InformationTemplates;

        public IReadOnlyCollection<IAppPackageTrendTemplate> TrendTemplates => _metadataPackage.TrendTemplates;

        public IReadOnlyCollection<IAppPackageAssembly> Assemblies => _metadataPackage.Assemblies;

        public IReadOnlyCollection<IAppPackageConnectorScript> Scripts => _metadataPackage.Scripts;

        public byte[] CreatePackage()
        {
            List<string> stagingDirectories = StageScripts();

            try
            {
                return _packageBuilder.Build().CreatePackage();
            }
            finally
            {
                CleanUpStagingDirectories(stagingDirectories);
            }
        }

        public void CreatePackage(string destinationFilePath)
        {
            List<string> stagingDirectories = StageScripts();

            try
            {
                _packageBuilder.Build().CreatePackage(destinationFilePath);
            }
            finally
            {
                CleanUpStagingDirectories(stagingDirectories);
            }
        }

        /// <summary>
        /// Stages each script's dependencies and adds them to the builder. The returned directories must only be
        /// deleted after the package has been serialized, since the builder just stores a path reference until then.
        /// </summary>
        private List<string> StageScripts()
        {
            List<string> stagingDirectories = new List<string>();

            try
            {
                foreach (ConnectorScript script in _scripts)
                {
                    string stagingDirectory = ScriptedConnectorStager.Factory
                        .StageAsync(_logCollector, script.ProjectDirectory, script.RequirementsFilePath)
                        .GetAwaiter().GetResult();
                    stagingDirectories.Add(stagingDirectory);

                    _packageBuilder.WithConnectorScript(stagingDirectory, script.Guid);
                }

                return stagingDirectories;
            }
            catch
            {
                CleanUpStagingDirectories(stagingDirectories);
                throw;
            }
        }

        private static void CleanUpStagingDirectories(IEnumerable<string> stagingDirectories)
        {
            foreach (string stagingDirectory in stagingDirectories)
            {
                FileSystem.Instance.Directory.DeleteDirectory(stagingDirectory);
            }
        }
    }
}
