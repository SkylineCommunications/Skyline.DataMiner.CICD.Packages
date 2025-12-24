namespace RemoveOldServices_1
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;

	public class HandleOrderActionInfo
	{
		[JsonRequired]
		public string Action { get; set; }

		[JsonRequired]
		public string OrderId { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<string> ServiceIds { get; set; }

		[JsonRequired]
		public bool RemoveAllServices { get; set; }

		public static HandleOrderActionInfo Deserialize(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<HandleOrderActionInfo>(json);
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