namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the DataMiner project type.
    /// </summary>
    public enum DataMinerProjectType
    {
        /// <summary>
        /// Unknown value.
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents a DataMiner Install Package project.
        /// </summary>
        Package,

        /// <summary>
        /// Represents a DataMiner Automation Script project.
        /// </summary>
        AutomationScript,

        /// <summary>
        /// Represents a DataMiner Automation Script project that will be a library.
        /// </summary>
        AutomationScriptLibrary,

        /// <summary>
        /// Represents a DataMiner Ad Hoc Data Source project.
        /// </summary>
        AdHocDataSource,

        /// <summary>
        /// Represents a DataMiner User-Defined API project.
        /// </summary>
        UserDefinedApi,

        /// <summary>
        /// Represents a DataMiner Test project.
        /// </summary>
        TestPackage,
    }

    /// <summary>
    /// Class to convert from and to the <see cref="DataMinerProjectType"/> enum.
    /// </summary>
    public static class DataMinerProjectTypeConverter
    {
        private static readonly Dictionary<string, DataMinerProjectType> StringToEnum = new Dictionary<string, DataMinerProjectType>
        {
            ["Package"] = DataMinerProjectType.Package,
            ["AutomationScript"] = DataMinerProjectType.AutomationScript,
            ["AutomationScriptLibrary"] = DataMinerProjectType.AutomationScriptLibrary,
            ["AdHocDataSource"] = DataMinerProjectType.AdHocDataSource,
            ["UserDefinedApi"] = DataMinerProjectType.UserDefinedApi,
            ["TestPackage"] = DataMinerProjectType.TestPackage,
        };

        private static readonly Dictionary<DataMinerProjectType, string> EnumToString = new Dictionary<DataMinerProjectType, string>
        {
            [DataMinerProjectType.Package] = "Package",
            [DataMinerProjectType.AutomationScript] = "AutomationScript",
            [DataMinerProjectType.AutomationScriptLibrary] = "AutomationScriptLibrary",
            [DataMinerProjectType.AdHocDataSource] = "AdHocDataSource",
            [DataMinerProjectType.UserDefinedApi] = "UserDefinedApi",
            [DataMinerProjectType.TestPackage] = "TestPackage",
        };

        /// <summary>
        /// Tries to convert the specified value to the <see cref="DataMinerProjectType"/> enum.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Converted value when successful, Unknown when not.</returns>
        public static DataMinerProjectType ToEnum(string value)
        {
            if (StringToEnum.TryGetValue(value, out DataMinerProjectType t))
            {
                return t;
            }

            return DataMinerProjectType.Unknown;
        }

        /// <summary>
        /// Tries to convert the specified <see cref="DataMinerProjectType"/> value to the string version for the project. Will return null when unable to convert to enum.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Converted value when successful, null when not.</returns>
        public static string ToString(DataMinerProjectType value)
        {
            if (EnumToString.TryGetValue(value, out string s))
            {
                return s;
            }

            return null;
        }
    }
}