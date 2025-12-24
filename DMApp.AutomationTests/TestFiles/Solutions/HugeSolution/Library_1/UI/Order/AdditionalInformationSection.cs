namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AdditionalInformationSection : Section
    {
        private readonly Order order;
        private readonly AdditionalInformationSectionConfiguration configuration;
        private readonly Helpers helpers;

        private readonly CollapseButton collapseButton = new CollapseButton(true) { CollapseText = "-", ExpandText = "+", Width = 44 };
        private readonly Label titleLabel = new Label("Additional Information") { Style = TextStyle.Bold };

        private readonly Label commentsLabel = new Label("Additional Information") { IsVisible = false };
        private readonly Label mcrOperatorNotesLabel = new Label("MCR Operator Notes") { IsVisible = false };
        private readonly Label mediaOperatorNotesLabel = new Label("Media Operator Notes") { IsVisible = false };
        private readonly Label errorDescriptionLabel = new Label("Error Description") { IsVisible = false };
        private readonly Label reasonForBeingCanceledOrRejectedLabel = new Label("Reason for being ") { IsVisible = false };

        [DisplaysProperty(nameof(Order.Comments))]
        private readonly YleTextBox commentsTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, IsVisible = false };

        [DisplaysProperty(nameof(Order.McrOperatorNotes))]
        private readonly YleTextBox mcrOperatorNotesTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, IsVisible = false };

        [DisplaysProperty(nameof(Order.MediaOperatorNotes))]
        private readonly YleTextBox mediaOperatorNotesTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, IsVisible = false };

        [DisplaysProperty(nameof(Order.ErrorDescription))]
        private readonly YleTextBox errorDescriptionTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, IsVisible = false };

        [DisplaysProperty(nameof(Order.ReasonForCancellationOrRejection))]
        private readonly YleTextBox reasonForBeingCanceledOrRejectedTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100, IsVisible = false };

        public AdditionalInformationSection(Order order, AdditionalInformationSectionConfiguration configuration, Helpers helpers = null)
        {
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;

            Initialize();
            GenerateUi();
            HandleVisibilityAndEnabledUpdate();
        }

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

        public void RegenerateUi()
        {
            Clear();
            GenerateUi();
            HandleVisibilityAndEnabledUpdate();
        }

        public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

        private void Initialize()
        {
            InitializeWidgets();
            SubscribeToWidgets();
            SubscribeToOrder();
        }

        private void InitializeWidgets()
        {
            collapseButton.IsCollapsed = configuration.IsCollapsed;
            commentsTextBox.Text = order.Comments;
            mcrOperatorNotesTextBox.Text = order.McrOperatorNotes;
            mediaOperatorNotesTextBox.Text = order.MediaOperatorNotes;
            errorDescriptionTextBox.Text = order.ErrorDescription;
            reasonForBeingCanceledOrRejectedTextBox.Text = order.ReasonForCancellationOrRejection;
        }

        private void SubscribeToWidgets()
        {
            collapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();

            commentsTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(commentsTextBox)), e.Value));

            mcrOperatorNotesTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(mcrOperatorNotesTextBox)), e.Value));

            mediaOperatorNotesTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(mediaOperatorNotesTextBox)), e.Value));

            errorDescriptionTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(errorDescriptionTextBox)), e.Value));

            reasonForBeingCanceledOrRejectedTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(reasonForBeingCanceledOrRejectedTextBox)), e.Value));
        }

        private void SubscribeToOrder()
        {
            order.CommentsChanged += (s, e) => commentsTextBox.Text = e;
            order.McrOperatorNotesChanged += (s, e) => mcrOperatorNotesTextBox.Text = e;
            order.MediaOperatorNotesChanged += (s, e) => mediaOperatorNotesTextBox.Text = e;
            order.ErrorDescriptionChanged += (s, e) => errorDescriptionTextBox.Text = e;
            order.ReasonForCancellationOrRejectionChanged += (s, e) => reasonForBeingCanceledOrRejectedTextBox.Text = e;

            this.SubscribeToDisplayedObjectValidation(order);
        }

        private void GenerateUi()
        {
            int row = -1;

            AddWidget(collapseButton, ++row, 0);
            AddWidget(titleLabel, row, 1, 1, 8);

            AddWidget(commentsLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(commentsTextBox, row, 4, 1, 3);

            AddWidget(mediaOperatorNotesLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(mediaOperatorNotesTextBox, row, 4, 1, 3);

            AddWidget(mcrOperatorNotesLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(mcrOperatorNotesTextBox, row, 4, 1, 3);

            AddWidget(errorDescriptionLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(errorDescriptionTextBox, row, 4, 1, 3);

            AddWidget(reasonForBeingCanceledOrRejectedLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(reasonForBeingCanceledOrRejectedTextBox, row, 4, 1, 3);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
        {
            collapseButton.IsVisible = IsVisible;
            collapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;

            titleLabel.IsVisible = IsVisible;

            commentsLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentsAreVisible;
            commentsTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentsAreVisible;
            commentsTextBox.IsEnabled = IsEnabled && configuration.CommentsAreEnabled;

            mcrOperatorNotesLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.McrOperatorNotesAreVisible;
            mcrOperatorNotesTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.McrOperatorNotesAreVisible;
            mcrOperatorNotesTextBox.IsEnabled = IsEnabled && configuration.McrOperatorNotesAreEnabled;

            mediaOperatorNotesLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.MediaOperatorNotesAreVisible;
            mediaOperatorNotesTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.MediaOperatorNotesAreVisible;
            mediaOperatorNotesTextBox.IsEnabled = IsEnabled && configuration.MediaOperatorNotesAreEnabled;

            errorDescriptionLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ErrorDescriptionIsVisible;
            errorDescriptionTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ErrorDescriptionIsVisible;
            errorDescriptionTextBox.IsEnabled = IsEnabled && configuration.ErrorDescriptionIsEnabled;

            reasonForBeingCanceledOrRejectedLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ReasonForBeingCancelledOrRejectedIsVisible;
            reasonForBeingCanceledOrRejectedTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ReasonForBeingCancelledOrRejectedIsVisible;
            reasonForBeingCanceledOrRejectedTextBox.IsEnabled = IsEnabled && configuration.ReasonForBeingCancelledOrRejectedIsEnabled;
            
            ToolTipHandler.SetTooltipVisibility(this);
        }
    }
}
