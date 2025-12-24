namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class NotificationSection : Section
	{
		private readonly Label headerLabel = new Label("Email Receivers");
		private readonly TextBox emailTextBox = new TextBox { IsMultiline = true, Height = 200, PlaceHolder = "Enter all Emails that should be notified (separate all emails by comma)." };

		private readonly ISectionConfiguration configuration;
		private readonly Helpers helpers;

		public NotificationSection(Helpers helpers, ISectionConfiguration configuration, NonLiveOrder nonLiveOrder = null)
		{
			if (nonLiveOrder != null) emailTextBox.Text = String.Join(", ", nonLiveOrder.EmailReceivers);

			this.helpers = helpers;
			this.configuration = configuration;

			GenerateUI();
		}

		public List<string> GetEmails()
		{
			List<string> emails = emailTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			return emails.Distinct().ToList();
		}

		protected void GenerateUI()
		{
			this.Clear();

			AddWidget(headerLabel, new WidgetLayout(0, 0));
			AddWidget(emailTextBox, new WidgetLayout(0, 1, 1, 2));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
