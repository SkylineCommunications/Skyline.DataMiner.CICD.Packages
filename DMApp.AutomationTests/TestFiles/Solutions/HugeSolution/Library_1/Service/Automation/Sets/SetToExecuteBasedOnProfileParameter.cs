namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.MediationSnippets;

	public class SetToExecuteBasedOnProfileParameter : ISetToExecute
	{
		public string Description => ProfileParameterName;

		public int ProtocolReadParameterId { get; set; }

		public object ValueToSet
		{
			get
			{
				try
				{
					return Mediator.ConvertProfileToDevice(null, ParameterValue.CreateValue(ProfileParameterValue)).MediatedValue.GetValue();
				}
				catch(Exception ex)
				{
					throw new InvalidOperationException($"Profile parameter value for parameter {ProfileParameterName} has invalid value {ProfileParameterValue}", ex);
				}
			}
		} 

		public string ProfileParameterName { get; set; }

		public object ProfileParameterValue { get; set; }

		public int NumberOfRetries { get; set; } = 4;

		public List<object> ValuesToNotSet { get; set; } = new List<object>();

		public bool ShouldSetValue => !ValuesToNotSet.Contains(ProfileParameterValue);

		public IMediator Mediator { get; set; }
	}
}
