namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ResourceSection : YleSection
	{
		private readonly Label resourceLabel = new Label(string.Empty);
		private readonly Label resourceLabel2 = new Label(string.Empty);
		private readonly Label resourceAdditionalInfoLabel = new Label(string.Empty) { IsVisible = false };
		private readonly Label resourceMetaDataLabel = new Label(string.Empty) { IsVisible = false };

		private readonly DisplayedFunction function;
		private readonly ResourceSectionConfiguration configuration;

		public ResourceSection(Helpers helpers, Function function, ResourceSectionConfiguration configuration) : base(helpers)
		{
			this.function = function as DisplayedFunction ?? throw new ArgumentNullException(nameof(function));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			Initialize();
			GenerateUi(out _);
		}

		/// <summary>
		/// Gets the dropDown that is used to configure the Resource assigned to the Function displayed by this section.
		/// </summary>
		public YleResourceDropDown ResourceDropDown { get; private set; }

		public YleResourceCheckBox IncludeOccupiedResourcesCheckBox { get; private set; }

		public YleResourceCheckBox AutomaticCheckBox { get; private set; }

		public override void RegenerateUi()
		{
			Clear();
			GenerateUi(out _);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			bool resourceSelectionVisible = IsVisible && (configuration.IsVisible || function.DisplayedResources.Any(r => r.GetResourcePropertyBooleanValue(ResourcePropertyNames.ResourceSelectionAllowed)));

			bool occupiedResourceSelectionVisible = IsVisible && configuration.IsVisible;

			AutomaticCheckBox.IsVisible = resourceSelectionVisible && configuration.AutomaticCheckboxIsVisible;
			AutomaticCheckBox.IsEnabled = IsEnabled && configuration.ResourceSelectionEnabled && !function.ResourceSelectionMandatory;

			resourceLabel.IsVisible = resourceSelectionVisible && AutomaticCheckBox.IsVisible;

			ResourceDropDown.IsVisible = resourceSelectionVisible && (!AutomaticCheckBox.IsChecked || configuration.ResourceDropdownAlwaysVisible);
			ResourceDropDown.IsEnabled = IsEnabled && configuration.ResourceSelectionEnabled && !AutomaticCheckBox.IsChecked;

			resourceLabel2.IsVisible = resourceSelectionVisible && !resourceLabel.IsVisible && ResourceDropDown.IsVisible;

			IncludeOccupiedResourcesCheckBox.IsVisible = occupiedResourceSelectionVisible && !AutomaticCheckBox.IsChecked && configuration.OccupiedResourceSelectionIsVisible;
			IncludeOccupiedResourcesCheckBox.IsEnabled = IsEnabled && configuration.ResourceSelectionEnabled;

			resourceAdditionalInfoLabel.IsVisible = resourceSelectionVisible && !string.IsNullOrWhiteSpace(resourceAdditionalInfoLabel.Text) && !AutomaticCheckBox.IsChecked;
			resourceMetaDataLabel.IsVisible = resourceSelectionVisible && !string.IsNullOrEmpty(resourceMetaDataLabel.Text);
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(resourceLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(AutomaticCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(resourceLabel2, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(ResourceDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(IncludeOccupiedResourcesCheckBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);
			AddWidget(resourceAdditionalInfoLabel, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(resourceMetaDataLabel, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
		}

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToFunction();
		}

		private void InitializeWidgets()
		{
			resourceLabel.Text = configuration.ResourceLabelText;
			resourceLabel2.Text = configuration.ResourceLabelText;

			AutomaticCheckBox = new YleResourceCheckBox(function.Definition.Label, "Automatic") { IsEnabled = configuration.ResourceSelectionEnabled, IsChecked = !function.EnforceSelectedResource, Name = $"{function.Definition.Label} - Automatic" };

			IncludeOccupiedResourcesCheckBox = new YleResourceCheckBox(function.Definition.Label, "Include occupied resources") { IsEnabled = configuration.ResourceSelectionEnabled, Name = $"{function.Definition.Label} - Include occupied resources" };

			ResourceDropDown = new YleResourceDropDown(function.Definition.Label, GetResourceDropDownOptions(), function.ResourceName) { IsEnabled = configuration.ResourceSelectionEnabled, IsDisplayFilterShown = true, Name = $"{function.Definition.Label} - Resource" };
			ResourceDropDown.Changed += (s, e) => HandleVisibilityAndEnabledUpdate();

			if (!string.IsNullOrWhiteSpace(configuration.ReasonForResourceSelectionBeingDisabled))
			{
				resourceAdditionalInfoLabel.Text = configuration.ReasonForResourceSelectionBeingDisabled;
				resourceAdditionalInfoLabel.IsVisible = true;
			}
		}

		private void SubscribeToFunction()
		{
			function.ResourceSelectionMandatoryChanged += Function_ResourceSelectionMandatoryChanged;
			function.EnforceSelectedResourceChanged += Function_EnforceSelectedResourceChanged;
			function.IncludeUnavailableResourcesChanged += (s, includeUnavailableResources) => IncludeOccupiedResourcesCheckBox.IsChecked = includeUnavailableResources;
			function.ResourceNameConverterChanged += DisplayedFunction_ResourceNameConverterChanged;
			function.ResourceChanged += Function_ResourceChanged;
			function.DisplayedResourcesChanged += Function_DisplayedResourcesChanged;
			function.OccupyingServicesForCurrentResourceChanged += Function_OccupyingServicesForCurrentResourceChanged;
		}

		private void DisplayedFunction_ResourceNameConverterChanged(object sender, EventArgs e)
		{
			ResourceDropDown.Options = GetResourceDropDownOptions();
		}

		private List<string> GetResourceDropDownOptions()
		{
			return function.DisplayedResourceNames.OrderBy(r => r).ToList();
		}

		private void Function_ResourceSelectionMandatoryChanged(object sender, bool e)
		{
			ResourceDropDown.Options = GetResourceDropDownOptions();
			HandleVisibilityAndEnabledUpdate();
		}

		private void Function_EnforceSelectedResourceChanged(object sender, bool e)
		{
			AutomaticCheckBox.IsChecked = !e;
			HandleVisibilityAndEnabledUpdate();
		}

		private void Function_OccupyingServicesForCurrentResourceChanged(object sender, EventArgs e)
		{
			if (function.OccupyingServicesForCurrentResource.Any())
			{
				var sb = new StringBuilder($"Selected resource is used in following orders:\n");

				foreach (var occupyingService in function.OccupyingServicesForCurrentResource)
				{
					foreach (var occupyingOrder in occupyingService.Orders)
					{
						sb.AppendLine($"- {occupyingOrder.Name} from {occupyingService.Service.Start.FromReservation()} until {occupyingService.Service.End.FromReservation()}");
					}
				}

				resourceAdditionalInfoLabel.Text = sb.ToString();
			}
			else
			{
				resourceAdditionalInfoLabel.Text = string.Empty;
			}

			HandleVisibilityAndEnabledUpdate();
		}

		private void Function_DisplayedResourcesChanged(object sender, Function.SelectableResourcesChangedEventArgs e)
		{
			ResourceDropDown.Options = GetResourceDropDownOptions();
			resourceMetaDataLabel.Text = string.Join(Environment.NewLine, function.Resource?.GetMetaData()?.Select(m => m.GetDisplayString()) ?? new string[0]);

			HandleVisibilityAndEnabledUpdate();
		}

		private void Function_ResourceChanged(object sender, Function.ResourceChangedEventArgs e)
		{
			ResourceDropDown.Selected = e.ResourceName;
			resourceMetaDataLabel.Text = string.Join(Environment.NewLine, function.Resource?.GetMetaData()?.Select(m => m.GetDisplayString()) ?? new string[0]);

			HandleVisibilityAndEnabledUpdate();
		}
	}
}
