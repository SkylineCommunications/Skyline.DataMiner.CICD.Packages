namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Integrations.Eurovision
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using System;

    public class EbuDetailsDialog : Dialog
    {
        private readonly Label synopsisContentTitle = new Label { Style = TextStyle.Heading };
        private readonly YleTextBox synopsisContentTextBox = new YleTextBox { IsMultiline = true, Height = 1400, MinWidth = 800 };

        public EbuDetailsDialog(IEngine engine, string transmissionNumber, string synopsisContent) : base((Engine)engine)
        {
            Title = "EBU Details";

            synopsisContentTitle.Text = String.Format("Synopsis for transmission {0}", transmissionNumber);
            synopsisContentTextBox.Text = synopsisContent;
            OkButton = new Button("OK") { Width = 100, Style = ButtonStyle.CallToAction };

            GenerateUI();
        }

        public Button OkButton { get; private set; }

        private void GenerateUI()
        {
            int row = -1;
            AddWidget(synopsisContentTitle, ++row, 0);
            AddWidget(synopsisContentTextBox, ++row, 0);
            AddWidget(new WhiteSpace(), ++row, 0);
            AddWidget(OkButton, ++row, 0);
        }
    }
}
