namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.MetaData
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	public class IpDecoderUrlAndKeyMetaData : IResourceMetaData
    {
        public static readonly string[] PropertyNames = new string[]
        {
            ResourcePropertyNames.IpRxServerUrl,
            ResourcePropertyNames.IpRxStreamKey,
        };

        public IpDecoderUrlAndKeyMetaData(Resource resource)
		{
			StreamKey = resource.GetResourcePropertyStringValue(ResourcePropertyNames.IpRxStreamKey);
            ServerURL = resource.GetResourcePropertyStringValue(ResourcePropertyNames.IpRxServerUrl);
        }

        public string StreamKey { get; }

        public string ServerURL { get; }

        public static bool TryConstructFromResource(Resource resource, out IpDecoderUrlAndKeyMetaData ipDecoderMetaData)
		{
            bool containsIpDecoderMetaData = resource.Properties.Select(p => p.Name).Intersect(PropertyNames).Any();

            ipDecoderMetaData = containsIpDecoderMetaData ? new IpDecoderUrlAndKeyMetaData(resource) : null;

            return ipDecoderMetaData != null;
		}

		public string GetDisplayString()
		{
            return $"Server URL: {ServerURL}{Environment.NewLine}Stream Key: {StreamKey}";
        }
	}
}
