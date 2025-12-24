using System;
using System.Collections.Generic;
using System.Text;

namespace kyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	public abstract class DisplayedObjectConfiguration
	{
		private Dictionary<string, bool> propertyVisibilities = new Dictionary<string, bool>();

		private Dictionary<string, bool> propertyEnabledStates = new Dictionary<string, bool>();

        public void SetPropertyVisibilityConfiguration(string propertyName, bool isVisible)
        {
            propertyVisibilities[propertyName] = isVisible;
        }

        public bool GetPropertyVisibility(string propertyName)
        {
            if (!propertyVisibilities.TryGetValue(propertyName, out var isVisible))
            {
                isVisible = true;
                propertyVisibilities.Add(propertyName, isVisible);
            }

            return isVisible;
        }

        public void SetPropertyEnabledStateConfiguration(string propertyName, bool isEnabled)
        {
            propertyEnabledStates[propertyName] = isEnabled;
        }

        public bool GetPropertyEnabledState(string propertyName)
        {
            if (!propertyEnabledStates.TryGetValue(propertyName, out var isEnabled))
            {
                isEnabled = true;
                propertyEnabledStates.Add(propertyName, isEnabled);
            }

            return isEnabled;
        }
    }
}
