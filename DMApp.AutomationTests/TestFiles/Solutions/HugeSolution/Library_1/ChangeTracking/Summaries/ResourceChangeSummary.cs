namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	using System;
	using Newtonsoft.Json;

	public class ResourceChangeSummary : ChangeSummary
	{
		[Flags]
		private enum TypesOfResourceChanges
		{
			None = 0,
			ResourceWasAddedOrSwapped = 1,
			ResourceWasRemoved = 2,
			ResourceAtBeginningOfServiceDefinitionHasChanged = 4,
			ResourceAtEndOfServiceDefinitionHasChanged = 8,
		}

		private TypesOfResourceChanges typesOfChanges = TypesOfResourceChanges.None;

		public override bool IsChanged => typesOfChanges != TypesOfResourceChanges.None;

		public bool ResourcesAddedOrSwapped => typesOfChanges.HasFlag(TypesOfResourceChanges.ResourceWasAddedOrSwapped);

		public bool ResourcesRemoved => typesOfChanges.HasFlag(TypesOfResourceChanges.ResourceWasRemoved);

		public bool ResourceAtBeginningOfServiceDefinitionChanged => typesOfChanges.HasFlag(TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged);

		public bool ResourceAtEndOfServiceDefinitionChanged => typesOfChanges.HasFlag(TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged);

		public void MarkResourceAddedOrSwapped()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasAddedOrSwapped;
		}

		public void MarkResourceRemoved()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasRemoved;
		}

		public void MarkResourceAtBeginningOfServiceDefinitionAddedOrSwapped()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasAddedOrSwapped;
			typesOfChanges |= TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged;
		}

		public void MarkResourceAtBeginningOfServiceDefinitionRemoved()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasRemoved;
			typesOfChanges |= TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged;
		}

		public void MarkResourceAtEndOfServiceDefinitionAddedOrSwapped()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasAddedOrSwapped;
			typesOfChanges |= TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged;
		}

		public void MarkResourceAtEndOfServiceDefinitionRemoved()
		{
			typesOfChanges |= TypesOfResourceChanges.ResourceWasRemoved;
			typesOfChanges |= TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged;
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is ResourceChangeSummary resourceChangeInfoToAdd)
			{
				typesOfChanges |= resourceChangeInfoToAdd.typesOfChanges;
				return true;
			}

			return false;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
