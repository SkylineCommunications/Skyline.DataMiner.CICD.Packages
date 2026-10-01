namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System.Collections.Generic;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Decorates an <see cref="IAppPackageProtocol"/> so that its resulting package bytes include the additional
    /// <c>Scripts/{guid}/...</c> content of a protocol's scripted connectors, on top of the base package produced by
    /// the external <c>Skyline.DataMiner.Core.AppPackageCreator</c> package builder.
    /// </summary>
    internal sealed class ScriptEmbeddingAppPackageProtocol : IAppPackageProtocol
    {
        private readonly IAppPackageProtocol _inner;
        private readonly byte[] _mergedPackageBytes;

        public ScriptEmbeddingAppPackageProtocol(IAppPackageProtocol inner, byte[] mergedPackageBytes)
        {
            _inner = inner;
            _mergedPackageBytes = mergedPackageBytes;
        }

        public string Name => _inner.Name;

        public string Version => _inner.Version;

        public string ProtocolPath => _inner.ProtocolPath;

        public byte[] ProtocolContent => _inner.ProtocolContent;

        public IReadOnlyCollection<IAppPackageAlarmTemplate> AlarmTemplates => _inner.AlarmTemplates;

        public IReadOnlyCollection<IAppPackageInformationTemplate> InformationTemplates => _inner.InformationTemplates;

        public IReadOnlyCollection<IAppPackageTrendTemplate> TrendTemplates => _inner.TrendTemplates;

        public IReadOnlyCollection<IAppPackageAssembly> Assemblies => _inner.Assemblies;

        public byte[] CreatePackage()
        {
            return _mergedPackageBytes;
        }

        public void CreatePackage(string destinationFilePath)
        {
            FileSystem.Instance.File.WriteAllBytes(destinationFilePath, _mergedPackageBytes);
        }
    }
}
