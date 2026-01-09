namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ExternalOrderTemplateRequest
	{
		[JsonProperty(Required = Required.Always)]
		public string ID { get; set; }

		public string TemplateId { get; set; }

		/// <summary>
		/// required when adding, editing or retrieving an Order Template
		/// </summary>
		[JsonProperty(Required = Required.Always)] // A non-null value is required for successfull parsing in the contract manager.
		public string OrderTemplateName { get; set; } = "random value for successfull parsing";

		/// <summary>
		/// Only required when adding a new Template
		/// </summary>
		public string[] UserGroups { get; set; }

		/// <summary>
		/// Only required when adding a new template or editing an existing one 
		/// </summary>
		public string Template { get; set; }

		[JsonProperty(Required = Required.Always)]
		public TemplateAction Action { get; set; }

		public static ExternalOrderTemplateRequest Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalOrderTemplateRequest>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public string Serialize(Helpers helpers)
		{
			try
			{
				return JsonConvert.SerializeObject(this, Formatting.None);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(ExternalOrderTemplateRequest), nameof(Serialize), $"Exception while serializing: {ex}");
				return null;
			}
		}
	}
}
