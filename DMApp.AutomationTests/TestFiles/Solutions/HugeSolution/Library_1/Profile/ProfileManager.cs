namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using ProfileParameterNotFoundException = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ProfileParameterNotFoundException;

	public class ProfileManager : IProfileManager
	{
		private readonly Helpers helpers;

		private List<Net.Profiles.ProfileDefinition> allSrmProfileDefinitions;
		private List<Net.Profiles.Parameter> allSrmProfileParameters;

		public ProfileManager(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		private List<Net.Profiles.ProfileDefinition> AllSrmProfileDefinitions => allSrmProfileDefinitions ?? (allSrmProfileDefinitions = GetAllSrmProfileDefinitions());

		private List<Net.Profiles.Parameter> AllSrmProfileParameters => allSrmProfileParameters ?? (allSrmProfileParameters = GetAllSrmProfileParameters());

		public void PrepareCache()
		{
			allSrmProfileDefinitions = allSrmProfileDefinitions ?? GetAllSrmProfileDefinitions();
			allSrmProfileParameters = allSrmProfileParameters ?? GetAllSrmProfileParameters();
		}

		public ProfileParameter GetProfileParameter(Guid guid)
		{
			var parameter = AllSrmProfileParameters.SingleOrDefault(pp => pp.ID == guid) ?? throw new ProfileParameterNotFoundException(guid);

			return new ProfileParameter(parameter);
		}

		public ProfileParameter GetProfileParameter(string profileParameterName)
		{
			var parameter = AllSrmProfileParameters.SingleOrDefault(pp => pp.Name == profileParameterName) ?? throw new ProfileParameterNotFoundException(profileParameterName);

			return new ProfileParameter(parameter);
		}

		public ProfileDefinition GetProfileDefinition(Guid id)
		{
			var srmProfileDefinition = AllSrmProfileDefinitions.SingleOrDefault(pd => pd.ID == id) ?? throw new ProfileDefinitionNotFoundException(id);

			return new ProfileDefinition(srmProfileDefinition);
		}

		public Dictionary<Guid, ProfileDefinition> GetInterfaceProfileDefinitions(FunctionDefinition functionDefinition)
		{
			if (functionDefinition == null) throw new ArgumentNullException(nameof(functionDefinition));

			var interfaceProfileDefinitions = new Dictionary<Guid, ProfileDefinition>();

			var functionInterfaces = functionDefinition.InputInterfaces.Concat(functionDefinition.OutputInterfaces).Concat(functionDefinition.InputOutputInterfaces).ToList();

			foreach (var functionInterface in functionInterfaces)
			{
				if (interfaceProfileDefinitions.ContainsKey(functionInterface.ProfileDefinition)) continue;

				var systemProfileDefinition = AllSrmProfileDefinitions.SingleOrDefault(pd => pd.ID == functionInterface.ProfileDefinition) ?? throw new ProfileDefinitionNotFoundException($"Unable to find profile definition {functionInterface.ProfileDefinition} for interface {functionInterface.Name} in function definition {functionDefinition.Name}");

				var profileDefinition = new ProfileDefinition(systemProfileDefinition);

				interfaceProfileDefinitions[profileDefinition.Id] = profileDefinition;
			}

			return interfaceProfileDefinitions;
		}

		private List<Net.Profiles.ProfileDefinition> GetAllSrmProfileDefinitions()
		{
			LogMethodStart(nameof(GetAllSrmProfileDefinitions), out var stopwatch);

			var retrievedSrmProfileDefinitions = DataMinerInterface.ProfileHelper.ReadAllProfileDefinitions(helpers);

			Log(nameof(GetAllSrmProfileDefinitions), $"Retrieved {retrievedSrmProfileDefinitions.Count} profile definitions");

			LogMethodCompleted(nameof(GetAllSrmProfileDefinitions), stopwatch);

			return retrievedSrmProfileDefinitions;
		}

		private List<Net.Profiles.Parameter> GetAllSrmProfileParameters()
		{
			LogMethodStart(nameof(GetAllSrmProfileParameters), out var stopwatch);

			var retrievedProfileParameters = DataMinerInterface.ProfileHelper.ReadAllProfileParameters(helpers);

			Log(nameof(GetAllSrmProfileParameters), $"Retrieved {retrievedProfileParameters.Count} profile parameters");

			LogMethodCompleted(nameof(GetAllSrmProfileParameters), stopwatch);

			return retrievedProfileParameters;
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(nameof(ProfileManager), nameOfMethod, message, nameOfObject);
		}

		private void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			Log(nameOfMethod, "Start");

			stopwatch = Stopwatch.StartNew();
		}

		private void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			stopwatch?.Stop();

			Log(nameOfMethod, $"Completed [{stopwatch?.Elapsed}]");
		}
	}
}