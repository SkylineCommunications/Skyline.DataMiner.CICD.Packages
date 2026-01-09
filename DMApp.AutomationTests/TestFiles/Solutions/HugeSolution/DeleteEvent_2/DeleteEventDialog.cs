namespace DeleteEvent_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;

	public class DeleteEventDialog : Dialog
	{
		private readonly Label informationLabel;

		public DeleteEventDialog(Engine engine, LockInfo lockInfo) : base(engine)
		{
			YesButton = new Button("Yes") { Width = 150, Style = ButtonStyle.CallToAction };
			NoButton = new Button("No") { Width = 150 };
			OkButton = new Button("Ok") { Width = 150, Style = ButtonStyle.CallToAction };

			Title = "Delete Event";

			if (lockInfo.IsLockGranted)
			{
				informationLabel = new Label("Are you sure you want to delete this Event?");

				AddWidget(informationLabel, 0, 0, 1, 2);
				AddWidget(YesButton, 1, 0);
				AddWidget(NoButton, 1, 1);
			}
			else
			{
				informationLabel = new Label(String.Format("Unable to delete this Event as it is currently locked by {0}", lockInfo.LockUsername));

				AddWidget(informationLabel, 0, 0, 1, 2);
				AddWidget(OkButton, 1, 0);
			}
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }

		public Button OkButton { get; private set; }
	}
}