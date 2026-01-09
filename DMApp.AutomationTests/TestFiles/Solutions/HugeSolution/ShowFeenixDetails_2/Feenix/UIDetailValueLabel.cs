namespace ShowFeenixDetails_2.Feenix
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UIDetailValueLabel : Label
	{
		public UIDetailValueLabel(string text) : base(text)
		{
			if (text == "-1" || text == null)
			{
				Text = "Not found";
			}
			else if (text.Length > 200)
			{
				Text = text.Substring(0, 75) + "...";
			}
			else
			{
				Text = text;
			}
		}
	}
}