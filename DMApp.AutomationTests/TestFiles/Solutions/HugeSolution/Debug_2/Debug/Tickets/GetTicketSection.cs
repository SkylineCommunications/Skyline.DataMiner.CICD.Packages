namespace Debug_2.Debug.Tickets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library.UI.Filters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class GetTicketSection : YleSection
	{
		private readonly Label header = new Label("Get Tickets with filters") { Style = TextStyle.Title };

		private readonly Label ticketDomainIds = new Label();

		private readonly FilterSection<Ticket> ticketIdFilterSection = new StringFilterSection<Ticket>("Ticket ID", x => TicketingExposers.FullID.Equal((String)x));

		private readonly FilterSection<Ticket> ticketCreationDateFromFilterSection = new DateTimeFilterSection<Ticket>("Creation Date From", x => TicketingExposers.CreationDate.GreaterThanOrEqual((DateTime)x));

		private readonly FilterSection<Ticket> ticketCreationDateUntilFilterSection = new DateTimeFilterSection<Ticket>("Creation Date Until", x => TicketingExposers.CreationDate.LessThanOrEqual((DateTime)x));

		private readonly FilterSection<Ticket> ticketDomainFilterSection = new GuidFilterSection<Ticket>("Ticket Domain ID", x => TicketingExposers.ResolverID.Equal((Guid)x));

		private readonly List<FilterSection<Ticket>> propertyFilterSections = new List<FilterSection<Ticket>>();

		private readonly RadioButtonList typeOfPropertyValueFilterToAdd = new RadioButtonList(new[] {"String", "Integer", "Enum"});
		private readonly Button addPropertyValueFilterButton = new Button("Add Property Value Filter");

		private readonly Button addPropertyExistenceFilterButton = new Button("Add Property Existence Filter");

		private readonly Button getSelectedTicketsButton = new Button("Get Selected Tickets") { Style = ButtonStyle.CallToAction };
		private readonly CollapseButton showSelectedTicketsButton;
		private readonly TextBox selectedTicketsTextBox = new TextBox { IsMultiline = true, MinWidth = 500 };
		
		private readonly TicketingGatewayHelper ticketingHelper;

		public GetTicketSection(Helpers helpers) : base(helpers)
		{
			ticketDomainIds.Text = $"Ticket Domain IDs:\nNon-Live Orders: {helpers.NonLiveOrderManager.TicketDomainId}\nUser Tasks: {helpers.UserTaskManager.TicketingManager.TicketFieldResolver.ID}\nNotes: {helpers.NoteManager.TicketDomainId}";

			ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Skyline.DataMiner.Automation.Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

			addPropertyExistenceFilterButton.Pressed += AddPropertyExistenceFilterButton_Pressed;

			addPropertyValueFilterButton.Pressed += AddPropertyValueFilterButton_Pressed;

			showSelectedTicketsButton = new CollapseButton(selectedTicketsTextBox.Yield(), true) { CollapseText = "Hide Selected Tickets", ExpandText = "Show Selected Tickets" };

			getSelectedTicketsButton.Pressed += (s,e) => SelectedTickets = GetSelectedTickets();

			GenerateUi();
		}

		public event EventHandler RegenerateUiRequired;

		public IEnumerable<Ticket> SelectedTickets { get; private set; } = new List<Ticket>();

		public void AddStringPropertyValueFilter(string propertyName, string propertyValue = null, bool setAsDefault = false)
		{
			var propertyFilterSection = new StringPropertyFilterSection<Ticket>("String Property", (pName, pValue) => TicketingExposers.CustomTicketFields.DictStringField((string)pName).Equal((string)pValue));

			if (setAsDefault)
			{
				propertyFilterSection.SetDefault(propertyName, propertyValue);
			}
			else
			{
				propertyFilterSection.Value = propertyName;
				propertyFilterSection.SecondValue= propertyValue ?? string.Empty;
			}

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		public void AddEnumPropertyValueFilter(string propertyName, string firstPropertyValue = null, int? secondPropertyValue = null, bool setAsDefault = false)
		{
			var propertyFilterSection = new TicketEnumFilterSection<Ticket>("Enum Property", (pName, pValue1, pValue2) => TicketingExposers.CustomTicketFields.DictField((string)pName).Equal($"{pValue1}/{pValue2}"));

			if (setAsDefault)
			{
				propertyFilterSection.SetDefault(propertyName, firstPropertyValue, secondPropertyValue);
			}
			else
			{
				propertyFilterSection.Value = propertyName;
				propertyFilterSection.SecondValue = firstPropertyValue ?? string.Empty;
				propertyFilterSection.ThirdValue = secondPropertyValue.HasValue ? secondPropertyValue.Value : 0;
			}

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		public void AddIntegerPropertyValueFilter(string propertyName, int? propertyValue = null, bool setAsDefault = false)
		{
			var propertyFilterSection = new IntegerPropertyFilterSection<Ticket>("Integer Property", (pName, pValue) => TicketingExposers.CustomTicketFields.DictField((string)pName).Equal(pValue));

			if (setAsDefault)
			{
				propertyFilterSection.SetDefault(propertyName, propertyValue);
			}
			else
			{
				propertyFilterSection.Value = propertyName;
				propertyFilterSection.SecondValue = propertyValue.HasValue ? propertyValue.Value : 0;
			}

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		public void AddPropertyExistenceFilter(string propertyName, bool setAsDefault = false)
		{
			var propertyExistenceFilterSection = new StringFilterSection<Ticket>("Property Exists", (pName) => TicketingExposers.CustomTicketFields.DictStringField((string)pName).NotEqual(string.Empty));

			if (setAsDefault)
			{
				propertyExistenceFilterSection.SetDefault(propertyName);
			}
			else
			{
				propertyExistenceFilterSection.Value = propertyName;
			}

			propertyFilterSections.Add(propertyExistenceFilterSection);

			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(header, ++row, 0, 1, 5);

			AddWidget(ticketDomainIds, ++row, 0);

			AddSection(ticketIdFilterSection, new SectionLayout(++row, 0));

			AddSection(ticketCreationDateFromFilterSection, new SectionLayout(++row, 0));

			AddSection(ticketCreationDateUntilFilterSection, new SectionLayout(++row, 0));

			AddSection(ticketDomainFilterSection, new SectionLayout(++row, 0));

			foreach (var propertyFilterSection in propertyFilterSections)
			{
				AddSection(propertyFilterSection, new SectionLayout(++row, 0));
				row += propertyFilterSection.RowCount;
			}

			AddWidget(addPropertyExistenceFilterButton, ++row, 0);
			AddWidget(addPropertyValueFilterButton, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(typeOfPropertyValueFilterToAdd, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(getSelectedTicketsButton, ++row, 0);
			AddWidget(showSelectedTicketsButton, row, 1);
			AddWidget(selectedTicketsTextBox, ++row, 1);
		}

		private void AddPropertyValueFilterButton_Pressed(object sender, EventArgs e)
		{
			switch (typeOfPropertyValueFilterToAdd.Selected)
			{
				case nameof(String):
					AddStringPropertyValueFilter(string.Empty);
					break;

				case "Integer":
					AddIntegerPropertyValueFilter(string.Empty);
					break;
				case "Enum":
					AddEnumPropertyValueFilter(string.Empty);
					break;

				default:
					break;
			}
		}

		private void AddPropertyExistenceFilterButton_Pressed(object sender, EventArgs e)
		{
			AddPropertyExistenceFilter(string.Empty);
		}

		private IEnumerable<Ticket> GetSelectedTickets()
		{
			using (UiDisabler.StartNew(this))
			{
				selectedTicketsTextBox.Text = String.Empty;
				if (!this.ActiveFiltersAreValid<Ticket>()) return new List<Ticket>();

				var selectedTickets = new HashSet<Ticket>(ticketingHelper.GetTickets(null, this.GetCombinedFilterElement<Ticket>(), false).ToList());

				selectedTicketsTextBox.Text = String.Join("\n", selectedTickets.Select(r => r.ID).OrderBy(id => id));

				return selectedTickets;
			}	
		}

		public override void RegenerateUi()
		{
			GenerateUi();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			ticketIdFilterSection.IsEnabled = IsEnabled;

			ticketCreationDateFromFilterSection.IsEnabled = IsEnabled;

			ticketCreationDateUntilFilterSection.IsEnabled = IsEnabled;

			ticketDomainFilterSection.IsEnabled = IsEnabled;

			propertyFilterSections.ForEach(f => f.IsEnabled = IsEnabled);
		}
	}
}
