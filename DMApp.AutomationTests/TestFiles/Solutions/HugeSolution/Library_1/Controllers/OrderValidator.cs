namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Service.Service;

	[Flags]
	public enum Options
	{
		None = 0,
		SaveOrder = 1,
		IsRunning = 2,
		MergeOrder = 4,
		ConfirmOrder = 8,
		RequestEventLock = 16
	}

	public sealed class OrderValidator : Validator
	{
		private readonly Order order;
		private readonly UserInfo userInfo;
		private readonly Options options;

		public OrderValidator(Helpers helpers, Order order, UserInfo userInfo, Options options = Options.None) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.options = options;
		}

		private bool SavedOrderIsBeingBooked => order.IsBooked && !order.AllServices.Exists(s => s.IsBooked) && !options.HasFlag(Options.SaveOrder);

		[ValidationStep]
		private bool ValidateOrderName()
		{
			if (string.IsNullOrWhiteSpace(order.Name))
			{
				order.SetPropertyValidation(nameof(Order.ManualName), UIValidationState.Invalid, "Please provide a valid name for this order");
				ValidationMessages.Add("Unable to book because the Order name is not valid");
				return false;
			}

			char disallowedCharacter = order.Name.FirstOrDefault(character => LiteOrder.OrderNameDisallowedCharacters.Contains(character) || (int)character <= 31);
			bool nameContainsDisallowedCharacter = disallowedCharacter != default(char);

			if (nameContainsDisallowedCharacter)
			{
				order.SetPropertyValidation(nameof(Order.ManualName), UIValidationState.Invalid, disallowedCharacter <= 31 ? $"ASCII character {(int)disallowedCharacter} is not allowed." : $"Character '{disallowedCharacter}' is not allowed.");
				ValidationMessages.Add("Unable to book because the Order name is not valid");
				return false;
			}

			if (!order.IsBooked || ((OrderChangeSummary)order.Change.Summary).NameChanged)
			{
				var orderWithSameNameExists = helpers.ReservationManager.GetReservation(order.Name) != null;

				helpers.Log(nameof(OrderValidator), nameof(ValidateOrderName), $"orderWithSameNameExists: {orderWithSameNameExists}");

				if (orderWithSameNameExists)
				{
					order.SetPropertyValidation(nameof(Order.ManualName), UIValidationState.Invalid, "An order with this name already exists");
					ValidationMessages.Add("Unable to book because the Order name is not valid");
					return false;
				}

				if (order.RecurringSequenceInfo.Recurrence.IsConfigured)
				{
					var recurringOrdersTable = helpers.OrderManagerElement.GetRecurringOrdersTable();

					bool recurringOrderSequenceNameAlreadyExists = recurringOrdersTable.Exists(r => r.Name == order.ManualName);

					if (recurringOrderSequenceNameAlreadyExists)
					{
						order.SetPropertyValidation(nameof(Order.ManualName), UIValidationState.Invalid, "A recurring order sequence with this name already exists");
						ValidationMessages.Add("Unable to book because the Order name is not valid");
						return false;
					}
				}
			}

			order.SetPropertyValidation(nameof(Order.ManualName), UIValidationState.Valid, "Valid");

			return true;
		}


		[ValidationStep]
		private bool ValidateBookingSavedRunningOrder()
		{
			// Booking a saved order that should be running should follow the start now logic

			if (SavedOrderIsBeingBooked && order.ShouldBeRunning && !order.StartNow)
			{
				ValidationMessages.Add("When booking a saved order that should be running, please check the Start Now checkbox.");
				return false;
			}

			return true;
		}

		[ValidationStep]
		private bool ValidatePlasmaId()
		{
			order.SetPropertyValidation(nameof(Order.PlasmaId), UIValidationState.Valid, "Valid");

			if (String.IsNullOrEmpty(order.PlasmaId)) return true;

			bool plasmaIdHasChanged = ((OrderChange)order.Change).GetPropertyChange(nameof(Order.PlasmaId))?.Summary?.IsChanged ?? false;
			bool plasmaIdTextBoxValueIsSameAsMergedOrderPlasmaId = options.HasFlag(Options.MergeOrder) && !plasmaIdHasChanged;
			if (plasmaIdTextBoxValueIsSameAsMergedOrderPlasmaId) return true;

			// check if there is not yet an existing order with this plasma id
			var orderWithPlasmaId = helpers.OrderManager.GetPlasmaReservationInstance("dummy", order.PlasmaId);
			if (orderWithPlasmaId != null && order.Id != orderWithPlasmaId.ID)
			{
				order.SetPropertyValidation(nameof(Order.PlasmaId), UIValidationState.Invalid, "An order with this Plasma ID already exists");
				ValidationMessages.Add($"An order with Plasma ID {order.PlasmaId} already exists");
				return false;
			}

			return true;
		}

		[ValidationStep]
		private bool ValidateOrderTiming()
		{
			order.SetPropertyValidation(nameof(order.Start), UIValidationState.Valid);
			order.SetPropertyValidation(nameof(order.End), UIValidationState.Valid);

			// Remove seconds to improve precision
			var orderStart = order.Start.Truncate(TimeSpan.FromMinutes(1));
			var orderEnd = order.End.Truncate(TimeSpan.FromMinutes(1));

			if (orderStart >= orderEnd)
			{
				order.SetPropertyValidation(nameof(order.Start), UIValidationState.Invalid, "The order start time cannot be later than the end time");
				ValidationMessages.Add($"Unable to book because the order start time cannot be later than the end time");

				order.SetPropertyValidation(nameof(order.End), UIValidationState.Invalid, "The order start time cannot be later than the end time");
				ValidationMessages.Add($"Unable to book because the order start time cannot be later than the end time");

				return false;
			}

			var earliestAllowedOrderStart = DateTime.Now.AddMinutes(Order.StartInTheFutureDelayInMinutes);

			bool validStartTime = true;
			bool validEndTime = true;
			if (!options.HasFlag(Options.IsRunning) && !order.StartNow && !options.HasFlag(Options.ConfirmOrder))
			{
				if (order.Start < DateTime.Now)
				{
					order.SetPropertyValidation(nameof(order.Start), UIValidationState.Invalid, "The order start time cannot be in the past");
					ValidationMessages.Add($"Unable to book because the order start time cannot be in the past");
					validStartTime = false;
				}
				else if (order.Start <= earliestAllowedOrderStart)
				{
					order.SetPropertyValidation(nameof(order.Start), UIValidationState.Invalid, $"The order start time cannot be closer than {Order.StartInTheFutureDelayInMinutes} minutes to going live");
					ValidationMessages.Add($"Unable to book because the order start time cannot be this close to going live");
					validStartTime = false;
				}
				else
				{
					//Nothing
				}
			}

			if (orderEnd <= DateTime.Now)
			{
				order.SetPropertyValidation(nameof(order.End), UIValidationState.Invalid, "The order end time cannot be in the past");
				ValidationMessages.Add($"Unable to book because the order end time cannot be in the past");
				validEndTime = false;
			}

			int startNowDelayInMinutes = (order.IntegrationType == IntegrationType.Feenix) ? Order.StartNowDelayInMinutesForFeenix : Order.StartNowDelayInMinutes;
			if (order.StartNow && orderStart.AddMinutes(startNowDelayInMinutes) >= orderEnd)
			{
				order.SetPropertyValidation(nameof(order.End), UIValidationState.Invalid, $"When starting now, end time must be at least {startNowDelayInMinutes} minutes later than the start time");
				ValidationMessages.Add($"Unable to book because when Start Now, end time must be at least {startNowDelayInMinutes} minutes later than the start time");
				validEndTime = false;
			}

			return validStartTime && validEndTime;
		}

		[ValidationStep]
		private bool ValidateOrderEventLocking()
		{
			if (!options.HasFlag(Options.RequestEventLock)) return true;

			if (order.Event != null && order.Event.Start <= order.Start && order.End <= order.Event.End)
			{
				return true;
			}

			if (order.Event != null)
			{
				var eventLockInfo = helpers.LockManager.RequestEventLock(order.Event.Id);
				if (eventLockInfo.LockUsername.Contains("error") || !eventLockInfo.IsLockGranted)
				{
					ValidationMessages.Add("Unable to Book Order as the Event under which the order will be added is currently locked by another process");
					return false;
				}
			}

			return true;
		}


		[ValidationStep]
		private bool ValidateServiceCombination()
		{
			if (!order.Sources.Any()) return options.HasFlag(Options.SaveOrder);

			bool orderContainsMainEndPointServices = order.GetAllMainServices().Exists(s => s.Definition.IsEndPointService);

			if (order.SourceService.Definition.IsSourceOnly && orderContainsMainEndPointServices)
			{
				ValidationMessages.Add("Order with this source does not allow any destination(s), recording(s) or transmission(s).");
				return false;
			}

			bool onlyMessiNewsRecordingsAllowed = order.SourceService.Functions.FirstOrDefault()?.Resource?.GetResourcePropertyBooleanValue(ResourcePropertyNames.OnlyMessiNewsRecordingAllowedPropertyName) ?? false;

			bool orderContainsNonMessiNewsMainEndPointServices = OrderManager.FlattenServices(order.SourceService.Children).Exists(s => s.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Destination
			|| s.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Transmission
			|| (s.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Recording && s.Definition.Description != "Messi News"));

			if (onlyMessiNewsRecordingsAllowed && orderContainsNonMessiNewsMainEndPointServices)
			{
				ValidationMessages.Add("Order with this source only allows Messi News recording(s).");
				return false;
			}

			return true;
		}

		[ValidationStep]
		private bool ValidateServices()
		{
			if (order.Subtype == OrderSubType.Vizrem)
			{
				var serviceWithoutResources = order.AllServices.Where(s => s.Functions.Exists(f => f.Resource is null)).ToList();

				foreach (var serviceWithoutResource in serviceWithoutResources)
				{
					ValidationMessages.Add($"Unable to book because there is no resource available for {serviceWithoutResource.Definition.VirtualPlatform.GetDescription()}");
				}

				bool servicesHaveResources = !serviceWithoutResources.Any();

				bool converterResourcesAreAvailable = ConverterResourcesAreAvailable();

				return servicesHaveResources && converterResourcesAreAvailable;
			}

			if (options.HasFlag(Options.SaveOrder))
			{
				/*
				// Check if Eurovision services are valid - Check already included when booking an order
				if (dialog.SourceServiceSection != null && dialog.SourceServiceSection.IsEurovisionReception && !dialog.SourceServiceSection.EurovisionReception.IsValid())
				{
					return false;
				}

				if (dialog.SourceTransmissionSection != null && dialog.SourceTransmissionSection.IsEurovisionTransmission && !dialog.SourceTransmissionSection.EurovisionTransmission.IsValid()) return false;

				if (dialog.BackupSourceServiceSection != null && dialog.BackupSourceServiceSection.IsEurovisionReception && !dialog.BackupSourceServiceSection.EurovisionReception.IsValid()) return false;

				if (dialog.BackupTransmissionSection != null && dialog.BackupTransmissionSection.IsEurovisionTransmission && !dialog.BackupTransmissionSection.EurovisionTransmission.IsValid()) return false;
				*/

				//if (order.SourceService.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.ReceptionEurovision)
				//{
				//	throw new NotImplementedException();
				//}

				var ebuServices = order.AllServices.Where(x => x.Definition.VirtualPlatformServiceName == ServiceDefinition.VirtualPlatformName.Eurovision && x.LinkEurovisionId);
				bool areEbuServicesValid = ebuServices.All(x => !String.IsNullOrWhiteSpace(x.EurovisionTransmissionNumber));

				if (!areEbuServicesValid) ValidationMessages.Add($"Unable to save as no EBU Synopsis ID is provided");

				return areEbuServicesValid;
			}
			else
			{
				return ValidateOrderServices();
			}
		}

		private bool ConverterResourcesAreAvailable()
		{
			var studioAsDestination = order.AllServices.SingleOrDefault(s => !s.Children.Any()) ?? throw new ServiceNotFoundException($"Unable to find vizrem studio as destination service", true);
			if (studioAsDestination.Definition.Id == ServiceDefinitionGuids.St26NdiRouter) return true;

			bool converterResourcesAreAvailable = true;

			var vizremFarmService = order.AllServices.SingleOrDefault(s => s.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremFarm) ?? throw new ServiceNotFoundException(ServiceDefinition.VirtualPlatform.VizremFarm);

			FunctionDefinition converterFunctionDefinition;
			string inputResourceName;
			string outputResourceName;

			// Check destination

			if (studioAsDestination.Definition.Name.Contains("Helsinki"))
			{
				converterFunctionDefinition = helpers.ServiceDefinitionManager.VizremConverterHelsinkiServiceDefinition.FunctionDefinitions.Single();
			}
			else if (studioAsDestination.Definition.Name.Contains("Mediapolis"))
			{
				converterFunctionDefinition = helpers.ServiceDefinitionManager.VizremConverterMediapolisServiceDefinition.FunctionDefinitions.Single();
			}
			else
			{
				throw new InvalidOperationException($"Unknown studio service definition: '{studioAsDestination.Definition.Name}'");
			}

			inputResourceName = vizremFarmService.Functions.Single().Resource.Name;
			outputResourceName = studioAsDestination.Functions.Single().Resource.Name;
			var converterToDestination = order.AllServices.SingleOrDefault(s => s.Children.Contains(studioAsDestination) && s.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremNC2Converter);

			converterResourcesAreAvailable &= MatchingConverterResourcesAreAvailable(converterToDestination, converterFunctionDefinition, ProfileParameterGuids.ResourceInputConnectionsNdi, ProfileParameterGuids.ResourceOutputConnectionsSdi, studioAsDestination.StartWithPreRoll, studioAsDestination.EndWithPostRoll, inputResourceName, outputResourceName);

			// Check source

			var studioAsSource = order.Sources.SingleOrDefault(s => s.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremStudio);
			if (studioAsSource is null || studioAsSource.Definition.Id == ServiceDefinitionGuids.St26NdiRouter) return converterResourcesAreAvailable;

			if (studioAsSource.Definition.Name.Contains("Helsinki"))
			{
				converterFunctionDefinition = helpers.ServiceDefinitionManager.VizremConverterHelsinkiServiceDefinition.FunctionDefinitions.Single();
			}
			else if (studioAsSource.Definition.Name.Contains("Mediapolis"))
			{
				converterFunctionDefinition = helpers.ServiceDefinitionManager.VizremConverterMediapolisServiceDefinition.FunctionDefinitions.Single();
			}
			else
			{
				throw new InvalidOperationException($"Unknown studio service definition: '{studioAsSource.Definition.Name}'");
			}

			inputResourceName = studioAsSource.Functions.Single().Resource.Name;
			outputResourceName = vizremFarmService.Functions.Single().Resource.Name;
			var converterFromSource = studioAsSource.Children.SingleOrDefault(s => s.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.VizremNC2Converter);

			converterResourcesAreAvailable &= MatchingConverterResourcesAreAvailable(converterFromSource, converterFunctionDefinition, ProfileParameterGuids.ResourceInputConnectionsSdi, ProfileParameterGuids.ResourceOutputConnectionsNdi, studioAsSource.StartWithPreRoll, studioAsSource.EndWithPostRoll, inputResourceName, outputResourceName);

			return converterResourcesAreAvailable;
		}

		private bool MatchingConverterResourcesAreAvailable(Service existingConverterService, FunctionDefinition converterFunctionDefinition, Guid inputConnectionsParameterId, Guid outputConnectionsParameterId, DateTime start, DateTime end, string inputResourceName, string outputResourceName)
		{
			if (converterFunctionDefinition is null) throw new ArgumentNullException(nameof(converterFunctionDefinition));
			if (string.IsNullOrEmpty(inputResourceName)) throw new ArgumentException($"'{nameof(inputResourceName)}' cannot be null or empty.", nameof(inputResourceName));
			if (string.IsNullOrEmpty(outputResourceName)) throw new ArgumentException($"'{nameof(outputResourceName)}' cannot be null or empty.", nameof(outputResourceName));

			var context = converterFunctionDefinition.GetEligibleResourceContext(helpers, start, end, existingConverterService?.Id, existingConverterService?.Functions?.SingleOrDefault()?.NodeId);

			var converterResources = helpers.ResourceManager.GetAvailableResources(context.Yield())[converterFunctionDefinition.Label];

			var resourceInputConnectionsProfileParameter = converterFunctionDefinition.InputInterfaces.SelectMany(pd => pd.ProfileDefinition.ProfileParameters).SingleOrDefault(pp => pp.Id == inputConnectionsParameterId) ?? throw new ProfileParameterNotFoundException(inputConnectionsParameterId);

			resourceInputConnectionsProfileParameter.Value = inputResourceName;

			var resourceOutputConnectionsProfileParameter = converterFunctionDefinition.OutputInterfaces.SelectMany(pd => pd.ProfileDefinition.ProfileParameters).SingleOrDefault(pp => pp.Id == outputConnectionsParameterId) ?? throw new ProfileParameterNotFoundException(outputConnectionsParameterId);

			resourceOutputConnectionsProfileParameter.Value = outputResourceName;

			if (converterResources.Any(r => r.MatchesProfileParameters(helpers, new List<Profile.ProfileParameter> { resourceInputConnectionsProfileParameter, resourceOutputConnectionsProfileParameter })))
			{
				return true;
			}

			ValidationMessages.Add($"Unable to book because there is no NC2 converter resource available from {resourceInputConnectionsProfileParameter.StringValue} to {resourceOutputConnectionsProfileParameter.StringValue}");

			return false;
		}

		private bool ValidateOrderServices()
		{
			var isValid = true;

			isValid &= ValidateOrderServices(out bool hasEurovisionService);

			// when booking an order there must be at least 1 destination, recording or transmission service defined

			bool validEndpointServicePresent = order.AllServices.Exists(s => s.Definition.IsEndPointService && s.BackupType == BackupType.None);

			bool eventLevelReceptionCreationByMcrUser = !order.SourceService.IsDummy && userInfo.IsMcrUser && order.AllServices.Exists(s => s.IsSharedSource);

			bool validEndpointServiceRequired = !order.SourceService.Definition.IsSourceOnly && !eventLevelReceptionCreationByMcrUser;

			if (validEndpointServiceRequired && !validEndpointServicePresent)
			{
				ValidationMessages.Add("Unable to book because there is no destination, recording or transmission");
				isValid = false;
			}

			if (order.SourceService.IsDummy && order.SourceService.Definition.VirtualPlatformServiceName != ServiceDefinition.VirtualPlatformName.Eurovision)
			{
				((DisplayedService)order.SourceService).AvailableVirtualPlatformNamesValidation = new UI.ValidationInfo
				{
					State = UIValidationState.Invalid,
					Text = "Order can only be booked with a valid source"
				};

				order.AvailableSharedSourcesValidation = new UI.ValidationInfo
				{
					State = UIValidationState.Invalid,
					Text = "Order can only be booked with a valid source"
				};

				ValidationMessages.Add($"Unable to book because there is no source");

				isValid = false;
			}
			else
			{
				((DisplayedService)order.SourceService).AvailableVirtualPlatformNamesValidation = new UI.ValidationInfo { State = UIValidationState.Valid };

				order.AvailableSharedSourcesValidation = new UI.ValidationInfo { State = UIValidationState.Valid };
			}

			if (isValid && hasEurovisionService)
			{
				isValid = false;
				ValidationMessages.Add("Unable to book as it contains a Eurovision reception and/or transmission");
			}

			return isValid;
		}

		private bool ValidateOrderServices(out bool hasEurovisionService)
		{
			hasEurovisionService = false;
			var isValid = true;
			foreach (var service in order.AllServices)
			{
				if (service.Definition.VirtualPlatformServiceName == ServiceDefinition.VirtualPlatformName.Eurovision) hasEurovisionService = true;

				var validator = GenerateServiceValidator(service as DisplayedService);
				isValid &= validator.Validate();
				ValidationMessages.AddRange(validator.ValidationMessages);
			}

			return isValid;
		}

		private ServiceValidator GenerateServiceValidator(DisplayedService displayedService)
		{
			var virtualPlatformsToNeverBlockResourcesFor = new[] { ServiceDefinition.VirtualPlatformType.Routing, ServiceDefinition.VirtualPlatformType.AudioProcessing, ServiceDefinition.VirtualPlatformType.VideoProcessing, ServiceDefinition.VirtualPlatformType.GraphicsProcessing };

			var serviceValidationOptions = ServiceValidator.Options.None;
			if (order.StartNow) serviceValidationOptions |= ServiceValidator.Options.StartNow;
			if (order.ShouldBeRunning) serviceValidationOptions |= ServiceValidator.Options.SkipStartTimeValidation;
			if (!userInfo.IsMcrUser && !virtualPlatformsToNeverBlockResourcesFor.Contains(displayedService.Definition.VirtualPlatformServiceType)) serviceValidationOptions |= ServiceValidator.Options.BlockResourceOverbooked;
			if (SavedOrderIsBeingBooked) serviceValidationOptions |= ServiceValidator.Options.SavedServiceIsBeingBooked;
			if ((helpers.Context is UpdateServiceContext updateServiceContext && updateServiceContext.IsResourceChangeAction) || (helpers.Context is UpdateServiceContext && !displayedService.IsDisplayed)) serviceValidationOptions |= ServiceValidator.Options.SkipEncryptionKeyValidation;

			return new ServiceValidator(helpers, displayedService, userInfo, serviceValidationOptions);
		}

		[ValidationStep]
		private bool ValidateSportsPlanning()
		{
			// Remove the validity check from these fields: Requested Broadcast Time & Competition Time[v2.0] Task[DCP185807]
			// Logic will still be needed in the future.

			/*
            bool isCompetitionTimeValid = CompetitionTime > DateTime.Now;
            bool isRequestedBroadcastTimeValid = RequestedBroadcastTime > DateTime.Now;

            order.SportsPlanning.SetPropertyValidation(nameof(order.SportsPlanning.CompetitionTime), isCompetitionTimeValid, "Competition time should not be in the past");

            order.SportsPlanning.SetPropertyValidation(nameof(order.SportsPlanning.RequestedBroadcastTime), isRequestedBroadcastTimeValid, "Requested broadcast time should not be in the past");

            if (!isCompetitionTimeValid || !isRequestedBroadcastTimeValid) ValidationMessages.Add($"Unable to proceed as the sports planning timing configuration is not valid");

            return isCompetitionTimeValid && isRequestedBroadcastTimeValid;
			*/

			return true;
		}

		[ValidationStep]
		private bool ValidateMultilineTextBoxes()
		{
			bool isAdditionalInfoValid = order.Comments == null || order.Comments.Length <= Constants.MaximumAllowedCharacters;

			order.SetPropertyValidation(nameof(order.Comments), isAdditionalInfoValid, $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters");

			bool isMediaOperatorNotesValid = order.MediaOperatorNotes == null || order.MediaOperatorNotes.Length <= Constants.MaximumAllowedCharacters;

			order.SetPropertyValidation(nameof(order.MediaOperatorNotes), isMediaOperatorNotesValid, $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters");

			bool isMcrOperatorNotesValid = order.McrOperatorNotes == null || order.McrOperatorNotes.Length <= Constants.MaximumAllowedCharacters;

			order.SetPropertyValidation(nameof(order.McrOperatorNotes), isMcrOperatorNotesValid, $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters");

			bool isReasonOfCancelationOrRejectionValid = order.ReasonForCancellationOrRejection == null || order.ReasonForCancellationOrRejection.Length <= Constants.MaximumAllowedCharacters;

			order.SetPropertyValidation(nameof(order.McrOperatorNotes), isReasonOfCancelationOrRejectionValid, $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters");

			bool isErrorDescriptionInfoValid = order.ErrorDescription == null || order.ErrorDescription.Length <= Constants.MaximumAllowedCharacters;

			order.SetPropertyValidation(nameof(order.ErrorDescription), isErrorDescriptionInfoValid, $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters");

			bool areOperatorNotesValid = isMcrOperatorNotesValid && isMediaOperatorNotesValid;
			return isAdditionalInfoValid && areOperatorNotesValid && isReasonOfCancelationOrRejectionValid && isErrorDescriptionInfoValid;
		}
	}
}
