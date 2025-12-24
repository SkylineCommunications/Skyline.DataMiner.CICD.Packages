using System;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Library.UI.NonLive
{
	public class UpdateNonLiveOrderCommentsDialog : Dialog
	{
		private readonly NonLiveOrder nonLiveOrder;

		private readonly Label isilonBackupFileLocationLabel = new Label("Isilon Backup File Location");
		private readonly YleTextBox isilonBackupFileLocationTextBox = new YleTextBox() { IsMultiline = true, Height = 300 };

        private readonly Label operatorCommentsLabel = new Label("Operator Comments");
		private readonly YleTextBox operatorCommentsTextBox = new YleTextBox() { IsMultiline = true, Height = 300 };

		public UpdateNonLiveOrderCommentsDialog(Helpers helpers, NonLiveOrder nonLiveOrder) : base(helpers.Engine)
		{
			Title = "Update Comments";

			this.nonLiveOrder = nonLiveOrder ?? throw new ArgumentNullException(nameof(nonLiveOrder));

			Initialize();
			GenerateUi();
		}

		public YleButton SaveButton { get; } = new YleButton("Save") { Style = ButtonStyle.CallToAction };

		public string IsilonBackupFileLocation => isilonBackupFileLocationTextBox.Text;
		public string OperatorComments => operatorCommentsTextBox.Text;

		private void Initialize()
		{
			isilonBackupFileLocationTextBox.Text = nonLiveOrder.IsilonBackupFileLocation;
            operatorCommentsTextBox.Text = nonLiveOrder.OperatorComment ?? string.Empty;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

            AddWidget(isilonBackupFileLocationLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
            AddWidget(isilonBackupFileLocationTextBox, row, 1);

            AddWidget(operatorCommentsLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(operatorCommentsTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(SaveButton, ++row, 0, 1, 2);
		}
	}
}
