namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	using System;

	using Newtonsoft.Json;

	public class ExternalRequestLockResponse
	{
		public static ExternalRequestLockResponse Deserialize(string serializedResponse)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalRequestLockResponse>(serializedResponse);
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

		public LockInfo GetLockInfo()
		{
			return new LockInfo(IsLockGranted, Username, ObjectId, ReleaseLocksAfter);
		}

		[JsonProperty("Id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("ObjectType", Required = Required.Always)]
		public ObjectTypes ObjectType { get; set; }

		[JsonProperty("ObjectId", Required = Required.Always)]
		public string ObjectId { get; set; }

		[JsonProperty("Username", Required = Required.Always)]
		public string Username { get; set; }

        [JsonProperty("ReleaseLocksAfter", Required = Required.Always)]
        public TimeSpan ReleaseLocksAfter { get; set; }

        [JsonProperty("IsLockGranted", Required = Required.Always)]
		public bool IsLockGranted { get; set; }

        [JsonProperty("IsLockExtended", Required = Required.Always)]
        public bool IsLockExtended { get; set; }
    }
}