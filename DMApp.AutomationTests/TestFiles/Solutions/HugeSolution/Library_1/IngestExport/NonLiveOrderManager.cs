namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
	using NPOI.SS.Formula.Functions;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Net.Ticketing;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NonLiveOrderManager
    {
        private readonly Helpers helpers;
        private const string TicketDomainName = "Ingest/Export";

        private readonly TicketingManager ticketingManager;

        public NonLiveOrderManager(Helpers helpers)
        {
            this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
            ticketingManager = new TicketingManager(helpers, TicketDomainName);
        }

		public Guid TicketDomainId => ticketingManager.TicketFieldResolver.ID;

        /// <summary>
        /// Tries to get the Non-Live Order for the given IDs.
        /// </summary>
        public bool TryGetNonLiveOrder(int dataminerId, int ticketId, out NonLiveOrder nonLiveOrder)
        {
            try
            {
                nonLiveOrder = GetNonLiveOrder(dataminerId, ticketId);
                return true;
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveOrderManager), nameof(TryGetNonLiveOrder), "An exception occurred: " + e);
                nonLiveOrder = null;
                return false;
            }
        }

        /// <summary>
		/// Tries to get the Non-Live Order for the given IDs.
		/// </summary>
		public bool TryGetNonLiveOrder(Guid uniqueId, out NonLiveOrder nonLiveOrder)
        {
            try
            {
                nonLiveOrder = GetNonLiveOrder(uniqueId);
                return true;
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveOrderManager), nameof(TryGetNonLiveOrder), "An exception occurred: " + e);
                nonLiveOrder = null;
                return false;
            }
        }

		/// <summary>
		/// Tries to get the Non-Live Order for the given Ticket.
		/// </summary>
		public bool TryGetNonLiveOrder(Ticket ticket, out NonLiveOrder nonLiveOrder)
		{
			try
			{
				nonLiveOrder = GetNonLiveOrder(ticket);
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(NonLiveOrderManager), nameof(TryGetNonLiveOrder), "An exception occurred: " + e);
				nonLiveOrder = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to get the Non-Live Orders which are having a completed state.
		/// </summary>
		public List<NonLiveOrder> GetAllCompletedNonLiveOrders()
		{
			List<NonLiveOrder> nonLiveOrders = new List<NonLiveOrder> ();

			try
            {
                var allCompletedTickets = ticketingManager.GetTicketsBasedOnCustomState(State.Completed).ToList();

                foreach (var completedTicket in allCompletedTickets)
                {
					if(TryGetNonLiveOrder(completedTicket, out var nonLiveOrder))
						nonLiveOrders.Add(nonLiveOrder);
                }
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveOrderManager), nameof(GetAllCompletedNonLiveOrders), "An exception occurred: " + e);
            }

			return nonLiveOrders;
		}

		public NonLiveOrder GetNonLiveOrder(string fullTicketId)
		{
			if (string.IsNullOrWhiteSpace(fullTicketId)) throw new ArgumentNullException(nameof(fullTicketId));

			var splitId = fullTicketId.Split('/');

			if (splitId.Count() != 2) throw new ArgumentException($"Argument value '{fullTicketId}' does not contain two elements when splitting on '/'.", nameof(fullTicketId));

			if (!int.TryParse(splitId[0], out int dataminerId)) throw new ArgumentException($"Argument value '{fullTicketId}' does not have a valid int as first part.", nameof(fullTicketId));

			if (!int.TryParse(splitId[1], out int ticketId)) throw new ArgumentException($"Argument value '{fullTicketId}' does not have a valid int as second part.", nameof(fullTicketId));

			return GetNonLiveOrder(dataminerId, ticketId);
		}

        public NonLiveOrder GetNonLiveOrder(int dataminerId, int ticketId)
        {
            var ticket = ticketingManager.GetTicket(dataminerId, ticketId);
            if (ticket == null) throw new TicketNotFoundException(dataminerId, ticketId);

			return GetNonLiveOrder(ticket);
        }

        public static NonLiveOrder GetNonLiveOrder(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            Type type = (Type)ticket.GetIntegerFieldValue(NonLiveOrder.TypeTicketField);
            switch (type)
            {
                case Type.Export:
                    return ConvertTicketToExport(ticket);
                case Type.Import:
                    return ConvertTicketToIngest(ticket);
                case Type.IplayFolderCreation:
                    return ConvertTicketToFolderCreation(ticket);
                case Type.IplayWgTransfer:
                    return ConvertTicketToTransfer(ticket);
                case Type.NonInterplayProject:
                    return ConvertTicketToProject(ticket);
                case Type.AsperaOrder:
                    return ConvertTicketToAspera(ticket);
                default:
                    throw new UnknownNonLiveOrderTypeException(ticket.ID.DataMinerID, ticket.ID.TID);
            }
        }

        public NonLiveOrder GetNonLiveOrder(Guid uniqueId)
        {
            var ticket = ticketingManager.GetTicket(uniqueId);
            if (ticket == null) throw new TicketNotFoundException(uniqueId);

			return GetNonLiveOrder(ticket);
        }

        public bool AddOrUpdateNonLiveOrder(NonLiveOrder nonLiveOrder, User lastModifier, out string ticketId)
        {
            if (nonLiveOrder is null) throw new ArgumentNullException(nameof(nonLiveOrder));
            if (lastModifier is null) throw new ArgumentNullException(nameof(lastModifier));

            Ticket ticket;
            if (nonLiveOrder.DataMinerId.HasValue && nonLiveOrder.TicketId.HasValue)
            {
                // Existing Ticket
                ticket = ticketingManager.GetTicket(nonLiveOrder.DataMinerId.Value, nonLiveOrder.TicketId.Value);
            }
            else
            {
                // New Ticket
                nonLiveOrder.CreatedBy = lastModifier.Name;
                nonLiveOrder.CreatedByEmail = lastModifier.Email;
                nonLiveOrder.CreatedByPhone = lastModifier.Phone;

				ticket = new Ticket (Guid.NewGuid()) { CustomFieldResolverID = ticketingManager.TicketFieldResolver.ID };
                nonLiveOrder.UID = ticket.UID;
            }

			nonLiveOrder.LastModifiedBy = lastModifier.Name;
			nonLiveOrder.LastModifiedByEmail = lastModifier.Email;
			nonLiveOrder.LastModifiedByPhone = lastModifier.Phone;

			nonLiveOrder.ModifiedBy.Add(lastModifier.Name);
			
			switch (nonLiveOrder.OrderType)
			{
				case Type.Export:
					ConvertExportToTicket((Export.Export)nonLiveOrder, ticket);
					break;
				case Type.Import:
					ConvertIngestToTicket((Ingest.Ingest)nonLiveOrder, ticket);
					break;
				case Type.IplayFolderCreation:
					ConvertFolderCreationToTicket((FolderCreation.FolderCreation)nonLiveOrder, ticket);
					break;
				case Type.IplayWgTransfer:
					ConvertTransferToTicket((Transfer.Transfer)nonLiveOrder, ticket);
					break;
				case Type.NonInterplayProject:
					ConvertProjectToTicket((Project.Project)nonLiveOrder, ticket);
					break;
				case Type.AsperaOrder:
					ConvertAsperaToTicket((Aspera.Aspera)nonLiveOrder, ticket);
					break;
				default:
					ticketId = null;
					return false;
			}

			bool isSuccessful = ticketingManager.AddOrUpdateTicket(ticket, out ticketId);
			
            string[] dmaAndTicketIdSplit = ticketId.Split('/');
            nonLiveOrder.DataMinerId = Convert.ToInt32(dmaAndTicketIdSplit[0]);
            nonLiveOrder.TicketId = Convert.ToInt32(dmaAndTicketIdSplit[1]);

            return isSuccessful;
        }

        public bool AssignNonLiveOrderTo(NonLiveOrder nonLiveOrder, User newOwner)
        {
            nonLiveOrder.Owner = newOwner.Name;
            nonLiveOrder.State = State.WorkInProgress;

            bool updateSuccessful = AddOrUpdateNonLiveOrder(nonLiveOrder, newOwner, out var ticketId);

            NotificationManager.SendNonLiveOrderWorkInProgressMail(helpers, nonLiveOrder);

            return updateSuccessful;
        }

		public bool UnassignNonLiveOrder(NonLiveOrder nonLiveOrder, User unassignedByUser)
        {
            nonLiveOrder.Owner = String.Empty;

            bool updateSuccessful = AddOrUpdateNonLiveOrder(nonLiveOrder, unassignedByUser, out var ticketId);

            return updateSuccessful;
        }

        public bool CompleteNonLiveOrder(NonLiveOrder nonLiveOrder, User currentUser)
        {
            if (nonLiveOrder is Ingest.Ingest ingestOrder)
            {
				var date = (bool)ingestOrder.BackUpsLongerStored ? ingestOrder.BackupDeletionDate : DateTime.Now.AddYears(1);
                ingestOrder.SetOriginalDeleteDate(date);
            }
            else if (nonLiveOrder is Project.Project projectOrder)
            {
				var date = projectOrder.IsLongerStoredBackUpChecked ? projectOrder.BackupDeletionDate : DateTime.Now.AddYears(1);
				projectOrder.SetOriginalDeleteDate(date);
            }
            else if (nonLiveOrder is FolderCreation.FolderCreation folderCreationOrder)
            {
                folderCreationOrder.SetOriginalDeleteDate(DateTime.Now.AddYears(1));
            }
			else
			{ 
                // do nothing
            }

            return UpdateNonLiveOrderState(nonLiveOrder, State.Completed, currentUser);
        }

        public bool SetNonLiveOrderToWorkInProgress(NonLiveOrder nonLiveOrder, User currentUser)
        {
            return UpdateNonLiveOrderState(nonLiveOrder, State.WorkInProgress, currentUser);
        }

        public bool SetNonLiveOrderToChangeRequested(NonLiveOrder nonLiveOrder, User currentUser)
        {
            return UpdateNonLiveOrderState(nonLiveOrder, State.ChangeRequested, currentUser);
        }

        public bool CancelNonLiveOrder(NonLiveOrder nonLiveOrder, User currentUser)
        {
            if (nonLiveOrder.State == State.Preliminary)
            {
                return DeleteNonLiveOrder((int)nonLiveOrder.DataMinerId, (int)nonLiveOrder.TicketId);
            }
            else
            {
                nonLiveOrder.State = State.Cancelled;
                if (AddOrUpdateNonLiveOrder(nonLiveOrder, currentUser, out string ticketId))
                {
                    NotificationManager.SendNonLiveOrderCancellationMail(helpers, nonLiveOrder);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteNonLiveOrder(int dataminerId, int ticketId)
        {
            var ticket = ticketingManager.GetTicket(dataminerId, ticketId);
            if (ticket == null) throw new TicketNotFoundException(dataminerId, ticketId);

            return ticketingManager.DeleteTicket(ticket);
        }

		/// <summary>
		/// Non live order status will be updated based on the given user task.
		/// </summary>
		/// <param name="nonLiveUserTaskManager">Handles the behavior of the user tasks.</param>
		/// <param name="userTask">is needed so that the linked non live order ticket id can be retrieved.</param>
		/// <param name="currentUser"></param>
		/// <param name="assignToMe">When a user task is assigned the order goes to work in progress.</param>
		/// <returns></returns>
		public bool TryUpdateNonLiveOrderStatusBasedOnUserTask(NonLiveUserTaskManager nonLiveUserTaskManager, NonLiveUserTask userTask, User currentUser, bool assignToMe = false)
        {
            string[] splitFullTicketId = userTask.IngestExportForeignKey.Split('/');
            if (splitFullTicketId.Length != 2)
                throw new ArgumentException("Unable to split in DataMiner ID and Ticket ID", nameof(userTask));
            if (!Int32.TryParse(splitFullTicketId[0], out int dataminerId))
                throw new ArgumentException("Unable to parse DataMiner ID", nameof(userTask));
            if (!Int32.TryParse(splitFullTicketId[1], out int ticketId))
                throw new ArgumentException("Unable to parse Ticket ID", nameof(userTask));

            if (!TryGetNonLiveOrder(Convert.ToInt32(splitFullTicketId[0]), Convert.ToInt32(splitFullTicketId[1]), out var retrievedNonLiveOrder))
            {
                helpers.Log(nameof(NonLiveOrderManager), nameof(TryUpdateNonLiveOrderStatusBasedOnUserTask), "Retrieving non live order fails with following ids: " + splitFullTicketId[0] + "/" + splitFullTicketId[1]);
                return false;
            }
            else
            {
                var retrievedUserTaks = nonLiveUserTaskManager.GetNonLiveUserTasks(retrievedNonLiveOrder);
                bool isThereAnyTaskInComplete = retrievedUserTaks.Exists(u => u.Status == UserTaskStatus.Incomplete);

                if (assignToMe || retrievedNonLiveOrder.State != State.Submitted && isThereAnyTaskInComplete)
                {
                    SetNonLiveOrderToWorkInProgress(retrievedNonLiveOrder, currentUser);
                }
                else if (!isThereAnyTaskInComplete)
                {
                    CompleteNonLiveOrder(retrievedNonLiveOrder, currentUser);
                }
				else
				{
					// nothing
				}

                return AddOrUpdateNonLiveOrder(retrievedNonLiveOrder, currentUser, out var foundTicketId);
            }
        }
        /// <summary>
        /// Retrieves all NonLive Orders.
        /// </summary>
        public List<NonLiveOrder> AllNonLiveOrdersOlderThan(TimeSpan time)
        {
            var allTickets = ticketingManager.AllTicketsOlderThan(time);
            List<NonLiveOrder> nonLiveOrders = allTickets.Select(t => GetNonLiveOrder(t)).ToList();
            return nonLiveOrders;
        }

        public bool UpdateNonLiveOrderStateAndMgmtState(NonLiveOrder nonLiveOrder, State state, MgmtState mgmtState, User currentUser)
        {
            if (nonLiveOrder.State == state && nonLiveOrder.MgmtState == mgmtState) return false;

            nonLiveOrder.State = state;
            nonLiveOrder.MgmtState = mgmtState;

            return AddOrUpdateNonLiveOrder(nonLiveOrder, currentUser, out string ticketId);
        }

        private bool UpdateNonLiveOrderState(NonLiveOrder nonLiveOrder, State state, User currentUser)
        {
            if (nonLiveOrder.State == state) return false;

            nonLiveOrder.State = state;

            if (AddOrUpdateNonLiveOrder(nonLiveOrder, currentUser, out string ticketId))
            {
                switch (state)
                {
                    case State.WorkInProgress:
                        NotificationManager.SendNonLiveOrderWorkInProgressMail(helpers, nonLiveOrder);
                        break;
                    case State.Completed:
                        NotificationManager.SendNonLiveOrderCompletionMail(helpers, nonLiveOrder);
                        break;
                    case State.ChangeRequested:
                        NotificationManager.SendNonLiveOrderRejectionMail(helpers, nonLiveOrder);
                        break;
					default:
						//nothing
						break;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static Export.Export ConvertTicketToExport(Ticket ticket)
        {
            Export.Export export = JsonConvert.DeserializeObject<Export.Export>(Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]));
            RetrieveGeneralInfo(export, ticket);

            var materialSourceTicketField = ticket.GetTicketField(NonLiveOrder.MaterialSourceTicketField) as Net.Ticketing.Validators.GenericEnumEntry<int>;
            if (materialSourceTicketField != null) export.MaterialSource = (Sources)materialSourceTicketField.Value;

            export.Deadline = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField]);

            return export;
        }

        private static Ingest.Ingest ConvertTicketToIngest(Ticket ticket)
        {
            Ingest.Ingest ingest = JsonConvert.DeserializeObject<Ingest.Ingest>(Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]));
            // Temporary support on different saved json property names
            if (ingest.CardImportDetails == null) ingest.CardImportDetails = ingest.CardIngestDetails;
            if (ingest.HddImportDetails == null) ingest.HddImportDetails = ingest.HddIngestDetails;
            if (ingest.FileImportDetails == null) ingest.FileImportDetails = ingest.FileIngestDetails;
            if (ingest.MetroMamImportDetails == null) ingest.MetroMamImportDetails = ingest.MetroMamIngestDetails;

            RetrieveGeneralInfo(ingest, ticket);

            if (ingest.IngestDestination.Destination != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA))
            {
                ingest.DeliveryTime = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeliveryDateTicketField]);
            }
		
			ingest.IsilonBackupFileLocation = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.IsilonBackupFileLocationTicketField, out var fileLocation) ? Convert.ToString(fileLocation) : String.Empty;
			ingest.Deadline = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.DeadlineTicketField, out var deadline) ? Convert.ToDateTime(deadline) : default(DateTime);		
	        ingest.BackupDeletionDate = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.BackupDeletionDateTicketField, out var backupDeletionDate) ? Convert.ToDateTime(backupDeletionDate) : default(DateTime);

            return ingest;
        }

        private static FolderCreation.FolderCreation ConvertTicketToFolderCreation(Ticket ticket)
        {
            FolderCreation.FolderCreation folderCreation = JsonConvert.DeserializeObject<FolderCreation.FolderCreation>(Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]));
            RetrieveGeneralInfo(folderCreation, ticket);

            folderCreation.Deadline = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField]);

            try
            {
                folderCreation.EarliestDeletionDate = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeletionDateTicketField]);
            }
            catch (Exception)
            {
                // Existing tickets don't have these fields
            }

            return folderCreation;
        }

        private static Transfer.Transfer ConvertTicketToTransfer(Ticket ticket)
        {
            Transfer.Transfer transfer = JsonConvert.DeserializeObject<Transfer.Transfer>(Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]));
            RetrieveGeneralInfo(transfer, ticket);

            transfer.Deadline = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField]);

            return transfer;
        }

        private static Project.Project ConvertTicketToProject(Ticket ticket)
        {
            string data = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]);
            Project.Project project = JsonConvert.DeserializeObject<Project.Project>(data);

            RetrieveGeneralInfo(project, ticket);

            project.Deadline = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.DeadlineTicketField, out var deadline) ? Convert.ToDateTime(deadline) : default;
            project.IsilonBackupFileLocation = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.IsilonBackupFileLocationTicketField, out var isilonBackupFileLocation) ? Convert.ToString(isilonBackupFileLocation) : String.Empty;

            project.BackupDeletionDate = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.BackupDeletionDateTicketField, out var deletionDate) ? Convert.ToDateTime(deletionDate) : default;

            project.MaterialDeliveryTime = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.DeliveryDateTicketField, out var deliveryDate) ? Convert.ToDateTime(deliveryDate) : project.Deadline.AddDays(-1);

            return project;
        }

        private static Aspera.Aspera ConvertTicketToAspera(Ticket ticket)
        {
            string data = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.DataTicketField]);
            Aspera.Aspera aspera = JsonConvert.DeserializeObject<Aspera.Aspera>(data);

            RetrieveGeneralInfo(aspera, ticket);

            aspera.Deadline = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField]);

            return aspera;
        }

        private static void RetrieveGeneralInfo(NonLiveOrder nonLiveOrder, Ticket ticket)
        {
            nonLiveOrder.DataMinerId = ticket.ID.DataMinerID;
            nonLiveOrder.TicketId = ticket.ID.TID;
            nonLiveOrder.UID = ticket.UID;
            nonLiveOrder.OrderDescription = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.OrderDescriptionTicketField]);
            nonLiveOrder.Deadline = Convert.ToDateTime(ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField]);
            nonLiveOrder.CreatedBy = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.CreatedByTicketField]);
            nonLiveOrder.ModifiedBy = new HashSet<string>(Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.ModifiedByTicketField]).Split(','));
            nonLiveOrder.LastModifiedBy = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.LastModifiedByTicketField]);
            nonLiveOrder.OperatorComment = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.OperatorCommentTicketField, out var operatorComment) ? Convert.ToString(operatorComment) : string.Empty;
            nonLiveOrder.IsilonBackupFileLocation = ticket.CustomTicketFields.TryGetValue(NonLiveOrder.IsilonBackupFileLocationTicketField, out var isilonBackupFileLocation) ? Convert.ToString(isilonBackupFileLocation) : string.Empty;

            if (ticket.CustomTicketFields.TryGetValue(NonLiveOrder.OwnerTicketField, out var owner))
            {
                nonLiveOrder.Owner = Convert.ToString(owner);
            }

            try
            {
                nonLiveOrder.ReasonOfRejection = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.ReasonOfRejectionField]);
                nonLiveOrder.TeamHki = Convert.ToBoolean(ticket.CustomTicketFields[NonLiveOrder.TeamHkiField]);
                nonLiveOrder.TeamNews = Convert.ToBoolean(ticket.CustomTicketFields[NonLiveOrder.TeamNewsField]);
                nonLiveOrder.TeamTre = Convert.ToBoolean(ticket.CustomTicketFields[NonLiveOrder.TeamTreField]);
                nonLiveOrder.TeamVsa = Convert.ToBoolean(ticket.CustomTicketFields[NonLiveOrder.TeamVsaField]);
                nonLiveOrder.TeamMgmt = Convert.ToBoolean(ticket.CustomTicketFields[NonLiveOrder.TeamMgmtField]);
            }
            catch (Exception)
            {
                // no handling
            }

            try
            {
                nonLiveOrder.CreatedByEmail = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.CreatedByEmailTicketField]);
                nonLiveOrder.CreatedByPhone = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.CreatedByPhoneTicketField]);
                nonLiveOrder.LastModifiedByEmail = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.LastModifiedByEmailTicketField]);
                nonLiveOrder.LastModifiedByPhone = Convert.ToString(ticket.CustomTicketFields[NonLiveOrder.LastModifiedByPhoneTicketField]);
            }
            catch
            {
                // 21/01/2022 Added 4 new fields. Existing tickets don't have these fields
            }
        }

        private static void ConvertExportToTicket(Export.Export export, Ticket ticket)
        {
            SetGeneralInfo(ticket, export);

            var materialSourceTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            materialSourceTicketField.Value = (int)export.MaterialSource;
            materialSourceTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(export.MaterialSource);
            ticket.CustomTicketFields[NonLiveOrder.MaterialSourceTicketField] = materialSourceTicketField;

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.Export;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.Export);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.AdditionalInformationTicketField] = export.AdditionalInformation;

            switch (export.MaterialSource)
            {
                case Sources.INTERPLAY_PAM:
                    ticket.CustomTicketFields[NonLiveOrder.SourceFolderPathTicketField] = string.Join(";", export.InterplayPamExport.FolderUrls);
                    break;
                case Sources.MEDIAPARKKI:
                    ticket.CustomTicketFields[NonLiveOrder.SourceFolderPathTicketField] = string.Join(";", export.MediaParkkiExport.SourceFolderUrls);
                    break;
                default:
					// nothing
                    break;
            }

            if (export.ExportInformation?.TargetOfExport == EnumExtensions.GetDescriptionFromEnumValue(ExportTargets.Mediaparkki))
            {
                ticket.CustomTicketFields[NonLiveOrder.TargetFolderPathTicketField] = export.ExportInformation?.MediaparkkiTargetFolder;
            }



            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(export, Formatting.None);
        }

        private static void ConvertIngestToTicket(Ingest.Ingest ingest, Ticket ticket)
        {
            SetGeneralInfo(ticket, ingest);

            if (ingest.IngestDestination.Destination != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA))
            {
                ticket.CustomTicketFields[NonLiveOrder.DeliveryDateTicketField] = ingest.DeliveryTime;
            }

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.Import;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.Import);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField] = ingest.Deadline;

            if (ingest.IngestDestination?.InterplayDestinationFolder != null)
            {
                ticket.CustomTicketFields[NonLiveOrder.InterplayFolderPathTicketField] = ingest.IngestDestination.InterplayDestinationFolder;
            }

            ticket.CustomTicketFields[NonLiveOrder.InterplayFormatTicketField] = ingest.InterplayFormat;

            if (ingest.CardImportDetails != null && ingest.CardImportDetails.Any())
            {
                string cardsCanBeReturnedToKalustovarastoTicketFieldValue = null;

                if (ingest.CardImportDetails.TrueForAll(c => c.CardCanBeReused))
                {
                    cardsCanBeReturnedToKalustovarastoTicketFieldValue = "Yes";
                }
                else if (ingest.CardImportDetails.TrueForAll(c => !c.CardCanBeReused))
                {
                    cardsCanBeReturnedToKalustovarastoTicketFieldValue = "No";
                }
                else
                {
                    cardsCanBeReturnedToKalustovarastoTicketFieldValue = "Multiple Values";
                }

                ticket.CustomTicketFields[NonLiveOrder.CardsCanBeReturnedToKalustovarastoTicketField] = cardsCanBeReturnedToKalustovarastoTicketFieldValue;
            }

            if (ingest.BackupDeletionDate != default)
            {
                ticket.CustomTicketFields[NonLiveOrder.BackupDeletionDateTicketField] = ingest.BackupDeletionDate;
            }

            ticket.CustomTicketFields[NonLiveOrder.AdditionalInformationTicketField] = ingest.AdditionalInformation;
            ticket.CustomTicketFields[NonLiveOrder.IsilonBackupFileLocationTicketField] = ingest.IsilonBackupFileLocation;
            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(ingest, Formatting.None);
			ticket.CustomTicketFields[NonLiveOrder.OriginalDeleteDateTicketField] = ingest.OriginalDeleteDate;
		}

        private static void ConvertFolderCreationToTicket(FolderCreation.FolderCreation folderCreation, Ticket ticket)
        {
            SetGeneralInfo(ticket, folderCreation);

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.IplayFolderCreation;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.IplayFolderCreation);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.InterplayFolderPathTicketField] = folderCreation.ParentFolder;

            ticket.CustomTicketFields[NonLiveOrder.DeletionDateTicketField] = folderCreation.RetrieveTheEarliestDeleteDate();

            ticket.CustomTicketFields[NonLiveOrder.OriginalDeleteDateTicketField] = folderCreation.OriginalDeleteDate;

            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(folderCreation, Formatting.None);
        }

        private static void ConvertTransferToTicket(Transfer.Transfer transfer, Ticket ticket)
        {
            SetGeneralInfo(ticket, transfer);

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.IplayWgTransfer;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.IplayWgTransfer);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.InterplayFolderPathTicketField] = transfer.SourceFolderUrls;

            ticket.CustomTicketFields[NonLiveOrder.AdditionalInformationTicketField] = transfer.AdditionalCustomerInformation;

            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(transfer, Formatting.None);
        }

        private static void ConvertProjectToTicket(Project.Project project, Ticket ticket)
        {
            SetGeneralInfo(ticket, project);

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.NonInterplayProject;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.NonInterplayProject);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField] = project.Deadline;

            ticket.CustomTicketFields[NonLiveOrder.DeliveryDateTicketField] = project.MaterialDeliveryTime;

            ticket.CustomTicketFields[NonLiveOrder.AdditionalInformationTicketField] = project.AdditionalInfo;

            ticket.CustomTicketFields[NonLiveOrder.IsilonBackupFileLocationTicketField] = project.IsilonBackupFileLocation;

            if (project.BackupDeletionDate != default)
            {
                ticket.CustomTicketFields[NonLiveOrder.BackupDeletionDateTicketField] = project.BackupDeletionDate;
            }

            ticket.CustomTicketFields[NonLiveOrder.OriginalDeleteDateTicketField] = project.OriginalDeleteDate;

            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(project, Formatting.None);
        }

        private static void ConvertAsperaToTicket(Aspera.Aspera aspera, Ticket ticket)
        {
            SetGeneralInfo(ticket, aspera);

            var typeTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            typeTicketField.Value = (int)Type.AsperaOrder;
            typeTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(Type.AsperaOrder);
            ticket.CustomTicketFields[NonLiveOrder.TypeTicketField] = typeTicketField;

            ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField] = aspera.Deadline;

            ticket.CustomTicketFields[NonLiveOrder.AdditionalInformationTicketField] = aspera.AdditionalInfo;

            ticket.CustomTicketFields[NonLiveOrder.DataTicketField] = JsonConvert.SerializeObject(aspera, Formatting.None);
        }

        private static void SetGeneralInfo(Ticket ticket, NonLiveOrder nonLiveOrder)
        {
            ticket.CustomTicketFields[NonLiveOrder.OrderDescriptionTicketField] = nonLiveOrder.OrderDescription;
            ticket.CustomTicketFields[NonLiveOrder.DeadlineTicketField] = nonLiveOrder.Deadline;
            ticket.CustomTicketFields[NonLiveOrder.StartTimeTicketField] = nonLiveOrder.StartTime;
            ticket.CustomTicketFields[NonLiveOrder.OwnerTicketField] = nonLiveOrder.Owner ?? NonLiveOrder.DefaultOwner;

            ticket.CustomTicketFields[NonLiveOrder.CreatedByTicketField] = nonLiveOrder.CreatedBy;
            ticket.CustomTicketFields[NonLiveOrder.LastModifiedByTicketField] = nonLiveOrder.LastModifiedBy;

            ticket.CustomTicketFields[NonLiveOrder.OperatorCommentTicketField] = nonLiveOrder.OperatorComment;

            try
            {
                ticket.CustomTicketFields[NonLiveOrder.LastModifiedByEmailTicketField] = nonLiveOrder.LastModifiedByEmail;
                ticket.CustomTicketFields[NonLiveOrder.LastModifiedByPhoneTicketField] = nonLiveOrder.LastModifiedByPhone;
                ticket.CustomTicketFields[NonLiveOrder.CreatedByEmailTicketField] = nonLiveOrder.CreatedByEmail;
                ticket.CustomTicketFields[NonLiveOrder.CreatedByPhoneTicketField] = nonLiveOrder.CreatedByPhone;
            }
            catch
            {
                // 21/01/2022 Added 4 new fields. Existing tickets do not have these fields
            }

            ticket.CustomTicketFields[NonLiveOrder.ModifiedByTicketField] = String.Join(",", nonLiveOrder.ModifiedBy);
            ticket.CustomTicketFields[NonLiveOrder.ShortDescriptionTicketField] = nonLiveOrder.ShortDescription;
            ticket.CustomTicketFields[NonLiveOrder.ReasonOfRejectionField] = nonLiveOrder.ReasonOfRejection;
            ticket.CustomTicketFields[NonLiveOrder.TeamHkiField] = nonLiveOrder.TeamHki.ToString();
            ticket.CustomTicketFields[NonLiveOrder.TeamNewsField] = nonLiveOrder.TeamNews.ToString();
            ticket.CustomTicketFields[NonLiveOrder.TeamTreField] = nonLiveOrder.TeamTre.ToString();
            ticket.CustomTicketFields[NonLiveOrder.TeamVsaField] = nonLiveOrder.TeamVsa.ToString();
            ticket.CustomTicketFields[NonLiveOrder.TeamMgmtField] = nonLiveOrder.TeamMgmt.ToString();
            ticket.CustomTicketFields[NonLiveOrder.IsilonBackupFileLocationTicketField] = nonLiveOrder.IsilonBackupFileLocation;

            var stateTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            stateTicketField.Value = (int)nonLiveOrder.State;
            stateTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(nonLiveOrder.State);
            ticket.CustomTicketFields[NonLiveOrder.StateTicketField] = stateTicketField;

            var mgmtStateTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            mgmtStateTicketField.Value = (int)nonLiveOrder.MgmtState;
            mgmtStateTicketField.Name = EnumExtensions.GetDescriptionFromEnumValue(nonLiveOrder.MgmtState);
            ticket.CustomTicketFields[NonLiveOrder.MgmtStateTicketField] = mgmtStateTicketField;
        }
    }
}