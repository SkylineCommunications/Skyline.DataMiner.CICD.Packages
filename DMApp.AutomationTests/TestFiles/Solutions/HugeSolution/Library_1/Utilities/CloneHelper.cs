namespace Library_1.Utilities
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class CloneHelper
	{
		private static Type[] enumerablesOfValueTypes = new Type[]
		{
			typeof(IEnumerable<string>),
			typeof(IEnumerable<bool>),
			typeof(IEnumerable<int>),
			typeof(IEnumerable<double>),
			typeof(IEnumerable<char>),
			typeof(IEnumerable<long>),
			typeof(IEnumerable<Guid>),
			typeof(IEnumerable<DateTime>),
			typeof(IEnumerable<TimeSpan>),
		};

		/// <summary>
		/// Makes a deep clone of all properties of all types except reference types that don't implement ICloneable and IEnumerables of reference-types.
		/// </summary>
		/// <remarks>Fields are not copied</remarks>
		public static void CloneProperties<T>(T sourceObject, T destinationObject, List<string> propertiesToIgnore = null)
		{
			propertiesToIgnore = propertiesToIgnore ?? new List<string>();

			var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).Where(p => !propertiesToIgnore.Contains(p.Name)).ToList();

			foreach (var property in properties)
			{
				if (!property.CanWrite) continue;

				var existingValue = property.GetValue(sourceObject);

				object clonedValue;

				if (existingValue is null || property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(string)))
				{
					clonedValue = existingValue;
				}
				else if (typeof(ICloneable).IsAssignableFrom(property.PropertyType))
				{
					clonedValue = ((ICloneable)existingValue).Clone();
				}
				else if(property.PropertyType.GetInterfaces().Any(i => enumerablesOfValueTypes.Contains(i)))
				{
					var copyConstructor = property.PropertyType.GetConstructor(new[] { property.PropertyType });
					if (copyConstructor is null) continue;

					clonedValue = copyConstructor.Invoke(new object[] { existingValue });
				}
				else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
				{
					continue;
				}
				else
				{
					clonedValue = existingValue;
				}

				property.SetValue(destinationObject, clonedValue);
			}
		}
	}
}
