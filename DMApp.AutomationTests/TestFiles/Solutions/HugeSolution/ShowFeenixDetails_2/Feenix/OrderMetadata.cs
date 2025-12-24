namespace ShowFeenixDetails_2.Feenix
{
	using System;

	public class OrderMetadata
	{
		public string MetaDataRelationMainClassId { get; set; }

		public string MetaDataRelationMainClassFinnishTitle { get; set; }

		public string MetaDataRelationMainClassSwedishTitle { get; set; }

		public string MetaDataRelationSubClassId { get; set; }

		public string MetaDataRelationSubClassFinnishTitle { get; set; }

		public string MetaDataRelationSubClassSwedishTitle { get; set; }

		public string MetaDataRelationContentClassId { get; set; }

		public string MetaDataRelationContentClassFinnishTitle { get; set; }

		public string MetaDataRelationContentClassSwedishTitle { get; set; }

		public string MetaDataRelationReportingClassId { get; set; }

		public string MetaDataRelationReportingClassFinnishTitle { get; set; }

		public string MetaDataRelationReportingClassSwedishTitle { get; set; }

		public DateTime? MetadataModificationCreated { get; set; }

		public DateTime? MetadataModificationModified { get; set; }

		public DateTime? MetadataModificationDeleted { get; set; }
	}
}