namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System.Collections.Generic;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Wraps an already-built <see cref="IAppPackageProtocol"/> together with its eagerly materialized package
    /// bytes, so that <see cref="CreatePackage()"/>/<see cref="CreatePackage(string)"/> can be called after the
    /// connector scripts' temporary staging directories have been cleaned up.
    /// </summary>
    internal sealed class PrecomputedAppPackageProtocol : IAppPackageProtocol
    {
        private readonly IAppPackageProtocol _inner;
        private readonly byte[] _packageBytes;

        public PrecomputedAppPackageProtocol(IAppPackageProtocol inner, byte[] packageBytes)
        {
            _inner = inner;
            _packageBytes = packageBytes;
        }

        public string Name => _inner.Name;

        public string Version => _inner.Version;

        public string ProtocolPath => _inner.ProtocolPath;

        public byte[] ProtocolContent => _inner.ProtocolContent;

        public IReadOnlyCollection<IAppPackageAlarmTemplate> AlarmTemplates => _inner.AlarmTemplates;

        public IReadOnlyCollection<IAppPackageInformationTemplate> InformationTemplates => _inner.InformationTemplates;

        public IReadOnlyCollection<IAppPackageTrendTemplate> TrendTemplates => _inner.TrendTemplates;

        public IReadOnlyCollection<IAppPackageAssembly> Assemblies => _inner.Assemblies;

        public IReadOnlyCollection<IAppPackageConnectorScript> Scripts => _inner.Scripts;

        public byte[] CreatePackage()
        {
            return _packageBytes;
        }

        public void CreatePackage(string destinationFilePath)
        {
            FileSystem.Instance.File.WriteAllBytes(destinationFilePath, _packageBytes);
        }
    }
}
