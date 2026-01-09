namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	using System;

	using Newtonsoft.Json;

	public class ExternalReleaseLockResponse
	{
		public static ExternalReleaseLockResponse Deserialize(string serializedResponse)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalReleaseLockResponse>(serializedResponse);
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

		[JsonProperty("Id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("IsLockReleased", Required = Required.Always)]
		public bool IsLockReleased { get; set; }
	}
}