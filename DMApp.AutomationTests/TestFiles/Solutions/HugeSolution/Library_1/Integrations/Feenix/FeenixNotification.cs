namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Feenix
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
    using System.Linq;
    using System.Text;

	public class FeenixNotification
	{
		private const string NotificationDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

		public Guid Id { get; set; }

		public string FinnishTitle { get; set; }

		public string SwedishTitle { get; set; }

		public string SamiTitle { get; set; }

		public DateTime Start { get; set; }

		public DateTime End { get; set; }

		public string SourceResourceName { get; set; }

		public string DestinationResourceName { get; set; }

		public string CeitonProjectId { get; set; }

		public string CeitonProductId { get; set; }

        public override string ToString()
		{
			var notification = "\"{\\\"description\\\":[{\\\"value\\\":{\\\"fin\\\":\\\"Kuvausteksti\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"main\\\"}],\\\"mediaResource\\\":[{\\\"type\\\":\\\"ImageResource\\\",\\\"language\\\":\\\"fin\\\",\\\"id\\\":\\\"13-1-65776800-1683273354261\\\",\\\"kind\\\":\\\"main\\\",\\\"version\\\":1683273494}],\\\"alternativeId\\\":[\\\"16-8-U0450250\\\",\\\"54-1-65776800\\\"],\\\"productionModel\\\":\\\"co-production\\\",\\\"type\\\":\\\"ProgramEditorialObject\\\",\\\"interaction\\\":[{\\\"type\\\":\\\"Interaction\\\",\\\"title\\\":{\\\"value\\\":{\\\"fin\\\":\\\"Linkin teksti\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"short\\\"},\\\"kind\\\":\\\"webPage\\\",\\\"url\\\":\\\"https://arenan.yle.fi/1-3296106\\\"},{\\\"type\\\":\\\"Interaction\\\",\\\"title\\\":{\\\"value\\\":{\\\"fin\\\":\\\"Elävä arkisto: \\\\\"Kansalaiset, medborgare\\\\\" ovat uudenvuoden avainsanat\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"short\\\"},\\\"kind\\\":\\\"webPage\\\",\\\"url\\\":\\\"https://www.yhteisvastuu.fi/osallistu/#tule-vapaaehtoiseksi\\\"}],\\\"promotionVideo\\\":[{\\\"type\\\":\\\"RelationObject\\\",\\\"language\\\":\\\"fin\\\",\\\"kind\\\":\\\"hero\\\",\\\"object\\\":{\\\"type\\\":\\\"ProgramEditorialObject\\\",\\\"id\\\":\\\"1-51004048\\\"}}],\\\"contentRating\\\":{\\\"type\\\":\\\"ContentRating\\\",\\\"title\\\":[{\\\"value\\\":{\\\"eng\\\":\\\"Not for persons under 7\\\",\\\"swe\\\":\\\"Förbjudet under 7 år\\\",\\\"fin\\\":\\\"Sallittu yli 7-vuotiaille\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"main\\\"}],\\\"ageRestriction\\\":7,\\\"reason\\\":[{\\\"key\\\":\\\"anxiety\\\",\\\"type\\\":\\\"ContentRatingReason\\\"}],\\\"ratingSystem\\\":\\\"meku\\\"},\\\"production\\\":[{\\\"value\\\":\\\"99740129367\\\",\\\"type\\\":\\\"IdentityObject\\\",\\\"kind\\\":\\\"productionId\\\"},{\\\"value\\\":\\\"40-1-[CEITON PRODUCT ID]\\\",\\\"type\\\":\\\"IdentityObject\\\",\\\"kind\\\":\\\"productId\\\"},{\\\"value\\\":\\\"40-2-[CEITON PROJECT ID]\\\",\\\"type\\\":\\\"IdentityObject\\\",\\\"kind\\\":\\\"projectId\\\"},{\\\"value\\\":\\\"60-11306\\\",\\\"type\\\":\\\"IdentityObject\\\",\\\"kind\\\":\\\"departmentCode\\\"},{\\\"value\\\":\\\"2023\\\",\\\"type\\\":\\\"IdentityObject\\\",\\\"kind\\\":\\\"productionYear\\\"}],\\\"title\\\":[{\\\"value\\\":{\\\"fin\\\":\\\"[FINNISH TITLE]\\\", \\\"swe\\\":\\\"[SWEDISH TITLE]\\\", \\\"smi\\\":\\\"[SAMI TITLE]\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"main\\\"},{\\\"value\\\":{\\\"fin\\\":\\\"11\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"episode\\\"},{\\\"value\\\":{\\\"fin\\\":\\\"22\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"transmission\\\"},{\\\"value\\\":{\\\"fin\\\":\\\"33\\\"},\\\"type\\\":\\\"TextObject\\\",\\\"kind\\\":\\\"promotion\\\"}],\\\"status\\\":\\\"ReadyToPublish\\\",\\\"id\\\":\\\"[ID]\\\",\\\"kind\\\":\\\"tvprogram\\\",\\\"version\\\":[{\\\"mediaResource\\\":[{\\\"format\\\":{\\\"mode\\\":\\\"1080p\\\",\\\"framerate\\\":50},\\\"type\\\":\\\"WebcastResource\\\",\\\"hasSource\\\":[{\\\"name\\\":\\\"[SOURCE RESOURCE NAME]\\\",\\\"type\\\":\\\"Signal\\\"}],\\\"language\\\":\\\"swe\\\",\\\"id\\\":\\\"[DESTINATION RESOURCE NAME]\\\",\\\"hasContact\\\":[{\\\"value\\\":\\\"lundati\\\",\\\"type\\\":\\\"Text\\\"}],\\\"track\\\":[{\\\"format\\\":\\\"stereo\\\",\\\"type\\\":\\\"AudioTrack\\\",\\\"duration\\\":\\\"PT900S\\\",\\\"language\\\":\\\"fin\\\"},{\\\"format\\\":\\\"stereo\\\",\\\"type\\\":\\\"AudioTrack\\\",\\\"duration\\\":\\\"PT900S\\\",\\\"language\\\":\\\"swe\\\"}]}],\\\"type\\\":\\\"PublicationEditorialObject\\\",\\\"publicationEvent\\\":{\\\"service\\\":{\\\"id\\\":\\\"yle-areena\\\",\\\"kind\\\":\\\"main\\\"},\\\"publisher\\\":{\\\"id\\\":\\\"radio-vega-osterbotten\\\"},\\\"startTime\\\":\\\"[START TIME]\\\",\\\"endTime\\\":\\\"[END TIME]\\\",\\\"type\\\":\\\"ScheduledTransmission\\\",\\\"duration\\\":\\\"PT900S\\\",\\\"region\\\":\\\"World\\\",\\\"id\\\":\\\"4-65776801\\\",\\\"version\\\":1683273494723,\\\"protection\\\":{\\\"id\\\":\\\"22-0\\\"}},\\\"metadata\\\":[{\\\"type\\\":\\\"Modification\\\",\\\"created\\\":\\\"2023-05-05T10:26:41+03:00\\\",\\\"modified\\\":\\\"2023-05-05T10:57:48+03:00\\\"}]}],\\\"_version\\\":1683273494723,\\\"metadata\\\":[{\\\"value\\\":{\\\"type\\\":\\\"Concept\\\",\\\"title\\\":{\\\"fi\\\":\\\"Draama\\\"},\\\"inScheme\\\":\\\"yle-content-main-classification\\\",\\\"id\\\":\\\"31-1-8\\\"},\\\"type\\\":\\\"Relation\\\",\\\"relation\\\":\\\"subject\\\"},{\\\"value\\\":{\\\"type\\\":\\\"Concept\\\",\\\"title\\\":{\\\"fi\\\":\\\"Asiaviihde\\\"},\\\"inScheme\\\":\\\"yle-content-format-classification\\\",\\\"id\\\":\\\"31-2-2.8\\\"},\\\"type\\\":\\\"Relation\\\",\\\"relation\\\":\\\"subject\\\"},{\\\"value\\\":{\\\"type\\\":\\\"Concept\\\",\\\"title\\\":{\\\"fi\\\":\\\"Arjen taidot ja tiedot\\\"},\\\"inScheme\\\":\\\"yle-content-topic-classification\\\",\\\"id\\\":\\\"31-3-4.57\\\"},\\\"type\\\":\\\"Relation\\\",\\\"relation\\\":\\\"subject\\\"},{\\\"value\\\":{\\\"key\\\":\\\"kulttuuri\\\",\\\"type\\\":\\\"Concept\\\",\\\"title\\\":{\\\"fi\\\":\\\"Kulttuuri\\\"},\\\"inScheme\\\":\\\"areena-analytics-classification\\\",\\\"id\\\":\\\"5-263\\\"},\\\"type\\\":\\\"Relation\\\",\\\"relation\\\":\\\"subject\\\"},{\\\"type\\\":\\\"Modification\\\",\\\"created\\\":\\\"2023-05-05T10:26:41+03:00\\\",\\\"modified\\\":\\\"2023-05-05T10:58:14+03:00\\\"},{\\\"key\\\":\\\"visible\\\",\\\"value\\\":true,\\\"type\\\":\\\"Tag\\\"}]}\"";

			notification = notification.Replace("[ID]", Id.ToString());
			notification = notification.Replace("[FINNISH TITLE]", FinnishTitle);
			notification = notification.Replace("[SWEDISH TITLE]", SwedishTitle);
			notification = notification.Replace("[SAMI TITLE]", SamiTitle);
			notification = notification.Replace("[START TIME]", Start.ToString(NotificationDateTimeFormat, CultureInfo.InvariantCulture));
			notification = notification.Replace("[END TIME]", End.ToString(NotificationDateTimeFormat, CultureInfo.InvariantCulture));
			notification = notification.Replace("[SOURCE RESOURCE NAME]", SourceResourceName);
			notification = notification.Replace("[DESTINATION RESOURCE NAME]", DestinationResourceName);
			notification = notification.Replace("[CEITON PROJECT ID]", CeitonProjectId);
			notification = notification.Replace("[CEITON PRODUCT ID]", CeitonProductId);

			return notification;
		}
	}
}
