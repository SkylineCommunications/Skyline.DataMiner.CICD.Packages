namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ImportGeneralInfoSection : YleSection
	{
		private readonly ImportMainSection section;

		private readonly ISectionConfiguration configuration;

		private readonly Label generalOrderInformationTitle = new Label("General Order Information") { Style = TextStyle.Bold };

		private readonly Label nameOfTheOrderLabel = new Label("Name of the order");
		private readonly YleTextBox nameOfTheOrderTextBox = new YleTextBox();

		private readonly Label deadlineLabel = new Label("Deadline");
		private readonly YleDateTimePicker deadlineDateTimePicker = new YleDateTimePicker(DateTime.Now.AddDays(1)) { ValidationText = "The deadline cannot be in the past" };

		private readonly Label materialDeliveryTimeLabel = new Label("Material delivery time");
		private readonly DateTimePicker materialDeliveryTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));

		private readonly Label productNumberLabel = new Label("Product number") { IsVisible = false };
		private readonly DropDown productNumberDropdown = new DropDown { IsVisible = false };

		protected readonly bool ignoreTimingValidationWhenOrderExists;

		public ImportGeneralInfoSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, Ingest ingest = null) : base(helpers)
		{
			this.section = section;
			productNumberDropdown.Options = EnumExtensions.GetEnumDescriptions<ProductNumbers>();

			ignoreTimingValidationWhenOrderExists = ingest != null;

			if (ingest != null)
			{
				nameOfTheOrderTextBox.Text = ingest.OrderDescription;
				if (ingest.IngestDestination?.Destination != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA)) MaterialDeliveryTime = ingest.DeliveryTime;
				if (ingest.IngestDestination?.Destination == EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA))
				{
					// Mapping old descriptions: DCP221522
					if (EnumExtensions.TryGetEnumValueFromDescription(ingest.ProductNumber, out ProductNumbers descriptionValue))
					{
						productNumberDropdown.Selected = descriptionValue.GetDescription();
					}
					else if (EnumExtensions.TryGetEnumValueFromOldDescription(ingest.ProductNumber, out ProductNumbers oldDescriptionValue))
					{
						productNumberDropdown.Selected = oldDescriptionValue.GetDescription();
					}
					else
					{
						productNumberDropdown.Selected = ProductNumbers.UUTISET.GetDescription();
					}
				}

				deadlineDateTimePicker.DateTime = ingest.Deadline;
			}
			else
			{
				productNumberDropdown.Selected = EnumExtensions.GetDescriptionFromEnumValue(ProductNumbers.UUTISET);
				deadlineDateTimePicker.DateTime = DateTime.Now.AddDays(2);
			}

			this.configuration = configuration;

			deadlineDateTimePicker.Changed += (o, e) => IsValid(OrderAction.Book);

			GenerateUi(out int row);
		}

		public DateTime MaterialDeliveryTime
		{
			get
			{
				return materialDeliveryTimeDateTimePicker.DateTime;
			}
			private set
			{
				materialDeliveryTimeDateTimePicker.DateTime = value;
			}
		}

		public bool IsValid(OrderAction action)
		{
			bool orderName = !String.IsNullOrWhiteSpace(nameOfTheOrderTextBox.Text);
			bool deadlineIsValid = MaterialDeliveryTime < deadlineDateTimePicker.DateTime && DateTime.Now < deadlineDateTimePicker.DateTime;
			bool deliveryTimeIsValid = MaterialDeliveryTime > DateTime.Now || section.IngestDestination == InterplayPamElements.UA;

			if (ignoreTimingValidationWhenOrderExists)
			{
				deadlineIsValid = true;
				deliveryTimeIsValid = true;
			}

			deadlineDateTimePicker.ValidationState = deadlineIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			deadlineDateTimePicker.ValidationText = "The deadline cannot be in the past and must be after the delivery time";

			nameOfTheOrderTextBox.ValidationState = orderName ? UIValidationState.Valid : UIValidationState.Invalid;
			nameOfTheOrderTextBox.ValidationText = "Provide a name for the order";

			materialDeliveryTimeDateTimePicker.ValidationState = deliveryTimeIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			materialDeliveryTimeDateTimePicker.ValidationText = "The delivery time cannot be in the past";

			if (action == OrderAction.Save)
			{
				return orderName;
			}
			else
			{
				return deadlineIsValid && orderName && deliveryTimeIsValid;
			}
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(generalOrderInformationTitle, new WidgetLayout(++row, 0));

			AddWidget(nameOfTheOrderLabel, new WidgetLayout(++row, 0));
			AddWidget(nameOfTheOrderTextBox, new WidgetLayout(row, 1, 1, 2));

			AddWidget(materialDeliveryTimeLabel, ++row, 0);
			AddWidget(materialDeliveryTimeDateTimePicker, row, 1, 1, 2);

			AddWidget(deadlineLabel, ++row, 0);
			AddWidget(deadlineDateTimePicker, row, 1, 1, 2);

			AddWidget(productNumberLabel, ++row, 0);
			AddWidget(productNumberDropdown, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public void UpdateIngest(Ingest ingest)
		{
			ingest.OrderDescription = nameOfTheOrderTextBox.Text;
			ingest.Deadline = deadlineDateTimePicker.DateTime;
			ingest.ProductNumber = productNumberDropdown.Selected;
			ingest.DeliveryTime = MaterialDeliveryTime;
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			generalOrderInformationTitle.IsVisible = IsVisible;

			nameOfTheOrderLabel.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsVisible = IsVisible;
			nameOfTheOrderTextBox.IsEnabled = IsEnabled;

			deadlineLabel.IsVisible = IsVisible;
			deadlineDateTimePicker.IsVisible = IsVisible;
			deadlineDateTimePicker.IsEnabled = IsEnabled;

			if (section.IngestDestinationSection != null)
			{
				materialDeliveryTimeLabel.IsVisible = IsVisible && section.IngestDestination != InterplayPamElements.UA;
				materialDeliveryTimeDateTimePicker.IsVisible = IsVisible && materialDeliveryTimeLabel.IsVisible;
				materialDeliveryTimeDateTimePicker.IsEnabled = IsEnabled;

				productNumberLabel.IsVisible = IsVisible && section.IngestDestination == InterplayPamElements.UA;
				productNumberDropdown.IsVisible = IsVisible && productNumberLabel.IsVisible;
				productNumberDropdown.IsEnabled = IsEnabled;
			}

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}
