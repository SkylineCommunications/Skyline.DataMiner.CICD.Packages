namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System;
	using System.IO;
	using Library_1.EventArguments;
	using Library_1.UI.NonLive.Import;
	using Library_1.UI.NonLive.NonIplayProject;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Aspera;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.NonIplayProject;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.WgTransfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using ScriptAction = LoadingScreens.LoadNonLiveOrderFormDialog.ScriptAction;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;

	public class MainDialog : YleDialog
	{
		private readonly UserInfo userInfo;
		private readonly ScriptAction scriptAction;

		private readonly Label readonlyReasonLabel = new Label();
		private readonly Label nonLiveOrderTitle = new Label("Non-Live Order") { Style = TextStyle.Bold };
		private readonly Label nonLiveOrderTypeLabel = new Label("Type");
		private readonly DropDown nonLiveOrderTypeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<Type>(), Type.Export.GetDescription()) { MinWidth = 150 };
		private readonly Button saveButton = new Button("Save Preliminary Order") { Width = 250 };
		private readonly Button bookButton = new Button("Book Order") { Width = 250, Style = ButtonStyle.CallToAction, IsEnabled = true, IsVisible = true };
		private readonly Label validationLabel = new Label(string.Empty) { IsVisible = false };

		private bool isReadOnly;
		private readonly bool editOrder;
		private MainSection mainSection;

		public MainDialog(Helpers helpers, UserInfo userInfo, ScriptAction scriptAction, NonLiveOrder nonLiveOrder = null) : base(helpers)
		{
			AllowOverlappingWidgets = true;
			this.userInfo = userInfo;
			this.scriptAction = scriptAction;
			this.NonLiveOrder = nonLiveOrder;
			Title = "Non-Live Order";

            editOrder = nonLiveOrder != null;
			Type = nonLiveOrder != null ? nonLiveOrder.OrderType : Type.Import;

			CancelButton.IsVisible = editOrder;
			saveButton.IsVisible = nonLiveOrder is null || nonLiveOrder.State == State.Preliminary;
			RejectOrderButton.IsVisible = editOrder && userInfo.IsMcrUser;

			bookButton.Pressed += BookButton_Pressed;
			saveButton.Pressed += SaveButton_Pressed;

			InitReadOnly(userInfo);

			nonLiveOrderTypeDropDown.Changed += NonLiveOrderTypeDropDown_Changed;

			GenerateUI();
            UpdateValidity(OrderAction.Book);
		}

		public NonLiveOrder NonLiveOrder { get; }

		public Button CancelButton { get; } = new Button("Cancel Order") { Width = 250 };

		public Button RejectOrderButton { get; } = new Button("Reject Order") { Width = 250 };

		public event EventHandler<StringEventArgs> BookOrSaveFinished;

		private Type Type
		{
			get
			{
				return nonLiveOrderTypeDropDown.Selected.GetEnumValue<Type>();
			}
			set
			{
				nonLiveOrderTypeDropDown.Selected = value.GetDescription();

				switch (value)
				{
					case Type.Export:
						mainSection = new ExportMainSection(helpers, (Export)NonLiveOrder);
						break;
					case Type.Import:
						var importConfiguration = new ImportSectionConfiguration(userInfo);
						mainSection = new ImportMainSection(helpers, (Ingest)NonLiveOrder, importConfiguration);
						break;
					case Type.IplayFolderCreation:
						mainSection = new FolderCreationSection(helpers, (FolderCreation)NonLiveOrder);
						break;
					case Type.IplayWgTransfer:
						mainSection = new TransferSection(helpers, (Transfer)NonLiveOrder);
						break;
					case Type.NonInterplayProject:
						var projectConfiguration = new ProjectSectionConfiguration(userInfo);
						mainSection = new ProjectSection(helpers, projectConfiguration, (Project)NonLiveOrder);
						break;
					case Type.AsperaOrder:
						mainSection = new AsperaMainSection(helpers, (IngestExport.Aspera.Aspera)NonLiveOrder);
						break;
					default:
						// no other options
						break;
				}

				mainSection.UiEnabledStateChangeRequired += MainSection_UiEnabledStateChangeRequired;
				mainSection.RegenerateUiRequired += MainSection_RegenerateUiRequired;

                UpdateValidity(OrderAction.Book);
			}
		}

		private bool UpdateValidity(OrderAction action)
        {
	        bool mainSectionIsValid = mainSection.IsValid(action);

	        validationLabel.Text = mainSectionIsValid ? string.Empty : "Please complete all required fields";
	        validationLabel.IsVisible = !mainSectionIsValid;

            return mainSectionIsValid;
        }

        private void GenerateUI()
		{
			Clear();

			MinWidth = 800;

			int row = -1;

			AddWidget(nonLiveOrderTitle, ++row, 0);

			if (NonLiveOrder == null)
			{
				AddWidget(nonLiveOrderTypeLabel, ++row, 0);
				AddWidget(nonLiveOrderTypeDropDown, row, 1, 1, 2);
			} 
			else if (isReadOnly && !String.IsNullOrWhiteSpace(readonlyReasonLabel.Text))
			{ 
				AddWidget(readonlyReasonLabel, ++row, 0, 1, 3); 
			}
			else
			{
				// nothing
			}
			
			AddSection(mainSection, new SectionLayout(++row, 0));
			row += mainSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(saveButton, ++row, 0, 1, 3);
			AddWidget(bookButton, ++row, 0, 1, 3);
			AddWidget(CancelButton, ++row, 0, 1, 3);
			AddWidget(RejectOrderButton, ++row, 0, 1, 3);

			AddWidget(validationLabel, row + 1, 0, 1, 3);

            SetColumnWidth(2, 350);

			HandleEnabledUpdate();
		}

		private static NonLiveOrder GenerateNewNonLiveOrder(Type type)
		{
			switch (type)
			{
				case Type.Export:
					return new Export();
				case Type.Import:
					return new Ingest();
				case Type.IplayFolderCreation:
					return new FolderCreation();
				case Type.IplayWgTransfer:
					return new Transfer();
				case Type.NonInterplayProject:
					return new Project();
				case Type.AsperaOrder:
					return new IngestExport.Aspera.Aspera();
				default:
					throw new ArgumentException($"{type} is an unsupported type of Non Live Order");
			}
		}

		private static void SetNonLiveOrderState(OrderAction action, NonLiveOrder nonLiveOrder)
		{
			if (action == OrderAction.Book)
			{
				if (nonLiveOrder.State == State.WorkInProgress && nonLiveOrder.IsAssignedToSomeone) return;

				nonLiveOrder.State = State.Submitted;
			}
			else
			{
				nonLiveOrder.State = State.Preliminary;
			}
		}

		private void MainSection_RegenerateUiRequired(object sender, EventArgs e)
		{
			mainSection.RegenerateUi();
			GenerateUI();
		}

		private void MainSection_UiEnabledStateChangeRequired(object sender, UiDisabling.EnabledStateEventArgs e)
		{
			if (e.EnabledState == UiDisabling.EnabledState.Enabled)
			{
				EnableUi();
			}
			else
			{
				DisableUi();
			}
		}

		private void BookButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleAction(OrderAction.Book);
			}
		}

		private void SaveButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleAction(OrderAction.Save);
			}
		}

		private void HandleAction(OrderAction action)
		{
			if (!UpdateValidity(action))
			{
				validationLabel.Text = "Form can't be saved. Make sure all required fields are correctly configured.";
				return;
			}

			var nonLiveOrder = editOrder ? NonLiveOrder : GenerateNewNonLiveOrder(Type);

			mainSection.UpdateNonLiveOrder(nonLiveOrder);

			SetNonLiveOrderState(action, nonLiveOrder);

			if (TryAddOrUpdateNonLiveOrder(nonLiveOrder))
			{
				BookOrSaveFinished?.Invoke(this, new StringEventArgs($"Order successfully {(action == OrderAction.Book ? "booked" : "saved")}"));
			}
			else
			{
				BookOrSaveFinished?.Invoke(this, new StringEventArgs("Unable to add or update Non Live Order"));
			}
		}

		private bool TryAddOrUpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (helpers.NonLiveOrderManager.AddOrUpdateNonLiveOrder(nonLiveOrder, userInfo.User, out var dmaAndTicketId))
			{
				string[] dmaAndTicketIdSplit = dmaAndTicketId.Split(new[] { '/' });
				nonLiveOrder.DataMinerId = Convert.ToInt32(dmaAndTicketIdSplit[0]);
				nonLiveOrder.TicketId = Convert.ToInt32(dmaAndTicketIdSplit[1]);

				switch (Type)
				{
					case Type.Export:
						var exportOrder = (Export)nonLiveOrder;
						string validFolderName = dmaAndTicketId.Replace('/', '_');
						string folderPath = Path.Combine(Constants.TicketAttachmentsFolderPath, validFolderName);

						exportOrder.UpdateExportFilesToSpecificDirectory(helpers, folderPath);
						break;

					case Type.IplayFolderCreation when !editOrder:
						NotificationManager.SendNonLiveOrderIplayFolderCreationMail(helpers, nonLiveOrder);
						break;

					case Type.IplayWgTransfer:
						helpers.NonLiveUserTaskManager.AddOrUpdateUserTasks(nonLiveOrder);
						break;

					default:
						// Unsupported non-live type
						break;
				}

				if (editOrder) NotificationManager.SendNonLiveOrderEditedMail(helpers, nonLiveOrder);
				else if (Type != Type.IplayFolderCreation)
				{
					//Avoid sending two emails for IPlayFolderCreation
					NotificationManager.SendNonLiveOrderCreationMail(helpers, nonLiveOrder);
				}
				else
				{
					//do nothing
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		private void InitReadOnly(UserInfo userInfo)
		{
			if (scriptAction == ScriptAction.View)
			{
				InitReadOnly(true);
				return;
			}

			bool newOrder = !editOrder;
			bool isMcrUser = userInfo.IsMcrUser;
			if (newOrder || isMcrUser)
			{
				InitReadOnly(false);
				return;
			}

			bool orderStateAllowsEditing = false;
			bool currentUserIsCreator = false;
			bool orderEditingAllowedForNonMcrUser = false;

			if (NonLiveOrder != null)
			{
				orderStateAllowsEditing = NonLiveOrder.State == State.Preliminary || NonLiveOrder.State == State.Submitted || NonLiveOrder.State == State.ChangeRequested;

				currentUserIsCreator = !string.IsNullOrWhiteSpace(NonLiveOrder.CreatedBy) && NonLiveOrder.CreatedBy == helpers.Engine.UserLoginName;

				orderEditingAllowedForNonMcrUser = (NonLiveOrder.State == State.WorkInProgress || NonLiveOrder.State == State.Completed) && 
					scriptAction == ScriptAction.Duplicate;
			}

			if (currentUserIsCreator || orderStateAllowsEditing || orderEditingAllowedForNonMcrUser)
			{
				InitReadOnly(false);
			}
			else
			{
				var message = $"Unable to edit the order as it was created by {NonLiveOrder.CreatedBy} and status is {NonLiveOrder.State.GetDescription()}";
				InitReadOnly(true, message);
			}
		}

		private void InitReadOnly(bool readOnly, string reason = "")
		{
			isReadOnly = readOnly;
			readonlyReasonLabel.Text = reason;
		}

		protected override void HandleEnabledUpdate()
		{
			nonLiveOrderTypeDropDown.IsEnabled = IsEnabled && !isReadOnly;

			mainSection.IsEnabled = IsEnabled && !isReadOnly;

			// Explicitly set this on the TreeViews so that the TextBox is disabled
			foreach (var treeViewSection in mainSection.TreeViewSections)
			{
				treeViewSection.IsEnabled = !isReadOnly;
			}

			saveButton.IsEnabled = IsEnabled && !isReadOnly;
			CancelButton.IsEnabled = IsEnabled && !isReadOnly;
			bookButton.IsEnabled = IsEnabled && !isReadOnly;
			RejectOrderButton.IsEnabled = IsEnabled && !isReadOnly;
		}

		private void NonLiveOrderTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				Type = e.Selected.GetEnumValue<Type>();
				GenerateUI();
			}
		}
	}
}
