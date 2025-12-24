namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using NPOI.SS.Formula.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

	public static class EnumExtensions
	{
		public static string GetDescription(this Enum value)
		{
			return GetDescriptionFromEnumValue(value);
		}

		public static string GetDescriptionFromEnumValue(Enum value)
		{
			var enumField = value.GetType().GetField(value.ToString()) ?? throw new InvalidOperationException($"Value '{value}' is not a valid enum value");

			var attribute = enumField.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

			return attribute == null ? value.ToString() : attribute.Description;
		}

		public static T GetEnumValueFromDescription<T>(string description)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new ArgumentException("Provided Type should be an enum");

			FieldInfo[] fields = type.GetFields();
			var field = fields.SelectMany(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false), (f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((DescriptionAttribute)a.Att).Description == description);

			return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
		}

		public static bool TryGetEnumValueFromDescription<T>(string description, out T value)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new ArgumentException("Provided Type should be an enum");

			FieldInfo[] fields = type.GetFields();
			var field = fields.SelectMany(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false), (f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((DescriptionAttribute)a.Att).Description == description);

			value = default;
			if (field == null) return false;

			value = (T)field.Field.GetRawConstantValue();
			return true;
		}

		public static bool TryGetEnumValueFromOldDescription<T>(string description, out T value)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new ArgumentException("Provided Type should be an enum");

			FieldInfo[] fields = type.GetFields();
			var field = fields.SelectMany(f => f.GetCustomAttributes(typeof(OldDescriptionAttribute), false), (f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((OldDescriptionAttribute)a.Att).Description == description);

			value = default;
			if (field == null) return false;

			value = (T)field.Field.GetRawConstantValue();
			return true;
		}

		public static IEnumerable<string> GetEnumDescriptions<T>()
		{
			var attributes = typeof(T).GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).ToList();

			return attributes.Select(x => x.Description);
		}

		public static IEnumerable<string> GetEnumDescriptions(object obj)
		{
			var attributes = obj.GetType().GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).ToList();

			return attributes.Select(x => x.Description);
		}

		public static IEnumerable<Enum> GetFlags(this Enum input)
		{
			foreach (Enum value in Enum.GetValues(input.GetType()))
			{
				if (input.HasFlag(value))
				{
					yield return value;
				}
			}
		}
	}
}