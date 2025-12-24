namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.MetaData
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	public class IpAndPortMetaData : IResourceMetaData
    {
        public static readonly string[] PropertyNames = new string[]
        {
            ResourcePropertyNames.IpRxServerIp,
            ResourcePropertyNames.IpRxPort,
        };

        public IpAndPortMetaData(Resource resource)
        {
            ServerIP = resource.GetResourcePropertyStringValue(ResourcePropertyNames.IpRxServerIp);
            Port = resource.GetResourcePropertyStringValue(ResourcePropertyNames.IpRxPort);
        }

        public string Port { get; }

        public string ServerIP { get; }

        public static bool TryConstructFromResource(Resource resource, out IpAndPortMetaData ipAndPortMetaData)
        {
            bool containsIpAndPortMetaData = resource.Properties.Select(p => p.Name).Intersect(PropertyNames).Any();

            ipAndPortMetaData = containsIpAndPortMetaData ? new IpAndPortMetaData(resource) : null;

            return ipAndPortMetaData != null;
        }

        public string GetDisplayString()
        {
            return $"Server IP: {ServerIP}{Environment.NewLine}Port: {Port}";   
        }
    }
}
