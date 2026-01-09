namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class PropertyCopier<T> where T : class
    {
        /// <summary>
        /// Will copy any matching property based on name, type and attribute from parent to child.
        /// </summary>
        /// <param name="parent">Parent class which contains the property data to copy from.</param>
        /// <param name="child">Child class which needs to be updated by the parent class</param>
        public static void Copy(T parent, T child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    var attributesForProperty = childProperty.GetCustomAttributes(typeof(CopyAttribute), true);
                    bool matchesAttribute = attributesForProperty.Any(attribute => attribute is CopyAttribute);

                    bool isSetAllowed = matchesAttribute && childProperty.CanWrite && parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType;
                    if (isSetAllowed)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }
}
