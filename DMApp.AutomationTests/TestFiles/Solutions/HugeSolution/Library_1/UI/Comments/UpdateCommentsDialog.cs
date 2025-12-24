namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments
{
	using System;
	using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Status = YLE.Order.Status;

	public class UpdateCommentsDialog : Dialog
	{
		private readonly Order order;
		private readonly UserInfo userInfo;
		private readonly LockInfo lockInfo;

		private readonly Dictionary<Guid, ServiceCommentSection> serviceCommentSections = new Dictionary<Guid, ServiceCommentSection>();
		private readonly Label orderHeader = new Label { Style = TextStyle.Bold };
		private readonly Label commentsLabel = new Label("Comments");
		private readonly Label mcrOperatorNotesLabel = new Label("MCR Operator Notes");
		private readonly Label mediaOperatorNotesLabel = new Label("Media Operator Notes");
		private readonly YleTextBox commentsTextBox = new YleTextBox() { IsMultiline = true, Height = 250, Width = 400 };
		private readonly YleTextBox mcrOperatorNotesTextBox = new YleTextBox() { IsMultiline = true, Height = 250, Width = 400 };
		private readonly YleTextBox mediaOperatorNotesTextBox = new YleTextBox() { IsMultiline = true, Height = 250, Width = 400 };

		private bool isReadOnly;

		public UpdateCommentsDialog(IEngine engine, Order order, LockInfo lockInfo, UserInfo userInfo) : base((Engine)engine)
		{
			this.order = order;
			this.userInfo = userInfo;
			this.lockInfo = lockInfo;

            InitializeServiceCommentSections();

            SaveButton = new Button("Save") { Width = 150, Style = ButtonStyle.CallToAction };

			Title = "Update Comments";
			orderHeader.Text = order.Name;
			commentsTextBox.Text = order.Comments;
			mcrOperatorNotesTextBox.Text = order.McrOperatorNotes;
			mediaOperatorNotesTextBox.Text = order.MediaOperatorNotes;

			mcrOperatorNotesTextBox.Changed += (sender, args) => UpdateValidity();
			mediaOperatorNotesTextBox.Changed += (sender, args) => UpdateValidity();

			GenerateUI();
			UpdateValidity();

			IsReadOnly = !lockInfo.IsLockGranted;
		}

        public string OrderComments { get { return commentsTextBox.Text; } }

        public string OrderMcrOperatorNotes { get { return userInfo.IsMcrUser ? mcrOperatorNotesTextBox.Text : null; } }

        public string OrderMediaOperatorNotes => mediaOperatorNotesTextBox.Text;

        internal bool AreServiceCommentsSectionsValid => serviceCommentSections.Values.All(serviceCommentSection => serviceCommentSection != null && serviceCommentSection.IsValid());

        public Button SaveButton { get; private set; }

        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }

            set
            {
                isReadOnly = value;

                InteractiveWidget interactiveWidget;
                foreach (Widget widget in Widgets)
                {
                    interactiveWidget = widget as InteractiveWidget;
                    if (interactiveWidget != null && !(interactiveWidget is CollapseButton))
                    {
                        interactiveWidget.IsEnabled = !value;
                    }
                }
            }
        }

        public string GetServiceComments(Guid guid)
		{
			ServiceCommentSection serviceCommentsSection;
			if (serviceCommentSections.TryGetValue(guid, out serviceCommentsSection))
			{
				return serviceCommentsSection.Comments.Clean(allowSiteContent: true);
			}

			return null;
		}

		public bool IsValid()
		{
            bool isOrderMcrOperatorNotesEmpty = String.IsNullOrWhiteSpace(OrderMcrOperatorNotes);
            bool completedWithErrorsAndMcrNotesAreEmpty = order.Status == Status.CompletedWithErrors && isOrderMcrOperatorNotesEmpty;
            bool amountOfMcrOperatorNotesCharsIsValid = isOrderMcrOperatorNotesEmpty || mcrOperatorNotesTextBox.Text?.Length <= Constants.MaximumAllowedCharacters;

            bool areMcrOperatorNotesValid = !completedWithErrorsAndMcrNotesAreEmpty && amountOfMcrOperatorNotesCharsIsValid;
            mcrOperatorNotesTextBox.ValidationState = areMcrOperatorNotesValid ? UIValidationState.Valid : UIValidationState.Invalid;
            if (completedWithErrorsAndMcrNotesAreEmpty) mcrOperatorNotesTextBox.ValidationText = "The Order was completed with Errors, provide some information about what went wrong";
            else if (!amountOfMcrOperatorNotesCharsIsValid) mcrOperatorNotesTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";
            else mcrOperatorNotesTextBox.ValidationText = string.Empty;

            bool isOrderMediaOperatorNotesEmpty = String.IsNullOrWhiteSpace(OrderMediaOperatorNotes);
            bool amountOfMediaOperatorNotesCharsIsValid = isOrderMediaOperatorNotesEmpty || mediaOperatorNotesTextBox.Text?.Length <= Constants.MaximumAllowedCharacters;

            bool areMediaOperatorNotesValid = amountOfMediaOperatorNotesCharsIsValid;
            mediaOperatorNotesTextBox.ValidationState = areMediaOperatorNotesValid ? UIValidationState.Valid : UIValidationState.Invalid;
            mediaOperatorNotesTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

            bool areOperatorNotesValid = areMcrOperatorNotesValid && areMediaOperatorNotesValid;

            bool areCommentSectionsValid = string.IsNullOrWhiteSpace(commentsTextBox.Text) || commentsTextBox.Text.Length <= Constants.MaximumAllowedCharacters;
            commentsTextBox.ValidationState = areCommentSectionsValid ? UIValidationState.Valid : UIValidationState.Invalid;
            commentsTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

            return areOperatorNotesValid && areCommentSectionsValid && AreServiceCommentsSectionsValid;
		}


        private void InitializeServiceCommentSections()
        {
            foreach (var service in order.AllServices.Where(s => s != null))
            {
                if (service.IsUnknownSourceService)
                {
                    serviceCommentSections.Add(service.Id, new ServiceCommentSection(service.GetShortDescription(order), service.AdditionalDescriptionUnknownSource));
                    continue;
                }

                serviceCommentSections.Add(service.Id, new ServiceCommentSection(service.GetShortDescription(order), service.Comments));
            }
        }

        private void UpdateValidity()
        {
            IsValid();
        }

        private void GenerateUI()
        {
            int row = -1;

            if (!lockInfo.IsLockGranted)
            {
                AddWidget(new Label(String.Format("Unable to update comments as this order is currently locked by {0}", lockInfo.LockUsername)), ++row, 0, 1, 3);
            }

            AddWidget(orderHeader, ++row, 0, 1, 3);

            AddWidget(commentsLabel, ++row, 0, 1, 2, verticalAlignment: VerticalAlignment.Top);
            AddWidget(commentsTextBox, row, 2);

            AddWidget(mediaOperatorNotesLabel, ++row, 0, 1, 2, verticalAlignment: VerticalAlignment.Top);
            AddWidget(mediaOperatorNotesTextBox, row, 2);
            
            AddWidget(mcrOperatorNotesLabel, ++row, 0, 1, 2, verticalAlignment: VerticalAlignment.Top);
            AddWidget(mcrOperatorNotesTextBox, row, 2);

            foreach (var id in order.AllServices.Select(service => service.Id))
            {
                AddSection(serviceCommentSections[id], new SectionLayout(++row, 0));
                row += serviceCommentSections[id].RowCount;
            }

            AddWidget(SaveButton, row + 1, 0, 1, 3);

            SetColumnWidth(0, 40);
        }
    }
}
