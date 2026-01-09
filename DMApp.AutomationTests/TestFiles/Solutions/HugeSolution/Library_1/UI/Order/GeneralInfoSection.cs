namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;

	/// <summary>
	/// This section is used to display general order information.
	/// </summary>
	public class GeneralInfoSection : YleSection
	{
		private readonly Order order;
		private readonly GeneralInfoSectionConfiguration configuration;

		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };
		private readonly Label orderNameLabel = new Label("Order Name");
		private readonly Label orderNamePostfixLabel = new Label(String.Empty);
		private readonly Label integrationTypeLabel = new Label(String.Empty);
		private readonly Label startDateTimeLabel = new Label("Order Start Date");
		private readonly Label endDateTimeLabel = new Label("Order End Date");
		private readonly Label timeZoneLabel = new Label(TimeZoneInfo.Local.DisplayName);
		private readonly Label plasmaIdLabel = new Label("Plasma ID");
		private readonly Label yleIdLabel = new Label("YLE ID");
		private readonly Label startNowLabel = new Label("The order will start as soon as all user tasks have been completed");
		private readonly Label billableCompanyLabel = new Label("Billable Company");
		private readonly Label customerCompanyLabel = new Label("Customer Company");
		private readonly Label userGroupLabel = new Label("User Group") { IsVisible = false };

		[DisplaysProperty(nameof(Order.ManualName))]
		private readonly YleTextBox orderNameTextBox = new YleTextBox(string.Empty) { Name = nameof(orderNameTextBox) };

		[DisplaysProperty(nameof(Order.Start))]
		private readonly YleDateTimePicker startDateTimePicker = new YleDateTimePicker { Name = nameof(startDateTimePicker) };

		[DisplaysProperty(nameof(Order.End))]
		private readonly YleDateTimePicker endDateTimePicker = new YleDateTimePicker { Name = nameof(endDateTimePicker) };

		private readonly YleCheckBox adjustOrderDetailsCheckBox = new YleCheckBox("Adjust Order Details") { IsChecked = false, Name = nameof(adjustOrderDetailsCheckBox) };

		[DisplaysProperty(nameof(Order.StartNow))]
		private readonly YleCheckBox startNowCheckBox = new YleCheckBox("Start Now") { IsChecked = false, Name = nameof(startNowCheckBox) };

		private readonly DropDown userGroupDropDown = new DropDown(); // TODO: verify that this should be a dropdown and not a checkbox list. Checking the logic it seems like the order is by default part of all usergroups of which the user is part of. Any selected user groups should be added to this list.

		[DisplaysProperty(nameof(Order.PlasmaId))]
		private readonly YleTextBox plasmaIdTextBox = new YleTextBox() { Name = nameof(plasmaIdTextBox) };

		[DisplaysProperty(nameof(Order.YleId))]
		private readonly YleTextBox yleIdTextBox = new YleTextBox() { Name = nameof(yleIdTextBox) };

		[DisplaysProperty(nameof(Order.BillingInfo.BillableCompany))]
		private readonly YleDropDown billableCompanyDropDown = new YleDropDown { Name = nameof(billableCompanyDropDown) };

		[DisplaysProperty(nameof(Order.BillingInfo.CustomerCompany))]
		private readonly YleTextBox customerCompanyTextBox = new YleTextBox() { Name = nameof(customerCompanyTextBox) };

		[DisplaysProperty(nameof(Order.SecurityViewIds))]
		private VisibilityRightsCheckBoxes visibilityRightsCheckBoxes;

		public GeneralInfoSection(Helpers helpers, Order order, GeneralInfoSectionConfiguration configuration) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			Initialize();
			RegenerateUi();
		}

		public RecurrenceSection RecurrenceSection { get; set; }

		public string UserGroup => userGroupDropDown.Selected;

		public event EventHandler<string> NameFocusLost;

		public event EventHandler<DateTime> StartChanged;

		public event EventHandler<bool> StartNowChanged;

		public event EventHandler<DateTime> EndChanged;

		public event EventHandler<string> UserGroupChanged;

		public event EventHandler<IEnumerable<int>> SecurityViewIdsChanged;

		public event EventHandler<string> BillableCompanyChanged;

		public event EventHandler<string> CustomerCompanyChanged;

		public event EventHandler<string> PlasmaIdChanged;

		public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

		public event EventHandler RegenerateDialog;

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToOrder();
		}

		private void InitializeWidgets()
		{
			orderNameTextBox.Text = order.ManualName;
			orderNamePostfixLabel.Text = order.NamePostFix;
			integrationTypeLabel.Text = $"Automatically created by {order.IntegrationType.GetDescription()}";
			startDateTimePicker.DateTime = order.Start.ToLocalTime();
			endDateTimePicker.DateTime = order.End.ToLocalTime();
			timeZoneLabel.Text = TimeZoneInfo.Local.DisplayName;
			startNowCheckBox.IsChecked = order.StartNow;
			plasmaIdTextBox.Text = order.PlasmaId;
			yleIdTextBox.Text = order.YleId;
			customerCompanyTextBox.Text = order.BillingInfo?.CustomerCompany;
			RecurrenceSection = new RecurrenceSection(order.RecurringSequenceInfo);

			UpdateUserGroupWidgets();
			UpdateVisibilityRightsWidgets();
			UpdateBillableWidgets();
		}

		private void UpdateUserGroupWidgets()
		{
			userGroupDropDown.Options = order.SelectableUserGroups.Select(u => u.Name).OrderBy(u => u);
			var selectedUserGroup = order.SelectableUserGroups.FirstOrDefault(x => order.UserGroupIds.Contains(Convert.ToInt32(x.ID)));
			if (selectedUserGroup == null) return;
			userGroupDropDown.Selected = selectedUserGroup.Name;
		}

		private void UpdateBillableWidgets()
		{
			billableCompanyDropDown.Options = order.SelectableCompanies;
			billableCompanyDropDown.Selected = order.BillingInfo?.BillableCompany ?? order.SelectableCompanies.FirstOrDefault();
		}

		private void UpdateVisibilityRightsWidgets()
		{
			visibilityRightsCheckBoxes = new VisibilityRightsCheckBoxes(configuration.UserInfo, configuration.PermanentlySelectedCompanies.ToList()) { SelectedViewIds = new HashSet<int>(order.SecurityViewIds), CheckBoxesColumn = 4, CheckBoxesColumnSpan = 3, VisibleViewIds = configuration.VisibleViewIds };

			visibilityRightsCheckBoxes.SelectedCompaniesChanged += (s, selectedSecurityViewIds) =>
			{
				SecurityViewIdsChanged?.Invoke(this, selectedSecurityViewIds.Values);
			};

			RegenerateDialog?.Invoke(this, new EventArgs());
		}

		private void SubscribeToWidgets()
		{
			startDateTimePicker.Changed += (s, e) =>
			{
				StartChanged?.Invoke(this, (DateTime)e.Value);
				RegenerateDialog?.Invoke(this, new EventArgs());
			};

			endDateTimePicker.Changed += (s, e) =>
			{
				EndChanged?.Invoke(this, (DateTime)e.Value);
				RegenerateDialog?.Invoke(this, new EventArgs());
			};

			orderNameTextBox.FocusLost += (s, e) => NameFocusLost?.Invoke(this, orderNameTextBox.Text);
			adjustOrderDetailsCheckBox.Changed += (s, e) => HandleVisibilityAndEnabledUpdate();
			userGroupDropDown.Changed += (s, e) => UserGroupChanged?.Invoke(this, e.Selected);

			startNowCheckBox.Changed += (s, e) =>
			{
				StartNowChanged?.Invoke(this, (bool)e.Value);
				HandleVisibilityAndEnabledUpdate();
			};

			plasmaIdTextBox.Changed += (s, e) => PlasmaIdChanged?.Invoke(this, Convert.ToString(e.Value));

			yleIdTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(yleIdTextBox)), e.Value));

			billableCompanyDropDown.Changed += (s, e) => BillableCompanyChanged?.Invoke(this, (string)e.Value);

			customerCompanyTextBox.Changed += (s, e) => CustomerCompanyChanged?.Invoke(this, Convert.ToString(e.Value));

			visibilityRightsCheckBoxes.SelectedCompaniesChanged += (s, selectedSecurityViewIds) =>
			{
				SecurityViewIdsChanged?.Invoke(this, selectedSecurityViewIds.Values);
			};
		}

		private void SubscribeToOrder()
		{
			order.NamePostFixChanged += (s, postfix) => orderNamePostfixLabel.Text = postfix;

			order.StartChanged += (s, e) => startDateTimePicker.DateTime = e;
			order.EndChanged += (s, e) => endDateTimePicker.DateTime = e;

			order.SelectableCompanies.CollectionChanged += (s, a) => UpdateBillableWidgets();
			order.BillingInfo.BillableCompanyChanged += (s, a) => UpdateBillableWidgets();
			order.SelectableSecurityViewIdsChanged += (s, a) => UpdateVisibilityRightsWidgets();
			order.SelectableUserGroups.CollectionChanged += (s, a) => UpdateUserGroupWidgets();
			order.UserGroupIds.CollectionChanged += (s, a) => UpdateUserGroupWidgets();
			order.SecurityViewIds.CollectionChanged += (s, a) => UpdateVisibilityRightsWidgets();

			this.SubscribeToDisplayedObjectValidation(order);
		}

		protected override void GenerateUi(out int row)
		{
			row = -1;
			AddWidget(generalOrderInformationTitle, ++row, configuration.LabelColumn, 1, 7);

			AddWidget(adjustOrderDetailsCheckBox, ++row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(orderNameLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(orderNameTextBox, row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(orderNamePostfixLabel, new WidgetLayout(row, 7, HorizontalAlignment.Left, VerticalAlignment.Center));

			AddWidget(integrationTypeLabel, ++row, configuration.InputColumn, 1, configuration.InputSpan); // display if integration type is not None

			AddWidget(startDateTimeLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(startDateTimePicker, row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(startNowCheckBox, row, 7, 1, 5);
			AddWidget(startNowLabel, ++row, configuration.InputColumn, 1, configuration.InputSpan);

			AddWidget(endDateTimeLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(endDateTimePicker, row, configuration.InputColumn, 1, configuration.InputSpan);

			AddWidget(timeZoneLabel, ++row, configuration.InputColumn, 1, configuration.InputSpan);

			AddSection(RecurrenceSection, new SectionLayout(++row, configuration.InputColumn));
			row += RecurrenceSection.RowCount;

			AddWidget(userGroupLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan); // MCR only
			AddWidget(userGroupDropDown, row, configuration.InputColumn, 1, configuration.InputSpan); // MCR only

			AddWidget(plasmaIdLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan); // Internal Users only
			AddWidget(plasmaIdTextBox, row, configuration.InputColumn, 1, configuration.InputSpan); // Internal Users only

			AddWidget(yleIdLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan); // Internal Users only
			AddWidget(yleIdTextBox, row, configuration.InputColumn, 1, configuration.InputSpan); // Internal Users only

			if (order.Subtype == OrderSubType.Normal)
			{
				// Setting IsVisible property to false does not work for some unknown reason
				// So not adding the section is the only way to 'hide' it

				AddSection(visibilityRightsCheckBoxes, new SectionLayout(++row, configuration.LabelColumn));
				row += visibilityRightsCheckBoxes.RowCount;
			}

			AddWidget(billableCompanyLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(billableCompanyDropDown, row, configuration.InputColumn, 1, configuration.InputSpan);

			AddWidget(customerCompanyLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(customerCompanyTextBox, row, configuration.InputColumn, 1, configuration.InputSpan);
			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		public override void RegenerateUi()
		{
			Clear();
			GenerateUi(out int _);
			HandleVisibilityAndEnabledUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		protected override void HandleVisibilityAndEnabledUpdate()
		{
			adjustOrderDetailsCheckBox.IsVisible = IsVisible && configuration.EditOrderInformationIsVisible;
			adjustOrderDetailsCheckBox.IsEnabled = IsEnabled && configuration.EditOrderInformationIsEnabled;

			orderNameLabel.IsVisible = IsVisible && configuration.OrderNameIsVisible;
			orderNameTextBox.IsVisible = IsVisible && configuration.OrderNameIsVisible;
			orderNameTextBox.IsEnabled = IsEnabled && configuration.OrderNameIsEnabled;
			orderNamePostfixLabel.IsVisible = IsVisible && configuration.OrderNameIsVisible;

			integrationTypeLabel.IsVisible = IsVisible && configuration.IntegrationTypeIsVisible;

			startDateTimePicker.IsVisible = IsVisible && configuration.StartDateIsVisible;
			startDateTimePicker.IsEnabled = IsEnabled && configuration.StartDateIsEnabled && !startNowCheckBox.IsChecked;

			startNowCheckBox.IsVisible = IsVisible && configuration.StartNowIsVisible;
			startNowCheckBox.IsEnabled = IsEnabled && configuration.StartNowIsEnabled;

			startNowLabel.IsVisible = IsVisible && configuration.StartDateIsVisible && startNowCheckBox.IsChecked;

			endDateTimePicker.IsVisible = IsVisible && configuration.EndDateIsVisible;
			endDateTimePicker.IsEnabled = IsEnabled && configuration.EndDateIsEnabled;

			timeZoneLabel.IsVisible = IsVisible && configuration.TimeZoneIsVisible && adjustOrderDetailsCheckBox.IsChecked;

			RecurrenceSection.IsVisible = IsVisible && configuration.RecurrenceIsVisible;
			RecurrenceSection.IsEnabled = IsEnabled && configuration.RecurrenceIsEnabled;

			userGroupLabel.IsVisible = IsVisible && configuration.UserGroupIsVisible;
			userGroupDropDown.IsVisible = IsVisible && configuration.UserGroupIsVisible;
			userGroupDropDown.IsEnabled = IsEnabled && configuration.UserGroupIsEnabled;

			plasmaIdLabel.IsVisible = IsVisible && configuration.PlasmaIdIsVisible && adjustOrderDetailsCheckBox.IsChecked;
			plasmaIdTextBox.IsVisible = IsVisible && configuration.PlasmaIdIsVisible && adjustOrderDetailsCheckBox.IsChecked;
			plasmaIdTextBox.IsEnabled = IsEnabled && configuration.PlasmaIdIsEnabled;

			yleIdLabel.IsVisible = IsVisible && configuration.YleIdIsVisible;
			yleIdTextBox.IsVisible = IsVisible && configuration.YleIdIsVisible;
			yleIdTextBox.IsEnabled = IsEnabled && configuration.YleIdIsEnabled;

			visibilityRightsCheckBoxes.IsVisible = IsVisible && configuration.VisibilityRightsAreVisible;
			visibilityRightsCheckBoxes.IsEnabled = IsEnabled && configuration.VisibilityRightsAreEnabled;

			bool companyOtherThanYleOrMcrSelected = visibilityRightsCheckBoxes.SelectedCompanies.Any(company => company != "YLE" && company != "MCR");
			billableCompanyLabel.IsVisible = IsVisible && (configuration.BillableCompanyIsVisible || companyOtherThanYleOrMcrSelected);
			billableCompanyDropDown.IsVisible = IsVisible && (configuration.BillableCompanyIsVisible || companyOtherThanYleOrMcrSelected);
			billableCompanyDropDown.IsEnabled = IsEnabled && configuration.BillableCompanyIsEnabled;

			customerCompanyLabel.IsVisible = IsVisible && (configuration.CustomerCompanyIsVisible || companyOtherThanYleOrMcrSelected);
			customerCompanyTextBox.IsVisible = IsVisible && (configuration.CustomerCompanyIsVisible || companyOtherThanYleOrMcrSelected);
			customerCompanyTextBox.IsEnabled = IsEnabled && configuration.CustomerCompanyIsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
