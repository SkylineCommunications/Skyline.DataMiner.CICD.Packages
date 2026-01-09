namespace ConfigureContractManager_1
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class UsersGroupWidgets
	{
		public Label Label { get; set; } = new Label();
		public InteractiveWidget InteractiveWidget { get; set; }

		public object WidgetValue
		{
			get
			{
				if (InteractiveWidget is CheckBox checkBox)
				{
					return Convert.ToInt32(checkBox.IsChecked);
				}
				else if (InteractiveWidget is TextBox textBox)
				{
					return textBox.Text;
				}
				else
				{
					throw new NotSupportedException("Widget type not supported.");
				}
			}
		}
	}
}
