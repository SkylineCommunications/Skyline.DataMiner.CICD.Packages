namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;

	public class UpdateServiceContext : Context
	{
		public UpdateServiceContext(IEngine engine) : base(engine, Scripts.UpdateService)
		{
			string serviceIdScriptParamValue = engine.GetScriptParam("ServiceId").Value;
			if (serviceIdScriptParamValue.Contains("["))
			{
				ServiceId = Guid.Parse(JsonConvert.DeserializeObject<List<string>>(serviceIdScriptParamValue).First());
			}
			else
			{
				ServiceId = Guid.Parse(serviceIdScriptParamValue);
			}

			Action = engine.GetScriptParam("Action").Value.GetEnumValue<EditOrderFlows>();
		}

		public Guid ServiceId { get; }

		public EditOrderFlows Action { get; }

		public bool IsResourceChangeAction => Action == EditOrderFlows.ChangeResourcesForService || Action == EditOrderFlows.ChangeResourcesForService_FromRecordingApp;
	}
}
