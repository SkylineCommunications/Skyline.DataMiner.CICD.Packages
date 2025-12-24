namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.EricssonRX8200;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Net.Messages;

	public static class EricssonRx8200DecodingSets
	{
		public static List<ISetToExecute> GetOrderedSetsToExecute(Helpers helpers, Resource decodingResource, IEnumerable<SrmParameterConfiguration> srmProfileParameters)
		{
			var setsToExecute = new List<ISetToExecute>();

			var encodingProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.Encoding) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.Encoding, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = encodingProfileParameter.ProfileParameterName,
				ProfileParameterValue = encodingProfileParameter.Value.DoubleValue,
				ProtocolReadParameterId = 143,
				Mediator = new DefaultMediation<string>(),
			});

			var serviceSelectionParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.ServiceSelection) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.ServiceSelection, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = serviceSelectionParameter.ProfileParameterName,
				ProfileParameterValue = serviceSelectionParameter.Value.DoubleValue,
				ProtocolReadParameterId = 8501,
				Mediator = new DefaultMediation<string>(),
			});

			var encryptionTypeProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.EncryptionType) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.EncryptionType, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = encryptionTypeProfileParameter.ProfileParameterName,
				ProfileParameterValue = encryptionTypeProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Encryption Type profile param has value {encryptionTypeProfileParameter.Value}"),
				ValuesToNotSet = new List<object> { "BISS-CA", "FREE" },
				ProtocolReadParameterId = 192,
				Mediator = new EncryptionType_EricssonRx8200(),
			});

			var encryptionKeyProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.EncryptionKey) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.EncryptionKey, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = encryptionKeyProfileParameter.ProfileParameterName,
				ProfileParameterValue = encryptionKeyProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Encryption Key profile param has value {encryptionKeyProfileParameter.Value}"),
				ProtocolReadParameterId = 391,
				Mediator = new DefaultMediation<string>(),
			});

			var resourceInputConnectionsAsiProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.ResourceInputConnectionsAsi) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.ResourceInputConnectionsAsi, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			helpers.Log(nameof(EricssonRx8200DecodingSets), nameof(GetOrderedSetsToExecute), $"ResourceInputConnections_ASI profile parameter value: '{resourceInputConnectionsAsiProfileParameter.Value.StringValue}'");

			bool resourceIsConnectedToNs3Demodulator = !string.IsNullOrEmpty(resourceInputConnectionsAsiProfileParameter.Value.StringValue) && resourceInputConnectionsAsiProfileParameter.Value.StringValue.Contains("NS3");

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = "Input Source",
				ProtocolReadParameterId = 58,
				ProfileParameterValue = resourceIsConnectedToNs3Demodulator ? 0.0 /* ASI (0) */ : 1.0 /* SAT (1) */ ,
				Mediator = new InputSource()
			});	

			return setsToExecute;
		}
	}
}
