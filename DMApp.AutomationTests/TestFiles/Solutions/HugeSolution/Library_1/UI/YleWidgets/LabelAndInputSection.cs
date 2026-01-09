namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class LabelAndInputSection : Section
	{
		private readonly Label label;
		private readonly IYleInteractiveWidget inputWidget;

		public LabelAndInputSection(string labelText, Type inputType)
		{
			label = new Label(labelText);
			inputWidget = Mapping.TypeToWidget[inputType].Invoke();

			GenerateUi();
		}

		public string LabelValue => label.Text;

		public object InputValue => inputWidget.Value;

		protected void GenerateUi()
		{
			Clear();

			AddWidget(label, 0, 0);
			AddWidget((InteractiveWidget)inputWidget, 0, 1);
		}
	}
}
