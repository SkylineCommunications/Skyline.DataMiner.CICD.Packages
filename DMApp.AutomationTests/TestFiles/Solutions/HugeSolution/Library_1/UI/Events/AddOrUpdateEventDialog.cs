using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Utils.YLE.Integrations;

    public class AddOrUpdateEventDialog : Dialog
	{
		private const string None = "None";

		private readonly Label nameLabel = new Label("Name");

		private readonly Label startTimeLabel = new Label("Start Time");

		private readonly Label endTimeLabel = new Label("End Time");

		private readonly Label internalLabel = new Label("Internal");

		private readonly Label projectNumberLabel = new Label("Project Number");

		private readonly Label productNumberLabel = new Label("Product Number");

		private readonly Label infoLabel = new Label("Info");

		private readonly Label companyLabel = new Label("Customer");

		private readonly Label contractLabel = new Label("Contract");

		private readonly Label operatorNotesLabel = new Label("Operator Notes");

		private readonly Label formValidationLabel = new Label("Please complete all required fields") { IsVisible = false };

		private readonly Event retrievedEvent;

		private readonly Helpers helpers;
		private readonly UserInfo userInfo;


		private readonly LockInfo lockInfo;

		private bool isReadOnly = false;

		public bool NewEvent => retrievedEvent == null || retrievedEvent.Id == Guid.Empty;

        public AddOrUpdateEventDialog(Helpers helpers, UserInfo userInfo, Event retrievedEvent = null, LockInfo lockInfo = null)
			: base((Engine)helpers.Engine)
		{
			if (retrievedEvent == null ^ lockInfo == null)
			{
				if (retrievedEvent == null) throw new ArgumentNullException(nameof(retrievedEvent));
				throw new ArgumentNullException(nameof(lockInfo));
			}

			this.helpers = helpers;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.lockInfo = lockInfo;
			this.retrievedEvent = retrievedEvent;

			InitializeWidgets();
			InitializeValues();
            SetVisibilityAndEnabledStatus();
            GenerateUI();
            UpdateValidity();

            if (lockInfo != null) IsReadonly = !lockInfo.IsLockGranted;
        }

        private YleTextBox NameTextBox { get; set; }

		private DateTimePicker StartDateTimePicker { get; set; }

		private DateTimePicker EndDateTimePicker { get; set; }

		private CheckBox IsInternalCheckBox { get; set; }

		private YleTextBox ProjectNumberTextBox { get; set; }

		private YleTextBox ProductNumberTextBox { get; set; }

		private YleTextBox InfoTextBox { get; set; }

		private DropDown CompanyDropDown { get; set; }

		private DropDown ContractDropDown { get; set; }

		private YleTextBox OperatorNotesTextBox { get; set; }

		private VisibilityRightsCheckBoxes VisibilityRightsCheckBoxes { get; set; }

		public Button SaveButton { get; private set; }

		public Button SavePreliminaryEventButton { get; private set; }

		public Button SavePlannedEventButton { get; private set; }

		public Button CancelButton { get; private set; }

		public Button SaveAsTemplateButton { get; private set; }

		public bool IsReadonly
		{
			get => isReadOnly;

			set
			{
				if (IsReadonly != value)
				{
					isReadOnly = value;

					InteractiveWidget interactiveWidget;
					foreach (var widget in Widgets)
					{
						interactiveWidget = widget as InteractiveWidget;
						if (interactiveWidget != null && !(interactiveWidget is CollapseButton))
						{
							interactiveWidget.IsEnabled = !IsReadonly;
						}
					}
				}
			}
		}

        public Event GetUpdatedEvent()
        {
            if (NewEvent)
            {
                DateTime start = StartDateTimePicker.DateTime;
                DateTime end = EndDateTimePicker.DateTime;

                return new Event(helpers)
                {
                    Name = NameTextBox.Text,
                    Start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0),
                    End = new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, 0),
                    IsInternal = IsInternalCheckBox.IsChecked,
                    Info = InfoTextBox.Text,
                    Contract = ContractDropDown.Selected,
                    Company = CompanyDropDown.Selected,
                    ProjectNumber = ProjectNumberTextBox.Text,
                    IntegrationType = IntegrationType.None,
                    OperatorNotes = OperatorNotesTextBox.Text,
                    SecurityViewIds = new HashSet<int>(VisibilityRightsCheckBoxes.SelectedViewIds),
                    CompanyOfCreator = userInfo.Contract.Company
                };
            }
            else
            {
                retrievedEvent.Name = NameTextBox.Text;
                retrievedEvent.Start = StartDateTimePicker.DateTime;
                retrievedEvent.End = EndDateTimePicker.DateTime;
                retrievedEvent.IsInternal = IsInternalCheckBox.IsChecked;
                retrievedEvent.ProjectNumber = ProjectNumberTextBox.Text;
                retrievedEvent.ProductNumbers = ProductNumberTextBox.Text.Split('/');
                retrievedEvent.Info = InfoTextBox.Text;
                retrievedEvent.Company = CompanyDropDown.Selected;
                retrievedEvent.Contract = ContractDropDown.Selected;
                retrievedEvent.OperatorNotes = OperatorNotesTextBox.Text;
                retrievedEvent.SecurityViewIds = new HashSet<int>(VisibilityRightsCheckBoxes.SelectedViewIds);

                return retrievedEvent;
            }
        }

        public bool IsValid()
		{
			bool isNameValid = ValidateName();

			bool isProjectNumberValid = String.IsNullOrWhiteSpace(ProjectNumberTextBox.Text) || (retrievedEvent != null && ProjectNumberTextBox.Text.Trim().Equals(retrievedEvent.ProjectNumber)) || (helpers.EventManager.GetEvent(ProjectNumberTextBox.Text.Trim()) == null);
			bool isContractValid = !ContractDropDown.Selected.Equals(None);
			bool isEndTimeLaterThenStartTime = StartDateTimePicker.DateTime < EndDateTimePicker.DateTime;
			bool isEndTimeInTheFuture = EndDateTimePicker.DateTime > DateTime.Now;
			bool shortenEndTime = retrievedEvent != null && EndDateTimePicker.DateTime < retrievedEvent.End;


			StartDateTimePicker.ValidationState = isEndTimeLaterThenStartTime ? UIValidationState.Valid : UIValidationState.Invalid;
			StartDateTimePicker.ValidationText = "Start time cannot be later than end time";

			string endTimeValidationText = String.Empty;
			if (!isEndTimeInTheFuture) endTimeValidationText = "The end time cannot be in the past";
			if (!isEndTimeLaterThenStartTime) endTimeValidationText = "Start time cannot be later than end time";
			if (shortenEndTime) endTimeValidationText = "It is not possible to make an event shorter";
			EndDateTimePicker.ValidationState = (isEndTimeLaterThenStartTime && isEndTimeInTheFuture && !shortenEndTime) ? UIValidationState.Valid : UIValidationState.Invalid;
			EndDateTimePicker.ValidationText = endTimeValidationText;

			ProjectNumberTextBox.ValidationState = isProjectNumberValid ? UIValidationState.Valid : UIValidationState.Invalid;
			ProjectNumberTextBox.ValidationText = "The project number must be unique";

			ContractDropDown.ValidationState = isContractValid ? UIValidationState.Valid : UIValidationState.Invalid;
			ContractDropDown.ValidationText = "Select a valid contract";

			bool isOperatorNotesValid = OperatorNotesTextBox.Text.Length <= Constants.MaximumAllowedCharacters;
			OperatorNotesTextBox.ValidationState = isOperatorNotesValid ? UIValidationState.Valid : UIValidationState.Invalid;
			OperatorNotesTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

			bool isInfoTextBoxValid = InfoTextBox.Text.Length <= Constants.MaximumAllowedCharacters;
			InfoTextBox.ValidationState = isInfoTextBoxValid ? UIValidationState.Valid : UIValidationState.Invalid;
			InfoTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

			bool isFormValid = isNameValid && isProjectNumberValid
								&& isContractValid && isEndTimeLaterThenStartTime && isEndTimeInTheFuture && isOperatorNotesValid && isInfoTextBoxValid;

			formValidationLabel.IsVisible = !isFormValid;

			return isFormValid;
		}

		private bool ValidateName()
		{
			bool nameContainsIllegalCharacters = !String.IsNullOrWhiteSpace(NameTextBox.Text) && NameTextBox.Text.ContainsIllegalCharacters();

			bool isNameValid = !String.IsNullOrWhiteSpace(NameTextBox.Text) && !nameContainsIllegalCharacters;
			bool isNameUnique = (retrievedEvent != null && NameTextBox.Text.Trim().Equals(retrievedEvent.Name)) || (helpers.EventManager.GetEventByName(NameTextBox.Text.Trim()) == null);
			string nameValidationText = String.Empty;
			if (!isNameValid) nameValidationText = nameContainsIllegalCharacters ? "Provide an event name without illegal characters like ('/', '?', etc)" : "Provide an event name";
			if (!isNameUnique) nameValidationText = "The event name must be unique";
			NameTextBox.ValidationState = (isNameValid && isNameUnique) ? UIValidationState.Valid : UIValidationState.Invalid;
			NameTextBox.ValidationText = nameValidationText;

			return isNameValid && isNameUnique;
		}

		private void StartTiming_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
		{
			if (EndDateTimePicker.DateTime < StartDateTimePicker.DateTime)
			{
				EndDateTimePicker.DateTime = StartDateTimePicker.DateTime;
			}

			UpdateContractDropDownOptions();
			UpdateValidity();
		}

        private void EndTiming_Changed(object sender, DateTimePicker.DateTimePickerChangedEventArgs e)
        {
	        if (EndDateTimePicker.DateTime < StartDateTimePicker.DateTime)
	        {
		        StartDateTimePicker.DateTime = EndDateTimePicker.DateTime;
	        }

	        UpdateContractDropDownOptions();
	        UpdateValidity();
        }

		private void CompanyDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			UpdateContractDropDownOptions(true);

			// Get selected contract
			Contract selectedContract = userInfo.AllContracts.FirstOrDefault(x => x.Name == ContractDropDown.Selected);

			// If the company for the contract has linked companies, check these ones by default in the visibility rights list
			if (selectedContract != null)
			{
				List<string> selectedCompanies = VisibilityRightsCheckBoxes.SelectedCompanies.ToList();
				foreach (Company company in selectedContract.LinkedCompanies)
				{
					if (!selectedCompanies.Contains(company.Name)) selectedCompanies.Add(company.Name);
				}

				VisibilityRightsCheckBoxes.SelectedCompanies = selectedCompanies;
			}

			UpdateValidity();
		}

        private void UpdateValidity()
        {
            IsValid();
            formValidationLabel.IsVisible = false;
        }

        private void InitializeWidgets()
        {
            NameTextBox = new YleTextBox(string.Empty) { PlaceHolder = "Name of the Event" };

            StartDateTimePicker = new DateTimePicker() { IsEnabled = NewEvent || !retrievedEvent.OrderIds.Any() };
            EndDateTimePicker = new DateTimePicker();
            if (retrievedEvent == null)
            {
                StartDateTimePicker.DateTime = DateTime.Now.AddMinutes(30);
                EndDateTimePicker.DateTime = DateTime.Now.AddMinutes(60);
            }

            IsInternalCheckBox = new CheckBox();

            ProjectNumberTextBox = new YleTextBox { PlaceHolder = "Number of optional project", IsEnabled = retrievedEvent?.Status != Status.Ongoing };

            ProductNumberTextBox = new YleTextBox { IsEnabled = false };

            InfoTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, PlaceHolder = "Additional information" };

            OperatorNotesTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, PlaceHolder = "Notes for MCR operators" };

            InitializeVisibilityRightsCheckBoxes();

            CompanyDropDown = new DropDown { IsEnabled = retrievedEvent == null || !retrievedEvent.OrderIds.Any() };
            ContractDropDown = new DropDown { IsEnabled = retrievedEvent == null || !retrievedEvent.OrderIds.Any() };
            InitCompanyAndContractDropDown();

            SaveButton = new Button("Save Changes") { Style = ButtonStyle.CallToAction };
            SavePreliminaryEventButton = new Button("Save Preliminary Event");
            SavePlannedEventButton = new Button("Save Planned Event") { Style = ButtonStyle.CallToAction };
            CancelButton = new Button("Cancel Event");
            SaveAsTemplateButton = new Button("Save As Template");

            StartDateTimePicker.Changed += StartTiming_Changed;
            EndDateTimePicker.Changed += EndTiming_Changed;
            CompanyDropDown.Changed += CompanyDropDown_Changed;
        }

        private void InitializeVisibilityRightsCheckBoxes()
        {
            var selectedAndDisabledCompanies = new List<string> { "MCR" };
            if (!NewEvent)
            {
                selectedAndDisabledCompanies.Add(retrievedEvent?.CompanyOfCreator);
            }
            else
            {
                selectedAndDisabledCompanies.AddRange(userInfo.UserGroups.Select(userGroup => userGroup.Company).ToList());
            }

            helpers?.Log(nameof(AddOrUpdateEventDialog), nameof(InitializeVisibilityRightsCheckBoxes), $"Found default selected and disabled companies: '{string.Join(";", selectedAndDisabledCompanies)}' for user: {userInfo.User?.Name}");

            VisibilityRightsCheckBoxes = new VisibilityRightsCheckBoxes(userInfo, selectedAndDisabledCompanies) { CheckBoxesColumn = 1 };
        }

        private void SetVisibilityAndEnabledStatus()
        {
            IsInternalCheckBox.IsVisible = userInfo.IsMcrUser;
            internalLabel.IsVisible = IsInternalCheckBox.IsVisible;

            ProjectNumberTextBox.IsVisible = userInfo.IsInternalUser;
            projectNumberLabel.IsVisible = ProjectNumberTextBox.IsVisible;

            ProductNumberTextBox.IsVisible = retrievedEvent != null && userInfo.IsInternalUser;
            productNumberLabel.IsVisible = ProductNumberTextBox.IsVisible;

            OperatorNotesTextBox.IsVisible = userInfo.IsMcrUser;
            operatorNotesLabel.IsVisible = OperatorNotesTextBox.IsVisible;

            CompanyDropDown.IsVisible = userInfo.IsMcrUser;
            companyLabel.IsVisible = CompanyDropDown.IsVisible;
        }

        private void InitializeValues()
        {
            if (retrievedEvent == null) return;

            NameTextBox.Text = retrievedEvent.Name;
            StartDateTimePicker.DateTime = retrievedEvent.Start;
            EndDateTimePicker.DateTime = retrievedEvent.End;
			if (retrievedEvent.OrderIds.Any()) EndDateTimePicker.Minimum = retrievedEvent.End;
            IsInternalCheckBox.IsChecked = retrievedEvent.IsInternal;
            ProjectNumberTextBox.Text = retrievedEvent.ProjectNumber;
            ProductNumberTextBox.Text = String.Join("/", retrievedEvent.ProductNumbers);
            InfoTextBox.Text = retrievedEvent.Info;
            CompanyDropDown.Selected = retrievedEvent.Company;
            ContractDropDown.Selected = String.IsNullOrWhiteSpace(retrievedEvent.Contract) ? None : retrievedEvent.Contract;
            OperatorNotesTextBox.Text = retrievedEvent.OperatorNotes;
            VisibilityRightsCheckBoxes.SelectedViewIds = retrievedEvent.SecurityViewIds;
        }

        private void InitCompanyAndContractDropDown()
		{
			IEnumerable<string> companyOptions = userInfo.AllContracts.Select(x => x.Company).Distinct().OrderBy(x => x);
			string selectedCompany;

			if (NewEvent)
			{
				// New -> Select company for user that has a valid base contract
				IEnumerable<string> userCompanies = userInfo.UserGroups.Select(x => x.Company).Distinct();
				selectedCompany = userCompanies.FirstOrDefault();

				Company[] linkedCompanies = new Company[0];
				foreach (Contract contract in userInfo.AllUserContracts.Where(x => userCompanies.Contains(x.Company)))
				{
					if (contract.Status == ContractStatus.Open && contract.Start < StartDateTimePicker.DateTime && contract.End > EndDateTimePicker.DateTime)
					{
						selectedCompany = contract.Company;
						linkedCompanies = contract.LinkedCompanies;
						break;
					}
				}

				// If the company for the contract has linked companies, check these ones by default in the visibility rights list
				List<string> selectedCompanies = VisibilityRightsCheckBoxes.SelectedCompanies.ToList();
				foreach (Company company in linkedCompanies)
				{
					if (!selectedCompanies.Contains(company.Name)) selectedCompanies.Add(company.Name);
				}

				VisibilityRightsCheckBoxes.SelectedCompanies = selectedCompanies;
			}
			else
			{
				// Edit -> set company and contract from existing event
				selectedCompany = retrievedEvent.Company;
			}

			CompanyDropDown.Options = companyOptions;
			CompanyDropDown.Selected = selectedCompany;

			UpdateContractDropDownOptions(true);
		}

        private void UpdateContractDropDownOptions(bool selectBaseContract = false)
		{
			List<Contract> validContracts = userInfo.AllContracts.Where(x => x.Company == CompanyDropDown.Selected && x.Status == ContractStatus.Open && x.Start < StartDateTimePicker.DateTime && x.End > EndDateTimePicker.DateTime).ToList();
			if (!validContracts.Any())
			{
				ContractDropDown.Options = new string[] { None };
			}
			else
			{
				ContractDropDown.Options = validContracts.Select(x => x.Name).OrderBy(x => x).ToList();

				if (selectBaseContract)
				{
					Contract baseContract = validContracts.FirstOrDefault(x => x.Type == ContractType.BaseContract);
					if (baseContract != null) ContractDropDown.Selected = baseContract.Name;
				}
			}
		}

		private void GenerateUI()
		{
			Title = (NewEvent) ? "New Event" : "Edit Event";
			int row = -1;

			if (lockInfo != null && !lockInfo.IsLockGranted)
			{
				AddWidget(new Label(String.Format("Unable to edit Event as it is currently locked by user {0}", lockInfo.LockUsername)), ++row, 0, 1, 2);
			}

			AddWidget(nameLabel, ++row, 0);
			AddWidget(NameTextBox, row, 1);

			AddWidget(startTimeLabel, ++row, 0);
			AddWidget(StartDateTimePicker, row, 1);

			AddWidget(endTimeLabel, ++row, 0);
			AddWidget(EndDateTimePicker, row, 1);

			AddWidget(internalLabel, ++row, 0);
			AddWidget(IsInternalCheckBox, row, 1);

			AddWidget(projectNumberLabel, ++row, 0);
			AddWidget(ProjectNumberTextBox, row, 1);

			AddWidget(productNumberLabel, ++row, 0);
			AddWidget(ProductNumberTextBox, row, 1);

			AddWidget(infoLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(InfoTextBox, row, 1);

			AddSection(VisibilityRightsCheckBoxes, new SectionLayout(++row, 0));
			row += VisibilityRightsCheckBoxes.RowCount;

			AddWidget(companyLabel, ++row, 0);
			AddWidget(CompanyDropDown, row, 1);

			AddWidget(contractLabel, ++row, 0);
			AddWidget(ContractDropDown, row, 1);

			AddWidget(operatorNotesLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(OperatorNotesTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			if (userInfo.CanConfigureTemplate && (NewEvent || retrievedEvent.IntegrationType == IntegrationType.None))
			{
				AddWidget(SaveAsTemplateButton, ++row, 1);
				AddWidget(new WhiteSpace(), ++row, 0);
			}

			if (!NewEvent)
			{
				AddWidget(SaveButton, ++row, 1);
				AddWidget(CancelButton, ++row, 1);
			}
			else
			{
				AddWidget(SavePreliminaryEventButton, ++row, 1);
				AddWidget(SavePlannedEventButton, ++row, 1);
			}

			AddWidget(formValidationLabel, ++row, 1, 1, 2);
		}
	}
}