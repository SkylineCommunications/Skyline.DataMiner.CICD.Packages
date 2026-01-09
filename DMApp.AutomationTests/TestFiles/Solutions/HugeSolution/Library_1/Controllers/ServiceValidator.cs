namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using NPOI.OpenXmlFormats.Dml.Chart;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Service = Service.Service;

	public sealed class ServiceValidator : Validator
	{
		private readonly DisplayedService service;
		private readonly UserInfo userInfo;
		private readonly Options options;

		[Flags]
		public enum Options
		{
			None = 0,
			SkipCheckOccupyingOrderLocks = 1,
			StartNow = 2,
			SkipStartTimeValidation = 4,
			BlockResourceOverbooked = 8,
			SavedServiceIsBeingBooked = 16,
			SkipEncryptionKeyValidation = 32,
		}

		public ServiceValidator(Helpers helpers, DisplayedService service, UserInfo userInfo, Options options = Options.None) : base(helpers)
		{
			helpers.Log(nameof(ServiceValidator), "Constructor", $"Options: {string.Join(", ", EnumExtensions.GetFlags(options))}");

			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.options = options;
		}

		[ValidationStep]
		private bool ValidateFunctions()
		{
			bool selectedResourcesAreValid = true;

			var now = DateTime.Now;

			foreach (var occupiedResource in service.Functions.Select(f => f.Resource).OfType<OccupiedResource>())
			{
				selectedResourcesAreValid = ValidateOccupiedResource(selectedResourcesAreValid, now, occupiedResource);
			}

			if (options.HasFlag(Options.BlockResourceOverbooked))
			{
				bool allFunctionsHaveResources = service.VerifyFunctionResources(helpers);
				selectedResourcesAreValid &= allFunctionsHaveResources;

				if (!allFunctionsHaveResources)
				{
					string serviceDisplayName = service.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Reception ? "source" : service.LofDisplayName;
					ValidationMessages.Add($"One or more resources for{(service.BackupType == Order.BackupType.None ? string.Empty : " backup")} {serviceDisplayName} are not available");
				}
			}

			return selectedResourcesAreValid;
		}

		private bool ValidateOccupiedResource(bool selectedResourcesAreValid, DateTime now, OccupiedResource occupiedResource)
		{
			var occupyingOrders = occupiedResource.OccupyingServices.SelectMany(os => os.Orders).ToList();

			var runningOccupyingOrders = occupyingOrders.Where(o => o.Start.FromReservation() <= now && now <= o.End.FromReservation()).ToList();

					bool occupyingOrdersAreRunning = runningOccupyingOrders.Any();

					selectedResourcesAreValid &= !occupyingOrdersAreRunning || !occupiedResource.IsFullyOccupied;

					if (occupyingOrdersAreRunning && occupiedResource.IsFullyOccupied)
					{
						ValidationMessages.Add($"Resource {occupiedResource.Name} is being used by running order(s) {string.Join(", ", runningOccupyingOrders.Select(o => o.Name))}");
					}

			var occupyingSharedServices = occupiedResource.OccupyingServices.Where(os => os.Orders.Count > 1).ToList();
			bool isUsedInSharedService = occupyingSharedServices.Any();

			selectedResourcesAreValid &= !isUsedInSharedService;

			if (isUsedInSharedService)
			{
				ValidationMessages.Add($"Resource {occupiedResource.Name} is being used by shared service(s) {string.Join(", ", occupyingSharedServices.Select(o => o.Service.Name))}");
			}

			bool isUsedByServiceInSameOrder = occupyingOrders.Select(o => o.ID).Intersect(service.OrderReferences).Any();

			selectedResourcesAreValid &= !isUsedByServiceInSameOrder;

			if (isUsedByServiceInSameOrder)
			{
				ValidationMessages.Add($"Resource {occupiedResource.Name} is already being used by a service in the same order");
			}

			if (!options.HasFlag(Options.SkipCheckOccupyingOrderLocks))
			{
				var occupyingOrderLocks = new Dictionary<ReservationInstance, LockInfo>();
				foreach (var occupyingOrder in occupyingOrders)
				{
					var lockInfo = helpers.LockManager.RequestOrderLock(occupyingOrder.ID);

					occupyingOrderLocks.Add(occupyingOrder, lockInfo);
				}

				var notGrantedLocks = occupyingOrderLocks.Where(pair => !pair.Value.IsLockGranted).ToList();
				if (notGrantedLocks.Any())
				{
					ValidationMessages.Add($"Following locks are already taken:\n{string.Join("\n-", notGrantedLocks.Select(pair => $"Lock for order {pair.Key.Name} is taken by {pair.Value.LockUsername}"))}");
				}

				selectedResourcesAreValid &= !notGrantedLocks.Any();
			}

			return selectedResourcesAreValid;
		}

		[ValidationStep]
		private bool ValidateAdditionalInformation()
		{
			bool isServiceCommentsValid = service.Comments == null || service.Comments.Length <= Constants.MaximumAllowedCharacters;

			if (!isServiceCommentsValid)
			{
				ValidationMessages.Add($"Maximum allowed characters for comments is {Constants.MaximumAllowedCharacters}");
			}

			service.SetPropertyValidation(nameof(Service.Comments), isServiceCommentsValid, $"Maximum {Constants.MaximumAllowedCharacters} characters allowed");

			return isServiceCommentsValid;
		}

		[ValidationStep]
		private bool ValidateStartTiming()
		{
			if (options.HasFlag(Options.SkipStartTimeValidation)) return true;

			var serviceChangeSummary = service.Change.Summary as ServiceChangeSummary;

			bool serviceStartTimingHasChanged = serviceChangeSummary.TimingChangeSummary.StartTimingChanged;
			bool serviceStartTimeIsBeforeNow = service.Start.ToUniversalTime() < DateTime.UtcNow;
			bool isServiceStartBeforeServiceEnd = service.Start < service.End;
			bool serviceStartWithPrerollIsBeforeEarliestAllowedStart = service.StartWithPreRoll < DateTime.Now.AddMinutes(Order.Order.StartInTheFutureDelayInMinutes);

			if (!isServiceStartBeforeServiceEnd)
			{
				service.StartValidation.SetValidationInfo(UIValidationState.Invalid, "Service start time cannot be later than the end time.");
			}
			else if (!options.HasFlag(Options.StartNow) && (serviceStartTimingHasChanged || options.HasFlag(Options.SavedServiceIsBeingBooked)) && serviceStartTimeIsBeforeNow)
			{
				service.StartValidation.SetValidationInfo(UIValidationState.Invalid, "Service start cannot be in the past.");
			}
			else if (!options.HasFlag(Options.StartNow) && (serviceStartTimingHasChanged || options.HasFlag(Options.SavedServiceIsBeingBooked)) && serviceStartWithPrerollIsBeforeEarliestAllowedStart)
			{
				service.StartValidation.SetValidationInfo(UIValidationState.Invalid, $"Service start with preroll should be at least {Order.Order.StartInTheFutureDelayInMinutes} minutes into the future.");
			}
			else
			{
				service.StartValidation.SetValidationInfo(UIValidationState.Valid, string.Empty);
			}

			if (!service.StartValidation.IsValid)
			{
				ValidationMessages.Add($"{(service.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Reception ? $"{(service.BackupType == Order.BackupType.None ? "S" : "Backup s")}ource" : $"Service {service.LofDisplayName}")} start time is not valid");
			}

			return service.StartValidation.IsValid;
		}

		[ValidationStep]
		private bool ValidateRecordingConfiguration()
		{
			bool isValid = true;

			if (new [] { ServiceDefinitionGuids.RecordingMessiLive, ServiceDefinitionGuids.RecordingMessiLiveBackup}.Contains(service.Definition.Id))
			{
				bool plasmaIdForArchiveIsValid = !string.IsNullOrEmpty(service.RecordingConfiguration.PlasmaIdForArchive);

				service.RecordingConfiguration.SetPropertyValidation(nameof(RecordingConfiguration.PlasmaIdForArchive), plasmaIdForArchiveIsValid, "Provide a value");

				if (!plasmaIdForArchiveIsValid)
				{
					ValidationMessages.Add($"Unable to book because there is no Plasma ID for Archive provided for {service.LofDisplayName}");
				}

				isValid &= plasmaIdForArchiveIsValid;
			}

			isValid &= ValidateSubRecordings();

			return isValid;
		}

		private bool ValidateSubRecordings()
		{
			bool isValid = true;

			if (service.RecordingConfiguration is null)
			{
				Log(nameof(ValidateSubRecordings), $"RecordingConfiguration is null for service {service.Name}");
				return isValid;
			}

			if (service.RecordingConfiguration.SubRecordings is null)
			{
				Log(nameof(ValidateSubRecordings), $"RecordingConfiguration.SubRecordings is null for service {service.Name}");
				return isValid;
			}

			foreach (var subRecording in service.RecordingConfiguration.SubRecordings)
			{
				bool subRecordingNameIsValid = !string.IsNullOrWhiteSpace(subRecording.Name);

				subRecording.SetPropertyValidation(nameof(SubRecording.Name), subRecordingNameIsValid, "Sub-Recording Name cannot be empty.");

				isValid &= subRecordingNameIsValid;
			}

			if (!isValid)
			{
				ValidationMessages.Add($"Sub-recordings are not valid");
			}

			return isValid;
		}

		[ValidationStep]
		private bool ValidateOtherSatelliteName()
		{
			if (service.Definition.VirtualPlatform != ServiceDefinition.VirtualPlatform.ReceptionSatellite) return true;

			bool isValid = true;

			var satelliteFunction = service.Functions.SingleOrDefault(x => x.Id == FunctionGuids.Satellite) ?? throw new NotFoundException($"Unable to find Satellite function (ID {FunctionGuids.Satellite})");
	
			var otherSatelliteNameProfileParameter = satelliteFunction.Parameters.SingleOrDefault(x => x.Id == ProfileParameterGuids.OtherSatelliteName) ?? throw new NotFoundException($"Unable to find Other Satellite Name profile parameter (ID {ProfileParameterGuids.OtherSatelliteName})");
			
			bool isOtherSatelliteNameValid = (!satelliteFunction.Resource?.GetResourcePropertyBooleanValue(ResourcePropertyNames.OtherSatelliteNameRequired) ?? true) || !string.IsNullOrWhiteSpace(Convert.ToString(otherSatelliteNameProfileParameter.Value));

			otherSatelliteNameProfileParameter.ValueValidation.SetValidationInfo(isOtherSatelliteNameValid ? UIValidationState.Valid : UIValidationState.Invalid, "Provide a Satellite Name in case Other is selected as Satellite Resource");

			isValid = otherSatelliteNameProfileParameter.ValueValidation.IsValid;

			if (!isValid)
			{
				ValidationMessages.Add($"Other satellite name is not valid");
			}

			return isValid;
		}

		[ValidationStep]
		private bool ValidateEncryptionKey()
		{
			if (options.HasFlag(Options.SkipEncryptionKeyValidation))
			{
				return true;
			}

			if (!userInfo.IsMcrUser)
			{
				return true;
			}

			if (service.Definition.VirtualPlatform != ServiceDefinition.VirtualPlatform.ReceptionSatellite)
			{
				// only validate for SAT RX
				// find better solution: based on widget visibility we should know if validation is required or not
				return true;
			}

			bool isValid = true;
			var functionsContainingEncryptionParams = service.Functions.Where(x => x.Parameters.Any(p => p.Id == ProfileParameterGuids.EncryptionType) && x.Parameters.Any(d => d.Id == ProfileParameterGuids.EncryptionKey)).ToList();
			foreach (var function in functionsContainingEncryptionParams)
			{
				bool currentFunctionIsValid = true;
				int lengthNumber;

                var encryptionType = function.Parameters.SingleOrDefault(x => x.Id == ProfileParameterGuids.EncryptionType)?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.EncryptionType);
                var encryptionKey = function.Parameters.SingleOrDefault(x => x.Id == ProfileParameterGuids.EncryptionKey)?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.EncryptionKey);
				switch(encryptionType.StringValue)
				{
					case "BISS-1":
                        lengthNumber = 12;
                        currentFunctionIsValid &= long.TryParse(encryptionKey.StringValue, System.Globalization.NumberStyles.HexNumber, null, out var output);
                        currentFunctionIsValid &= encryptionKey.StringValue.Length == lengthNumber;
						break;
					case "BISS-E":
					case "BISS-E LA LIGA":
                        lengthNumber = 16;
                        currentFunctionIsValid &= long.TryParse(encryptionKey.StringValue, System.Globalization.NumberStyles.HexNumber, null, out var output2);
                        currentFunctionIsValid &= encryptionKey.StringValue.Length == lengthNumber;
                        break;
					default:
						lengthNumber = 0; // ValueValidation will always be true in this case.
                        break;
				}

                encryptionKey.ValueValidation.SetValidationInfo(currentFunctionIsValid ? UIValidationState.Valid : UIValidationState.Invalid, $"Provide a hexadecimal encryption key of {lengthNumber} characters.");
                isValid &= currentFunctionIsValid;
            }

			if (!isValid)
			{
                ValidationMessages.Add($"Encryption key is not valid.");
            }

			return isValid;
		}

		[ValidationStep]
		private bool ValidateSymbolRate()
		{
			var symbolRate = service.Functions.SelectMany(x => x.Parameters).FirstOrDefault(x => x.Id == ProfileParameterGuids.SymbolRate);

			if (symbolRate is null)
			{
				return true;
			}

			Log(nameof(ValidateSymbolRate), $"Value of symbol rate after converting it to double is {(double)symbolRate.Value}");

			var symbolRateIsValid = (double)symbolRate.Value >= symbolRate.RangeMin && (double)symbolRate.Value <= symbolRate.RangeMax;
			symbolRate.ValueValidation.SetValidationInfo(symbolRateIsValid ? UIValidationState.Valid : UIValidationState.Invalid, $"Provide Symbol Rate in range from {symbolRate.RangeMin} to {symbolRate.RangeMax}.");

			if (!symbolRateIsValid)
			{
				ValidationMessages.Add($"Symbol Rate is not valid.");
			}

			return symbolRateIsValid;
		}

		[ValidationStep]
		private bool ValidateDownlinkFrequency()
		{
			var downlinkFrequency = service.Functions.SelectMany(x => x.Parameters).FirstOrDefault(x => x.Id == ProfileParameterGuids.DownlinkFrequency);

			if (downlinkFrequency is null)
			{
				return true;
			}

			Log(nameof(ValidateDownlinkFrequency), $"Value of Downlink Frequency after converting it to double is {(double)downlinkFrequency.Value}");

			var downlinkFrequencyIsValid = (double)downlinkFrequency.Value >= downlinkFrequency.RangeMin && (double)downlinkFrequency.Value <= downlinkFrequency.RangeMax;
			downlinkFrequency.ValueValidation.SetValidationInfo(downlinkFrequencyIsValid ? UIValidationState.Valid : UIValidationState.Invalid, $"Provide downlink frequency in range from {downlinkFrequency.RangeMin} to {downlinkFrequency.RangeMax}.");

			if (!downlinkFrequencyIsValid)
			{
				ValidationMessages.Add($"Downlink Frequency is not valid.");
			}

			return downlinkFrequencyIsValid;
		}
	}
}
