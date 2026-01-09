namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class AddOrderTemplateDialog : Dialog
	{
		private readonly Helpers helpers;
		private readonly Dictionary<string, string> fullUserGroups = new Dictionary<string, string>();

		public AddOrderTemplateDialog(Helpers helpers, Order order, UserInfo userInfo) : base(helpers.Engine)
		{
			this.helpers = helpers;
			InitFullUserGroups(userInfo);

			Title = "Add Template";
			Order = order;

			TemplateNameTextBox = new YleTextBox($"{order.Name}_Template");
			UserGroupsCheckBoxList = new CheckBoxList(fullUserGroups.Keys) { IsSorted = true };
			SaveTemplateButton = new Button("Save Template") { Style = ButtonStyle.CallToAction };
			BackButton = new Button("Back");

			int row = -1;
			AddWidget(new Label("Template Name"), ++row, 0);
			AddWidget(TemplateNameTextBox, row, 1);

			AddWidget(new Label("User Groups"), ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(UserGroupsCheckBoxList, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(SaveTemplateButton, ++row, 0, 1, 2);
			AddWidget(BackButton, ++row, 0, 1, 2);

			SetColumnWidth(0, 120);
			SetColumnWidth(1, 350);
		}

		public void Init(OrderTemplate template, UserInfo userInfo)
		{
			OrderTemplate = template;
			TemplateName = template.Name;
			
			string group;
			foreach (UserGroup userGroup in userInfo.AllUserGroups.Where(x => !String.IsNullOrWhiteSpace(x.Company)))
			{
				Engine.Log($"AddEventTemplateDialog.Init|Event Templates for user group {userGroup.Name}: {String.Join(", ", userGroup.OrderTemplates)}");

				if (!userGroup.OrderTemplates.Contains(template.Name)) continue;

				string[] splitGroupName = userGroup.Name.Split('\\');
				group = splitGroupName.Last();

				if (!UserGroupsCheckBoxList.Options.Contains(group)) continue;

				UserGroupsCheckBoxList.Check(group);
			}
		}

		public bool IsValid
		{
			get
			{
				bool hasValidTemplateName = !String.IsNullOrWhiteSpace(TemplateNameTextBox.Text);
				bool hasUniqueTemplateName = IsTemplateNameUnique(TemplateNameTextBox.Text);

				if (!hasValidTemplateName) TemplateNameTextBox.ValidationText = "The name cannot be empty";
				if (!hasUniqueTemplateName) TemplateNameTextBox.ValidationText = "There already is a template with this name";
				TemplateNameTextBox.ValidationState = (hasValidTemplateName && hasUniqueTemplateName) ? UIValidationState.Valid : UIValidationState.Invalid;

				return hasValidTemplateName && hasUniqueTemplateName;
			}
		}

		private void InitFullUserGroups(UserInfo userInfo)
		{
			string group;
			foreach (string userGroup in userInfo.AllUserGroups.Where(x => !String.IsNullOrWhiteSpace(x.Company)).Select(x => x.Name))
			{
				string[] splitGroupName = userGroup.Split('\\');
				group = splitGroupName.Last();

				if (fullUserGroups.ContainsKey(group)) continue;

				fullUserGroups.Add(group, userGroup);
			}
		}

		private bool IsTemplateNameUnique(string templateName)
		{
			// In case of edit the name can remain the same
			if (OrderTemplate != null && templateName == OrderTemplate.Name) return true;

			foreach (var templateNameIdPair in helpers.ContractManager.GetAllOrderTemplateNamesAndIds())
			{
				if (templateNameIdPair.Key.Equals(templateName, StringComparison.InvariantCultureIgnoreCase)) return false;
			}

			return true;
		}

		private CheckBoxList UserGroupsCheckBoxList { get; set; }

		private YleTextBox TemplateNameTextBox { get; set; }

		public IEnumerable<string> SelectedUserGroups
		{
			get
			{
				List<string> userGroups = new List<string>();
				foreach (string checkedUserGroup in UserGroupsCheckBoxList.Checked)
				{
					userGroups.Add(fullUserGroups[checkedUserGroup]);
				}

				return userGroups;
			}
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

		public Order Order { get; private set; }

		public OrderTemplate OrderTemplate { get; private set; }

		public Button SaveTemplateButton { get; private set; }

		public Button BackButton { get; private set; }
	}
}