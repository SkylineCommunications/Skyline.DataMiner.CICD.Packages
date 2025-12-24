namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	using System;

	using Newtonsoft.Json;

	public class ExternalReleaseLockRequest
	{
		public static ExternalReleaseLockRequest Deserialize(string serializedResponse)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalReleaseLockRequest>(serializedResponse);
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

		[JsonProperty("ObjectType", Required = Required.Always)]
		public ObjectTypes ObjectType { get; set; }

		[JsonProperty("ObjectId", Required = Required.Always)]
		public string ObjectId { get; set; }
	}
}