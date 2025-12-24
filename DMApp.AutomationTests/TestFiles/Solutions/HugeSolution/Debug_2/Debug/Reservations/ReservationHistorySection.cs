using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.Net.History;
using Skyline.DataMiner.Net.History.ReservationInstances;

namespace Debug_2.Debug.Reservations
{
	public sealed class HistorySection : Section
	{
		private readonly Label timeTitle = new Label("Time");
		private readonly Label userTitle = new Label("User");
		private readonly Label DmaIdTitle = new Label("DMA ID");
		private readonly Label TypeTitle = new Label("Type");

		private readonly Label timeLabel = new Label();
		private readonly Label userLabel = new Label();
		private readonly Label DmaIdLabel = new Label();
		private readonly Label TypeLabel = new Label();
		private readonly Label ChangeLabel = new Label { Style = TextStyle.Heading };

		public HistorySection(IChangeDescription changeDescription, DateTime time, string userName, int dmaId, bool isOrderHistory)
		{
			Time = time;

			timeLabel.Text = $"{time.ToUniversalTime()} [UTC]";
			userLabel.Text = userName;
			DmaIdLabel.Text = dmaId.ToString();

			if (changeDescription is ResourceUsageChange resourceUsageChange)
			{
				TypeLabel.Text = "Resources changed";
				ChangeLabel.Text = changeDescription.ToString();
			}
			else if (changeDescription is StatusChange statusChange)
			{
				TypeLabel.Text = "Status changed";
				ChangeLabel.Text = changeDescription.ToString();
			}
			else
			{
				throw new ArgumentException($"{nameof(changeDescription)} type is not supported");
			}

			GenerateUi(isOrderHistory);
		}

		public DateTime Time { get; }

		private void GenerateUi(bool isOrderHistory)
		{
			int row = -1;

			int valueColumnSpan = isOrderHistory ? 2 : 1;
			int valueColumnIdx = isOrderHistory ? 2 : 1;

			AddWidget(ChangeLabel, ++row, 0, 1, valueColumnSpan + 1);

			AddWidget(timeTitle, ++row, 0);
			AddWidget(timeLabel, row, valueColumnIdx, 1, valueColumnSpan);

			AddWidget(TypeTitle, ++row, 0);
			AddWidget(TypeLabel, row, valueColumnIdx, 1, valueColumnSpan);

			AddWidget(userTitle, ++row, 0);
			AddWidget(userLabel, row, valueColumnIdx, 1, valueColumnSpan);

			AddWidget(DmaIdTitle, ++row, 0);
			AddWidget(DmaIdLabel, row, valueColumnIdx, 1, valueColumnSpan);
		}
	}
}
