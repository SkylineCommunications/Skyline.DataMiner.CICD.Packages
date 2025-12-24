namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

    public class ServiceCommentSection : Section
	{
		private readonly Label headerLabel = new Label { Style = TextStyle.Heading };
		private readonly Label commentsLabel = new Label("Comments") { IsVisible = false };
		private readonly YleTextBox commentsTextBox = new YleTextBox() { IsMultiline = true, Height = 250, Width = 400, IsVisible = false };
		private readonly CollapseButton collapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };

		public ServiceCommentSection(string shortDescription, string comments)
		{
			headerLabel.Text = shortDescription;
			commentsTextBox.Text = comments;

			collapseButton.LinkedWidgets.AddRange(new Widget[] { commentsLabel, commentsTextBox });

			GenerateUI();
		}

        public bool IsValid()
        {
			bool isCommentsTextBoxValid = string.IsNullOrWhiteSpace(Comments) || (Comments != null && Comments.Length <= Constants.MaximumAllowedCharacters);
			commentsTextBox.ValidationState = isCommentsTextBoxValid ? Automation.UIValidationState.Valid : Automation.UIValidationState.Invalid;
			commentsTextBox.ValidationText = $"Content shouldn't contain more than {Constants.MaximumAllowedCharacters} characters";

			return isCommentsTextBoxValid;
        }

		private void GenerateUI()
		{
			AddWidget(collapseButton, 0, 0);
			AddWidget(headerLabel, 0, 1, 1, 2);

			AddWidget(commentsLabel, 1, 1, verticalAlignment: VerticalAlignment.Top);
			AddWidget(commentsTextBox, 1, 2);
		}

        public string Comments { get { return commentsTextBox.Text; } }
	}
}