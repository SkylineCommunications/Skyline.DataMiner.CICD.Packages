namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Library;

	public class VisibilityRightsCheckBoxes : Section
	{
		private const string McrCompany = "MCR";

		private readonly Label visibilityRightsLabel = new Label("Visibility Rights");
		private readonly List<CompanyVisibilityRightCheckBox> companyCheckBoxes = new List<CompanyVisibilityRightCheckBox>();

		private List<string> visibleCompanies = new List<string>();
		private List<string> permanentlyVisibleCompanies = new List<string>();
		private List<string> permanentlyDisabledCompanies = new List<string>();
		private List<string> permanentlySelectedCompanies = new List<string>();
		private int checkBoxesColumn = 2;
		private int checkBoxesColumnSpan = 3;

		public VisibilityRightsCheckBoxes(UserInfo userInfo, List<string> permanentlyVisibleSelectedAndDisabledCompanies = null)
		{
			if(userInfo == null) throw new ArgumentNullException(nameof(userInfo));

			var companyViewIds = CreateDictionaryFromUserInfo(userInfo);

			Initialize(companyViewIds);

			SetPermanentlyVisibleSelectedAndDisabledCompanies(permanentlyVisibleSelectedAndDisabledCompanies);

			// Check linked companies by default
			foreach (Company linkedCompany in userInfo.Contract.LinkedCompanies)
            {
				var linkedCompanyCheckBox = companyCheckBoxes.FirstOrDefault(x => x.Company.Equals(linkedCompany.Name));
				if (linkedCompanyCheckBox == null) continue;

				linkedCompanyCheckBox.IsChecked = true;
			}

			GenerateUi();
		}

		public VisibilityRightsCheckBoxes(Dictionary<string, int> companyViewIds, List<string> permanentlyVisibleSelectedAndDisabledCompanies = null)
		{
			if(companyViewIds == null) throw new ArgumentNullException(nameof(companyViewIds));

			Initialize(companyViewIds);

			SetPermanentlyVisibleSelectedAndDisabledCompanies(permanentlyVisibleSelectedAndDisabledCompanies);

			GenerateUi();
		}

		public new bool IsVisible
		{
			get => base.IsVisible;
			set
			{
				base.IsVisible = value;
				visibilityRightsLabel.IsVisible = value;
				companyCheckBoxes.ForEach(c => c.IsVisible = value && (visibleCompanies.Contains(c.Company) || permanentlyVisibleCompanies.Contains(c.Company)));
			}
		}

		public new bool IsEnabled
		{
			get => companyCheckBoxes.Where(c => !permanentlyDisabledCompanies.Contains(c.Company)).All(c => c.IsEnabled);
			set => companyCheckBoxes.ForEach(c => c.IsEnabled = value && !permanentlyDisabledCompanies.Contains(c.Company));
		}

		public IEnumerable<string> PermanentlyVisibleCompanies
		{
			get => permanentlyVisibleCompanies;
			set
			{
				permanentlyVisibleCompanies = value.ToList();
				if (!permanentlyVisibleCompanies.Contains(McrCompany)) permanentlyVisibleCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsVisible = IsVisible && (visibleCompanies.Contains(c.Company) || permanentlyVisibleCompanies.Contains(c.Company)));
			}
		}

		public IEnumerable<int> PermanentlyVisibleViewIds
		{
			get => companyCheckBoxes.Where(c => permanentlyVisibleCompanies.Contains(c.Company)).Select(c => c.CompanyViewId);
			set
			{
				permanentlyVisibleCompanies = companyCheckBoxes.Where(c => value.Contains(c.CompanyViewId)).Select(c => c.Company).ToList();
				if (!permanentlyVisibleCompanies.Contains(McrCompany)) permanentlyVisibleCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsVisible = IsVisible && (visibleCompanies.Contains(c.Company) || permanentlyVisibleCompanies.Contains(c.Company)));
			}
		}

		/// <summary>
		/// Gets or sets the permanent visibility of the checkboxes that correspond to the Companies;
		/// </summary>
		public IEnumerable<string> VisibleCompanies
		{
			get => visibleCompanies;
			set
			{
				visibleCompanies = value.ToList();
				companyCheckBoxes.ForEach(c => c.IsVisible = IsVisible && (visibleCompanies.Contains(c.Company) || permanentlyVisibleCompanies.Contains(c.Company)));
			}
		}

		/// <summary>
		/// Gets or sets the permanent visibility of the checkboxes that correspond to the ViewIds;
		/// </summary>
		public IEnumerable<int> VisibleViewIds
		{
			get => companyCheckBoxes.Where(c => visibleCompanies.Contains(c.Company)).Select(c => c.CompanyViewId);
			set
			{
				visibleCompanies = companyCheckBoxes.Where(c => value.Contains(c.CompanyViewId)).Select(c => c.Company).ToList();
				companyCheckBoxes.ForEach(c => c.IsVisible = IsVisible && (visibleCompanies.Contains(c.Company) || permanentlyVisibleCompanies.Contains(c.Company)));
			}
		}

		/// <summary>
		/// Gets or sets the permanent enabled state of the checkboxes that correspond to the Companies;
		/// </summary>
		public IEnumerable<string> PermanentlyDisabledCompanies
		{
			get => permanentlyDisabledCompanies;
			set
			{
				permanentlyDisabledCompanies = value.ToList();
				if (!permanentlyDisabledCompanies.Contains(McrCompany)) permanentlyDisabledCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsEnabled &= !permanentlyDisabledCompanies.Contains(c.Company));
			}
		}

		/// <summary>
		/// Gets or sets the permanent enabled state of the checkboxes that correspond to the ViewIds;
		/// </summary>
		public IEnumerable<int> PermanentlyDisabledViewIds
		{
			get => companyCheckBoxes.Where(c => permanentlyDisabledCompanies.Contains(c.Company)).Select(c => c.CompanyViewId);
			set
			{
				permanentlyDisabledCompanies = companyCheckBoxes.Where(c => value.Contains(c.CompanyViewId)).Select(c => c.Company).ToList();
				if (!permanentlyDisabledCompanies.Contains(McrCompany)) permanentlyDisabledCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsEnabled &= !permanentlyDisabledCompanies.Contains(c.Company));
			}
		}

		public IEnumerable<string> PermanentlySelectedCompanies
		{
			get => permanentlySelectedCompanies;
			set
			{
				permanentlySelectedCompanies = value.ToList();
				if (!permanentlySelectedCompanies.Contains(McrCompany)) permanentlySelectedCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsChecked |= permanentlySelectedCompanies.Contains(c.Company));
			}
		}

		public IEnumerable<int> PermanentlySelectedViewIds
		{
			get => companyCheckBoxes.Where(c => permanentlySelectedCompanies.Contains(c.Company)).Select(c => c.CompanyViewId);
			set
			{
				permanentlySelectedCompanies = companyCheckBoxes.Where(c => value.Contains(c.CompanyViewId)).Select(c => c.Company).ToList();
				if (!permanentlySelectedCompanies.Contains(McrCompany)) permanentlySelectedCompanies.Add(McrCompany);

				companyCheckBoxes.ForEach(c => c.IsChecked |= permanentlySelectedCompanies.Contains(c.Company));
			}
		}

		public IEnumerable<string> SelectedCompanies
		{
			get => companyCheckBoxes.Where(c => c.IsChecked).Select(c => c.Company);
			set
			{
				if (value == null) return;

				companyCheckBoxes.ForEach(c => c.IsChecked = value.Contains(c.Company) || permanentlySelectedCompanies.Contains(c.Company));
			}
		}

		public IEnumerable<int> SelectedViewIds
		{
			get => companyCheckBoxes.Where(c => c.IsChecked).Select(c => c.CompanyViewId);
			set
			{
				if (value == null) return;

				companyCheckBoxes.ForEach(c => c.IsChecked = value.Contains(c.CompanyViewId) || permanentlySelectedCompanies.Contains(c.Company));
			}
		}

		public int CheckBoxesColumn
		{
			get => checkBoxesColumn;
			set
			{
				if (checkBoxesColumn != value)
				{
					checkBoxesColumn = value;
					GenerateUi();
				}
			}
		}

		public int CheckBoxesColumnSpan
		{
			get => checkBoxesColumnSpan;
			set
			{
				if (checkBoxesColumnSpan != value)
				{
					checkBoxesColumnSpan = value;

					foreach (var companyCheckBox in companyCheckBoxes)
					{
						companyCheckBox.ColumnSpan = checkBoxesColumnSpan;
					}

					GenerateUi();
				}
			}
		}

		public event EventHandler<Dictionary<string, int>> SelectedCompaniesChanged;

		public void UpdateSelectableCompanies(Dictionary<string, int> updatedSelectableCompanies, List<string> permanentlyVisibleSelectedAndDisabledCompanies = null)
		{
			Initialize(updatedSelectableCompanies);

			SetPermanentlyVisibleSelectedAndDisabledCompanies(permanentlyVisibleSelectedAndDisabledCompanies);

			var companiesToDelete = companyCheckBoxes.Select(c => c.Company).Except(updatedSelectableCompanies.Keys).ToList();
			companyCheckBoxes.RemoveAll(c => companiesToDelete.Contains(c.Company));

			GenerateUi();
		}

        public override string ToString()
        {
			return $"Visibility Rights Checkboxes: displayed companies {String.Join(", ", visibleCompanies)}";
        }

        private void SetPermanentlyVisibleSelectedAndDisabledCompanies(List<string> permanentlySelectedAndDisabledCompanies)
		{
			permanentlySelectedAndDisabledCompanies = permanentlySelectedAndDisabledCompanies ?? new List<string>();
			if (!permanentlySelectedAndDisabledCompanies.Contains(McrCompany)) permanentlySelectedAndDisabledCompanies.Add(McrCompany);
			PermanentlyDisabledCompanies = permanentlySelectedAndDisabledCompanies;
			PermanentlySelectedCompanies = permanentlySelectedAndDisabledCompanies;
			PermanentlyVisibleCompanies = permanentlySelectedAndDisabledCompanies;
		}

		private Dictionary<string, int> CreateDictionaryFromUserInfo(UserInfo userInfo)
		{
			if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

			var companyViewIds = new Dictionary<string, int>
			{
				{ McrCompany, userInfo.McrSecurityViewId }
			};

			foreach (var company in userInfo.AllCompanies.Distinct())
			{
				int companyViewId = userInfo.AllUserGroups.First(u => u.Company == company).CompanySecurityViewId;
				companyViewIds.Add(company, companyViewId);
			}

			return companyViewIds;
		}

		/// <summary>
		/// Creates a visible, enabled checkbox for each given key-value pair. Keeps the existing checkboxes where possible.
		/// </summary>
		/// <param name="companyViewIds">A dictionary with the company name as key and its security view ID as value.</param>
		private void Initialize(Dictionary<string, int> companyViewIds)
		{
			permanentlyDisabledCompanies.Clear();
			visibleCompanies.Clear();
			
			foreach (var company in companyViewIds.Keys.Distinct())
			{
				bool companyIsAlreadyAdded = companyCheckBoxes.Any(c => c.Company == company);
				if (companyIsAlreadyAdded)
				{
					visibleCompanies.Add(company);
					continue;
				}

				int companyViewId = companyViewIds[company];

				var companyCheckbox = new CompanyVisibilityRightCheckBox(company, companyViewId);
				companyCheckbox.Changed += CompanyVisibilityRightsCheckbox_Changed;
				companyCheckBoxes.Add(companyCheckbox);

				visibleCompanies.Add(company);
			}
		}

		private void CompanyVisibilityRightsCheckbox_Changed(object sender, int e)
		{
			var dict = companyCheckBoxes.Where(c => c.IsChecked).ToDictionary(c => c.Company, c => c.CompanyViewId);

			SelectedCompaniesChanged?.Invoke(this, dict);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(visibilityRightsLabel, new WidgetLayout(++row, 0, 1, CheckBoxesColumn));

			foreach (var checkBox in companyCheckBoxes)
			{
				AddSection(checkBox, new SectionLayout(row++, CheckBoxesColumn));
				row += checkBox.RowCount;
			}
		}

		private class CompanyVisibilityRightCheckBox : Section
		{
			private readonly CheckBox checkBox;

			private int columnSpan;

			public CompanyVisibilityRightCheckBox(string company, int viewId, int columnSpan = 3)
			{
				this.Company = company ?? throw new ArgumentNullException(nameof(company));
				this.CompanyViewId = viewId;
				this.columnSpan = columnSpan;

				checkBox = new CheckBox(company);
				checkBox.Changed += (o, e) => Changed?.Invoke(this, CompanyViewId);

				GenerateUi();
			}

			public bool IsChecked
			{
				get => checkBox.IsChecked;
				set => checkBox.IsChecked = value;
			}

			public new bool IsVisible
			{
				get => checkBox.IsVisible;
				set => checkBox.IsVisible = value;
			}

			public new bool IsEnabled
			{
				get => checkBox.IsEnabled;
				set => checkBox.IsEnabled = value;
			}

			public int CompanyViewId { get; }

			public string Company { get; }

			public event EventHandler<int> Changed;

			public int ColumnSpan
			{
				get => columnSpan;
				set
				{
					if (columnSpan != value)
					{
						columnSpan = value;
						GenerateUi();
					}
				}
			}

			private void GenerateUi()
			{
				Clear();

				AddWidget(checkBox, new WidgetLayout(0, 0, 1, ColumnSpan));
			}
		}
	}
}
