namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class ConfigurationHelper
	{
		private static List<PropertyInfo> GetIsVisibleProperties<T>()
		{
			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var isVisibleProperties = properties.Where(p => p.GetCustomAttributes(typeof(IsIsVisiblePropertyAttribute), true).Any()).ToList();

			return isVisibleProperties;
		}

		private static List<PropertyInfo> GetIsEnabledProperties<T>()
		{
			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var isEnabledProperties = properties.Where(p => p.GetCustomAttributes(typeof(IsIsEnabledProperty), true).Any()).ToList();

			return isEnabledProperties;
		}

		private static List<ISectionConfiguration> GetConfigurationProperties<T>()
		{
			return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).OfType<ISectionConfiguration>().ToList();
		}

		public static void SetIsVisiblePropertyValues<T>(T objectInstance, bool valueToSet)
		{
			var isVisibleProperties = GetIsVisibleProperties<T>();

			foreach (var isVisibleProperty in isVisibleProperties)
			{
				isVisibleProperty.SetValue(objectInstance, valueToSet);
			}

			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in properties)
			{
				var currentPropertyValue = property.GetValue(objectInstance);

				if (currentPropertyValue is ISectionConfiguration sectionConfigurationObject)
				{
					sectionConfigurationObject.SetIsVisiblePropertyValues(valueToSet);
				}
				else if (currentPropertyValue is IEnumerable<ISectionConfiguration> collectionOfsectionConfigurationObjects)
				{
					foreach (var sectionConfiguration in collectionOfsectionConfigurationObjects)
					{
						sectionConfiguration.SetIsVisiblePropertyValues(valueToSet);
					}
				}
				else if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
				{
					SetIsVisibleOnDictionary(valueToSet, currentPropertyValue);
				}
			}		
		}

		private static void SetIsVisibleOnDictionary(bool valueToSet, object currentPropertyValue)
		{
			var dictionary = (IDictionary)currentPropertyValue;

			foreach (var value in dictionary.Values)
			{
				if (typeof(ISectionConfiguration).IsAssignableFrom(value.GetType()))
				{
					var sectionConfiguration = (ISectionConfiguration)value;

					sectionConfiguration.SetIsVisiblePropertyValues(valueToSet);
				}
			}
		}

		public static void SetIsEnabledPropertyValues<T>(T objectInstance, bool valueToSet, Helpers helpers = null)
		{
			var isEnabledProperties = GetIsEnabledProperties<T>();

			foreach (var isEnabledProperty in isEnabledProperties)
			{
				isEnabledProperty.SetValue(objectInstance, valueToSet);
			}

			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in properties)
			{
				var currentPropertyValue = property.GetValue(objectInstance);

				if (currentPropertyValue is ISectionConfiguration sectionConfigurationObject)
				{
					sectionConfigurationObject.SetIsEnabledPropertyValues(valueToSet);
				}
				else if (currentPropertyValue is IEnumerable<ISectionConfiguration> collectionOfSectionConfigurationObjects)
				{
					foreach (var sectionConfiguration in collectionOfSectionConfigurationObjects)
					{
						sectionConfiguration.SetIsEnabledPropertyValues(valueToSet);
					}
				}
				else if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
				{
					SetIsEnabledOnDictionary(valueToSet, currentPropertyValue);
				}
				else 
                {
					// Not support collection type
                }
			}
		}

		private static void SetIsEnabledOnDictionary(bool valueToSet, object currentPropertyValue)
		{
			var dictionary = (IDictionary)currentPropertyValue;

			foreach (var value in dictionary.Values)
			{
				if (typeof(ISectionConfiguration).IsAssignableFrom(value.GetType()))
				{
					var sectionConfiguration = (ISectionConfiguration)value;

					sectionConfiguration.SetIsEnabledPropertyValues(valueToSet);
				}
			}
		}
	}
}
