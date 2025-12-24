namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using NPOI.SS.Formula.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class ChangeTrackingHelper
	{
		private static List<PropertyInfo> GetChangeTrackedProperties<T>()
		{
			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var changeTrackedProperties = properties.Where(p => p.GetCustomAttributes(typeof(ChangeTrackedAttribute), true).Any()).ToList();

			return changeTrackedProperties;
		}

		/// <summary>
		/// Updates the given dictionary with the current values of the ChangeTracked properties of the objectInstance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectInstance">An instance of type T to take the current values of ChangeTracked properties from.</param>
		/// <param name="initialValues">The dictionary of initial values to be updated.</param>
		/// <param name="helpers">Optional Helpers object for logging.</param>
		public static void AcceptChanges<T>(T objectInstance, Dictionary<string, object> initialValues, Helpers helpers = null)
		{
			helpers?.LogMethodStart(typeof(T).Name, nameof(AcceptChanges), out var stopwatch, ((IYleChangeTracking)objectInstance).UniqueIdentifier);

			foreach (var property in GetChangeTrackedProperties<T>())
			{
				var currentValue = property.GetValue(objectInstance);

				helpers?.Log(typeof(T).Name, nameof(AcceptChanges), $"Setting initial value for property {property.Name} to '{currentValue}'");

				CallAccceptChangesOnChangeTrackingObjects(helpers, property.Name, currentValue);

				SaveCurrentValueAsInitialValue(helpers, initialValues, property, currentValue);
			}

			helpers?.Log(typeof(T).Name, nameof(AcceptChanges), $"Initial values: \n{string.Join("\n", initialValues.Select(v => $"{v.Key}={v.Value}"))}");

			helpers?.LogMethodCompleted(typeof(T).Name, nameof(AcceptChanges));
		}

		public static Change GetUpdatedChange<T>(T objectInstance, Dictionary<string, object> initialValues, Change changeToUpdate, Helpers helpers = null)
		{
			foreach (var property in GetChangeTrackedProperties<T>())
			{
				try
				{
					var currentValue = property.GetValue(objectInstance);

					if (currentValue is IYleChangeTracking changeTrackingObject)
					{
						RegisterChangeTrackingChanges(initialValues, changeToUpdate, changeTrackingObject, property, currentValue, helpers);
					}
					else if (currentValue is IEnumerable<IYleChangeTracking> collectionOfChangeTrackingObjects)
					{
						RegisterChangeTrackingCollectionChanges(initialValues, changeToUpdate, collectionOfChangeTrackingObjects, property, helpers);
					}
					else if (!(currentValue is string) && currentValue is IEnumerable)
					{
						RegisterCollectionChanges(initialValues, changeToUpdate, property, currentValue, helpers);
					}
					else
					{
						RegisterObjectChanges(initialValues, changeToUpdate, property, currentValue, helpers);
					}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Getting change for property {property.Name} failed for object '{((IYleChangeTracking)objectInstance).UniqueIdentifier}' of type {typeof(T).Name}: {ex}");
				}
			}

			return changeToUpdate;
		}

		private static void RegisterChangeTrackingChanges(Dictionary<string, object> initialValues, Change changeToUpdate, IYleChangeTracking changeTrackingObject, PropertyInfo property, object currentValue, Helpers helpers = null)
		{
			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Property {property.Name} implements {nameof(IYleChangeTracking)}");

			bool successfullyAdded;
			if (initialValues[property.Name] is null)
			{
				successfullyAdded = changeToUpdate.TryAddChange(new PropertyChange(property.Name, string.Empty, currentValue.ToString()));
			}
			else
			{
				successfullyAdded = changeToUpdate.TryAddChange(changeTrackingObject.Change);
			}

			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding change for ChangeTracking object {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
		}

		private static void RegisterChangeTrackingCollectionChanges(Dictionary<string, object> initialValues, Change changeToUpdate, IEnumerable<IYleChangeTracking> collectionOfChangeTrackingObjects, PropertyInfo property, Helpers helpers = null)
		{
			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Property {property.Name} implements IEnumerable<{nameof(IYleChangeTracking)}>");

			foreach (var changeTrackingItem in collectionOfChangeTrackingObjects)
			{
				bool successfullyAdded = changeToUpdate.TryAddChange(changeTrackingItem.Change);

				helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding change for item {changeTrackingItem.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}

			var initialValue = initialValues[property.Name] as IEnumerable<IYleChangeTracking>;

			var addedObjects = collectionOfChangeTrackingObjects.Except(initialValue).ToList();
			var removedObjects = initialValue.Except(collectionOfChangeTrackingObjects).ToList();

			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Removed objects in collection {property.Name}: '{string.Join(", ", removedObjects.Select(o => o.UniqueIdentifier))}'");
			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Added objects in collection {property.Name}: '{string.Join(", ", addedObjects.Select(o => o.UniqueIdentifier))}'");

						foreach (var addedObject in addedObjects)
						{
							bool successfullyAdded = changeToUpdate.TryAddChange(new CollectionChanges(property.Name, CollectionChangeType.Add, addedObject.UniqueIdentifier, addedObject.DisplayName));

				helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding collection change for added item {addedObject.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}

						foreach (var removedObject in removedObjects)
						{
							bool successfullyAdded = changeToUpdate.TryAddChange(new CollectionChanges(property.Name, CollectionChangeType.Remove, removedObject.UniqueIdentifier, removedObject.DisplayName));

				helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding collection change for removed item {removedObject.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}
		}

		private static void RegisterCollectionChanges(Dictionary<string, object> initialValues, Change changeToUpdate, PropertyInfo property, object currentValue, Helpers helpers = null)
		{
			CollectionChanges collectionChanges = null;

			if (currentValue is IEnumerable<int> integerCollection)
			{
				var initialCollection = initialValues[property.Name] as IEnumerable<int>;
				collectionChanges = CreateCollectionChange(helpers, property.Name, integerCollection, initialCollection);
			}
			else if (currentValue is IEnumerable<string> stringCollection)
			{
				var initialCollection = initialValues[property.Name] as IEnumerable<string>;
				collectionChanges = CreateCollectionChange(helpers, property.Name, stringCollection, initialCollection);
			}
			else if (currentValue is IEnumerable<Guid> guidCollection)
			{
				var initialCollection = initialValues[property.Name] as IEnumerable<Guid>;
				collectionChanges = CreateCollectionChange(helpers, property.Name, guidCollection, initialCollection);
			}
			else
			{
				throw new NotSupportedException($"Property {property.Name} of class {typeof(T).Name} is a collection of an unsupported type");
			}

			if (collectionChanges != null && collectionChanges.Changes.Any())
			{
				changeToUpdate.TryAddChange(collectionChanges);
			}
		}

		private static void RegisterObjectChanges(Dictionary<string, object> initialValues, Change changeToUpdate, PropertyInfo property, object currentValue, Helpers helpers = null)
		{
			var initialValueToSave = initialValues[property.Name]?.ToString() ?? String.Empty;
			var currentValueToSave = currentValue?.ToString() ?? String.Empty;

			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Accepting value '{currentValueToSave}' for property {property.Name}, initial value is '{initialValueToSave}'");

			if (initialValueToSave != currentValueToSave)
			{
				var propertyChangeHistory = new PropertyChange(property.Name, initialValueToSave, currentValueToSave);

				bool successfullyAdded = changeToUpdate.TryAddChange(propertyChangeHistory);

				helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding change for property {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}
		}

		public static Change GetChangeComparedTo<T>(T firstObjectInstance, T secondObjectInstance, Change changeToUpdate, Helpers helpers = null)
		{
			foreach (var property in GetChangeTrackedProperties<T>())
			{
				var firstValue = property.GetValue(firstObjectInstance);
				var secondValue = property.GetValue(secondObjectInstance);

				if (firstValue is IYleChangeTracking firstChangeTrackingObject)
				{
					RegisterChangeTrackingChangeComparedTo(firstChangeTrackingObject, secondValue, property, changeToUpdate, helpers);
				}
				else if (firstValue is IEnumerable<IYleChangeTracking> firstCollectionOfChangeTrackingObjects && secondValue is IEnumerable<IYleChangeTracking> secondCollectionOfChangeTrackingObjects)
				{
					RegisterChangeTrackingCollectionChangeComparedTo(firstCollectionOfChangeTrackingObjects, secondCollectionOfChangeTrackingObjects, property, changeToUpdate, helpers);
				}
				else if (!(firstValue is string) && firstValue is IEnumerable)
				{
					RegisterCollectionChangesComparedTo(firstValue, secondValue, property, changeToUpdate, helpers);
				}
				else
				{
					RegisterObjectChangesComparedTo(firstValue, secondValue, property, changeToUpdate, helpers);
				}
			}

			return changeToUpdate;
		}

		private static void RegisterChangeTrackingChangeComparedTo(IYleChangeTracking firstValue, object secondValue, PropertyInfo property, Change changeToUpdate, Helpers helpers = null)
		{
			helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Property {property.Name} implements {nameof(IYleChangeTracking)}");

			bool successfullyAdded = false;
			if (secondValue is null)
			{
				successfullyAdded = changeToUpdate.TryAddChange(new PropertyChange(property.Name, string.Empty, firstValue.ToString()));
			}
			else if (secondValue is IYleChangeTracking secondChangeTrackingObject)
			{
				var change = firstValue.GetChangeComparedTo(helpers, secondChangeTrackingObject);
				successfullyAdded = changeToUpdate.TryAddChange(change);
			}
			else
			{
				// nothing
			}

			helpers?.Log(typeof(T).Name, nameof(GetUpdatedChange), $"Adding change for ChangeTracking object {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
		}

		private static void RegisterChangeTrackingCollectionChangeComparedTo(IEnumerable<IYleChangeTracking> firstCollectionOfChangeTrackingObjects, IEnumerable<IYleChangeTracking> secondCollectionOfChangeTrackingObjects, PropertyInfo property, Change changeToUpdate, Helpers helpers = null)
		{
			helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Property {property.Name} implements IEnumerable<{nameof(IYleChangeTracking)}>");

			foreach (var changeTrackingItem in firstCollectionOfChangeTrackingObjects)
			{
				var otherChangeTrackingItem = secondCollectionOfChangeTrackingObjects.SingleOrDefault(o => o.UniqueIdentifier == changeTrackingItem.UniqueIdentifier);
				if (otherChangeTrackingItem is null) continue;

				var change = changeTrackingItem.GetChangeComparedTo(helpers, otherChangeTrackingItem);

				bool successfullyAdded = changeToUpdate.TryAddChange(change);

				helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Adding change for item {changeTrackingItem.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}

			var addedObjects = firstCollectionOfChangeTrackingObjects.Except(secondCollectionOfChangeTrackingObjects).ToList();
			var removedObjects = secondCollectionOfChangeTrackingObjects.Except(firstCollectionOfChangeTrackingObjects).ToList();

			helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Removed objects in collection {property.Name}: '{string.Join(", ", removedObjects.Select(o => o.UniqueIdentifier))}'");
			helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Added objects in collection {property.Name}: '{string.Join(", ", addedObjects.Select(o => o.UniqueIdentifier))}'");

					foreach (var addedObject in addedObjects)
					{
						bool successfullyAdded = changeToUpdate.TryAddChange(new CollectionChanges(property.Name, CollectionChangeType.Add, addedObject.UniqueIdentifier, addedObject.DisplayName));

				helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Adding collection change for added item {addedObject.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}

					foreach (var removedObject in removedObjects)
					{
						bool successfullyAdded = changeToUpdate.TryAddChange(new CollectionChanges(property.Name, CollectionChangeType.Remove, removedObject.UniqueIdentifier, removedObject.DisplayName));

				helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Adding collection change for removed item {removedObject.UniqueIdentifier} in collection {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}
		}

		private static void RegisterCollectionChangesComparedTo(object firstValue, object secondValue, PropertyInfo property, Change changeToUpdate, Helpers helpers = null)
		{
			CollectionChanges collectionChanges = null;

			if (firstValue is IEnumerable<int> integerCollection && secondValue is IEnumerable<int> secondIntegerCollection)
			{
				collectionChanges = CreateCollectionChange(helpers, property.Name, integerCollection, secondIntegerCollection);
			}
			else if (firstValue is IEnumerable<string> stringCollection && secondValue is IEnumerable<string> secondStringCollection)
			{
				collectionChanges = CreateCollectionChange(helpers, property.Name, stringCollection, secondStringCollection);
			}
			else if (firstValue is IEnumerable<Guid> guidCollection && secondValue is IEnumerable<Guid> secondGuidCollection)
			{
				collectionChanges = CreateCollectionChange(helpers, property.Name, guidCollection, secondGuidCollection);
			}
			else
			{
				throw new NotSupportedException($"Property {property.Name} of class {typeof(T).Name} is a collection of an unsupported type");
			}

			if (collectionChanges != null && collectionChanges.Changes.Any())
			{
				changeToUpdate.TryAddChange(collectionChanges);
			}
		}

		private static void RegisterObjectChangesComparedTo(object firstValue, object secondValue, PropertyInfo property, Change changeToUpdate, Helpers helpers = null)
		{
			var currentValueToSave = firstValue?.ToString() ?? String.Empty;
			var initialValueToSave = secondValue?.ToString() ?? String.Empty;

			helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Accepting value '{currentValueToSave}' for property {property.Name}, initial value is '{initialValueToSave}'");

			if (initialValueToSave != currentValueToSave)
			{
				var propertyChangeHistory = new PropertyChange(property.Name, initialValueToSave, currentValueToSave);

				bool successfullyAdded = changeToUpdate.TryAddChange(propertyChangeHistory);

				helpers?.Log(typeof(T).Name, nameof(GetChangeComparedTo), $"Adding change for property {property.Name} {(successfullyAdded ? "succeeded" : "failed")}");
			}
		}

		private static CollectionChanges CreateCollectionChange<T>(Helpers helpers, string collectionName, IEnumerable<T> newCollection, IEnumerable<T> initialCollection)
		{
			var collectionChange = new CollectionChanges(collectionName);

			var removedObjects = initialCollection.Except(newCollection).ToList();
			var addedObjects = newCollection.Except(initialCollection).ToList();

			helpers?.Log(typeof(ChangeTrackingHelper).Name, nameof(GetUpdatedChange), $"{removedObjects.Count} removed objects in collection {collectionName}: '{string.Join(", ", removedObjects.Select(ro => ro.ToString()))}'");
			helpers?.Log(typeof(ChangeTrackingHelper).Name, nameof(GetUpdatedChange), $"{addedObjects.Count} added objects in collection {collectionName}: '{string.Join(", ", addedObjects.Select(ro => ro.ToString()))}'");

			foreach (var addedObject in addedObjects)
			{
				collectionChange.AddCollectionChange(new CollectionChange
				{
					ItemIdentifier = addedObject.ToString(),
					Type = CollectionChangeType.Add,
					DisplayName = addedObject.ToString(),
				});
			}

			foreach (var removedObject in removedObjects)
			{
				collectionChange.AddCollectionChange(new CollectionChange
				{
					ItemIdentifier = removedObject.ToString(),
					Type = CollectionChangeType.Remove,
					DisplayName = removedObject.ToString(),
				});
			}

			return collectionChange;
		}

		private static void SaveCurrentValueAsInitialValue(Helpers helpers, Dictionary<string, object> initialValues, PropertyInfo property, object currentValue)
		{
			object initialValueToSave;

			if (!(currentValue is string) && currentValue is IEnumerable)
			{
				helpers?.Log(nameof(ChangeTrackingHelper), nameof(AcceptChanges), $"Creating collection copy for property {property.Name} to set as initial value");

				var copyConstructor = property.PropertyType.GetConstructor(new[] { property.PropertyType });
				initialValueToSave = copyConstructor.Invoke(new object[] { currentValue });
			}
			else
			{
				initialValueToSave = currentValue;
			}

			initialValues[property.Name] = initialValueToSave;
		}

		private static void CallAccceptChangesOnChangeTrackingObjects(Helpers helpers, string propertyName, object currentPropertyValue)
		{
			if (currentPropertyValue is IYleChangeTracking changeTrackingObject)
			{
				helpers?.Log(nameof(ChangeTrackingHelper), nameof(AcceptChanges), $"Property {propertyName} implements {nameof(IYleChangeTracking)}, calling {nameof(AcceptChanges)}");

				changeTrackingObject.AcceptChanges();
			}
			else if (currentPropertyValue is IEnumerable<IYleChangeTracking> collectionOfChangeTrackingObjects)
			{
				helpers?.Log(nameof(ChangeTrackingHelper), nameof(AcceptChanges), $"Property {propertyName} implements IEnumerable<{nameof(IYleChangeTracking)}>, calling {nameof(AcceptChanges)} on each element");

				foreach (var changeTrackingItem in collectionOfChangeTrackingObjects)
				{
					changeTrackingItem.AcceptChanges();
				}
			}
			else
			{
				// No change tracking object
			}
		}
	}
}
