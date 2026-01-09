namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class NonLiveUserTaskSection : Section
    {
        protected readonly Helpers helpers;

        protected NonLiveUserTaskSection(Helpers helpers)
        {
            this.helpers = helpers;
        }

        public abstract void GenerateUI();

        public abstract void UpdateUserTask();
    }
}
