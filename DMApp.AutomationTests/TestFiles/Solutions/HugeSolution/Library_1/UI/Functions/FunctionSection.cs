namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Function = Function.Function;

	/// <summary>
	/// A section that is used to display a Function.
	/// This contains the function profile parameters (except for audio configuration) and it's resource.
	/// </summary>
	public class FunctionSection : YleSection
	{
		private readonly Label functionHeader = new Label { Style = TextStyle.Heading };

		private readonly Label noParametersLabel = new Label("No Parameters");

		private readonly DisplayedFunction displayedFunction;
		private readonly FunctionSectionConfiguration configuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionSection" /> class.
		/// </summary>
		/// <param name="function">Function that is displayed by this section.</param>
		/// <param name="functionSectionConfiguration">Configuration object that will be used to configure this section.</param>
		/// <param name="helpers"></param>
		/// <exception cref="ArgumentNullException"/>
		public FunctionSection(Function function, FunctionSectionConfiguration functionSectionConfiguration, Helpers helpers) : base(helpers)
        {
            this.displayedFunction = function as DisplayedFunction ?? throw new ArgumentNullException(nameof(function));
            this.configuration = functionSectionConfiguration ?? throw new ArgumentNullException(nameof(functionSectionConfiguration));

            Initialize();
            GenerateUI();
            HandleVisibilityAndEnabledUpdate();
        }

        /// <summary>
        /// Gets or sets a value indicating if the section is visible or not.
        /// </summary>
        public new bool IsVisible
		{
			get => base.IsVisible;
			set
			{
				base.IsVisible = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;
			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public Guid FunctionId => displayedFunction.Id;

		public string FunctionDefinitionLabel => displayedFunction.Definition.Label;

		/// <summary>
		/// Gets a list of ProfileParameterSections contained in this section.
		/// </summary>
		public IReadOnlyCollection<ProfileParameterSection> ProfileParameterSections { get; private set; }

		public ResourceSection ResourceSection { get; private set; }

		/// <summary>
		/// Regenerates all underlying UI components.
		/// </summary>
		public override void RegenerateUi()
		{
			Clear();
			foreach (var parameterSection in ProfileParameterSections) parameterSection.RegenerateUi();
			ResourceSection.RegenerateUi();
			GenerateUI();
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			InitializeWidgets();
		}

		private void InitializeWidgets()
		{
			if (configuration.FunctionSectionDividerCharacter != default)
			{
				functionHeader.Text = $"{new string(configuration.FunctionSectionDividerCharacter, 85)}\n{configuration.DisplayedFunctionLabel}";
			}
			else
			{
				functionHeader.Text = configuration.DisplayedFunctionLabel;
			}

			ResourceSection = new ResourceSection(helpers, displayedFunction, configuration.ResourceSectionConfiguration);

			ProfileParameterSections = displayedFunction.NonDtrNonAudioProfileParameters.Select(p => new ProfileParameterSection(p, configuration.ProfileParameterSectionConfigurations[p.Id], helpers)).ToList();
		}

		/// <summary>
		/// Update the visibility of the widgets and underlying sections.
		/// </summary>
		protected override void HandleVisibilityAndEnabledUpdate()
		{
			functionHeader.IsVisible = IsVisible;

			foreach (var profileParameterSection in ProfileParameterSections)
			{
				profileParameterSection.IsVisible = IsVisible && configuration.ProfileParameterSectionConfigurations[profileParameterSection.ProfileParameterId].IsVisible && IsVisibleSpecialCases(profileParameterSection);
				profileParameterSection.IsEnabled = IsEnabled && configuration.ProfileParameterSectionConfigurations[profileParameterSection.ProfileParameterId].IsEnabled;
			}

			ResourceSection.IsVisible = IsVisible;
			ResourceSection.IsEnabled = IsEnabled;

			noParametersLabel.IsVisible = IsVisible && !ProfileParameterSections.Any(s => s.IsVisible) && configuration.ResourceSelectionPosition != HorizontalAlignment.Left;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		/// <summary>
		/// This method contains all special cases for the visibility of profile parameter sections.
		/// </summary>
		/// <param name="profileParameterSection"></param>
		private bool IsVisibleSpecialCases(ProfileParameterSection profileParameterSection)
		{
			// Show Other Satellite Profile Parameter in case Other Satellite is selected
			if (profileParameterSection.ProfileParameterId == ProfileParameterGuids.OtherSatelliteName)
			{
				return displayedFunction.Resource?.GetResourcePropertyBooleanValue(ResourcePropertyNames.OtherSatelliteNameRequired) ?? false;
            }

			// FEC profile param should only be visible for DVB-S modulation standard
			if (profileParameterSection.ProfileParameterId == ProfileParameterGuids.Fec)
			{
				var modulationStandardProfileParameter = displayedFunction.Parameters.SingleOrDefault(p => p.Id == ProfileParameterGuids.ModulationStandard);

				return modulationStandardProfileParameter?.StringValue == "DVB-S";
			}

			// show Service Selection profile parameter only in case selected Channel is "V Sport Live - A vastaanotin", "V Sport Live - B vastaanotin" or "V Sport Live -C vastaanotin"
			if (displayedFunction.Definition.Name.Equals("Fixed Service", StringComparison.InvariantCultureIgnoreCase) && profileParameterSection.ProfileParameterId == ProfileParameterGuids.ServiceSelection)
            {
				var channelProfileParameter = displayedFunction.Parameters.SingleOrDefault(p => p.Id == ProfileParameterGuids.Channel) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.Channel);

                return channelProfileParameter.StringValue.Contains("V Sport Live");
			}

			return true;
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(functionHeader, ++row, 0, 1, 20);

			AddWidget(noParametersLabel, ++row, 0);

			var orderedProfileParameterSectionsAboveResourceSelection = ProfileParameterSections.Where(s => configuration.ProfileParametersInDisplayOrderAboveResourceSelection.Contains(s.ProfileParameterId)).OrderBy(x => configuration.ProfileParametersInDisplayOrderAboveResourceSelection.IndexOf(x.ProfileParameterId));
			
			foreach (var section in orderedProfileParameterSectionsAboveResourceSelection)
			{
				AddSection(section, new SectionLayout(row, 0));
				row += section.RowCount;
			}

			int resourceRow = configuration.ResourceSelectionPosition == HorizontalAlignment.Right ? 0 : row;
			int resourceLabelColumn = configuration.ResourceSelectionPosition == HorizontalAlignment.Right ? configuration.InputColumn + configuration.InputSpan + 1 : 0;

			AddSection(ResourceSection, new SectionLayout(resourceRow, resourceLabelColumn));

			if (configuration.ResourceSelectionPosition == HorizontalAlignment.Left)
			{
				row = ResourceSection.RowCount;
			}

			var orderedProfileParameterSectionsBelowResourceSelection = ProfileParameterSections.Where(s => configuration.ProfileParametersInDisplayOrderBelowResourceSelection.Contains(s.ProfileParameterId)).OrderBy(x => configuration.ProfileParametersInDisplayOrderBelowResourceSelection.IndexOf(x.ProfileParameterId));

			foreach (var section in orderedProfileParameterSectionsBelowResourceSelection)
			{
				AddSection(section, new SectionLayout(row, 0));
				row += section.RowCount;
			}

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}
	}
}