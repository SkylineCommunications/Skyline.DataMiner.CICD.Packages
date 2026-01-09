namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using System.Collections;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class OrderHistoryChapter
	{
		private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			ContractResolver = ShouldSerializeContractResolver.Instance,
		};

		public OrderHistoryChapter()
		{

		}

		public OrderHistoryChapter(OrderChange orderChange, string userName, DateTime timestamp, string scriptName)
		{
			UserName = userName;
			Timestamp = timestamp.Truncate(TimeSpan.FromMinutes(1));
			ScriptName = scriptName;
			OrderChange = orderChange;
		}

		public string UserName { get; set; }

		public string ScriptName { get; set; }

		public DateTime Timestamp { get; set; }

		public OrderChange OrderChange { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, jsonSettings);
		}
	}

	public class ShouldSerializeContractResolver : DefaultContractResolver
	{
		public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (property.PropertyType != typeof(string) && property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
			{
				property.ShouldSerialize = instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
			}
					
			return property;
		}
	}
}
