using System.Collections.Generic;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	public class RequestResultSection : ResponseSection
	{
		private readonly CollapseButton collapseButton;
		private readonly Label header;
		private readonly List<TextBox> contentTextBoxes = new List<TextBox>();

		public RequestResultSection(string header, params string[] results)
		{
			this.header = new Label(header) { Style = TextStyle.Heading };

			foreach (var result in results)
			{
				contentTextBoxes.Add(new TextBox(result) { IsEnabled = true, IsVisible = true, IsMultiline = true, MinWidth = 600 });
			}

			this.collapseButton = new CollapseButton(contentTextBoxes, true) { CollapseText = "-", ExpandText = "+", Width = 44 };

			GenerateUi();
		}

		public override void Collapse()
		{
			collapseButton.IsCollapsed = true;
		}

        public void SetContentTextBoxWidth(int width)
        {
            foreach (var contentTextBox in contentTextBoxes)
            {
                contentTextBox.Width = width;
            }
        }

        public void SetContentTextBoxHeight(int height)
        {
            foreach (var contentTextBox in contentTextBoxes)
            {
                contentTextBox.Height = height;
            }
        }

        private void GenerateUi()
		{
			int row = -1;

			AddWidget(collapseButton, ++row, 0);
			AddWidget(header, row, 1, 1, 2);

			row++;
			int column = 0;
			foreach (var contentTextBox in contentTextBoxes)
			{
				AddWidget(contentTextBox, row, column, 1, 5);
				column += 5;
			}
		}
	}
}