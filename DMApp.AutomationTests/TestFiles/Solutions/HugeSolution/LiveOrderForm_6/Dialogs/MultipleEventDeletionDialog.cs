namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;

	public class MultipleEventDeletionDialog : Dialog
	{
		private readonly Label informationLabel = new Label("Some enclosing Events from merging Orders have no Orders left.\nSelect the Events you want to delete.");
		private readonly Label eventsLabel = new Label("EVENTS") { Style = TextStyle.Bold };

		private readonly IEnumerable<Event> events;
		private readonly List<CheckBox> enclosingEventCheckBoxes = new List<CheckBox>();

		public MultipleEventDeletionDialog(Engine engine, IEnumerable<Event> events, IEnumerable<LockInfo> lockInfos) : base(engine)
		{
			this.Title = "Delete empty enclosing Event(s)";
			this.events = events;

			foreach (Event @event in events)
			{
				enclosingEventCheckBoxes.Add(new CheckBox(@event.Name) { IsEnabled = !lockInfos.Any(x => x.ObjectId == @event.Id.ToString() && !x.IsLockGranted) });
			}

			DeleteSelectedEventsButton = new Button("Delete Selected Events") { Style = ButtonStyle.CallToAction };

			GenerateUI();
		}

		public Button DeleteSelectedEventsButton { get; set; }

		public IEnumerable<Guid> GetEventGuidsToDelete()
		{
			IEnumerable<string> eventNamesToDelete = enclosingEventCheckBoxes.Where(x => x.IsChecked).Select(x => x.Text);

			return events.Where(x => eventNamesToDelete.Contains(x.Name)).Select(x => x.Id);
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(informationLabel, new WidgetLayout(++row, 0, 2, 1));
			row += 1;

			AddWidget(eventsLabel, new WidgetLayout(++row, 0));

			foreach (CheckBox checkBox in enclosingEventCheckBoxes)
			{
				AddWidget(checkBox, new WidgetLayout(++row, 0));
				if (!checkBox.IsEnabled)
				{
					AddWidget(new Label("Event is locked"), new WidgetLayout(row, 1));
				}
			}

			AddWidget(DeleteSelectedEventsButton, new WidgetLayout(++row, 0));
		}
	}
}