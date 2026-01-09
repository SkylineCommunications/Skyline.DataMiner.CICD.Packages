namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;

	using Newtonsoft.Json;

	public class ExternalUserRequest
	{
		public string ID { get; set; }

		public string Username { get; set; }

		public static ExternalUserRequest Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalUserRequest>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public string Serialize()
		{
			try
			{
				return JsonConvert.SerializeObject(this, Formatting.None);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}