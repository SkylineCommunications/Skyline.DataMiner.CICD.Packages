namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class ReflectionHandler
    {
        public static readonly BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static readonly string UI_Tooltip_Directory = @"C:\Skyline DataMiner\Documents\LOF_Tooltip";

        public static Dictionary<string, string> UiToolTips { get; private set; }

        private static readonly Regex regex = new Regex(@"<(.*)>k_+BackingField",
                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                                        TimeSpan.FromMilliseconds(250));

        public ReflectionHandler()
        {
            UiToolTips = new Dictionary<string, string>();
        }

        public static List<string> GetFieldsNames(Helpers helpers, Type type)
        {
            try
            {
                return GetFieldsInfo(type)
                    .Select(field => CleanName(field))
                    .ToList();

            }
            catch (Exception e)
            {
                helpers.Log(nameof(ReflectionHandler), nameof(GetTypesInNamespace), $"Error trying to get fields {e}");
                return new List<string>();
            }
        }

        public static List<string> GetPropertiesNames(Helpers helpers, Type type)
        {
            try
            {
                return GetPropertiesInfo(type)
                    .Select(property => CleanName(property))
                    .ToList();

            }
            catch (Exception e)
            {
                helpers.Log(nameof(ReflectionHandler), nameof(GetTypesInNamespace), $"Error trying to get fields {e}");
                return new List<string>();
            }
        }

        public static IEnumerable<T> GetInfo<T>(Type type)
        {

            if (typeof(T) == typeof(PropertyInfo))
            {
                return (IEnumerable<T>)GetPropertiesInfo(type);
            }

            if (typeof(T) == typeof(FieldInfo))
            {
                return (IEnumerable<T>)GetFieldsInfo(type);
            }

            return new List<T>();
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInfo(Type type)
        {
            return type.GetProperties(bindingFlags)
                            .Where(p =>
                                !p.Name.Equals(nameof(Helpers), StringComparison.InvariantCultureIgnoreCase)
                                && !p.Name.Equals(nameof(Engine), StringComparison.InvariantCultureIgnoreCase)
                                && p.PropertyType != typeof(bool)
                                && p.PropertyType != typeof(int)
                                && p.PropertyType != typeof(string)
                                && p.PropertyType != typeof(EventHandler)
                            ).ToList();
        }

        private static string CleanName<T>(T field)
        {
            if (field is FieldInfo fieldObj)
            {
                var matches = regex.Matches(fieldObj.Name);
                return matches.Count > 0 ? matches[0].Groups[1].Value : fieldObj.Name;
            }

            if (field is PropertyInfo propertyObj)
            {
                var matches = regex.Matches(propertyObj.Name);
                return matches.Count > 0 ? matches[0].Groups[1].Value : propertyObj.Name;
            }

            return String.Empty;
        }

        public static IEnumerable<FieldInfo> GetFieldsInfo(Type type)
        {
            return type.GetFields(bindingFlags)
                            .Where(f =>
                                !f.Name.Equals(nameof(Helpers), StringComparison.InvariantCultureIgnoreCase)
                                && !f.Name.Equals(nameof(Engine), StringComparison.InvariantCultureIgnoreCase)
                                && f.FieldType != typeof(bool)
                                && f.FieldType != typeof(int)
                                && f.FieldType != typeof(string)
                                && f.FieldType != typeof(EventHandler)
                                && !f.Name.Contains("BackingField")
                            ).ToList();
        }

        public static string[] GetReflectionNames(Helpers helpers, Type type)
        {
            return new HashSet<string>(GetFieldsNames(helpers, type)
                            .Concat(GetPropertiesNames(helpers, type))).ToArray();
        }

        public static Type[] GetTypesInNamespace(Helpers helpers, Assembly assembly, string nameSpace)
        {
            try
            {
                return assembly.GetTypes()
                              .Where(t => t.Namespace != null
                              && t.Namespace.Contains(nameSpace)
                              && t.IsClass
                              && t.MemberType == MemberTypes.TypeInfo)
                              .ToArray();
            }
            catch (Exception e)
            {
                helpers.Log(nameof(ReflectionHandler), nameof(GetTypesInNamespace), $"Error trying to get types {e}");
                return new Type[0];
            }
        }

        public static void GetFields(Engine engine, Helpers helpers, string getNamespace)
        {
            UiToolTips = new Dictionary<string, string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Assembly lof_assemblie = AppDomain.CurrentDomain.GetAssemblies()
                                                            .FirstOrDefault(a => a.FullName.Contains(getNamespace));

            lof_assemblie = lof_assemblie ?? Assembly.GetCallingAssembly();

            try
            {
                Type[] typelist = GetTypesInNamespace(helpers, Assembly.GetCallingAssembly(), getNamespace);

                for (int i = 0; i < typelist.Length; i++)
                {
                    foreach (var field in GetReflectionNames(helpers, typelist[i]))
                    {
                        UiToolTips[typelist[i].Name + "_" + field] = typelist[i].Name + "_" + field;
                    }
                }
            }
            catch (Exception e)
            {
                helpers.Log(nameof(ReflectionHandler), nameof(GetFields), $"Error trying to get fields {e}");
            }
        }

        public static void WriteFile(Helpers helpers)
        {
            try
            {
                if (!Directory.Exists(UI_Tooltip_Directory))
                {
                    Directory.CreateDirectory(UI_Tooltip_Directory);
                }

                var filePath = Path.Combine(
                                                UI_Tooltip_Directory,
                                                UI_Tooltip_Directory.Split('\\').LastOrDefault()?.ToLower() + ".json");

                helpers.Log(nameof(ReflectionHandler), nameof(WriteFile), $"filePath: {filePath}");

                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.Write(JsonConvert.SerializeObject(UiToolTips, Formatting.Indented)) ;
                }
            }
            catch (Exception e)
            {
                helpers.Log(nameof(ReflectionHandler), nameof(WriteFile), $"Error trying to write file {e}");
            }
        }

        public static Dictionary<string, string> ReadTooltipFile()
        {
            string text = String.Empty;

            try
            {
                var filePath = Path.Combine(
                                            UI_Tooltip_Directory,
                                            UI_Tooltip_Directory.Split('\\').LastOrDefault()?.ToLower() + ".json");

                text = File.Exists(filePath) ? File.ReadAllText(filePath) : String.Empty;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }

            try
            {
                return !String.IsNullOrEmpty(text) ?
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(text)
                    : new Dictionary<string, string>();
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}