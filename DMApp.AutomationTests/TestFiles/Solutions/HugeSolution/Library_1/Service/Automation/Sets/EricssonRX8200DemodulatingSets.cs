namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.EricssonRX8200;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.MediationSnippets;
	using Skyline.DataMiner.Net.Messages;

	public static class EricssonRX8200DemodulatingSets
	{
		public static List<ISetToExecute> GetOrderedSetsToExecute(Resource resource, IEnumerable<SrmParameterConfiguration> srmProfileParameters)
		{
			var setsToExecute = new List<ISetToExecute>();

			var lnbFrequencyProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids._LnbFrequency) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids._LnbFrequency, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = lnbFrequencyProfileParameter.ProfileParameterName,
				ProfileParameterValue = lnbFrequencyProfileParameter.Value.DoubleValue,
				ProtocolReadParameterId = 1602,
				Mediator = new DefaultMediation<double>(),
			});

			var downlinkFrequencyParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.DownlinkFrequency) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.DownlinkFrequency, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = downlinkFrequencyParameter.ProfileParameterName,
				ProfileParameterValue = downlinkFrequencyParameter.Value.DoubleValue,
				ProtocolReadParameterId = 1603,
				Mediator = new DefaultMediation<double>(),
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

			var modulationStandardProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.ModulationStandard) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.ModulationStandard, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = modulationStandardProfileParameter.ProfileParameterName,
				ProfileParameterValue = modulationStandardProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Modulation Standard profile param has value {modulationStandardProfileParameter.Value}"),
				ProtocolReadParameterId = 1605,
				Mediator = resource.GetResourcePropertyBooleanValue(ResourcePropertyNames.MapDemodulationStandardToMatchingValue) ? (IMediator)new ModulationStandard() : new ModulationStandardMappedToAuto(),
			});

			var polarizationProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.Polarization) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.Polarization, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = polarizationProfileParameter.ProfileParameterName,
				ProfileParameterValue = polarizationProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Polarization profile param has value {polarizationProfileParameter.Value}"),
				ProtocolReadParameterId = 1610,
				Mediator = new Polarization_EricssonRx8200(),
			});

			var symbolRateProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.SymbolRate) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.SymbolRate, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = symbolRateProfileParameter.ProfileParameterName,
				ProfileParameterValue = symbolRateProfileParameter.Value.DoubleValue,
				ProtocolReadParameterId = 1604,
				Mediator = new DefaultMediation<double>(),
			});

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = "Input Source",
				ProtocolReadParameterId = 58,
				ProfileParameterValue = 1.0 /* SAT (1) */,
				Mediator = new InputSource()
			});

			setsToExecute.Add(new SetToExecute
			{
				Description = "Selected Input RF",
				ProtocolReadParameterId = 69,
				ValueToSet = GetSelectedInputRfToSet(resource),
			});

            var rollOffParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.RollOff) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.RollOff, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

            setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
            {
                ProfileParameterName = rollOffParameter.ProfileParameterName,
                ProfileParameterValue = rollOffParameter.Value.StringValue,
                ProtocolReadParameterId = 1606,
                Mediator = new RollOffEricssonRx8200(),
            });

            return setsToExecute;
		}

		private static string GetSelectedInputRfToSet(Resource resource)
		{
			if (resource.Name.Contains("Input 1")) return "RF #1";
			else if (resource.Name.Contains("Input 2")) return "RF #2";
			else if (resource.Name.Contains("Input 3")) return "RF #3";
			else if (resource.Name.Contains("Input 4")) return "RF #4";
			else return null;
		}
	}

}
