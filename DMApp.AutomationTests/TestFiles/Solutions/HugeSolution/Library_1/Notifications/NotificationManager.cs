namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Mail;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Type = IngestExport.Type;
	using UserGroup = Contracts.UserGroup;

	/// <summary>
	/// Used to notify users of changes to an order.
	/// </summary>
	public static class NotificationManager
	{
		private static readonly User kaytontukiHKI = new User { Email = "kaytontuki.hki@yle.fi" };
		private static readonly User kaytontukiTRE = new User { Email = "kaytontuki.tre@yle.fi" };
		private static readonly User kaytontukiVSA = new User { Email = "kaytontuki.vsa@yle.fi" };

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was manually confirmed by an MCR user.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Order that was confirmed.</param>
		public static void SendLiveOrderConfirmationMail(Helpers helpers, Order order)
		{
			var contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			var usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderConfirmationByOperatorNotificationRequired)).ToList();

			SendAllEmails(helpers, usersThatRequireNotification, $"Confirmation of Order {order.Name}", $"An MCR Operator has confirmed Order {order.Name}");
		}

		public static void SendUnableToBookVizremOrderMail(Helpers helpers, Order vizremOrder, List<FunctionResource> unavailableResources)
		{
			var contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			var usersThatRequireNotification = contractDetails.AllUsers.Where(user => user.Name == vizremOrder.CreatedByUserName || user.Notifications.HasFlag(Notifications.LiveOrderRejectionByOperatorNotificationRequired)).ToList();

			helpers.Log(nameof(NotificationManager), nameof(SendUnableToBookVizremOrderMail), $"Users that require notification: {string.Join(",", usersThatRequireNotification.Select(u => u.Name))}", vizremOrder.Name);

			SendAllEmails(helpers, usersThatRequireNotification, $"Unable to book recurring Vizrem order {vizremOrder.Name}", $"The system was unable to create recurring Vizrem order {vizremOrder.Name} from {vizremOrder.Start} until {vizremOrder.End} because following resource are not available:<br>{string.Join("<br>", unavailableResources.Select(r => r.Name))}");
		}

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was manually rejected by an MCR user.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="rejectedOrder">Order that was rejected.</param>
		/// <param name="reasonForOrderRejection"></param>
		public static void SendLiveOrderRejectionMail(Helpers helpers, Order rejectedOrder, string reasonForOrderRejection = null)
		{
			var contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			var usersThatRequireNotification = GetPossibleRecipients(rejectedOrder, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderRejectionByOperatorNotificationRequired)).ToList();

			helpers.Log(nameof(NotificationManager), nameof(SendLiveOrderRejectionMail), $"Users that require notification: {string.Join(",", usersThatRequireNotification.Select(u => u.Name))}", rejectedOrder.Name);

			SendAllEmails(helpers, usersThatRequireNotification, $"Rejection of Order {rejectedOrder.Name}", $"An MCR Operator has rejected Order {rejectedOrder.Name}<br>Reason: {reasonForOrderRejection}");
		}

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was cancelled.
		/// This methods checks if the user triggering the notification is MCR, customer or DM.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Order that was cancelled.</param>
		/// <param name="reasonForOrderCancellation"></param>
		public static void SendLiveOrderCancellationMail(Helpers helpers, Order order, string reasonForOrderCancellation = null)
		{
			// Only send mails when the Order was cancelled from a non-saved state.
			if (order.IsSaved) return;

			var contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			string username = helpers.Engine.UserLoginName;

			List<User> usersThatRequireNotification;
			string message;
			var user = contractDetails.AllUsers.FirstOrDefault(x => x.Name.Equals(username));
			if (user == null)
			{
				// DataMiner - Integration
				// Only send mail when the order was cancelled 24h before start
				if (DateTime.Now > order.Start.ToLocalTime() || order.Start.ToLocalTime().Subtract(DateTime.Now).TotalHours > 24) return;

				message = $"Order {order.Name} was automatically cancelled by DataMiner";
				usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderCancellationByIntegrationNotificationRequired)).ToList();
			}
			else
			{
				if (IsMcrUser(user, contractDetails.AllUserGroups))
				{
					// MCR Operator
					message = $"An MCR Operator has cancelled Order {order.Name}.<br>Reason: {reasonForOrderCancellation}";
					usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderCancellationByOperatorNotificationRequired)).Distinct().ToList();
				}
				else
				{
					// Customer
					message = $"A customer has cancelled Order {order.Name}.<br>Reason: {reasonForOrderCancellation}";
					usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderCancellationByCustomerNotificationRequired)).Distinct().ToList();
				}
			}

			helpers.Log(nameof(NotificationManager), nameof(SendLiveOrderCancellationMail), $"Users that require notification: {string.Join(",", usersThatRequireNotification.Select(u => u.Name))}", order.Name);

			SendAllEmails(helpers, usersThatRequireNotification, $"Cancellation of Order {order.Name}", message);
		}

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was updated and all services are booked.
		/// </summary>
		public static void SendLiveOrderServicesBookedMail(Helpers helpers, Order order, bool servicesSuccessfullyBooked, string message)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			var usersWhoCreatedOrModifiedOrder = new List<User>();
			if (order != null)
			{
				usersWhoCreatedOrModifiedOrder = GetPossibleRecipients(order, contractDetails, false, false).ToList();
			}

			List<User> usersThatRequireNotification;
			if (!usersWhoCreatedOrModifiedOrder.Any())
			{
				// when there are no users who created/modified the order then it's an integration order that was not yet updated
				if (servicesSuccessfullyBooked || (order != null && order.IntegrationType == IntegrationType.None)) return;

				// only when the services could not be booked for an integration order should a notification be sent
				usersThatRequireNotification = contractDetails.AllUsers.Where(u => u.Notifications.HasFlag(Notifications.LiveOrderServicesBookedByIntegrationNotificationRequired)).Distinct().ToList();
			}
			else
			{
				usersThatRequireNotification = usersWhoCreatedOrModifiedOrder.Where(x => x.Notifications.HasFlag(Notifications.LiveOrderServicesBookedNotificationRequired)).Distinct().ToList();
			}

			if (!usersThatRequireNotification.Any()) return;

			SendAllEmails(helpers, usersThatRequireNotification, $"Service booking summary for Order {order?.Name}", message);
		}

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was completed by DataMiner.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Order that was completed.</param>
		public static void SendLiveOrderCompletionMail(Helpers helpers, Order order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<User> usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderCompletionByDataminerNotificationRequired)).Distinct().ToList();

			SendAllEmails(helpers, usersThatRequireNotification, $"Completion of Order {order.Name}", $"Order {order.Name} was completed");
		}

		/// <summary>
		/// Used to update every user that created or updated the Order that the Order was completed with errors by DataMiner.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that was completed with errors.</param>
		public static void SendLiveOrderCompletionWithErrorsMail(Helpers helpers, Order order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<User> usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false).Where(x => x.Notifications.HasFlag(Notifications.LiveOrderCompletionWithErrorsByDataminerNotificationRequired)).Distinct().ToList();

			SendAllEmails(helpers, usersThatRequireNotification, $"Completion with errors of Order {order.Name}", $"Order {order.Name} was completed with errors");
		}

		/// <summary>
		/// Used to update every user that created or updated the Non Live Order that the Order was reassigned by an MCR Operator.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that was reassigned.</param>
		public static void SendNonLiveOrderReassignmentMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<User> usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderReassignedByOperatorNotificationRequired)).Distinct().ToList();

			SendAllEmails(helpers, usersThatRequireNotification, $"Non live Order {order.OrderDescription} was reassigned", $"Non live Order {order.OrderDescription} was reassigned");
		}

		/// <summary>
		/// Used to update every user that created or updated the Non Live Order that the Order was rejected by an MCR Operator.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that was rejected.</param>
		public static void SendNonLiveOrderRejectionMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<string> emailsThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderRejectedByOperatorNotificationRequired)).Select(s => s.Email).Distinct().ToList();
			emailsThatRequireNotification.AddRange(order.EmailReceivers);

			helpers.Log(nameof(NotificationManager), nameof(SendNonLiveOrderRejectionMail), $"Non Live Order: {Convert.ToString(order.TicketId)}|Users that require notification: {string.Join(", ", emailsThatRequireNotification)}");

			foreach (string email in emailsThatRequireNotification)
			{
				EmailOptions emailOptions = new EmailOptions { TO = email, Title = $"Non live Order {order.OrderDescription} was rejected", Message = order.ReasonOfRejection };

				TrySendMail(helpers, emailOptions);
			}
		}

		/// <summary>
		/// Used to update every user that created or updated the Non Live Order that the Order was completed by DataMiner.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that was completed.</param>
		public static void SendNonLiveOrderCompletionMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<string> emailsThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderCompletedByDataMinerNotificationRequired)).Select(s => s.Email).Distinct().ToList();
			emailsThatRequireNotification.AddRange(order.EmailReceivers);

			string title = string.Empty;
			string message = string.Empty;

			switch (order.OrderType)
			{
				case Type.AsperaOrder:
					var asperaOrder = (Aspera)order;
					string asperaType = (asperaOrder.AsperaType.Equals(AsperaType.Shares.GetDescription())) ? AsperaType.Shares.GetDescription() : AsperaType.Faspex.GetDescription();
					title = $"{asperaType} order {order.OrderDescription} was completed";
					message = $"{asperaType} order {order.OrderDescription} was completed. <br>" +
						$"<br>Original order in Dataminer: {GenerateLink(asperaOrder.UID)}";
					break;
				case Type.Export:
					var exportOrder = (Export)order;
					title = $"{order.OrderType.GetDescription()} order from {exportOrder.MaterialSource.GetDescription()} to {exportOrder.ExportInformation.TargetOfExport} {order.OrderDescription} was completed";
					message = $"{order.OrderType.GetDescription()} order from {exportOrder.MaterialSource.GetDescription()} to {exportOrder.ExportInformation.TargetOfExport} {order.OrderDescription} was completed.<br>" +
						$"<br>Original order in Dataminer: {GenerateLink(exportOrder.UID)}";
					break;
				case Type.IplayWgTransfer:
					var wgOrder = (Transfer)order;
					title = $"{order.OrderType.GetDescription()} from {wgOrder.Source} to {wgOrder.Destination} {order.OrderDescription} was completed";
					message = $"{order.OrderType.GetDescription()} from {wgOrder.Source} to {wgOrder.Destination} {order.OrderDescription} was completed.<br>" +
						$"<br>Material can be found at {wgOrder.Destination} in {wgOrder.InterplayDestinationFolder} " +
						$"<br>Original order in Dataminer: {GenerateLink(wgOrder.UID)}";
					break;
				case Type.NonInterplayProject:
					var nonInterplayOrder = (Project)order;
					title = $"{order.OrderType.GetDescription()} order {order.OrderDescription} was completed";
					var nonInterplayDate = nonInterplayOrder.IsLongerStoredBackUpChecked ? nonInterplayOrder.BackupDeletionDate : DateTime.Now.AddYears(1);
					message = $"A backup of this material has been created to Isilon network drive. It will be stored in the backup until {nonInterplayDate}. If the backup delete date needs to be changed contact {nonInterplayOrder.ImportDepartment} Ingest department.<br> Original order in Dataminer: {GenerateLink(nonInterplayOrder.UID)}";
					break;
				case Type.Import:
					var importOrder = (Ingest)order;
					var importDate = (importOrder.BackUpsLongerStored == true) ? importOrder.BackupDeletionDate : DateTime.Now.AddYears(1);
					title = $"{importOrder.IngestDestination.Destination}: {order.OrderType.GetDescription()} order {order.OrderDescription} was completed";
					message = $"{importOrder.IngestDestination.Destination}: {order.OrderType.GetDescription()} order {order.OrderDescription} was completed.<br>" +
						$"<br>A backup of this material has been created to Isilon network drive and it will be stored there until {importDate}. If the backup delete date needs to be changed contact {importOrder.IngestDestination.Destination} Ingest department.<br>" +
						$"<br>Original order in Dataminer: {GenerateLink(importOrder.UID)}";
					break;
				case Type.IplayFolderCreation:
					var folderCreationOrder = (FolderCreation)order;
					title = $"{folderCreationOrder.Destination}: {order.OrderType.GetDescription()} order {order.OrderDescription} was completed";
					message = $"{folderCreationOrder.Destination}: {order.OrderType.GetDescription()} order {order.OrderDescription} was completed.<br>";

					if (folderCreationOrder.ContentType == NewFolderContentTypes.PROGRAM.GetDescription())
					{
						message += $"<br>The delete date for {folderCreationOrder.NewProgramFolderRequestDetails.ProgramName} folder in {folderCreationOrder.Destination} is {folderCreationOrder.NewProgramFolderRequestDetails.DeleteDate}.<br>";
					}

					foreach (var episode in folderCreationOrder.NewEpisodeFolderRequestDetails)
					{
						message += $"<br>The delete date for Episode {episode.ProductOrProductionName} {episode.EpisodeNumberOrName} is {episode.DeleteDate}<br>";
					}

					message += $"<br> Original order in Dataminer: {GenerateLink(folderCreationOrder.UID)}<br> " +
						$"<br> If any changes must be done considering deletion, please contact {folderCreationOrder.Destination} -system's user support (Käytöntuki).";
					break;
				default:
					title = $"Non live Order {order.OrderDescription} was completed";
					message = $"Non live Order {order.OrderDescription} was completed";
					break;
			}

			foreach (string email in emailsThatRequireNotification)
			{
				EmailOptions emailOptions = new EmailOptions { TO = email, Title = title, Message = message };

				TrySendMail(helpers, emailOptions);
			}
		}

		/// <summary>
		/// Used to update every user that created or updated the Non Live Order that is set to Work In Progress by Dataminer.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that is set to work in progress.</param>
		public static void SendNonLiveOrderWorkInProgressMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;
			var emailsThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderWorkInProgressByDataMinerNotificationRequired)).Select(x => x.Email).Distinct().ToList();

			foreach (var email in order.EmailReceivers)
			{
				emailsThatRequireNotification.Add(email);

			}

			foreach (string email in emailsThatRequireNotification)
			{
				EmailOptions emailOptions = new EmailOptions { TO = email, Title = $"Non live Order {order.OrderDescription} is set to work in progress", Message = $"Non live Order {order.OrderDescription} is set to work in progress" };

				TrySendMail(helpers, emailOptions);
			}
		}

		/// <summary>
		/// Used to update every user that created or updated the Non Live Order that the Order was manually cancelled.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that was cancelled.</param>
		public static void SendNonLiveOrderCancellationMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			var emailsThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderCancellationByOperatorNotificationRequired)).Select(x => x.Email).Distinct().ToList();

			foreach (var email in order.EmailReceivers)
			{
				emailsThatRequireNotification.Add(email);
			}

			foreach (string email in emailsThatRequireNotification)
			{
				EmailOptions emailOptions = new EmailOptions { TO = email, Title = $"Non live Order {order.OrderDescription} was canceled", Message = $"Non live Order {order.OrderDescription} was cancelled" };

				TrySendMail(helpers, emailOptions);
			}
		}

		/// <summary>
		/// Used to update every user the Non Live Order was created by the operator.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that is just created.</param>
		public static void SendNonLiveOrderCreationMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<User> usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderCreationByDataMinerNotificationRequired)).Distinct().ToList();

			var title = string.Empty;
			var message = string.Empty;

			switch (order.OrderType)
			{
				case Type.AsperaOrder:
					var asperaOrder = (Aspera)order;
					title = $"{asperaOrder.AsperaType} order {order.OrderDescription} was created";
					message = $"{asperaOrder.AsperaType} order {order.OrderDescription} was created. <br>";

					if (asperaOrder.AsperaType.Equals(AsperaType.Faspex.GetDescription()))
						message += $"<br>{asperaOrder.AsperaType} is assigned to {asperaOrder.Workgroup}. <br>";

					if (asperaOrder.AsperaType.Equals(AsperaType.Shares.GetDescription()))
						message += $"<br>{asperaOrder.AsperaType} is assigned to {asperaOrder.ImportDepartment}. <br>";

					message += $"<br>Original order in Dataminer: {GenerateLink(asperaOrder.UID)}";

					usersThatRequireNotification.Add(kaytontukiHKI);
					usersThatRequireNotification.Add(kaytontukiTRE);
					usersThatRequireNotification.Add(kaytontukiVSA);

					break;
				case Type.IplayFolderCreation:
					var folderCreationOrder = (FolderCreation)order;
					title = $"{folderCreationOrder.Destination}: New Iplay folder order {order.OrderDescription} was created";
					message = $"Iplay folder order {order.OrderDescription} was created. <br>" +
						$"<br> Open order in Dataminer: {GenerateLink(folderCreationOrder.UID)}";

					usersThatRequireNotification.Add(kaytontukiHKI);
					usersThatRequireNotification.Add(kaytontukiTRE);
					usersThatRequireNotification.Add(kaytontukiVSA);

					break;
				default:
					title = $"Non live Order {order.OrderDescription} is created";
					message = $"Non live Order {order.OrderDescription} is created";
					break;
			}

			SendAllEmails(helpers, usersThatRequireNotification, title, message);
		}

		public static string GenerateLink(Guid orderId)
		{
			string link = string.Empty;

			if (Agents.AgentIDs.TryGetValue(Engine.SLNetRaw.ServerDetails.AgentID, out string value))
			{
				link = "https://" + value + "/yle/ticket/" + Convert.ToString(orderId);
			}

			return link;
		}

		/// <summary>
		/// Used to update every user of the operating support group when a new Iplay folder Non Live Order is created.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order"> New Iplay folder that is created.</param>
		public static void SendNonLiveOrderIplayFolderCreationMail(Helpers helpers, NonLiveOrder order)
		{
			var contractDetails = helpers.ContractManager.RequestUserContractDetails(helpers.Engine.UserLoginName) ?? throw new InvalidContractResponseException();

			var usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false, true).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderIplayFolderCreationByOperatorNotificationRequired)).Distinct().ToList();

			usersThatRequireNotification.Add(kaytontukiHKI);
			usersThatRequireNotification.Add(kaytontukiTRE);
			usersThatRequireNotification.Add(kaytontukiVSA);

			string title = $"Iplay folder order {order.OrderDescription} was created";

			string message = $"Iplay folder order {order.OrderDescription} was created.<br><br>Open order in Dataminer: {GenerateLink(order.UID)}";

			SendAllEmails(helpers, usersThatRequireNotification, title, message);
		}

		/// <summary>
		/// Used to update every user of the operating support group when a new Iplay folder Non Live Order is created.
		/// </summary>
		/// <param name="helpers">Link with DataMiner.</param>
		/// <param name="folderCreationUserTask"> New Iplay folder that is created.</param>
		public static void SendNonLiveUsertaskIplayFolderDeletionMail(Helpers helpers, IplayFolderCreationUserTask folderCreationUserTask)
		{
			if (folderCreationUserTask is null) throw new ArgumentNullException(nameof(folderCreationUserTask));

			var possibleRecipients = new List<User>();
			possibleRecipients.Add(new User { Email = folderCreationUserTask.MediaManagerEmail });
			possibleRecipients.Add(new User { Name = folderCreationUserTask.OrdererName, Email = folderCreationUserTask.OrdererEmail });
			possibleRecipients.Add(new User { Email = folderCreationUserTask.ProducerEmail });
			possibleRecipients.Add(kaytontukiHKI);
			possibleRecipients.Add(kaytontukiTRE);

			EmailOptions emailOptions;
			if (folderCreationUserTask.Description.Contains(Descriptions.NonLiveFolderCreation.FolderDeletionProgram))
			{

				emailOptions = new EmailOptions
				{
					Title = $"{folderCreationUserTask.Destination} Folder deletion: Folder {folderCreationUserTask.ProgramName} Interplay-deletion date is {folderCreationUserTask.DeleteDate}",
					Message = $@"The delete date of Program folder {folderCreationUserTask.ProgramName} in {folderCreationUserTask.Destination} is {folderCreationUserTask.DeleteDate}.<br>The program folder is located in {folderCreationUserTask.Destination}: {folderCreationUserTask.FolderPath}.<br>This is an automatic message about interplay-folder's deletion date from Dataminer-system.<br>If any changes must be done considering deletion, please contact {folderCreationUserTask.Destination} system's user support (Käytöntuki)."
				};
			}
			else
			{
				emailOptions = new EmailOptions
				{
					Title = $"{folderCreationUserTask.Destination} Folder deletion: Episode {folderCreationUserTask.ProductOrProductionNumber}, {folderCreationUserTask.EpisodeNumberOrName} Interplay-deletion date is {folderCreationUserTask.DeleteDate}",
					Message = $@"The delete date of Episode folder {folderCreationUserTask.ProductOrProductionNumber},{folderCreationUserTask.EpisodeNumberOrName} in {folderCreationUserTask.Destination} is {folderCreationUserTask.DeleteDate}.<br>The Episode folder is located in {folderCreationUserTask.Destination}: {folderCreationUserTask.FolderPath}.<br>This is an automatic message about interplay-folder's deletion date from Dataminer-system.<br>If any changes must be done considering deletion, please contact {folderCreationUserTask.Destination} system's user support (Käytöntuki)."
				};
			}

			SendAllEmails(helpers, possibleRecipients, emailOptions.Title, emailOptions.Message);
		}

		public static void SendNonLiveImportUserTaskIsilonBackupDeletionMail(Helpers helpers, ImportUserTask importUserTask)
		{
			if (importUserTask is null) throw new ArgumentNullException(nameof(importUserTask));

			if (String.IsNullOrEmpty(importUserTask.OrdererEmail))
			{
				helpers.Log(nameof(NotificationManager), nameof(SendNonLiveImportUserTaskIsilonBackupDeletionMail), $"Unable to send mail cause orderer email address is empty.");
				return;
			}

			var emailOptions = new EmailOptions
			{
				TO = importUserTask.OrdererEmail,
				Title = $"Isilon backup deletion: Material {importUserTask.OrderName} Isilon-backup deletion date is {importUserTask.DeleteDate.Date}",
				Message = $@"Material {importUserTask.OrderName} Isilon-backup deletion date is {importUserTask.DeleteDate.Date}.<br>Material has been used in system {importUserTask.ImportDestination}.<br>This is an automatic message about Isilon-backup deletion date from Dataminer-system.<br>If any changes must be done considering deletion, please contact {importUserTask.ImportDestination} - system's import department."
			};

			TrySendMail(helpers, emailOptions);
		}

		public static void SendNonLiveProjectUserTaskIsilonBackupDeletionMail(Helpers helpers, NonIplayProjectUserTask projectUserTask)
		{
			if (projectUserTask is null) throw new ArgumentNullException(nameof(projectUserTask));

			if (String.IsNullOrEmpty(projectUserTask.OrdererEmail))
			{
				helpers.Log(nameof(NotificationManager), nameof(SendNonLiveProjectUserTaskIsilonBackupDeletionMail), $"Unable to send mail cause orderer email address is empty.");
				return;
			}

			var emailOptions = new EmailOptions
			{
				TO = projectUserTask.OrdererEmail,
				Title = $"Isilon backup deletion {projectUserTask.ProjectName} Isilon-backup deletion date is {projectUserTask.DeleteDate.Date}",
				Message = $@"Material {projectUserTask.ProjectName} {projectUserTask.ProductionNumber} Isilon-backup deletion date is{projectUserTask.DeleteDate.Date}.<br>This is an automatic message about Isilon-backup deletion date from Dataminer-system.<br>If any changes must be done considering deletion, please contact import department: {projectUserTask.ImportDepartment}."
			};

			TrySendMail(helpers, emailOptions);
		}

		/// <summary>
		/// Used to update every user if the Non Live Order was edited by the operator.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Non Live Order that is edited.</param>
		public static void SendNonLiveOrderEditedMail(Helpers helpers, NonLiveOrder order)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			List<User> usersThatRequireNotification = GetPossibleRecipients(order, contractDetails, false, false).Where(x => x.Notifications.HasFlag(Notifications.NonLiveOrderEditedByDataMinerNotificationRequired)).Distinct().ToList();
			foreach (User user in usersThatRequireNotification)
			{
				EmailOptions emailOptions = new EmailOptions { TO = user.Email, Title = $"Non live Order {order.OrderDescription} was edited", Message = $"Non live Order {order.OrderDescription} was edited" };

				TrySendMail(helpers, emailOptions);
			}
		}

		public static void SendMailToMcrUsers(Helpers helpers, string title, string message)
		{
			ExternalResponse contractDetails = GetContractDetails(helpers);
			if (contractDetails == null) return;

			foreach (User mcrUser in contractDetails.AllMcrUsers)
			{
				EmailOptions emailOptions = new EmailOptions { TO = mcrUser.Email, Title = title, Message = message };

				TrySendMail(helpers, emailOptions);
			}
		}

		public static void SendMailTo(Helpers helpers, string title, string message, string receiverEmail)
		{
			SendMailWithoutAttachment(helpers, receiverEmail, title, message);
		}

		public static void SendMailToSkylineDevelopers(Helpers helpers, string title, string message, string pathToAttachmentFile = null)
		{
			string receiver = "squad.deploy-the.pioneers@skyline.be";
			string fullTitle = "YLE DEBUG - " + title;

			bool attachmentRequired = pathToAttachmentFile != null;
			if (attachmentRequired)
			{
				SendMailWithAttachment(helpers, receiver, fullTitle, message, pathToAttachmentFile);
			}
			else
			{
				SendMailWithoutAttachment(helpers, receiver, fullTitle, message);
			}
		}

		public static void SendMailToTeemuJussiJariJuho(Helpers helpers, string title, string message)
		{
			var emails = new List<string>
		{
			"ext-teemu.laine@yle.fi",
			"ext-jari.suni@yle.fi",
			"jussi.huhtala@yle.fi",
			"juho.pentinmikko@yle.fi"
		};

			foreach (var email in emails)
			{
				SendMailWithoutAttachment(helpers, email, title, message);
			}
		}

		private static void SendMailWithoutAttachment(Helpers helpers, string to, string title, string message)
		{
			var emailOptions = new EmailOptions { TO = to, Title = title, Message = message };

			TrySendMail(helpers, emailOptions);
		}

		private static void SendMailWithAttachment(Helpers helpers, string receiver, string title, string message, string pathToAttachmentFile)
		{
			var mail = new MailMessage("noreply-yledebugging@skyline.be", receiver);
			var client = new SmtpClient();
			client.Port = 25;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Host = "ylehkimr.yle.fi";
			mail.Subject = title;
			mail.Body = message;

			try
			{
				mail.Attachments.Add(new Attachment(pathToAttachmentFile, System.Net.Mime.MediaTypeNames.Application.Octet));
				client.Send(mail);
			}
			catch (Exception e)
			{
				string newMessage = message.Replace(Environment.NewLine, "<br>");
				newMessage += "<br> Automation logging can be found at " + pathToAttachmentFile;
				newMessage += "<br><br> Unable to send file as attachment because of exception: " + e;

				SendMailWithoutAttachment(helpers, receiver, title, newMessage);
			}
		}

		/// <summary>
		/// Tries to retrieve the lists with all users and all user groups from the Contract Manager element.
		/// </summary>
		/// <returns>External response as received from the Contract Manager element. Or null if something went wrong.</returns>
		private static ExternalResponse GetContractDetails(Helpers helpers)
		{
			try
			{
				return helpers.ContractManager.RequestUserContractDetails(helpers.Engine.UserLoginName);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NotificationManager), nameof(GetContractDetails), $"Exception while requesting contract details for user {helpers.Engine.UserLoginName}: {e}");
				return null;
			}
		}

		/// <summary>
		/// Gets the list of recipients for a specific Live Order.
		/// </summary>
		/// <param name="order">Order for which the related users are retrieved.</param>
		/// <param name="contractDetails">Contains all users and user groups.</param>
		/// <param name="includeAllUsersFromOrderCreatorUserGroup">Determines whether user part of the user group(s) of the user that created the order need to be included.</param>
		/// <param name="removeMcrUsersFromPossibleRecipients"></param>
		/// <param name="includeAllUsers"></param>
		/// <returns>List of users that can be updated by a change of the Order.</returns>
		private static IEnumerable<User> GetPossibleRecipients(Order order, ExternalResponse contractDetails, bool includeAllUsersFromOrderCreatorUserGroup, bool removeMcrUsersFromPossibleRecipients = true, bool includeAllUsers = false)
		{
			var modifiedByUsers = new List<string>();
			return GetPossibleRecipients(order.CreatedByUserName, modifiedByUsers, contractDetails, includeAllUsersFromOrderCreatorUserGroup, removeMcrUsersFromPossibleRecipients, includeAllUsers);
		}

		/// <summary>
		/// Gets the list of recipients for a specific non Live Order.
		/// </summary>
		/// <param name="order">Order for which the related users are retrieved.</param>
		/// <param name="contractDetails">Contains all users and user groups.</param>
		/// <param name="includeAllUsersFromOrderCreatorUserGroup">Determines whether user part of the user group(s) of the user that created the order need to be included.</param>
		/// <param name="removeMcrUsersFromRecipients"></param>
		/// <param name="includeAllUsers"></param>
		/// <returns>List of users that can be updated by a change of the Order.</returns>
		private static IEnumerable<User> GetPossibleRecipients(NonLiveOrder order, ExternalResponse contractDetails, bool includeAllUsersFromOrderCreatorUserGroup, bool removeMcrUsersFromRecipients, bool includeAllUsers = false)
		{
			return GetPossibleRecipients(order.CreatedBy, order.ModifiedBy, contractDetails, includeAllUsersFromOrderCreatorUserGroup, removeMcrUsersFromRecipients, includeAllUsers);
		}

		/// <summary>
		/// Used to retrieve list of possible users that should be notified of the order change.
		/// MCR users and users without an email address are filtered out.
		/// </summary>
		/// <param name="createdBy">Name of the user that created the order.</param>
		/// <param name="modifiedBy">Names of the users that modified the order.</param>
		/// <param name="contractDetails">Contains the list of all users and users groups in the DM system.</param>
		/// <param name="includeAllUsersFromOrderCreatorUserGroup">Indicates if the users in the user group of the user that created the order should be included.</param>
		/// <param name="removeMcrUsersFromPossibleRecipients"></param>
		/// <param name="includeAllUsers"></param>
		/// <returns>List of eligible users that should be notified of the order change.</returns>
		private static IEnumerable<User> GetPossibleRecipients(string createdBy, IEnumerable<string> modifiedBy, ExternalResponse contractDetails, bool includeAllUsersFromOrderCreatorUserGroup, bool removeMcrUsersFromPossibleRecipients = true, bool includeAllUsers = false)
		{
			var possibleRecipients = new HashSet<User>();

			// Get user that created order
			var createdByUser = contractDetails.AllUsers.FirstOrDefault(x => x.Name == createdBy); // Will be null if the Order was created by DataMiner
			if (createdByUser != null && !String.IsNullOrEmpty(createdByUser.Email)) possibleRecipients.Add(createdByUser);

			// Get users that modified order
			possibleRecipients.UnionWith(modifiedBy.Select(username => contractDetails.AllUsers.FirstOrDefault(x => x.Name == username)).Where(user => !string.IsNullOrEmpty(user?.Email)).ToList());

			// Get users that are part of the User Group(s) of the user that created the order
			if (createdByUser != null && includeAllUsersFromOrderCreatorUserGroup)
			{
				possibleRecipients.UnionWith(contractDetails.AllUsers.Where(user => user.UsergroupIds.Intersect(createdByUser.UsergroupIds).Any() && !string.IsNullOrEmpty(user.Email)).ToList());
			}

			if (includeAllUsers)
			{
				possibleRecipients.UnionWith(contractDetails.AllUsers.Where(user => !string.IsNullOrEmpty(user.Email)).ToList());
			}

			// Filter out MCR users
			if (removeMcrUsersFromPossibleRecipients) possibleRecipients.RemoveWhere(r => IsMcrUser(r, contractDetails.AllUserGroups));

			return possibleRecipients.ToList();
		}

		/// <summary>
		/// Checks if the specified user is part of an MCR user group.
		/// </summary>
		/// <param name="user">User to be checked.</param>
		/// <param name="userGroups">List of all user groups.</param>
		/// <returns>True if user is part of an MCR user group, else false.</returns>
		private static bool IsMcrUser(User user, UserGroup[] userGroups)
		{
			if (user == null) throw new ArgumentNullException("user");
			if (userGroups == null) throw new ArgumentNullException("userGroups");

			foreach (UserGroup userGroup in userGroups)
			{
				if (user.UsergroupIds.Contains(userGroup.ID) && userGroup.IsMcr)
				{
					return true;
				}
			}

			return false;
		}

		private static bool TrySendMail(Helpers helpers, EmailOptions emailOptions)
		{
			try
			{
				helpers.Engine.SendEmail(emailOptions);

				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NotificationManager), nameof(TrySendMail), $"Unable to send mail to {emailOptions.TO}: {e}");

				return false;
			}
		}

		private static void SendAllEmails(Helpers helpers, List<User> possibleRecipients, string title, string message)
		{
			var recipientsEmails = possibleRecipients.Select(x => x.Email).Where(x => !String.IsNullOrEmpty(x)).Distinct().ToList();

			foreach (var email in recipientsEmails)
			{
				EmailOptions emailOptions = new EmailOptions
				{
					TO = email,
					Title = title,
					Message = message,
				};

				TrySendMail(helpers, emailOptions);
			}
		}
	}
}