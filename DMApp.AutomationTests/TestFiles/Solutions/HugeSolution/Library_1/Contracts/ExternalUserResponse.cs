namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;

	using Newtonsoft.Json;

	public class ExternalUserResponse : ExternalResponse
	{
		public string Username { get; set; }

		public static ExternalUserResponse Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalUserResponse>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override string ToString()
		{
			return "ExternalUserResponse: " + JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}