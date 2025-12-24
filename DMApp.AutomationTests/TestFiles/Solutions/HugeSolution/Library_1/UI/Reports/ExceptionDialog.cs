namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports
{
    using System;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    /// <summary>
    ///	Dialog used to display an Exception.
    /// </summary>
    
    public class ExceptionDialog : Dialog
    {
        private readonly Label exceptionLabel = new Label();

        public ExceptionDialog(IEngine engine) : base(engine)
        {
            Title = "Exception Occurred";
            OkButton = new Button("OK") { Width = 150, Style = ButtonStyle.CallToAction };
            CollapseButton = new CollapseButton() { IsVisible = true, IsCollapsed = true, CollapseText = "Hide Exception", ExpandText = "Show Exception", Width = 150 };
            CollapseButton.Pressed += (o, e) => HandleVisibiltyUpdate();

            GenerateUi();
            HandleVisibiltyUpdate();
        }

        public ExceptionDialog(IEngine engine, Exception exception) : this(engine)
        {
            exceptionLabel.Text = exception.ToString();
        }

        public Button OkButton { get; private set; }

        private CollapseButton CollapseButton { get; set; }

        public string ExceptionContent
        {
            get
            {
                return exceptionLabel.Text;
            }
        }

        internal void GenerateUi()
        {
            int row = -1;

            AddWidget(CollapseButton, ++row, 0);
            AddWidget(exceptionLabel, ++row, 0, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(OkButton, row + 1, 0);
        }

        private void HandleVisibiltyUpdate()
        {
            exceptionLabel.IsVisible = !CollapseButton.IsCollapsed;
        }
    }
}
