namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using System.Collections.ObjectModel;
	using Utilities;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

    public class AddEventTemplateDialog : Dialog
	{
		private readonly Dictionary<string, string> FullUserGroups = new Dictionary<string, string>();

		// Used during edit of Event Template
		private List<OrderTemplate> linkedOrderTemplates = new List<OrderTemplate>(); // List of linked Order Templates
		private ObservableCollection<OrderTemplate> availableLinkableOrderTemplates = new ObservableCollection<OrderTemplate>(); // List of Linkable Order Templates that are not added to the Event Template
		
		private readonly List<LinkOrderTemplateSection> linkedOrderTemplateSections = new List<LinkOrderTemplateSection>();
		private readonly Helpers helpers;

		public AddEventTemplateDialog(Helpers helpers, Event @event, UserInfo userInfo) : base(helpers.Engine)
		{
			InitFullUserGroups(userInfo);

			Title = "Add Template";
			this.helpers = helpers;
			Event = @event;

			TemplateNameTextBox = new YleTextBox($"{@event.Name}_Template") { Width = 250 };
			UserGroupsCheckBoxList = new CheckBoxList(FullUserGroups.Keys) { IsSorted = true };
			LinkedOrderTemplatesCheckBoxList = new CheckBoxList { IsSorted = true };
			AddOrderTemplateButton = new Button("Add Order Template");
			SaveTemplateButton = new Button("Save Template") { Style = ButtonStyle.CallToAction };
			BackButton = new Button("Back");

			AddOrderTemplateButton.Pressed += (s, a) => AddOrderTemplate();

			GenerateUI();
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;
			int linkedOrderTemplateSectionDepth = LinkedOrderTemplateSectionsDepth;
			int valueColumn = 2 + linkedOrderTemplateSectionDepth;

			AddWidget(new Label("Template Name"), ++row, 0, 1, valueColumn);
			AddWidget(TemplateNameTextBox, row, valueColumn);

			AddWidget(new Label("User Groups"), ++row, 0, 1, valueColumn, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(UserGroupsCheckBoxList, row, valueColumn);

			if (LinkedOrderTemplatesCheckBoxList.Options.Any())
			{
				AddWidget(new Label("Linked Order Templates"), ++row, 0, 1, valueColumn, HorizontalAlignment.Left, VerticalAlignment.Top);
				AddWidget(LinkedOrderTemplatesCheckBoxList, row, valueColumn);
			}

			if (linkedOrderTemplateSections.Any()) AddWidget(new Label("Order Templates to Add") { Style = TextStyle.Bold }, ++row, 0, 1, valueColumn);

			foreach (var linkedOrderTemplateSection in linkedOrderTemplateSections)
			{
				linkedOrderTemplateSection.GenerateUI(linkedOrderTemplateSectionDepth, valueColumn);
				AddSection(linkedOrderTemplateSection, ++row, 0);
				row += linkedOrderTemplateSection.RowCount;
			}

			if (availableLinkableOrderTemplates.Any()) AddWidget(AddOrderTemplateButton, ++row, 0, 1, valueColumn);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(SaveTemplateButton, ++row, 0, 1, valueColumn);
			AddWidget(BackButton, row + 1, 0, 1, valueColumn);

			if (linkedOrderTemplateSections.Any() && linkedOrderTemplateSectionDepth > 0)
			{
				SetColumnWidth(0, 40);
				for (int i = 1; i <= linkedOrderTemplateSectionDepth; i++) SetColumnWidth(0, 25);
			}
		}

		public void Init(EventTemplate template, List<OrderTemplate> linkedOrderTemplates, UserInfo userInfo)
		{
			EventTemplate = template;
			TemplateName = template.Name;

			Title = "Edit Template";
			
			// Check UserGroups
			string group;
			foreach (UserGroup userGroup in userInfo.AllUserGroups.Where(x => !String.IsNullOrWhiteSpace(x.Company)))
			{
				if (!userGroup.EventTemplates.Contains(TemplateName)) continue;

				string[] splitGroupName = userGroup.Name.Split('\\');
				group = splitGroupName.Last();

				if (!UserGroupsCheckBoxList.Options.Contains(group)) continue;

				UserGroupsCheckBoxList.Check(group);
			}

			// Check Linked Order Templates
			this.linkedOrderTemplates = linkedOrderTemplates;
			LinkedOrderTemplatesCheckBoxList.SetOptions(linkedOrderTemplates.Select(x => x.Name));
			LinkedOrderTemplatesCheckBoxList.CheckAll();

			// Get all Order Templates that are not linked
			this.availableLinkableOrderTemplates = new ObservableCollection<OrderTemplate>();
			foreach (string orderTemplateName in userInfo.GetOrderTemplates())
			{
				if (linkedOrderTemplates.Any(x => x.Name == orderTemplateName)) continue;

				OrderTemplate orderTemplate;
				if (!helpers.ContractManager.TryGetOrderTemplate(orderTemplateName, out orderTemplate) || orderTemplate == null) continue;

				availableLinkableOrderTemplates.Add(orderTemplate);
			}

			if (LinkedOrderTemplatesCheckBoxList.Options.Any()) GenerateUI();
		}

		public bool IsValid
		{
			get
			{
				bool hasValidTemplateName = !String.IsNullOrWhiteSpace(TemplateName);
				bool hasUniqueTemplateName = IsTemplateNameUnique(TemplateName);

				if (!hasValidTemplateName) TemplateNameTextBox.ValidationText = "The name cannot be empty";
				if (!hasUniqueTemplateName) TemplateNameTextBox.ValidationText = "There already is a template with this name";
				TemplateNameTextBox.ValidationState = (hasValidTemplateName && hasUniqueTemplateName) ? UIValidationState.Valid : UIValidationState.Invalid;

				return hasValidTemplateName && hasUniqueTemplateName;
			}
		}

		private void AddOrderTemplate()
		{
			var linkOrderTemplateSection = new LinkOrderTemplateSection(availableLinkableOrderTemplates);
			linkOrderTemplateSection.DeleteLinkedOrderTemplateButton.Pressed += DeleteLinkedOrderTemplateButton_Pressed;
			linkOrderTemplateSection.RequiresUiUpdate += (s, a) => GenerateUI();

			linkedOrderTemplateSections.Add(linkOrderTemplateSection);
			GenerateUI();
		}

		private void DeleteLinkedOrderTemplateButton_Pressed(object sender, EventArgs e)
		{
			List<LinkOrderTemplateSection> sectionsToRemove = new List<LinkOrderTemplateSection>();
			foreach (LinkOrderTemplateSection section in linkedOrderTemplateSections)
			{
				if (!section.DeleteLinkedOrderTemplateButton.Equals(sender)) continue;
				sectionsToRemove.Add(section);
				if (section.SelectedTemplate != null) availableLinkableOrderTemplates.Add(section.SelectedTemplate);
			}

			linkedOrderTemplateSections.RemoveAll(x => sectionsToRemove.Contains(x));
			GenerateUI();
		}

		private void InitFullUserGroups(UserInfo userInfo)
		{
			string group;
			foreach (string userGroup in userInfo.AllUserGroups.Where(x => !String.IsNullOrWhiteSpace(x.Company)).Select(x => x.Name))
			{
				string[] splitGroupName = userGroup.Split('\\');
				group = splitGroupName.Last();

				if (FullUserGroups.ContainsKey(group)) continue;

				FullUserGroups.Add(group, userGroup);
			}
		}

		private bool IsTemplateNameUnique(string templateName)
		{
			// In case of edit the name can remain the same
			if (EventTemplate != null && templateName == EventTemplate.Name) return true;

			foreach (string name in helpers.ContractManager.GetAllEventTemplates())
			{
				if (name.Equals(templateName, StringComparison.InvariantCultureIgnoreCase)) return false;
			}

			return true;
		}

		private CheckBoxList UserGroupsCheckBoxList { get; set; }

		private YleTextBox TemplateNameTextBox { get; set; }

		private CheckBoxList LinkedOrderTemplatesCheckBoxList { get; set; }

		private Button AddOrderTemplateButton { get; set; }

		public IReadOnlyList<string> SelectedUserGroups
		{
			get
			{
				List<string> userGroups = new List<string>();
				foreach (string checkedUserGroup in UserGroupsCheckBoxList.Checked)
				{
					userGroups.Add(FullUserGroups[checkedUserGroup]);
				}

				return userGroups;
			}
		}

		public IReadOnlyList<OrderTemplate> GetLinkedOrderTemplatesToKeep()
		{
			List<string> checkOrderTemplates = LinkedOrderTemplatesCheckBoxList.Checked.ToList();
			return linkedOrderTemplates.Where(x => checkOrderTemplates.Contains(x.Name)).ToList();
		}

		public string TemplateName
		{
			get
			{
				return TemplateNameTextBox.Text;
			}

			private set
			{
				TemplateNameTextBox.Text = value;
			}
		}

		public List<OrderTemplate> NewOrderTemplates
		{
			get
			{
				List<OrderTemplate> templates = new List<OrderTemplate>();
				foreach(LinkOrderTemplateSection section in linkedOrderTemplateSections)
				{
					if (section.SelectedTemplate != null) templates.Add(section.SelectedTemplate);
				}

				return templates;
			}
		}

		public Dictionary<Guid, TimeSpan> NewOrderTemplateOffsets
		{
			get
			{
				Dictionary<Guid, TimeSpan> offsets = new Dictionary<Guid, TimeSpan>();
				foreach (LinkOrderTemplateSection section in linkedOrderTemplateSections)
				{
					if (section.SelectedTemplate != null) offsets.Add(section.SelectedTemplate.Id, section.Offset);
				}

				return offsets;
			}
		}

		public Event Event { get; private set; }

		public EventTemplate EventTemplate { get; private set; }

		public Button SaveTemplateButton { get; private set; }

		public Button BackButton { get; private set; }

		private int LinkedOrderTemplateSectionsDepth
		{
			get
			{
				if (!linkedOrderTemplateSections.Any()) return 0;
				return linkedOrderTemplateSections.Max(x => x.ServiceDepth);
			}
		}
	}
}
