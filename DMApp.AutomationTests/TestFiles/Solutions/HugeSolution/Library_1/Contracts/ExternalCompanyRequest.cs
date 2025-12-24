namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;

	using Newtonsoft.Json;

	public class ExternalCompanyRequest
	{
		public string ID { get; set; }

		public string Company { get; set; }

		public static ExternalCompanyRequest Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalCompanyRequest>(serializedRequest);
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