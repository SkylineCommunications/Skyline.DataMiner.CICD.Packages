namespace ShowPlasmaDetails_2
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UIDetailValueLabel : Label
	{
		public UIDetailValueLabel(string text) : base(text)
		{
			if (text == null || text.Equals("-1") || text.Equals("12/29/1899 12:00:00 AM"))
				Text = "N/A";
			else if (text.Length > 200)
				Text = text.Substring(0, 75) + "...";
			else
				Text = text;
		}
	}
}