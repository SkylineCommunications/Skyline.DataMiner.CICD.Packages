namespace ShowPlasmaDetails_2.Plasma
{
	using System.Collections.Generic;

	public class PlasmaTransmissionSubtitles
	{
		// Copied over from Mediagenix WHATS On 2.0.0.3
		public static readonly IReadOnlyDictionary<string, string> SubtitleLanguageMapping = new Dictionary<string, string>
		{
			{"fin", "Finnish"},
			{"swe", "Swedish"},
			{"fih", "Finnish for the hard of hearing"},
			{"swh", "Swedish for the hard of hearing"},
			{"eng", "English"},
			{"nor", "Norwegian"},
			{"dan", "Danish"},
			{"isl", "Icelandic"},
			{"fra", "French"},
			{"deu", "German"},
			{"ita", "Italian"},
			{"spa", "Spanish"},
			{"por", "Portuguese"},
			{"est", "Estonian"},
			{"rus", "Russian"},
			{"smi", "Sapmi"},
			{"7", "Other"},
			{"16", "No language"},
			{"10", "Subtitling for the hard of hearing (not in use)"},
			{"8", "Sign language"},
			{"25", "Finnish audio description"},
			{"17", "Swedish/Finnish (combination, not in use)"},
			{"18", "Finnish/Finnish (combination, not in use)"},
			{"19", "Finnish for the visually impaired"},
			{"24", "Swedish for the visually impaired"},
			{"sqi", "Albanian"},
			{"ara", "Arabic"},
			{"bos", "Bosnian"},
			{"bul", "Bulgarian"},
			{"sma", "Southern Sami"},
			{"heb", "Hebrew"},
			{"hin", "Hindi"},
			{"nld", "Dutch"},
			{"smn", "Inari Sami"},
			{"jpn", "Japanese"},
			{"krl", "Carelian"},
			{"zho", "Chinese"},
			{"sms", "Eastern Sami"},
			{"kor", "Korean"},
			{"ell", "Greek"},
			{"hrv", "Croatian"},
			{"lat", "Latin"},
			{"lav", "Latvian"},
			{"lit", "Lithuanian"},
			{"fas", "Persian"},
			{"sme", "Northern Sami"},
			{"pol", "Polish"},
			{"ron", "Romanian"},
			{"srp", "Serbian"},
			{"slk", "Slovakian"},
			{"slv", "Slovenian"},
			{"ces", "Czech"},
			{"tur", "Turkish"},
			{"ukr", "Ukrainian"},
			{"hun", "Hungarian"}
		};

		// Copied over from Mediagenix WHATS On 2.0.0.3
		public static readonly IReadOnlyDictionary<string, string> SubtitleCodeMapping = new Dictionary<string, string>
		{
			{"01", "Timecoded Subtitle File"},
			{"02", "Live Subtitling"},
			{"03", "Subtitles are Burnt in Video"}
		};

		public string Language { get; set; }

		public string MappedLanguage
		{
			get
			{
				if (SubtitleLanguageMapping.TryGetValue(Language, out var mappedLanguage)) return mappedLanguage;
				return Language;
			}
		}

		public string MediaId { get; set; }

		public string Type { get; set; }

		public string Code { get; set; }

		public string MappedCode
		{
			get
			{
				if (SubtitleCodeMapping.TryGetValue(Code, out var mappedCode)) return mappedCode;
				return Code;
			}
		}

		public string SubtitlingModeResponsible { get; set; }

		public string SubtitlingCodeResonsible { get; set; }
	}
}