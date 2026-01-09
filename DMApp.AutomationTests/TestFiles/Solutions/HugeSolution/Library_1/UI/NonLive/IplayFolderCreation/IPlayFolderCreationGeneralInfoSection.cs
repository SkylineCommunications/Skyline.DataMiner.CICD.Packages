namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class IPlayFolderCreationGeneralInfoSection : YleSection
	{
		private readonly FolderCreation folderCreation;
		private readonly ISectionConfiguration configuration;
		private readonly FolderCreationSection section;

		private NewFolderContentTypes selectedFolderContentType = NewFolderContentTypes.NONE;

		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private readonly Label destinationLabel = new Label("Destination");
		private readonly DropDown destinationDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>().Where(x => x != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA)), EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.Helsinki));

		private readonly Label newFolderContentTypeLabel = new Label("New folder content type");
		private readonly DropDown newFolderContentTypeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<NewFolderContentTypes>().OrderBy(x => x), EnumExtensions.GetDescription(NewFolderContentTypes.NONE));

		private readonly Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection> treeViewSections = new Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection>();

		protected bool ignoreTimingValidationWhenOrderExists;

		public IPlayFolderCreationGeneralInfoSection(Helpers helpers, ISectionConfiguration configuration, FolderCreationSection section, FolderCreation folderCreation = null) : base(helpers)
		{
			this.folderCreation = folderCreation;
			this.section = section;
			this.configuration = configuration;

			InitializeFolderCreation();

			destinationDropDown.Changed += DestinationDropDown_Changed;
			newFolderContentTypeDropDown.Changed += NewFolderContentTypeDropDown_Changed;
			deadlineDateTimePicker.Changed += (o, e) => IsValid(OrderAction.Book);

			GenerateUi(out int row);
		}

		public IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return treeViewSections.Select(x => x.Value);
			}
		}

		public InterplayPamElements Destination { get => EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(destinationDropDown.Selected); }

		public NewFolderContentTypes NewFolderContentType
		{
			get
			{
				return selectedFolderContentType;
			}
			set
			{
				if (NewFolderContentType != value)
				{
					selectedFolderContentType = value;
					newFolderContentTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedFolderContentType);

                    switch (selectedFolderContentType)
                    {
                        case NewFolderContentTypes.PROGRAM:
                            section.NewProgramFolderRequestSection = new NewProgramFolderRequestSection(helpers, configuration, section);
							section.NewProgramFolderRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
							break;

                        case NewFolderContentTypes.EPISODE:
                            section.NewProgramFolderRequestSection = null;
                            if (!section.NewEpisodeFolderRequestSections.Any())
                            {
								var newEpisodeFolderRequestSection = new NewEpisodeFolderRequestSection(helpers, configuration, section);
								newEpisodeFolderRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

								section.NewEpisodeFolderRequestSections.Add(newEpisodeFolderRequestSection);
                            }
                            break;
                        default:
							section.NewProgramFolderRequestSection = null;
							section.NewEpisodeFolderRequestSections.Clear();
							break;
                    }
                }
            }
        }

        public Folder ParentFolder { get => treeViewSections[Destination].SelectedFolder; }

		public bool IsValid(OrderAction action)
		{
			bool isOrderDescriptionValid = !String.IsNullOrWhiteSpace(nameOfTheOrderTextBox.Text);
			nameOfTheOrderTextBox.ValidationState = isOrderDescriptionValid ? UIValidationState.Valid : UIValidationState.Invalid;
			nameOfTheOrderTextBox.ValidationText = "Provide a name for the order";

			bool isDeadLineValid = ignoreTimingValidationWhenOrderExists || deadlineDateTimePicker.IsValid;

			bool isDescriptionAndDeadlineValid = isOrderDescriptionValid && isDeadLineValid;
			if (action == OrderAction.Save) return isDescriptionAndDeadlineValid;

			bool isFolderContentTypeValid = NewFolderContentType != NewFolderContentTypes.NONE;
			newFolderContentTypeDropDown.ValidationState = isFolderContentTypeValid ? UIValidationState.Valid : UIValidationState.Invalid;
			newFolderContentTypeDropDown.ValidationText = "Select a folder content type";

			bool isFolderValid = ParentFolder != null;
			treeViewSections[Destination].ValidationState = isFolderValid ? UIValidationState.Valid : UIValidationState.Invalid;
			treeViewSections[Destination].ValidationText = "Select a parent folder";

			return isDescriptionAndDeadlineValid
				&& isFolderContentTypeValid
				&& isFolderValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(generalOrderInformationTitle, ++row, 0, 1, 5);

			AddWidget(nameOfTheOrderLabel, ++row, 0);
			AddWidget(nameOfTheOrderTextBox, row, 1, 1, 2);

			AddWidget(deadlineLabel, ++row, 0);
			AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

			AddWidget(destinationLabel, ++row, 0);
			AddWidget(destinationDropDown, row, 1, 1, 2);

			AddWidget(newFolderContentTypeLabel, ++row, 0);
			AddWidget(newFolderContentTypeDropDown, row, 1, 1, 2);

			AddSection(treeViewSections[Destination], new SectionLayout(++row, 0));

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public void UpdateFolderCreation(FolderCreation folderCreation)
		{
			folderCreation.OrderDescription = nameOfTheOrderTextBox.Text;
			folderCreation.Destination = EnumExtensions.GetDescriptionFromEnumValue(Destination);
			folderCreation.ContentType = EnumExtensions.GetDescriptionFromEnumValue(NewFolderContentType);
			folderCreation.ParentFolder = treeViewSections[Destination]?.SelectedFolder?.URL ?? String.Empty;

			if (this.folderCreation != null) return;
			
			// Set the Deadline to 1h from now for new Folder Creation orders
			DateTime nowPlus1h = DateTime.Now.AddHours(1).AddMinutes(1);
			folderCreation.Deadline = new DateTime(nowPlus1h.Year, nowPlus1h.Month, nowPlus1h.Day, nowPlus1h.Hour, nowPlus1h.Minute, 0);
		}

		private void InitializeFolderCreation()
		{
			ignoreTimingValidationWhenOrderExists = folderCreation != null;

			var helsinkiTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Helsinki), TreeViewType.FolderSelector);
			helsinkiTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			var vaasaTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Vaasa), TreeViewType.FolderSelector);
			vaasaTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			var tampereTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Tampere), TreeViewType.FolderSelector);
			tampereTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			helsinkiTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			vaasaTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			tampereTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);

			helsinkiTreeViewSection.SourceLabel.Text = "Parent folder";
			vaasaTreeViewSection.SourceLabel.Text = "Parent folder";
			tampereTreeViewSection.SourceLabel.Text = "Parent folder";

			helsinkiTreeViewSection.InitRoot();
			vaasaTreeViewSection.InitRoot();
			tampereTreeViewSection.InitRoot();

			treeViewSections.Add(InterplayPamElements.Helsinki, helsinkiTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Vaasa, vaasaTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Tampere, tampereTreeViewSection);

			if (folderCreation == null)
			{
				deadlineDateTimePicker.DateTime = DateTime.Now.AddHours(1);
				return;
			}

			nameOfTheOrderTextBox.Text = folderCreation.OrderDescription;
			deadlineDateTimePicker.DateTime = folderCreation.Deadline;

			destinationDropDown.Selected = folderCreation.Destination;

			selectedFolderContentType = EnumExtensions.GetEnumValueFromDescription<NewFolderContentTypes>(folderCreation.ContentType);
			newFolderContentTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedFolderContentType);

			if (!String.IsNullOrWhiteSpace(folderCreation.ParentFolder)) treeViewSections[Destination].Initialize(new[] { folderCreation.ParentFolder });
		}

		private void DestinationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
		}

		private void NewFolderContentTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				NewFolderContentType = EnumExtensions.GetEnumValueFromDescription<NewFolderContentTypes>(e.Selected);
				if (NewFolderContentType != NewFolderContentTypes.EPISODE)
				{
					section.NewEpisodeFolderRequestSections.Clear();
				}

				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			generalOrderInformationTitle.IsVisible = IsVisible;

			nameOfTheOrderLabel.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsEnabled = IsEnabled;

			// Hide Deadline for new Folder Creation orders
			deadlineLabel.IsVisible = IsVisible && folderCreation != null;
			deadlineDateTimePicker.IsVisible = IsVisible && folderCreation != null;
			deadlineDateTimePicker.IsEnabled = false;

			destinationLabel.IsVisible = IsVisible;
			destinationDropDown.IsVisible = IsVisible;
			destinationDropDown.IsEnabled = IsEnabled;

			newFolderContentTypeLabel.IsVisible = IsVisible;
			newFolderContentTypeDropDown.IsVisible = IsVisible;
			newFolderContentTypeDropDown.IsEnabled = IsEnabled;

			treeViewSections.Values.ForEach(tvs =>
			{
				tvs.IsVisible = IsVisible;
				tvs.IsEnabled = IsEnabled;
			});
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}
