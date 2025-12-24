namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfirmationDialog : Dialog
	{
		private readonly Label messageLabel = new Label();

		public ConfirmationDialog(IEngine engine, string message) : base(engine)
		{
			messageLabel.Text = message;

			AddWidget(messageLabel, 0, 0, 1, 2);
			AddWidget(YesButton, 1, 0, HorizontalAlignment.Left);
			AddWidget(NoButton, 1, 1, HorizontalAlignment.Right);
		}

		public Button YesButton { get; } = new Button("Yes") { Width = 80 };

		public Button NoButton { get; } = new Button("No") { Width = 80 };
	}
}
