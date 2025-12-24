namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using Newtonsoft.Json;
	using System;

	public class ExternalEventTemplateRequest
	{
		/// <summary>
		/// Id of the request
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public string ID { get; set; }

		public string TemplateId { get; set; }

		/// <summary>
		/// required when adding, editing or retrieving an Event Template
		/// </summary>
		[JsonProperty(Required = Required.Always)] // A non-null value is required for successfull parsing in the contract manager.
		public string EventTemplateName { get; set; } = "random value for successfull parsing";

		/// <summary>
		/// Only required when adding a new Template
		/// </summary>
		public string[] UserGroups { get; set; }

		/// <summary>
		/// Only required when adding a new template or editing an existing one 
		/// </summary>
		public string Template { get; set; }

		/// <summary>
		/// Always optional
		/// </summary>
		public EventOrderTemplate[] OrderTemplates { get; set; } = new EventOrderTemplate[0];

		[JsonProperty(Required = Required.Always)]
		public TemplateAction Action { get; set; }

		public static ExternalEventTemplateRequest Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalEventTemplateRequest>(serializedRequest);
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

	public class EventOrderTemplate
	{
		public EventOrderTemplate()
		{
			TemplateId = default(string);
			Name = default(string);
			Template = default(string);
		}

		[JsonProperty(Required = Required.Always)]
		public string TemplateId { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string Name { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string Template { get; set; }
	}
}
