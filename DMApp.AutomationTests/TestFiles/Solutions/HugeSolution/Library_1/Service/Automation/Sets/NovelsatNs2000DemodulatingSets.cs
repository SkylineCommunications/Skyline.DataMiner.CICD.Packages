namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.MediationSnippets.NovelsatNS2000;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Net.Messages;

	internal class NovelsatNs2000DemodulatingSets
	{
		public static List<ISetToExecute> GetOrderedSetsToExecute(Resource resource, IEnumerable<SrmParameterConfiguration> srmProfileParameters)
		{
			var setsToExecute = new List<ISetToExecute>();

			var lnbFrequencyProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids._LnbFrequency) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids._LnbFrequency, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = lnbFrequencyProfileParameter.ProfileParameterName,
				ProfileParameterValue = lnbFrequencyProfileParameter.Value.DoubleValue,
				ProtocolReadParameterId = 212,
				NumberOfRetries = 10,
				Mediator = new DefaultMediation<double>(),
			});

			var downlinkFrequencyParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.DownlinkFrequency) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.DownlinkFrequency, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = downlinkFrequencyParameter.ProfileParameterName,
				ProfileParameterValue = downlinkFrequencyParameter.Value.DoubleValue,
				ProtocolReadParameterId = 201,
				NumberOfRetries = 10,
				Mediator = new DefaultMediation<double>(),
			});

			var modulationProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.Modulation) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.Modulation, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = modulationProfileParameter.ProfileParameterName,
				ProfileParameterValue = modulationProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Modulation Standard profile param has value {modulationProfileParameter.Value}"),
				ProtocolReadParameterId = 432,
				NumberOfRetries = 10,
				Mediator = new ModulationNovelsetNs2000(),
			});

			var fecProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.Fec) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.Fec, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = fecProfileParameter.ProfileParameterName,
				ProfileParameterValue = fecProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Modulation Standard profile param has value {fecProfileParameter.Value}"),
				ProtocolReadParameterId = 431,
				ValuesToNotSet = new List<object> { "6/7", "7/15" },
				NumberOfRetries = 10,
				Mediator = new FecNovelsatNs2000(),
			});

			var modulationStandardProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.ModulationStandard) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.ModulationStandard, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = modulationStandardProfileParameter.ProfileParameterName,
				ProfileParameterValue = modulationStandardProfileParameter.Value.StringValue ?? throw new InvalidOperationException($"Modulation Standard profile param has value {modulationStandardProfileParameter.Value}"),
				ProtocolReadParameterId = 200,
				NumberOfRetries = 10,
				Mediator = new ModulationStandardNovelsetNs2000(),
			});

			var symbolRateProfileParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.SymbolRate) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.SymbolRate, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = symbolRateProfileParameter.ProfileParameterName,
				ProfileParameterValue = symbolRateProfileParameter.Value.DoubleValue,
				ProtocolReadParameterId = 203,
				NumberOfRetries = 10,
				Mediator = new DefaultMediation<double>(),
			});

			var rollOffParameter = srmProfileParameters.SingleOrDefault(p => p.ProfileParameterEntry.ParameterID == ProfileParameterGuids.RollOff) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.RollOff, null, srmProfileParameters.Select(pp => pp.ProfileParameterEntry.ParameterID));

			setsToExecute.Add(new SetToExecuteBasedOnProfileParameter
			{
				ProfileParameterName = rollOffParameter.ProfileParameterName,
				ProfileParameterValue = rollOffParameter.Value.StringValue,
				ProtocolReadParameterId = 202,
				NumberOfRetries = 10,
				Mediator = new RollOffNovelsatNs2000(),
			});

			return setsToExecute;
		}
	}
}
