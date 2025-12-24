namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	using System;

	using Newtonsoft.Json;

	public class ExternalRequestLockRequest
	{
		public static ExternalRequestLockRequest Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalRequestLockRequest>(serializedRequest);
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

		[JsonProperty("Username", Required = Required.Always)]
		public string Username { get; set; }

        [JsonProperty("IsLockExtended", Required = Required.Always)]
        public bool IsLockExtended { get; set; }
    }
}