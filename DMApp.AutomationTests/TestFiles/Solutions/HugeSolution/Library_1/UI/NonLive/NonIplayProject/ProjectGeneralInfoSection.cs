namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.NonIplayProject
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ProjectGeneralInfoSection : YleSection
    {
		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label ingestDepartmentLabel = new Label("Import department");
        private readonly DropDown ingestDepartmentDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<IngestDepartments>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(IngestDepartments.HELSINKI));

        private readonly Label productionDepartmentNameLabel = new Label("Production department name");
        private readonly DropDown productionDepartmentNameDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<HelsinkiProductionDepartmentNames>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(HelsinkiProductionDepartmentNames.None));

        private readonly Label vsaOtherDepartmentNameLabel = new Label("Other department name");
        private readonly YleTextBox vsaOtherDepartmentTextBox = new YleTextBox();

        private readonly Label materialDeliveryTimeLabel = new Label("Material delivery time");
        private readonly DateTimePicker materialDeliveryTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private readonly ISectionConfiguration configuration;

		protected bool ignoreTimingValidationWhenOrderExists;

		public ProjectGeneralInfoSection(Helpers helpers, ISectionConfiguration configuration, Project project) : base(helpers)
        {
            this.configuration = configuration;

            ingestDepartmentDropDown.Changed += IngestDepartmentDropDown_Changed;
            productionDepartmentNameDropDown.Changed += ProductionDepartmentNameDropDown_Changed;

			InitializeProject(project);

            GenerateUi(out int row);
        }

		private void InitializeProject(Project project)
		{
			ignoreTimingValidationWhenOrderExists = project != null;

			if (project != null)
			{
				nameOfTheOrderTextBox.Text = project.OrderDescription;
				deadlineDateTimePicker.DateTime = project.Deadline;
			}
		}

        public IngestDepartments IngestDepartment
        {
            get
            {
                return EnumExtensions.GetEnumValueFromDescription<IngestDepartments>(ingestDepartmentDropDown.Selected);
            }
            internal set
            {
                ingestDepartmentDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
                switch (IngestDepartment)
                {
                    case IngestDepartments.TAMPERE:
                        productionDepartmentNameDropDown.Options = EnumExtensions.GetEnumDescriptions<TampereProductionDepartmentNames>().OrderBy(x => x);
                        productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(TampereProductionDepartmentNames.None);
                        break;
                    case IngestDepartments.HELSINKI:
                        productionDepartmentNameDropDown.Options = EnumExtensions.GetEnumDescriptions<HelsinkiProductionDepartmentNames>().OrderBy(x => x);
                        productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(HelsinkiProductionDepartmentNames.None);
                        break;
                    case IngestDepartments.VAASA:
                        productionDepartmentNameDropDown.Options = EnumExtensions.GetEnumDescriptions<VaasaProductionDepartmentNames>().OrderBy(x => x);
                        productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(VaasaProductionDepartmentNames.None);
                        break;
                    default:
                        break;
                }
            }
        }

        public event EventHandler<DropDown.DropDownChangedEventArgs> IngestDepartmentChanged;

        public HelsinkiProductionDepartmentNames? HelsinkiProductionDepartmentName
        {
            get
            {
                if (productionDepartmentNameDropDown.Selected.Equals(EnumExtensions.GetDescriptionFromEnumValue(HelsinkiProductionDepartmentNames.None)))
                {
                    return null;
                }
                else
                {
                    return EnumExtensions.GetEnumValueFromDescription<HelsinkiProductionDepartmentNames>(productionDepartmentNameDropDown.Selected);
                }
            }
            internal set
            {
                productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
            }
        }

        public TampereProductionDepartmentNames? TampereProductionDepartmentName
        {
            get
            {
                if (productionDepartmentNameDropDown.Selected.Equals(EnumExtensions.GetDescriptionFromEnumValue(TampereProductionDepartmentNames.None)))
                {
                    return null;
                }
                else
                {
                    return EnumExtensions.GetEnumValueFromDescription<TampereProductionDepartmentNames>(productionDepartmentNameDropDown.Selected);
                }
            }
            internal set
            {
                productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
            }
        }

        public VaasaProductionDepartmentNames? VaasaProductionDepartmentName
        {
            get
            {
                if (productionDepartmentNameDropDown.Selected.Equals(EnumExtensions.GetDescriptionFromEnumValue(VaasaProductionDepartmentNames.None)))
                {
                    return null;
                }
                else
                {
                    return EnumExtensions.GetEnumValueFromDescription<VaasaProductionDepartmentNames>(productionDepartmentNameDropDown.Selected);
                }
            }
            internal set
            {
                productionDepartmentNameDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
            }
        }

        public string VsaOtherDepartmentName
        {
            get
            {
                return vsaOtherDepartmentTextBox.Text;
            }
            internal set
            {
                vsaOtherDepartmentTextBox.Text = value;
            }
        }

        public DateTime MaterialDeliveryTime
        {
            get
            {
                return materialDeliveryTimeDateTimePicker.DateTime;
            }
            protected set
            {
                materialDeliveryTimeDateTimePicker.DateTime = value;
            }
        }

        public DateTime MaterialExportDeadLine
        {
            get
            {
                return deadlineDateTimePicker.DateTime;
            }
            protected set
            {
                deadlineDateTimePicker.DateTime = value;
            }
        }

        public bool IsValid(OrderAction action)
        {
			bool isOrderDescriptionValid = !String.IsNullOrWhiteSpace(nameOfTheOrderTextBox.Text);
			nameOfTheOrderTextBox.ValidationState = isOrderDescriptionValid ? UIValidationState.Valid : UIValidationState.Invalid;
			nameOfTheOrderTextBox.ValidationText = "Provide a name for the order";

			bool isDeadLineValid = ignoreTimingValidationWhenOrderExists || deadlineDateTimePicker.IsValid;

			bool isBaseValid = action == OrderAction.Save ? isOrderDescriptionValid : isOrderDescriptionValid && isDeadLineValid;

			bool isProductionDepartmentNameValid = productionDepartmentNameDropDown.Selected != Constants.None;
            productionDepartmentNameDropDown.ValidationState = isProductionDepartmentNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
            productionDepartmentNameDropDown.ValidationText = "Select a department";

            bool isOtherVaasaDepartmentNameValid = IngestDepartment != IngestDepartments.VAASA || VaasaProductionDepartmentName != VaasaProductionDepartmentNames.OTHER || !String.IsNullOrWhiteSpace(VsaOtherDepartmentName);
            vsaOtherDepartmentTextBox.ValidationState = isOtherVaasaDepartmentNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
            vsaOtherDepartmentTextBox.ValidationText = "Provide a department name";

            bool isDepartmentValid = isProductionDepartmentNameValid && isOtherVaasaDepartmentNameValid;

            bool isDeliveryTimeValid = MaterialDeliveryTime > DateTime.Now;
            materialDeliveryTimeDateTimePicker.ValidationState = isDeliveryTimeValid ? UIValidationState.Valid : UIValidationState.Invalid;
            materialDeliveryTimeDateTimePicker.ValidationText = "The material delivery time cannot be in the past";

            bool isMaterialExportDeadLineValid = MaterialExportDeadLine > DateTime.Now && MaterialExportDeadLine > MaterialDeliveryTime;
            deadlineDateTimePicker.ValidationState = isMaterialExportDeadLineValid ? UIValidationState.Valid : UIValidationState.Invalid;
            deadlineDateTimePicker.ValidationText = "The material export deadline cannot be in the past and must be after the delivery time";

            bool isTimingValid = isDeliveryTimeValid && isMaterialExportDeadLineValid;

            if (action == OrderAction.Save)
            {
                return isBaseValid;
            }
            else
            {
                return isBaseValid
                    && isDepartmentValid
                    && isTimingValid;
            }
        }

        public void UpdateProject(Project projectOrder)
        {
			projectOrder.OrderDescription = nameOfTheOrderTextBox.Text;
			projectOrder.Deadline = deadlineDateTimePicker.DateTime;

            projectOrder.ImportDepartment = EnumExtensions.GetDescriptionFromEnumValue(IngestDepartment);
			switch (IngestDepartment)
			{
				case IngestDepartments.TAMPERE:
					projectOrder.ProductionDepartmentName = TampereProductionDepartmentName != null ? EnumExtensions.GetDescriptionFromEnumValue(TampereProductionDepartmentName) : Constants.None;
					break;
				case IngestDepartments.HELSINKI:
					projectOrder.ProductionDepartmentName = HelsinkiProductionDepartmentName != null ? EnumExtensions.GetDescriptionFromEnumValue(HelsinkiProductionDepartmentName) : Constants.None;
					break;
				case IngestDepartments.VAASA:
					projectOrder.ProductionDepartmentName = VaasaProductionDepartmentName != null ? EnumExtensions.GetDescriptionFromEnumValue(VaasaProductionDepartmentName) : Constants.None;
					if (VaasaProductionDepartmentName == VaasaProductionDepartmentNames.OTHER)
					{
                        projectOrder.OtherDepartmentName = VsaOtherDepartmentName;
					}
					break;
				default:
					break;
			}

            projectOrder.MaterialDeliveryTime = materialDeliveryTimeDateTimePicker.DateTime;
            projectOrder.Deadline = deadlineDateTimePicker.DateTime;
        }

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(generalOrderInformationTitle, ++row, 0, 1, 5);

			AddWidget(nameOfTheOrderLabel, ++row, 0);
			AddWidget(nameOfTheOrderTextBox, row, 1, 1, 2);

			AddWidget(ingestDepartmentLabel, ++row, 0);
			AddWidget(ingestDepartmentDropDown, row, 1, 1, 2);

			AddWidget(productionDepartmentNameLabel, ++row, 0);
			AddWidget(productionDepartmentNameDropDown, row, 1, 1, 2);

			AddWidget(vsaOtherDepartmentNameLabel, ++row, 0);
			AddWidget(vsaOtherDepartmentTextBox, row, 1, 1, 2);

			AddWidget(materialDeliveryTimeLabel, ++row, 0);
			AddWidget(materialDeliveryTimeDateTimePicker, row, 1, 1, 2);

			AddWidget(deadlineLabel, ++row, 0);
			AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public override void RegenerateUi()
        {
            GenerateUi(out int row);
        }

        internal void Initialize(Project project)
        {
            IngestDepartment = EnumExtensions.GetEnumValueFromDescription<IngestDepartments>(project.ImportDepartment);
            productionDepartmentNameDropDown.Selected = project.ProductionDepartmentName;

            if (IngestDepartment == IngestDepartments.VAASA && VaasaProductionDepartmentName == VaasaProductionDepartmentNames.OTHER)
            {
                VsaOtherDepartmentName = project.OtherDepartmentName;
            }

            materialDeliveryTimeDateTimePicker.DateTime = project.MaterialDeliveryTime;
            deadlineDateTimePicker.DateTime = new DateTime(project.Deadline.Year, project.Deadline.Month, project.Deadline.Day, project.Deadline.Hour, project.Deadline.Minute, project.Deadline.Second);
        }

        private void IngestDepartmentDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
        {
			using (UiDisabler.StartNew(this))
			{
				IngestDepartment = EnumExtensions.GetEnumValueFromDescription<IngestDepartments>(e.Selected);

				IngestDepartmentChanged?.Invoke(this, e);

				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
        }

        private void ProductionDepartmentNameDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
        {
			using (UiDisabler.StartNew(this))
			{
				productionDepartmentNameDropDown.Options = productionDepartmentNameDropDown.Options.ToList();

				switch (IngestDepartment)
				{
					case IngestDepartments.TAMPERE:
						TampereProductionDepartmentName = EnumExtensions.GetEnumValueFromDescription<TampereProductionDepartmentNames>(e.Selected);
						break;
					case IngestDepartments.HELSINKI:
						HelsinkiProductionDepartmentName = EnumExtensions.GetEnumValueFromDescription<HelsinkiProductionDepartmentNames>(e.Selected);
						break;
					case IngestDepartments.VAASA:
						VaasaProductionDepartmentName = EnumExtensions.GetEnumValueFromDescription<VaasaProductionDepartmentNames>(e.Selected);
						break;
					default:
						break;
				}

				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
        }

        protected override void HandleVisibilityAndEnabledUpdate()
        {
			generalOrderInformationTitle.IsVisible = IsVisible;

			nameOfTheOrderLabel.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsEnabled = IsEnabled;

			ingestDepartmentLabel.IsVisible = IsVisible;
			ingestDepartmentDropDown.IsVisible = IsVisible;
			ingestDepartmentDropDown.IsEnabled = IsEnabled;

			productionDepartmentNameLabel.IsVisible = IsVisible;
			productionDepartmentNameDropDown.IsVisible = IsVisible;
			productionDepartmentNameDropDown.IsEnabled = IsEnabled;

			vsaOtherDepartmentNameLabel.IsVisible = IngestDepartment == IngestDepartments.VAASA && VaasaProductionDepartmentName == VaasaProductionDepartmentNames.OTHER;
			vsaOtherDepartmentTextBox.IsVisible = vsaOtherDepartmentNameLabel.IsVisible;
			vsaOtherDepartmentTextBox.IsEnabled = IsEnabled;

			materialDeliveryTimeLabel.IsVisible = IsVisible;
			materialDeliveryTimeDateTimePicker.IsVisible = IsVisible;
			materialDeliveryTimeDateTimePicker.IsEnabled = IsEnabled;

			deadlineLabel.IsVisible = IsVisible;
			deadlineDateTimePicker.IsVisible = IsVisible;
			deadlineDateTimePicker.IsEnabled = IsEnabled;    

            ToolTipHandler.SetTooltipVisibility(this);
        }
    }
}
