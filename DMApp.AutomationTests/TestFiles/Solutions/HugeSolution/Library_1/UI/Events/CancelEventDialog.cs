using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;

    public class CancelEventDialog : Dialog
    {
        private readonly Label ongoingEventLabel = new Label("Unable to edit the event as it is already ongoing.");

        private readonly Label runningOrdersLabel = new Label("The event contains active orders. Cancelling the event will cause these orders to be cancelled as well.");

        public CancelEventDialog(IEngine engine, LockInfo lockInfo, bool isOngoing, bool containsActiveOrders) : base((Engine)engine)
        {
            Title = "Cancel Event";

            YesButton = new Button("Yes") { Width = 150 };
            NoButton = new Button("No") { Width = 150 };
            OkButton = new Button("OK") { Width = 150 };

            int row = -1;

            if (!lockInfo.IsLockGranted)
            {
                Label lockedLabel = new Label(String.Format("Unable to edit Event as it is currently locked by {0}", lockInfo.LockUsername));
                AddWidget(lockedLabel, ++row, 0, 1, 3);
                AddWidget(OkButton, ++row, 0);
            }
            else
            {
                if (isOngoing)
                {
                    AddWidget(ongoingEventLabel, ++row, 0, 1, 3);
                    if (containsActiveOrders) AddWidget(runningOrdersLabel, ++row, 0, 1, 3);
                    AddWidget(new Label("Would you like to cancel the event instead?"), ++row, 0, 1, 3);
                }
                else
                {
                    if (containsActiveOrders) AddWidget(runningOrdersLabel, ++row, 0, 1, 3);
                    AddWidget(new Label("Are you sure you want to cancel the event?"), ++row, 0, 1, 3);
                }

                AddWidget(YesButton, ++row, 0);
                AddWidget(NoButton, row, 1);
            }
        }

        public Button YesButton { get; private set; }

        public Button NoButton { get; private set; }

        public Button OkButton { get; private set; }
    }
}
