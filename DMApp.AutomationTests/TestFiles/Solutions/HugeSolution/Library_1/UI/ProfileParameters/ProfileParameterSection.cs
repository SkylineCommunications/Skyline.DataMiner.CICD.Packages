namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// A section that is used to display a Profile Parameter.
	/// </summary>
	public class ProfileParameterSection : Section
    {
        private readonly ProfileParameter profileParameter;
        private readonly ProfileParameterSectionConfiguration configuration;
        private readonly Label nameLabel = new Label();
        private readonly Label unitLabel = new Label();
        private IYleInteractiveWidget valueWidget;
        private readonly Helpers helpers;


		/// <summary>
		/// Initializes a new instance of the <see cref="ProfileParameterSection" /> class.
		/// </summary>
		/// <param name="profileParameter">ProfileParameter that is displayed by this section.</param>
		/// <param name="configuration">Configuration of the section</param>
		/// <param name="helpers"></param>
		/// <exception cref="ArgumentNullException"/>
		public ProfileParameterSection(ProfileParameter profileParameter, ProfileParameterSectionConfiguration configuration, Helpers helpers = null)
        {
            this.profileParameter = profileParameter ?? throw new ArgumentNullException(nameof(profileParameter));
            this.configuration = configuration;
            this.helpers = helpers;

            Initialize();
            GenerateUi();
            UpdateVisibilityAndEnabledState();
        }

        /// <summary>
        /// Gets the ProfileParameter that is displayed by this Section.
        /// </summary>
        public Guid ProfileParameterId => profileParameter.Id;

        public new bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                UpdateVisibilityAndEnabledState();
            }
        }

        public new bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;
                UpdateVisibilityAndEnabledState();
            }
        }

        public event EventHandler<YleValueWidgetChangedEventArgs> Changed;

        public void RegenerateUi()
        {
            Clear();
            GenerateUi();
        }

        private void UpdateVisibilityAndEnabledState()
        {
            nameLabel.IsVisible = IsVisible && configuration.IsVisible;

            valueWidget.IsVisible = IsVisible && configuration.IsVisible;
            valueWidget.IsEnabled = IsEnabled && configuration.IsEnabled;

            unitLabel.IsVisible = IsVisible && configuration.IsVisible;
        }

        /// <summary>
        /// Initializes the widgets within this section and the linking with the underlying model objects.
        /// </summary>
        private void Initialize()
        {
            nameLabel.Text = profileParameter.Name;
            unitLabel.Text = profileParameter.Unit;

            switch (profileParameter.Type)
            {
                case ParameterType.Number:
                    double value;
                    try
                    {
                        value = Convert.ToDouble(profileParameter.Value);
                    }
                    catch (Exception e)
                    {
                        throw new FormatException($"Unable to parse value {profileParameter.StringValue} for profile parameter {profileParameter.Name} to a double {e}");
                    }

                    var newNumeric = new YleNumeric(value)
                    {
                        StepSize = profileParameter.Stepsize,
						Decimals = profileParameter.Decimals,
						IsVisible = IsVisible
                    };

					if (profileParameter.Id != ProfileParameterGuids.SymbolRate && profileParameter.Id != ProfileParameterGuids.DownlinkFrequency) // Removing slider for symbol rate and downlink frequency
					{
						newNumeric.Maximum = profileParameter.RangeMax;
						newNumeric.Minimum = profileParameter.RangeMin;
					}

					valueWidget = newNumeric;

					break;
                case ParameterType.Discrete:
                    var profileParameterOptions = profileParameter.Discreets.OrderBy(x => x.OrderByValue).Select(d => d.DisplayValue).Except(configuration.DisallowedValues).ToList();
                    valueWidget = new YleDropDown(profileParameterOptions, !String.IsNullOrWhiteSpace(profileParameter.StringValue) ? profileParameter.StringValue : "None") { IsVisible = true };
                    break;
                default:
                    valueWidget = new YleTextBox(profileParameter.StringValue) { IsVisible = true };
                    break;
            }

            valueWidget.Id = profileParameter.Id;
            valueWidget.Name = profileParameter.Name;
            valueWidget.Changed += (o, e) =>
            {
                Changed?.Invoke(this, e);
            };

            profileParameter.ValueChanged += ProfileParameter_ValueChanged;
            profileParameter.ValueValidation.ValidationInfoChanged += ValueValidation_ValidationInfoChanged;
        }

        /// <summary>
        /// Adds the widgets to this section.
        /// </summary>
        private void GenerateUi()
        {
            AddWidget(nameLabel, 0, 0, 1, configuration.LabelSpan);
            AddWidget((InteractiveWidget)valueWidget, 0, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
            AddWidget(unitLabel, 0, configuration.InputWidgetColumn + configuration.InputWidgetSpan, horizontalAlignment: HorizontalAlignment.Left);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }

        /// <summary>
        /// Executed when the value of the linked ProfileParameter is updated.
        /// </summary>
        /// <param name="sender">ProfileParameter of which the value was changed.</param>
        /// <param name="e">Updated value of the ProfileParameter.</param>
        private void ProfileParameter_ValueChanged(object sender, object e)
        {
            switch (profileParameter.Type)
            {
                case ParameterType.Number:
                    YleNumeric yleNumeric = (YleNumeric)valueWidget;
                    yleNumeric.Value = Convert.ToDouble(e);
                    break;
                case ParameterType.Discrete:
                    YleDropDown yleDropDown = (YleDropDown)valueWidget;
                    yleDropDown.Selected = Convert.ToString(e);
                    break;
                default:
                    YleTextBox yleTextBox = (YleTextBox)valueWidget;
                    yleTextBox.Text = Convert.ToString(e);
                    break;
            }
        }

        /// <summary>
        /// Executed when the Validation state of the linked profileParameter is updated.
        /// </summary>
        /// <param name="sender">ProfileParameter of which the validation state was changed.</param>
        /// <param name="e">Information on the updated validation state.</param>
        private void ValueValidation_ValidationInfoChanged(object sender, ValidationInfo.ValidationInfoChangedEventArgs e)
        {
            switch (profileParameter.Type)
            {
                case ParameterType.Number:
                    YleNumeric numeric = (YleNumeric)valueWidget;
                    numeric.ValidationState = e.State;
                    numeric.ValidationText = e.Text;
                    break;
                case ParameterType.Discrete:
                    YleDropDown dropDown = (YleDropDown)valueWidget;
                    dropDown.ValidationState = e.State;
                    dropDown.ValidationText = e.Text;
                    break;
                default:
                    YleTextBox textBox = (YleTextBox)valueWidget;
                    textBox.ValidationState = e.State;
                    textBox.ValidationText = e.Text;
                    break;
            }
        }

        /// <summary>
        /// Generates a HashCode for this object.
        /// </summary>
        /// <returns>HashCode for this object.</returns>
        public override int GetHashCode()
        {
            return profileParameter.GetHashCode();
        }

        /// <summary>
        /// Checks if this object matches another one.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>True if object matches else false.</returns>
        public override bool Equals(object obj)
        {
            ProfileParameterSection other = obj as ProfileParameterSection;
            if (other == null) return false;

            return profileParameter.Id == other.profileParameter.Id;
        }
    }
}