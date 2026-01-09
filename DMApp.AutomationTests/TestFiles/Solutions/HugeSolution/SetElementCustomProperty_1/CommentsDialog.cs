using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetElementCustomProperty_1
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class CommentsDialog : Dialog
	{
		private readonly Label commentsLabel = new Label("Comments");

		public CommentsDialog(IEngine engine, string savedCommentsValue = null, bool showCancelButton = true) : base(engine)
		{
			Title = "Add Comments";

			if (!showCancelButton)
			{
				CancelButton.IsVisible = false;
			}

			// Support edit as well, retrieve property value while loading and put it in the textbox

			Initialize(savedCommentsValue);
			GenerateUI();
		}

		private void Initialize(string savedCommentsValue)
		{
			MessageTextBox.Text = savedCommentsValue;
		}

		public TextBox MessageTextBox { get; private set; } = new TextBox(String.Empty) { IsMultiline = true, MinHeight = 100 };

		public Button OkButton { get; private set; } = new Button("OK");

		public Button CancelButton { get; private set; } = new Button("Cancel");

		public void SetComment(string newCommentValue)
		{
			MessageTextBox.Text = newCommentValue;
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(commentsLabel, ++row, 0);
			AddWidget(MessageTextBox, row, 1);

			row += row + 4;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(OkButton, ++row, 0);
			AddWidget(CancelButton, row, 1);
		}
	}
}
