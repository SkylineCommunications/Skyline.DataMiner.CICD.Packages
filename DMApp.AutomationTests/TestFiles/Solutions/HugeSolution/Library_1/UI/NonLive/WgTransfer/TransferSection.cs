namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.WgTransfer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public sealed class TransferSection : MainSection
	{
		private const string None = "None";

		private readonly Label sourceDetailsTitle = new Label("Source Details") { Style = TextStyle.Bold };

		private readonly Label materialSourceLabel = new Label("Material source");
		private readonly DropDown materialSourceDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.Helsinki));

		private readonly Label interplaySourceFolderLabel = new Label("Interplay source folder");
		private readonly YleTextBox interplaySourceFolderTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label fileNameLabel = new Label("File name");
		private readonly YleTextBox fileNameTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

		private readonly Label fileTypeLabel = new Label("File type");
		private readonly DropDown fileTypeDropDown = new DropDown(new[] { None }.Concat(EnumExtensions.GetEnumDescriptions<SourceFileTypes>().OrderBy(x => x)), None);

		private readonly Label destinationDetailsTitle = new Label("Destination Details") { Style = TextStyle.Bold };

		private readonly Label materialDestinationLabel = new Label("Destination of transfer");
		private readonly DropDown materialDestinationDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.Tampere));

		private readonly Label interplayDestinationFolderLabel = new Label("Interplay destination folder");
		private readonly YleTextBox interplayDestinationFolderTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label additionalInformationTitle = new Label("Additional information") { Style = TextStyle.Bold };
		private readonly Label additionalInfoLabel = new Label("Additional customer information");
		private readonly YleTextBox additionalInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

		private readonly Transfer transfer;

		private readonly Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection> treeViewSections = new Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection>();
		private TransferGeneralInfoSection generalInfoSection;
		private readonly NotificationSection notificationSection;
		private readonly ISectionConfiguration configuration = new NonLiveOrderConfiguration();

		public TransferSection(Helpers helpers, Transfer transfer = null) : base(helpers)
		{
			this.transfer = transfer;

			materialSourceDropDown.Changed += MaterialSourceDropDown_Changed;
			materialDestinationDropDown.Changed += MaterialDestinationDropDown_Changed;
			fileTypeDropDown.Changed += FileTypeDropDown_Changed;

			interplaySourceFolderTextBox.Changed += (sender, args) => IsValid(OrderAction.Book);
			fileNameTextBox.Changed += (sender, args) => IsValid(OrderAction.Book);

			generalInfoSection = new TransferGeneralInfoSection(helpers, configuration, transfer);
			notificationSection = new NotificationSection(helpers, configuration, transfer);

			InitializeTransfer();

			GenerateUi(out int row);
		}

		public InterplayPamElements MaterialSource { get => EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(materialSourceDropDown.Selected); }

		public Folder SourceFolder
		{
			get
			{
				if (MaterialSource == InterplayPamElements.UA)
				{
					return !String.IsNullOrWhiteSpace(interplaySourceFolderTextBox.Text) ? new Folder { URL = interplaySourceFolderTextBox.Text } : null;
				}
				else
				{
					return treeViewSections[MaterialSource].SelectedFolder;
				}
			}
		}

		public HashSet<File> SourceFiles
		{
			get
			{
				if (MaterialSource == InterplayPamElements.UA)
				{
					return !String.IsNullOrWhiteSpace(fileNameTextBox.Text) ? new HashSet<File> { new File { URL = fileNameTextBox.Text } } : null;
				}
				else
				{
					return treeViewSections[MaterialSource].SelectedFiles;
				}
			}
		}

		public SourceFileTypes? SourceFileType
		{
			get
			{
				if (fileTypeDropDown.Selected == None)
				{
					return null;
				}
				else
				{
					return EnumExtensions.GetEnumValueFromDescription<SourceFileTypes>(fileTypeDropDown.Selected);
				}
			}
			private set
			{
				fileTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
			}
		}

		private InterplayPamElements MaterialDestination { get => EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(materialDestinationDropDown.Selected); }

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return treeViewSections.Select(x => x.Value);
			}
		}

		public override bool IsValid(OrderAction action)
		{
			bool isOrderDescriptionAndDeadlineValid = generalInfoSection.IsValid(action);
			if (action == OrderAction.Save)
			{
				return isOrderDescriptionAndDeadlineValid;
			}

			bool isInterplaySourceFolderValid = MaterialSource == InterplayPamElements.UA || (SourceFiles != null && SourceFiles.Any());
			interplaySourceFolderTextBox.ValidationState = isInterplaySourceFolderValid ? UIValidationState.Valid : UIValidationState.Invalid;
			interplaySourceFolderTextBox.ValidationText = "Select one or more multiple source files";

			if (MaterialSource != InterplayPamElements.UA)
			{
				treeViewSections[MaterialSource].ValidationState = isInterplaySourceFolderValid ? UIValidationState.Valid : UIValidationState.Invalid;
				treeViewSections[MaterialSource].ValidationText = "Select one or multiple source files";
			}

			bool isUaFileNameValid = MaterialSource != InterplayPamElements.UA || (MaterialSource == InterplayPamElements.UA && !String.IsNullOrWhiteSpace(fileNameTextBox.Text));
			fileNameTextBox.ValidationState = isUaFileNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			fileNameTextBox.ValidationText = "Provide a file name";

			bool isUaFileTypeValid = MaterialSource != InterplayPamElements.UA || (MaterialSource == InterplayPamElements.UA && SourceFileType != null);
			fileTypeDropDown.ValidationState = isUaFileTypeValid ? UIValidationState.Valid : UIValidationState.Invalid;
			fileTypeDropDown.ValidationText = "Select a file type";

			bool isInterplayDestinationFolderValid = !String.IsNullOrEmpty(interplayDestinationFolderTextBox.Text);
			interplayDestinationFolderTextBox.ValidationState = isInterplayDestinationFolderValid ? UIValidationState.Valid : UIValidationState.Invalid;
			interplayDestinationFolderTextBox.ValidationText = "Provide a destination folder";

			return isOrderDescriptionAndDeadlineValid
				&& isInterplaySourceFolderValid
				&& isUaFileNameValid
				&& isUaFileTypeValid
				&& isInterplayDestinationFolderValid;

		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddSection(generalInfoSection, new SectionLayout(++row, 0));
			row += generalInfoSection.RowCount;

			AddWidget(sourceDetailsTitle, ++row, 0);

			AddWidget(materialSourceLabel, ++row, 0);
			AddWidget(materialSourceDropDown, row, 1, 1, 2);

			if (MaterialSource != InterplayPamElements.UA)
			{
				// IPlay Tampere, Vaasa, Helsinki
				foreach (var kvp in treeViewSections)
				{
					AddSection(kvp.Value, new SectionLayout(++row, 0));
					row += kvp.Value.RowCount;
				}
			}

			// IPlay UA
			AddWidget(interplaySourceFolderLabel, ++row, 0);
			AddWidget(interplaySourceFolderTextBox, row, 1, 1, 2);

			AddWidget(fileNameLabel, ++row, 0);
			AddWidget(fileNameTextBox, row, 1, 1, 2);

			AddWidget(fileTypeLabel, ++row, 0);
			AddWidget(fileTypeDropDown, row, 1, 1, 2);

			AddWidget(destinationDetailsTitle, ++row, 0);

			AddWidget(materialDestinationLabel, ++row, 0);
			AddWidget(materialDestinationDropDown, row, 1, 1, 2);

			AddWidget(interplayDestinationFolderLabel, ++row, 0);
			AddWidget(interplayDestinationFolderTextBox, row, 1, 1, 2);

			AddWidget(additionalInformationTitle, ++row, 0);
			AddWidget(additionalInfoLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(additionalInfoTextBox, row, 1, 1, 2);

			AddSection(notificationSection, new SectionLayout(row + 1, 0));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			generalInfoSection.IsVisible = IsVisible;
			generalInfoSection.IsEnabled = IsEnabled;

			sourceDetailsTitle.IsVisible = IsVisible;

			materialSourceLabel.IsVisible = IsVisible;
			materialSourceDropDown.IsVisible = IsVisible;
			materialSourceDropDown.IsEnabled = IsEnabled;

			foreach (var kvp in treeViewSections)
			{
				kvp.Value.IsVisible = MaterialSource == kvp.Key;
				kvp.Value.IsEnabled = IsEnabled;
			}

			interplaySourceFolderLabel.IsVisible = interplaySourceFolderTextBox.IsVisible;

			interplaySourceFolderTextBox.IsVisible = MaterialSource == InterplayPamElements.UA;
			interplaySourceFolderTextBox.IsEnabled = IsEnabled;

			fileNameLabel.IsVisible = MaterialSource == InterplayPamElements.UA;
			fileNameTextBox.IsVisible = fileNameLabel.IsVisible;
			fileNameTextBox.IsEnabled = IsEnabled;

			fileTypeLabel.IsVisible = MaterialSource == InterplayPamElements.UA;
			fileTypeDropDown.IsVisible = fileTypeLabel.IsVisible;
			fileTypeDropDown.IsEnabled = IsEnabled;

			destinationDetailsTitle.IsVisible = IsVisible;

			materialDestinationLabel.IsVisible = IsVisible;
			materialDestinationDropDown.IsVisible = IsVisible;
			materialDestinationDropDown.IsEnabled = IsEnabled;

			interplayDestinationFolderLabel.IsVisible = IsVisible;
			interplayDestinationFolderTextBox.IsVisible = IsVisible;
			interplayDestinationFolderTextBox.IsEnabled = IsEnabled;

			additionalInformationTitle.IsVisible = IsVisible;
			additionalInfoLabel.IsVisible = IsVisible;
			additionalInfoTextBox.IsVisible = IsVisible;
			additionalInfoTextBox.IsEnabled = IsEnabled;

			notificationSection.IsVisible = IsVisible;
			notificationSection.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (nonLiveOrder.OrderType != IngestExport.Type.IplayWgTransfer) return;

			Transfer transferOrder = (Transfer)nonLiveOrder;

			if (this.transfer != null)
			{
				transferOrder.DataMinerId = this.transfer.DataMinerId != null ? this.transfer.DataMinerId : null;
				transferOrder.TicketId = this.transfer.TicketId != null ? this.transfer.TicketId : null;
			}

			generalInfoSection.UpdateTransfer((Transfer)nonLiveOrder);

			transferOrder.Source = EnumExtensions.GetDescriptionFromEnumValue(MaterialSource);
			transferOrder.SourceFolderUrls = MaterialSource != InterplayPamElements.UA ? treeViewSections[MaterialSource].GetFolderUrls().ToArray() : new[] { interplaySourceFolderTextBox.Text };
			transferOrder.FileUrls = MaterialSource != InterplayPamElements.UA ? treeViewSections[MaterialSource].GetSelectedFileUrls() : new[] { fileNameTextBox.Text };

			if (MaterialSource == InterplayPamElements.UA)
			{
				transferOrder.FileUrls = new[] { fileNameTextBox.Text };
				transferOrder.FileType = SourceFileType != null ? EnumExtensions.GetDescriptionFromEnumValue(SourceFileType) : None;
			}

			transferOrder.Destination = EnumExtensions.GetDescriptionFromEnumValue(MaterialDestination);
			transferOrder.InterplayDestinationFolder = interplayDestinationFolderTextBox.Text;
			transferOrder.AdditionalCustomerInformation = additionalInfoTextBox.Text;
			transferOrder.EmailReceivers = notificationSection.GetEmails();

			// Update non - live teams filtering
			transferOrder.TeamHki = MaterialDestination == InterplayPamElements.Helsinki || MaterialSource == InterplayPamElements.Helsinki;
			transferOrder.TeamNews = MaterialDestination == InterplayPamElements.UA || MaterialSource == InterplayPamElements.UA;
			transferOrder.TeamTre = MaterialDestination == InterplayPamElements.Tampere || MaterialSource == InterplayPamElements.Tampere;
			transferOrder.TeamVsa = MaterialDestination == InterplayPamElements.Vaasa || MaterialSource == InterplayPamElements.Vaasa;
			transferOrder.TeamMgmt = false;
		}

		private void InitializeTransfer()
		{
			var helsinkiTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Helsinki), TreeViewType.FileSelector);
			helsinkiTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			var vaasaTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Vaasa), TreeViewType.FileSelector);
			vaasaTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			var tampereTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Tampere), TreeViewType.FileSelector);
			tampereTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			helsinkiTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			vaasaTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			tampereTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);

			helsinkiTreeViewSection.SourceLabel.Text = "Interplay source file(s)";
			tampereTreeViewSection.SourceLabel.Text = "Interplay source file(s)";
			vaasaTreeViewSection.SourceLabel.Text = "Interplay source file(s)";

			helsinkiTreeViewSection.InitRoot();
			vaasaTreeViewSection.InitRoot();
			tampereTreeViewSection.InitRoot();

			treeViewSections.Add(InterplayPamElements.Helsinki, helsinkiTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Vaasa, vaasaTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Tampere, tampereTreeViewSection);

			if (transfer == null) return;

			InitializeSourceDetails(transfer);
			InitializeDestinationDetails(transfer);
		}

		private void InitializeSourceDetails(Transfer transfer)
		{
			materialSourceDropDown.Selected = transfer.Source;

			if (MaterialSource == InterplayPamElements.UA)
			{
				interplaySourceFolderTextBox.Text = transfer.SourceFolderUrls != null && transfer.SourceFolderUrls.Any() ? string.Join(Environment.NewLine, transfer.SourceFolderUrls) : String.Empty;
				fileNameTextBox.Text = transfer.FileUrls != null && transfer.FileUrls.Any() ? string.Join(Environment.NewLine, transfer.FileUrls) : String.Empty;
				fileTypeDropDown.Selected = transfer.FileType;
			}
			else if (transfer.SourceFolderUrls != null && transfer.SourceFolderUrls.Any())
			{
				treeViewSections[MaterialSource].Initialize(transfer.SourceFolderUrls, transfer.FileUrls);
			}
		}

		private void InitializeDestinationDetails(Transfer transfer)
		{
			generalInfoSection = new TransferGeneralInfoSection(helpers, configuration, transfer);
			interplayDestinationFolderTextBox.Text = transfer.InterplayDestinationFolder;

			materialDestinationDropDown.Options = EnumExtensions.GetEnumDescriptions<InterplayPamElements>().Where(x => x != materialSourceDropDown.Selected).OrderBy(y => y);
			materialDestinationDropDown.Selected = transfer.Destination;

			additionalInfoTextBox.Text = transfer.AdditionalCustomerInformation;
		}

		private void MaterialSourceDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
		}

		private void MaterialDestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
			}
		}

		private void FileTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				fileTypeDropDown.Options = fileTypeDropDown.Options.Where(x => !x.Equals(None)).ToList();

				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}

		public override void RegenerateUi()
		{
			generalInfoSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}
