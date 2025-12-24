// --- auto-generated code --- do not modify ---

/*
{{StartPackageInfo}}
<PackageInfo xmlns="http://www.skyline.be/ClassLibrary">
	<BasePackage>
		<Identity>
			<Name>Class Library</Name>
			<Version>1.2.0.10</Version>
		</Identity>
	</BasePackage>
	<CustomPackages>
		<Package>
			<Identity>
				<Name>YLE</Name>
				<Version>1.0.0.3</Version>
			</Identity>
		</Package>
	</CustomPackages>
</PackageInfo>
{{EndPackageInfo}}
*/

namespace Skyline.DataMiner
{
    namespace Library.Common.Attributes
    {
        /// <summary>
        /// This attribute indicates a DLL is required.
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
        public sealed class DllImportAttribute : System.Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref = "DllImportAttribute"/> class.
            /// </summary>
            /// <param name = "dllImport">The name of the DLL to be imported.</param>
            public DllImportAttribute(string dllImport)
            {
                DllImport = dllImport;
            }

            /// <summary>
            /// Gets the name of the DLL to be imported.
            /// </summary>
            public string DllImport
            {
                get;
                private set;
            }
        }
    }

    namespace DeveloperCommunityLibrary.YLE
    {
        public class ProgressReporter
        {
            public event System.EventHandler<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressLoggingEventArgs> ProgressLogging;
            public void LogMethodStart(string nameOfClass, string nameOfMethod, string nameOfObject = null)
            {
                LogProgress(nameOfClass, nameOfMethod, "Start", nameOfObject);
            }

            public void LogMethodCompleted(string nameOfClass, string nameOfMethod, string nameOfObject = null, System.Diagnostics.Stopwatch stopwatch = null)
            {
                LogProgress(nameOfClass, nameOfMethod, $"Completed{(stopwatch != null ? $" [{stopwatch.Elapsed}]" : string.Empty)}", nameOfObject);
            }

            public void LogProgress(string nameOfClass, string nameOfMethod, string message, string nameOfObject = null)
            {
                LogProgress(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressLog(nameOfClass, nameOfMethod, message, nameOfObject));
            }

            public void LogProgress(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressLog progress)
            {
                ProgressLogging?.Invoke(this, new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressLoggingEventArgs(progress.ToString()));
            }
        }

        public class ProgressLoggingEventArgs : System.EventArgs
        {
            public ProgressLoggingEventArgs(string progress)
            {
                Progress = progress;
            }

            public string Progress
            {
                get;
                private set;
            }
        }

        public struct ProgressLog
        {
            private readonly string className;
            private readonly string methodName;
            private readonly string objectName;
            private readonly string message;
            public ProgressLog(string methodName, string message): this(string.Empty, methodName, message, string.Empty)
            {
            }

            public ProgressLog(string className, string methodName, string message): this(className, methodName, message, string.Empty)
            {
            }

            public ProgressLog(string className, string methodName, string message, string objectName)
            {
                this.className = className;
                this.methodName = methodName;
                this.message = message;
                this.objectName = objectName;
            }

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(className))
                {
                    sb.Append(className);
                    sb.Append("|");
                }

                sb.Append(methodName);
                sb.Append("|");
                if (!string.IsNullOrWhiteSpace(objectName))
                {
                    sb.Append(objectName);
                    sb.Append("|");
                }

                sb.Append(message);
                return sb.ToString();
            }
        }

        namespace AvidInterplayPAM
        {
            public enum InterplayPamElements
            {
                [System.ComponentModel.Description("IPLAY Helsinki")]
                Helsinki,
                [System.ComponentModel.Description("IPLAY Tampere")]
                Tampere,
                [System.ComponentModel.Description("IPLAY Vaasa")]
                Vaasa,
                [System.ComponentModel.Description("IPLAY UA")]
                UA
            }
        }

        namespace ChangeTracking
        {
            public class ChangeInfo
            {
                public ChangeInfo()
                {
                    ProfileParameterChangeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo();
                    ResourceChangeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo();
                    TimingChangeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo();
                }

                public bool IsChanged => ProfileParameterChangeInfo.ProfileParametersChanged || ResourceChangeInfo.ResourcesChanged || TimingChangeInfo.TimingChanged || CustomPropertiesChanged || ServiceDefinitionChanged || SecurityViewIdsChanged;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo ProfileParameterChangeInfo
                {
                    get;
                    private set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo ResourceChangeInfo
                {
                    get;
                    private set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo TimingChangeInfo
                {
                    get;
                    private set;
                }

                public bool CustomPropertiesChanged
                {
                    get;
                    private set;
                }

                public bool SecurityViewIdsChanged
                {
                    get;
                    set;
                }

                public bool ServiceDefinitionChanged
                {
                    get;
                    private set;
                }

                public void CombineWith(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo second)
                {
                    ProfileParameterChangeInfo.CombineWith(second.ProfileParameterChangeInfo);
                    ResourceChangeInfo.CombineWith(second.ResourceChangeInfo);
                    TimingChangeInfo.CombineWith(second.TimingChangeInfo);
                    CustomPropertiesChanged |= second.CustomPropertiesChanged;
                    ServiceDefinitionChanged |= second.ServiceDefinitionChanged;
                }

                public void MarkCustomPropertiesChanged()
                {
                    CustomPropertiesChanged = true;
                }

                public void MarkServiceDefinitionChanged()
                {
                    ServiceDefinitionChanged = true;
                }

                public override string ToString()
                {
                    return System.String.Format("( Profile Parameters changed = {0}; Resources changed = {1}; Timing changed = {2}; custom properties changed = {3}; Service Definition changed = {4} )", ProfileParameterChangeInfo.ProfileParametersChanged, ResourceChangeInfo.ResourcesChanged, TimingChangeInfo.TimingChanged, CustomPropertiesChanged, ServiceDefinitionChanged);
                }
            }

            public interface IYleChangeTracking : System.ComponentModel.IChangeTracking
            {
                /// <summary>
                /// Gets a boolean indicating if Change Tracking is enabled.
                /// </summary>
                bool ChangeTrackingEnabled
                {
                    get;
                }

                /// <summary>
                /// Gets an object containing all change info since object construction or since last <see cref = "IChangeTracking.AcceptChanges"/> call.
                /// </summary>
                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo
                {
                    get;
                }
            }

            public class ProfileParameterChangeInfo
            {
                [System.Flags]
                private enum TypesOfProfileParameterChanges
                {
                    None = 0,
                    AudioProcessingProfileParametersChanged = 1,
                    VideoProcessingProfileParametersChanged = 2,
                    GraphicsProcessingProfileParametersChanged = 4,
                    CapabilitiesChanged = 8,
                    OtherProfileParametersChanged = 16
                }

                private const Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges AllTypesOfProfileParametersChanges = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged | Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged | Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged | Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.CapabilitiesChanged | Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.OtherProfileParametersChanged;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges typesOfChanges = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.None;
                public bool ProfileParametersChanged => typesOfChanges != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.None;
                public bool AudioProcessingProfileParametersChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged;
                public bool VideoProcessingProfileParametersHaveChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged;
                public bool GraphicsProcessingProfileParametersHaveChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged;
                public bool CapabilitiesChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.CapabilitiesChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.CapabilitiesChanged;
                public bool OtherProfileParametersChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.OtherProfileParametersChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.OtherProfileParametersChanged;
                public bool OnlyAudioProcessingProfileParametersChanged => (typesOfChanges & AllTypesOfProfileParametersChanges) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged;
                public bool OnlyVideoProcessingProfileParametersChanged => (typesOfChanges & AllTypesOfProfileParametersChanges) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged;
                public bool OnlyGraphicsProcessingProfileParametersChanged => (typesOfChanges & AllTypesOfProfileParametersChanges) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged;
                public bool OnlyCapabilitiesChanged => (typesOfChanges & AllTypesOfProfileParametersChanges) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.CapabilitiesChanged;
                public bool OnlyOtherProfileParametersChanged => (typesOfChanges & AllTypesOfProfileParametersChanges) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.OtherProfileParametersChanged;
                public void CombineWith(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo second)
                {
                    typesOfChanges |= second.typesOfChanges;
                }

                public void MarkAudioProcessingProfileParametersChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged;
                }

                public void MarkVideoProcessingProfileParametersChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged;
                }

                public void MarkGraphicsProcessingProfileParametersChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged;
                }

                public void MarkDtrProfileParametersChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.CapabilitiesChanged;
                }

                public void MarkOtherProfileParametersChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ProfileParameterChangeInfo.TypesOfProfileParameterChanges.OtherProfileParametersChanged;
                }
            }

            public class ResourceChangeInfo
            {
                [System.Flags]
                private enum TypesOfResourceChanges
                {
                    None = 0,
                    ResourceWasAddedOrSwapped = 1,
                    ResourceWasRemoved = 2,
                    ResourceAtBeginningOfServiceDefinitionHasChanged = 4,
                    ResourceAtEndOfServiceDefinitionHasChanged = 8
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges typesOfChanges = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.None;
                public bool ResourcesChanged => typesOfChanges != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.None;
                public bool ResourcesAddedOrSwapped => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasAddedOrSwapped) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasAddedOrSwapped;
                public bool ResourcesRemoved => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasRemoved) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasRemoved;
                public bool ResourceAtBeginningOfServiceDefinitionChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged;
                public bool ResourceAtEndOfServiceDefinitionChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged;
                public void CombineWith(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo second)
                {
                    typesOfChanges |= second.typesOfChanges;
                }

                public void MarkResourceAddedOrSwapped()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasAddedOrSwapped;
                }

                public void MarkResourceRemoved()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasRemoved;
                }

                public void MarkResourceAtBeginningOfServiceDefinitionAddedOrSwapped()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasAddedOrSwapped;
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtBeginningOfServiceDefinitionHasChanged;
                }

                public void MarkResourceAtEndOfServiceDefinitionAddedOrSwapped()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceWasAddedOrSwapped;
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ResourceChangeInfo.TypesOfResourceChanges.ResourceAtEndOfServiceDefinitionHasChanged;
                }
            }

            public class TimingChangeInfo
            {
                [System.Flags]
                private enum TypesOfTimingChanges
                {
                    None = 0,
                    StartTimingChanged = 1,
                    EndTimingChanged = 2
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges typesOfChanges;
                public bool TimingChanged => typesOfChanges != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.None;
                public bool StartTimingChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.StartTimingChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.StartTimingChanged;
                public bool EndTimingChanged => (typesOfChanges & Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.EndTimingChanged) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.EndTimingChanged;
                public void CombineWith(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo second)
                {
                    typesOfChanges |= second.typesOfChanges;
                }

                public void MarkStartTimingChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.StartTimingChanged;
                }

                public void MarkEndTimingChanged()
                {
                    typesOfChanges |= Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.TimingChangeInfo.TypesOfTimingChanges.EndTimingChanged;
                }
            }
        }

        namespace Configuration
        {
            public static class Constants
            {
                public static readonly string None = "None";
                public static readonly string NotApplicable = "N/A";
                public static readonly int MaximumAllowedCharacters = 400;
                public static readonly string OrderLoggingDirectoryName = "OrderLogging";
            }

            public static class FunctionGuids
            {
                public static readonly System.Guid Satellite = new System.Guid(SatelliteString);
                public const string SatelliteString = "b4ac401c-455c-4e62-85fc-d0261dd9b922";
                public static readonly System.Guid Antenna = new System.Guid(AntennaString);
                public const string AntennaString = "3e578238-3588-4c74-820f-def427cc9953";
                public static readonly System.Guid Demodulating = new System.Guid(DemodulatingString);
                public const string DemodulatingString = "f6af0f47-fc8c-4be2-9eba-50593e1488bf";
                public static readonly System.Guid Decoding = new System.Guid(DecodingString);
                public const string DecodingString = "89c8f8ae-ef77-45b1-b6d3-6a2d8a87d9f1";
                public static readonly System.Guid FiberDecoding = new System.Guid("f52e89f0-c646-4bf9-84ee-1fb126c247d1");
                public static readonly System.Guid FiberSource = new System.Guid("5ab13709-198d-42e2-b0ed-cb48caa96852");
                public static readonly System.Guid FiberDestination = new System.Guid("9ab71ea5-13ce-4bc8-b412-9bd2df69a66f");
                public static readonly System.Guid LiveUPasilaSource = new System.Guid("0f855e47-797a-4fae-b4ea-bf581ab57b8e");
                public const string LiveUPasilaSourceString = "0f855e47-797a-4fae-b4ea-bf581ab57b8e";
                public static readonly System.Guid GenericEncoding = new System.Guid("a6c7084d-c0b8-4b37-be7a-014f15eed3b4");
                public static readonly System.Guid GenericModulating = new System.Guid("24042a26-41e0-47d7-9ba9-59b14f11f21c");
                public static readonly System.Guid MatrixInputAsi = new System.Guid("7bd8d399-b503-4fd9-9b2e-8dc188d591b8");
                public static readonly System.Guid MatrixOutputAsi = new System.Guid("a33a13bd-b40f-4080-9ac4-aa77585f83db");
                public static readonly System.Guid MatrixInputLband = new System.Guid("8c58be38-b7ed-4369-bb89-1a7ad49b29db");
                public static readonly System.Guid MatrixOutputLband = new System.Guid("3492292d-4ba4-47f3-9bd7-7d983d95c064");
                public static readonly System.Guid MatrixInputSdi = new System.Guid("605fa59e-ca49-431c-8874-438e47f3851a");
                public static readonly System.Guid MatrixOutputSdi = new System.Guid("de7c117c-6161-4172-8b4a-35ccb0304765");
                public static readonly System.Collections.Generic.IEnumerable<System.Guid> AllMatrixGuids = new[]{MatrixInputAsi, MatrixOutputAsi, MatrixInputSdi, MatrixOutputSdi, MatrixInputLband, MatrixOutputLband};
                public static readonly System.Collections.Generic.IEnumerable<System.Guid> AllMatrixInputGuids = new[]{MatrixInputSdi, MatrixInputAsi, MatrixInputLband};
                public static readonly System.Collections.Generic.Dictionary<System.Guid, System.Guid> MatrixFunctionPairs = new System.Collections.Generic.Dictionary<System.Guid, System.Guid>{{MatrixInputAsi, MatrixOutputAsi}, {MatrixInputLband, MatrixOutputLband}, {MatrixInputSdi, MatrixOutputSdi}, {MatrixOutputAsi, MatrixInputAsi}, {MatrixOutputLband, MatrixInputLband}, {MatrixOutputSdi, MatrixInputSdi}};
                public static bool IsMatrixFunction(System.Guid functionID)
                {
                    return System.Linq.Enumerable.Contains(AllMatrixGuids, functionID);
                }
            }

            public static class OrderManagerProtocol
            {
                public const string Name = "Finnish Broadcasting Company Order Manager";
                public const int LockRequestParameterId = 5200;
            }

            public static class ProfileParameterGuids
            {
                public static readonly System.Guid _ServiceConfiguration = new System.Guid("9083c73c-7eb3-4fc9-85ea-02fea98312a9");
                public static readonly System.Guid _Matrix = new System.Guid("bce4b6a3-4045-4ec0-88d3-ba61c15f8038");
                public static readonly System.Guid _OrbitalPosition = new System.Guid("a9810b80-c127-47a1-9e08-777dbb758ea1");
                public static readonly System.Guid _TvChannel = new System.Guid("26560675-38a6-4dba-af61-e23d75b3af1d");
                public static readonly System.Guid _FeedType = new System.Guid("07278965-6138-46de-b1d6-1f02861c240d");
                public static readonly System.Guid AreenaCopy = new System.Guid("798393c6-4ddf-48c2-92ae-668433de3428");
                public static readonly System.Guid AreenaDestinationLocation = new System.Guid("96cc6585-2b3a-4129-a7d3-85c58b61b5de");
                public static readonly System.Guid AudioDeembeddingRequired = new System.Guid("1d51086a-ca66-4e1d-b84f-6fd05a24ee68");
                public static readonly System.Guid AudioEmbeddingRequired = new System.Guid("a2726fd1-cc4b-4b23-b464-3684a0db4dd6");
                public static readonly System.Guid AudioShufflingRequired = new System.Guid("01682ff9-2cc4-4cd5-8252-d5956424479a");
                public static readonly System.Guid AudioDolbyDecodingRequired = new System.Guid("35215bb4-0faa-423f-beed-58ada9b604b3");
                public static readonly System.Guid AudioChannel1 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel1String);
                public static readonly System.Guid AudioChannel2 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel2String);
                public static readonly System.Guid AudioChannel3 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel3String);
                public static readonly System.Guid AudioChannel4 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel4String);
                public static readonly System.Guid AudioChannel5 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel5String);
                public static readonly System.Guid AudioChannel6 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel6String);
                public static readonly System.Guid AudioChannel7 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel7String);
                public static readonly System.Guid AudioChannel8 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel8String);
                public static readonly System.Guid AudioChannel9 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel9String);
                public static readonly System.Guid AudioChannel10 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel10String);
                public static readonly System.Guid AudioChannel11 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel11String);
                public static readonly System.Guid AudioChannel12 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel12String);
                public static readonly System.Guid AudioChannel13 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel13String);
                public static readonly System.Guid AudioChannel14 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel14String);
                public static readonly System.Guid AudioChannel15 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel15String);
                public static readonly System.Guid AudioChannel16 = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.AudioChannel16String);
                public static readonly System.Guid AudioChannel1Description = new System.Guid("fab7b647-df5f-43ac-9396-511fc6ba9f81");
                public static readonly System.Guid AudioChannel2Description = new System.Guid("f08209ca-898b-40d3-835d-e5bca4420960");
                public static readonly System.Guid AudioChannel3Description = new System.Guid("c4dc3e48-1d7e-48f0-832e-af4eecf716d4");
                public static readonly System.Guid AudioChannel4Description = new System.Guid("8b2499e6-653b-409c-8c6c-ed9d56f989fe");
                public static readonly System.Guid AudioChannel5Description = new System.Guid("19e761d7-31d1-48fe-8265-9b24f386eafd");
                public static readonly System.Guid AudioChannel6Description = new System.Guid("cf014313-b2d3-46a3-8cc3-c7bb57946c5c");
                public static readonly System.Guid AudioChannel7Description = new System.Guid("4a7c08cf-67a1-4e37-85fd-89bfe39331a8");
                public static readonly System.Guid AudioChannel8Description = new System.Guid("0e478f07-4c48-4cb2-bd44-615cc197141a");
                public static readonly System.Guid AudioChannel9Description = new System.Guid("145eda52-eb24-49bd-87b5-31edf8e1002f");
                public static readonly System.Guid AudioChannel10Description = new System.Guid("868a72c4-7ed5-4d81-b8fd-c4e803681100");
                public static readonly System.Guid AudioChannel11Description = new System.Guid("5b65ad74-d72b-42be-b1d6-87319580df88");
                public static readonly System.Guid AudioChannel12Description = new System.Guid("bd8065e8-72d7-4907-a910-9f47ad48e79f");
                public static readonly System.Guid AudioChannel13Description = new System.Guid("f114e37c-3627-4dda-8bd2-b0ce466c446b");
                public static readonly System.Guid AudioChannel14Description = new System.Guid("3887c50c-c1fc-45aa-97bc-c24eeca812a4");
                public static readonly System.Guid AudioChannel15Description = new System.Guid("45f43ff0-9c3b-4dd1-a50a-34faac268612");
                public static readonly System.Guid AudioChannel16Description = new System.Guid("f28b5c26-639c-4881-80d1-dc1d6a95c422");
                public static readonly System.Guid AudioReturnChannel = new System.Guid("b765fd4c-92ee-48f1-b858-4d04e3172dd9");
                public static readonly System.Guid LiveUAudioReturnInfo = new System.Guid("97dcf302-7cf5-4350-bbf5-9efe6cf0480c");
                public static readonly System.Guid DownlinkFrequency = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.DownlinkFrequencyString);
                public static readonly System.Guid Encoding = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.EncodingString);
                public static readonly System.Guid EncryptionKey = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.EncryptionKeyString);
                public static readonly System.Guid Fec = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.FecString);
                public static readonly System.Guid FastRerun = new System.Guid("9100b6ea-5b40-4fce-8066-532bea045f65");
                public static readonly System.Guid FeedName = new System.Guid("50e277dc-e0b5-4ba8-8384-d7b1764a3815");
                public static readonly System.Guid FixedTieLineSource = new System.Guid("bfe1faa3-62a1-44d6-9db1-6ff7ac07605f");
                public static readonly System.Guid Polarization = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.PolarizationString);
                public static readonly System.Guid ResourceInputConnectionsAsi = new System.Guid("58a7e9e4-6a39-42c9-b275-92f9dc7f9003");
                public static readonly System.Guid ResourceInputConnectionsLband = new System.Guid("5c8ea48d-fafb-4fae-9f43-8ce2e9347d90");
                public static readonly System.Guid ResourceInputConnectionsSdi = new System.Guid("3fab320b-6059-47ee-aad3-bf98fbba9f06");
                public static readonly System.Guid ResourceOutputConnectionsAsi = new System.Guid("3c08c3a7-d1fb-46f2-877b-8927345cf931");
                public static readonly System.Guid ResourceOutputConnectionsLband = new System.Guid("a12733e0-1ba5-48b8-a253-1f353eaeae79");
                public static readonly System.Guid ResourceOutputConnectionsSdi = new System.Guid("1f5d4759-1d66-4cbe-a4c0-6bb6fb6937d6");
                public static readonly System.Guid RemoteGraphics = new System.Guid("7504c7e2-4c65-4230-8302-41e4fc90d210");
                public static readonly System.Guid SubtitleProxy = new System.Guid("7610d0f7-e068-4cd9-a014-0308b277d4f2");
                public static readonly System.Guid SymbolRate = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.SymbolRateString);
                public static readonly System.Guid ModulationStandard = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.ModulationStandardString);
                public static readonly System.Guid Modulation = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.ModulationString);
                public static readonly System.Guid EncryptionType = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.EncryptionTypeString);
                public static readonly System.Guid VideoFormat = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.VideoFormatString);
                public static readonly System.Guid RecordingVideoFormat = new System.Guid("105baed9-4031-4d7c-a436-d26fe608af7b");
                public static readonly System.Guid ExternalCompaniesDestinationLocation = new System.Guid("62e5716c-85d8-4d01-a308-b87cc461b1b3");
                public static readonly System.Guid EbuDestinationLocation = new System.Guid("b4213f38-3bb7-48a4-a61c-3ec0b609785e");
                public static readonly System.Guid YleHelsinkiDestinationLocation = new System.Guid("603c52e0-ca2b-4171-bba9-9469250031d4");
                public static readonly System.Guid YleMediapolisDestinationLocation = new System.Guid("ea655627-a17d-442a-9e51-5ddebee9755f");
                public static readonly System.Guid FixedLineExternalCompaniesSourceLocation = new System.Guid("f0c352c7-5fa6-4338-a201-94c18fdb8d4e");
                public static readonly System.Guid FixedLineHelsinkiCityConnectionsSourceLocation = new System.Guid("b1224510-bfbf-4506-9120-1eb33ad353d4");
                public static readonly System.Guid FixedLineYleHelsinkiSourceLocation = new System.Guid("b9565c0d-a26f-4fa9-985a-cc162e8524d7");
                public static readonly System.Guid FixedLineEbuSourceLocation = new System.Guid("b6877c1e-99c3-4971-a4da-7c47bcf274e3");
                public static readonly System.Guid FixedLineLySourcePlasmaUserCode = new System.Guid("7bab8775-841d-4b14-ae83-e665d4768b47");
                public static readonly System.Guid Channel = new System.Guid("af8e34b9-6a5b-4126-b65e-ec670e4099bd");
                public static readonly System.Guid ServiceSelection = new System.Guid(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Strings.ServiceSelectionString);
                public static readonly System.Guid OtherSatelliteName = new System.Guid("c375a18f-60d7-497f-88b3-2b113f18f3ce");
                public static readonly System.Guid[] AllAudioProcessingRequiredGuids = new[]{AudioDeembeddingRequired, AudioDolbyDecodingRequired, AudioEmbeddingRequired, AudioShufflingRequired};
                public static readonly System.Guid[] AllAudioChannelConfigurationGuids = new[]{AudioDeembeddingRequired, AudioEmbeddingRequired, AudioShufflingRequired, AudioDolbyDecodingRequired, AudioChannel1, AudioChannel1Description, AudioChannel2, AudioChannel2Description, AudioChannel3, AudioChannel3Description, AudioChannel4, AudioChannel4Description, AudioChannel5, AudioChannel5Description, AudioChannel6, AudioChannel6Description, AudioChannel7, AudioChannel7Description, AudioChannel8, AudioChannel8Description, AudioChannel9, AudioChannel9Description, AudioChannel10, AudioChannel10Description, AudioChannel11, AudioChannel11Description, AudioChannel12, AudioChannel12Description, AudioChannel13, AudioChannel13Description, AudioChannel14, AudioChannel14Description, AudioChannel15, AudioChannel15Description, AudioChannel16, AudioChannel16Description};
                public static readonly System.Guid[] AllResourceInputConnectionGuids = new[]{ResourceInputConnectionsSdi, ResourceInputConnectionsLband, ResourceInputConnectionsAsi};
                public static readonly System.Guid[] AllResourceOutputConnectionGuids = new[]{ResourceOutputConnectionsSdi, ResourceOutputConnectionsLband, ResourceOutputConnectionsAsi};
                public static class Strings
                {
                    public const string AudioChannel1String = "d4f1e0fe-1a72-4ec3-9fd8-aefd4785ebdf";
                    public const string AudioChannel2String = "bce7eff1-a884-47af-a182-559e0cfa4379";
                    public const string AudioChannel3String = "471d52de-b90c-4d8d-9cb5-b11916a06d08";
                    public const string AudioChannel4String = "5f014774-c3ce-492e-96c3-cd7402dbe171";
                    public const string AudioChannel5String = "fad366e6-2f29-466b-a286-434446cf6437";
                    public const string AudioChannel6String = "5c68fb8a-5c8f-4970-aeb4-b04b5f465724";
                    public const string AudioChannel7String = "83b3d476-39dd-4660-854f-27bbe6436914";
                    public const string AudioChannel8String = "e9aeb156-a027-4898-b41e-6af88169a3ff";
                    public const string AudioChannel9String = "5eadca8d-f96a-464d-81a4-139dc6da0fba";
                    public const string AudioChannel10String = "7178133a-d8a8-485d-99fa-b909ef73d848";
                    public const string AudioChannel11String = "052babfe-b396-4d3f-a305-21442b3e0fd1";
                    public const string AudioChannel12String = "acb1bef4-f80b-424b-abd1-a51a05586a6d";
                    public const string AudioChannel13String = "fa83f575-0935-4952-b561-ce349c7e59fb";
                    public const string AudioChannel14String = "f60e0b3e-840a-47f1-b521-9e1dbdc28562";
                    public const string AudioChannel15String = "2712a2f9-273a-439f-b85a-5101789782cf";
                    public const string AudioChannel16String = "cc76cdc0-925c-472f-985f-c0c5e477639c";
                    public const string DownlinkFrequencyString = "d045fe27-2163-4e9b-b4e6-41ec3eac2a5d";
                    public const string EncodingString = "3efcc36d-2cb9-4f22-a1e1-fae50def51ca";
                    public const string EncryptionKeyString = "09995fd9-9526-485c-99cb-1d9eb6178195";
                    public const string EncryptionTypeString = "27465703-cd3c-40a7-81e4-cf72119a1e3b";
                    public const string FecString = "3f6fb925-e996-496c-8760-34436590c47e";
                    public const string PolarizationString = "3f5f30c2-a4b1-4ab6-be06-ecec2e35cd5d";
                    public const string SymbolRateString = "9a752cb7-77f2-4f75-8b9a-9a290eaf81b5";
                    public const string ModulationString = "ea300fb7-527f-4b14-aff9-7985875b82e3";
                    public const string ModulationStandardString = "ed8446e9-e373-419d-9b9f-8a1c94ec8ae7";
                    public const string ServiceSelectionString = "0e4008ca-5dd8-40f5-b141-cd8b631444ee";
                    public const string VideoFormatString = "8dc6df35-c574-4412-bf52-de7e3c78201c";
                }
            }

            /// <summary>
            ///     A static class containing all the names of custom properties on service reservations as defined in the Booking
            ///     Managers.
            /// </summary>
            public static class ServicePropertyNames
            {
                public const string StatusPropertyName = "Status";
                public const string VirtualPlatformPropertyName = "Virtual Platform";
                public const string ShortDescription = "Short Description";
                public const string ReportedIssuePropertyName = "Reported_Issue";
                public const string ServiceLevelPropertyName = "ServiceLevel";
                public const string IsEventLevelReceptionPropertyName = "IsEventLevelReception";
                public const string IsGlobalEventLevelReceptionPropertyName = "IsGlobalEventLevelReception";
                public const string IntegrationTypePropertyName = "Integration";
                public const string IntegrationIsMasterPropertyName = "IntegrationIsMaster";
                public const string LinkedServiceIdPropertyName = "LinkedServiceId";
                public const string EurovisionIdPropertyName = "EurovisionId";
                public const string EurovisionTransmissionNumberPropertyName = "EurovisionTransmissionNumber";
                public const string EurovisionBookingDetailsPropertyName = "EurovisionBookingDetails";
                public const string LinkedEventIdPropertyName = "LinkedEventId";
                public const string CommentsPropertyName = "Comments";
                public const string OrderIdsPropertyName = "OrderIds";
                public const string ContactInformationNamePropertyName = "ContactInformationName";
                public const string ContactInformationTelephoneNumberPropertyName = "ContactInformationTelephoneNumber";
                public const string LiveUDeviceNamePropertyName = "LiveUDeviceName";
                public const string VidigoStreamSourceLinkPropertyName = "VidigoStreamSourceLink";
                public const string RecordingConfigurationPropertyName = "RecordingConfiguration";
                public const string BackupServicePropertyName = "UI_Backup";
                public const string CommentaryAudioPropertyName = "UI_Comm_Audio";
                public const string OrderReferencesPropertyName = "OrderIds";
                public const string NameOfServiceToTransmitPropertyName = "NameOfServiceToTransmit";
                public const string OrderNamePropertyName = "OrderName";
                public const string AudioReturnInfoPropertyName = "AudioReturnInfo";
            }
        }

        namespace Contracts
        {
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            public class ContractManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts.IContractManager
            {
                public const string DataminerUserLoginName = "DataMiner Agent";
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                /// <summary>
                /// Initializes a new instance of the ContractManager class.
                /// </summary>
                /// <param name = "engine">Link with DataMiner.</param>
                /// <exception cref = "ArgumentNullException">Thrown when the provided Engine is null.</exception>
                /// <exception cref = "ElementNotFoundException">Thrown when no Finnish Broadcasting Company Contract Manager element running the production version of the protocol was found on the DMS.</exception>
                /// <exception cref = "ElementNotStartedException">Thrown when the found Finnish Broadcasting Company Contract Manager element is not running.</exception>
                public ContractManager(Skyline.DataMiner.Automation.IEngine engine)
                {
                    this.engine = engine ?? throw new System.ArgumentNullException(nameof(engine));
                    Element = System.Linq.Enumerable.FirstOrDefault(engine.FindElementsByProtocol("Finnish Broadcasting Company Contract Manager", "Production"));
                    if (Element == null)
                        throw new Skyline.DataMiner.Library.Exceptions.ElementNotFoundException("Unable to find a Contract Manager element");
                    if (!Element.IsActive)
                        throw new Skyline.DataMiner.Library.Exceptions.ElementNotStartedException("The Contract Manager element is not running");
                }

                /// <summary>
                /// Gets the ContractManager element that this class instance interacts with.
                /// </summary>
                public Skyline.DataMiner.Automation.Element Element
                {
                    get;
                    private set;
                }
            }

            public interface IContractManager
            {
            }
        }

        namespace Event
        {
            public enum Status
            {
                /// <summary>
                /// Event which may or may not happen.
                /// </summary>
                [System.ComponentModel.Description("Preliminary")]
                Preliminary = 0,
                /// <summary>
                /// Event that does not contain any orders.
                /// </summary>
                [System.ComponentModel.Description("Planned")]
                Planned = 1,
                /// <summary>
                /// Event that contains at least one order.
                /// </summary>
                [System.ComponentModel.Description("Confirmed")]
                Confirmed = 2,
                /// <summary>
                /// Event that contains at least one order and where start time is passed and end time is not reached.
                /// </summary>
                [System.ComponentModel.Description("Ongoing")]
                Ongoing = 3,
                /// <summary>
                /// Event that contains at least one order and where end time is passed.
                /// </summary>
                [System.ComponentModel.Description("Completed")]
                Completed = 4,
                /// <summary>
                /// Event that was cancelled.
                /// </summary>
                [System.ComponentModel.Description("Cancelled")]
                Cancelled = 5
            }

            // TODO: move methods from EventManager to Event that directly manipulate the Event object itself
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            public class Event : System.ICloneable
            {
                public const string OrderReservationFieldDescriptorName = "Order Reservation Field Descriptor";
                public const string OrderIsIntegrationFieldDescriptorName = "Order Is Integration Field Descriptor";
                public const string PropertyNameName = "Name";
                public const string PropertyNameStartTime = "Start Time";
                public const string PropertyNameEndTime = "End Time";
                public const string PropertyNameInfo = "Info";
                public const string PropertyNameProjectNumber = "Project Number";
                public const string PropertyNameProductNumbers = "Product Number";
                public const string PropertyNameAttachments = "Attachments";
                public const string PropertyNameContract = "Contract";
                public const string PropertyNameStatus = "Status";
                public const string PropertyNameCompany = "Company";
                public const string PropertyNameInternal = "Internal";
                public const string PropertyNameIntegration = "Integration";
                public const string PropertyNameOperatorNotes = "Operator Notes";
                public const string PropertyNameCompanyOfCreator = "Company Of Creator";
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager jobManager;
                private readonly Skyline.DataMiner.Net.Jobs.Job job;
                private readonly Skyline.DataMiner.Net.Sections.SectionDefinition customEventSectionDefinition;
                private readonly Skyline.DataMiner.Net.Sections.Section customEventSection;
                private readonly Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition;
                public Event(Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition, Skyline.DataMiner.Net.Sections.SectionDefinition customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    if (orderSectionDefinition == null)
                        throw new System.ArgumentNullException(nameof(orderSectionDefinition));
                    if (customEventSectionDefinition == null)
                        throw new System.ArgumentNullException(nameof(customEventSectionDefinition));
                    progressReporter?.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor");
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    jobManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager(Skyline.DataMiner.Automation.Engine.SLNetRaw, progressReporter);
                    job = new Skyline.DataMiner.Net.Jobs.Job();
                    var defaultJobDomain = jobManager.GetDefaultJobDomain();
                    if (defaultJobDomain == null)
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AddOrUpdateEventFailedException("Default job domain not found");
                    job.JobDomainID = defaultJobDomain.ID;
                    this.customEventSectionDefinition = customEventSectionDefinition;
                    this.customEventSection = new Skyline.DataMiner.Net.Sections.Section(customEventSectionDefinition);
                    job.Sections.Add(customEventSection);
                    this.orderSectionDefinition = orderSectionDefinition;
                    IsInternal = false; // Default value. Should be set to make sure Event is visible in app UI.
                    progressReporter?.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor", null, stopwatch);
                }

                public Event(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.LiteOrder order): this(helpers?.EventManager?.OrderSectionDefinition, helpers?.EventManager?.CustomEventSectionDefinition, helpers?.ProgressReporter)
                {
                    if (helpers == null)
                        throw new System.ArgumentNullException(nameof(helpers));
                    if (order == null)
                        throw new System.ArgumentNullException(nameof(order));
                    helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor", out var stopwatch);
                    Name = order.RecurringSequenceInfo?.Name ?? order.Name;
                    Start = order.Start;
                    End = order.End;
                    Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status.Confirmed;
                    Contract = order.Contract;
                    Company = order.Company;
                    CompanyOfCreator = order.Company;
                    IntegrationType = order.IntegrationType;
                    SecurityViewIds = order.SecurityViewIds;
                    if (order.Id != System.Guid.Empty)
                    {
                        AddOrUpdateOrderSection(order, helpers.ProgressReporter);
                        helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor", $"Added order section with ID '{order.Id}' to job object.");
                    }
                    else
                    {
                        helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor", $"Order ID is empty, no order section added to job object.");
                    }

                    helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), "Constructor", null, stopwatch);
                }

                public Event(Skyline.DataMiner.Net.Jobs.Job job, Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition, Skyline.DataMiner.Net.Sections.SectionDefinition customEventSectionDefinition)
                {
                    this.job = job ?? throw new System.ArgumentNullException(nameof(job));
                    jobManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager(Skyline.DataMiner.Automation.Engine.SLNetRaw);
                    this.customEventSectionDefinition = customEventSectionDefinition;
                    this.customEventSection = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetCustomEventSection(job, customEventSectionDefinition.GetID().Id);
                    this.orderSectionDefinition = orderSectionDefinition;
                    Id = job.ID.Id;
                }

                public System.Guid Id
                {
                    get;
                    set;
                }

                public string Name
                {
                    get => Skyline.DataMiner.Net.Sections.DefaultJobSectionExtensions.GetJobName(job);
                    set => Skyline.DataMiner.Net.Sections.DefaultJobSectionExtensions.SetJobName(job, value);
                }

                public System.DateTime Start
                {
                    get => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetStartTime(job);
                    set => Skyline.DataMiner.Net.Sections.DefaultJobSectionExtensions.SetJobStartTime(job, value);
                }

                public System.DateTime End
                {
                    get => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetEndTime(job);
                    set => Skyline.DataMiner.Net.Sections.DefaultJobSectionExtensions.SetJobEndTime(job, value);
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status Status
                {
                    get => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.StringExtensions.GetEnumValue<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status>(System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameStatus) ?? string.Empty));
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameStatus, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescription(value));
                }

                public string ProjectNumber
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameProjectNumber) ?? string.Empty);
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameProjectNumber, value ?? string.Empty);
                }

                public System.Collections.Generic.IEnumerable<System.String> ProductNumbers
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameProductNumbers) ?? string.Empty).Split('/') ?? new string[0];
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameProductNumbers, value != null ? string.Join("/", value) : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                public string Info
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameInfo) ?? string.Empty);
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameInfo, value ?? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                public System.Collections.Generic.IEnumerable<System.String> Attachments
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameInfo) ?? string.Empty).Split('\n');
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameAttachments, value != null ? string.Join("\n", value) : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                /// <summary>
                /// The Contract for this Event.
                /// </summary>
                public string Contract
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, PropertyNameContract) ?? string.Empty);
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameContract, value ?? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                /// <summary>
                /// The Company of this Event.
                /// </summary>
                public string Company
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, PropertyNameCompany) ?? string.Empty);
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameCompany, value ?? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                /// <summary>
                /// The Company of the user who created this Event.
                /// </summary>
                public string CompanyOfCreator
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, PropertyNameCompanyOfCreator));
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameCompanyOfCreator, value ?? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.NotApplicable);
                }

                public bool IsInternal
                {
                    get => System.Convert.ToBoolean(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameInternal));
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameInternal, value.ToString());
                }

                public string OperatorNotes
                {
                    get => System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameOperatorNotes));
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameOperatorNotes, value ?? string.Empty);
                }

                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> EventLevelServices
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType IntegrationType
                {
                    get => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.StringExtensions.GetEnumValue<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType>(System.Convert.ToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameIntegration)));
                    set => jobManager.UpdateSectionField(customEventSection, customEventSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameIntegration, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescription(value));
                }

                /// <summary>
                /// Gets a collection of Cube View IDs of views where this event is visible.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Int32> SecurityViewIds
                {
                    get => new System.Collections.Generic.HashSet<System.Int32>(job.SecurityViewIDs);
                    set => job.SecurityViewIDs = value != null ? System.Linq.Enumerable.ToList(value) : new System.Collections.Generic.List<System.Int32>();
                }

                /// <summary>
                /// A collection of Guids representing the Orders that belong to this Event.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Guid> OrderIds => new System.Collections.Generic.HashSet<System.Guid>(System.Linq.Enumerable.Select(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetOrderSections(job), s => s.OrderId));
                /// <summary>
                /// A Dictionary indicating whether an order is made by an integration or not. Used for performance reasons when merging events.
                /// </summary>
                public System.Collections.Generic.Dictionary<System.Guid, System.Boolean> OrderIsIntegrations => System.Linq.Enumerable.ToDictionary(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetOrderSections(job), s => s.OrderId, s => s.OrderIsIntegration);
                public void AddOrUpdateOrderSection(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.LiteOrder order, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    if (order == null)
                        throw new System.ArgumentNullException(nameof(order));
                    progressReporter?.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), nameof(AddOrUpdateOrderSection));
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    bool addOrUpdateNotRequired = order.Id == System.Guid.Empty || (HasOrder(order.Id) && OrderSectionIsValid(order.Id));
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), nameof(AddOrUpdateOrderSection), $"Add or Update Order Section is {(addOrUpdateNotRequired ? "not " : string.Empty)}required");
                    if (addOrUpdateNotRequired)
                    {
                        stopwatch.Stop();
                        progressReporter?.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), nameof(AddOrUpdateOrderSection), null, stopwatch);
                        return;
                    }

                    var newOrUpdatedOrderSection = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection(order, orderSectionDefinition, progressReporter);
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.AddOrUpdateOrderSection(job, newOrUpdatedOrderSection);
                    stopwatch.Stop();
                    progressReporter?.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event), nameof(AddOrUpdateOrderSection), null, stopwatch);
                }

                public bool HasOrder(System.Guid orderId)
                {
                    return OrderIds.Contains(orderId);
                }

                public bool OrderSectionIsValid(System.Guid orderId)
                {
                    var orderSection = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetOrderSection(job, orderId);
                    if (orderSection == null)
                        return false;
                    return orderSection.IsValid;
                }

                public object Clone()
                {
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event((Skyline.DataMiner.Net.Jobs.Job)job.Clone(), orderSectionDefinition, customEventSectionDefinition);
                }

                public override string ToString()
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            public class EventManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.IEventManager
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager jobManager;
                private Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition; // saved for performance reasons
                private Skyline.DataMiner.Net.Sections.SectionDefinition customEventSectionDefinition; // saved for performance reasons
                public EventManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    Helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                    jobManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager(Skyline.DataMiner.Automation.Engine.SLNetRaw, Helpers.ProgressReporter);
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers Helpers
                {
                    get;
                }

                public Skyline.DataMiner.Net.Sections.SectionDefinition OrderSectionDefinition => orderSectionDefinition ?? (orderSectionDefinition = jobManager.GetOrderSectionDefinition());
                public Skyline.DataMiner.Net.Sections.SectionDefinition CustomEventSectionDefinition => customEventSectionDefinition ?? (customEventSectionDefinition = jobManager.GetCustomEventSectionDefinition());
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event> GetAllEvents()
                {
                    var @events = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event>();
                    var jobs = jobManager.GetJobs(Skyline.DataMiner.Net.Jobs.JobExposerExtensions.JobStartGreaterThan(Skyline.DataMiner.Net.Jobs.JobExposers.FieldValues, default(System.DateTime)));
                    foreach (var job in jobs)
                        @events.Add(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event(job, OrderSectionDefinition, CustomEventSectionDefinition));
                    return @events;
                }
            }

            public interface IEventManager
            {
                System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event> GetAllEvents();
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            public class JobManager
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter;
                private readonly Skyline.DataMiner.Net.Jobs.JobManagerHelper helper;
                private System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.Sections.SectionDefinition> allSectionDefinitions;
                public JobManager(Skyline.DataMiner.Net.Connection connection, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    this.progressReporter = progressReporter;
                    helper = new Skyline.DataMiner.Net.Jobs.JobManagerHelper(m => connection.HandleMessages(m));
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.Sections.SectionDefinition> AllSectionDefinitions => allSectionDefinitions ?? (allSectionDefinitions = helper.SectionDefinitions.Read(Skyline.DataMiner.Net.Messages.SLDataGateway.ExposerExtensions.NotEqual(Skyline.DataMiner.Net.Sections.SectionDefinitionExposers.Name, string.Empty)));
                public System.Collections.Generic.List<Skyline.DataMiner.Net.Jobs.Job> GetJobs(Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<Skyline.DataMiner.Net.Jobs.Job> filter)
                {
                    var jobs = helper.Jobs.Read(filter);
                    if (jobs == null)
                        return new System.Collections.Generic.List<Skyline.DataMiner.Net.Jobs.Job>();
                    helper.StitchJobs(jobs);
                    return jobs;
                }

                public Skyline.DataMiner.Net.Jobs.JobDomain GetDefaultJobDomain()
                {
                    var eventSectionDefinition = GetCustomEventSectionDefinition();
                    if (eventSectionDefinition == null)
                        return null;
                    var eventSectionDefinitionId = eventSectionDefinition.GetID();
                    if (eventSectionDefinitionId == null)
                        return null;
                    var jobDomains = helper.JobDomains.Read(Skyline.DataMiner.Net.Messages.SLDataGateway.ListManagedFilterExtensions.Contains(Skyline.DataMiner.Net.Jobs.JobDomainExposers.SectionDefinitionIDs, eventSectionDefinitionId.Id));
                    if (jobDomains == null)
                        return null;
                    return System.Linq.Enumerable.FirstOrDefault(jobDomains);
                }

                public void UpdateSectionField(Skyline.DataMiner.Net.Sections.Section section, Skyline.DataMiner.Net.Sections.SectionDefinition sectionDefinition, string name, string value)
                {
                    if (section == null)
                        throw new System.ArgumentNullException(nameof(section));
                    if (sectionDefinition == null)
                        throw new System.ArgumentNullException(nameof(sectionDefinition));
                    if (string.IsNullOrWhiteSpace(name))
                        throw new System.ArgumentNullException(nameof(name));
                    var fieldDescriptor = GetFieldDescriptor(sectionDefinition, name);
                    if (fieldDescriptor == null)
                        return;
                    section.AddOrReplaceFieldValue(new Skyline.DataMiner.Net.Sections.FieldValue(fieldDescriptor)
                    {Value = new Skyline.DataMiner.Net.Sections.ValueWrapper<System.String>(value)});
                }

                public Skyline.DataMiner.Net.Sections.SectionDefinition GetCustomEventSectionDefinition()
                {
                    LogMethodStart(nameof(GetCustomEventSectionDefinition), out var stopwatch);
                    var customEventSectionDefinition = System.Linq.Enumerable.FirstOrDefault(AllSectionDefinitions, x => System.Linq.Enumerable.Any(x.GetAllFieldDescriptors(), y => y.Name.Equals("Status", System.StringComparison.OrdinalIgnoreCase)));
                    LogMethodCompleted(nameof(GetCustomEventSectionDefinition), stopwatch);
                    // search for a section with a field named Status
                    return customEventSectionDefinition;
                }

                public Skyline.DataMiner.Net.Sections.SectionDefinition GetOrderSectionDefinition()
                {
                    LogMethodStart(nameof(GetOrderSectionDefinition), out var stopwatch);
                    var orderSectionDefinition = System.Linq.Enumerable.FirstOrDefault(AllSectionDefinitions, sd => sd.GetName().Equals("Orders", System.StringComparison.OrdinalIgnoreCase));
                    LogMethodCompleted(nameof(GetOrderSectionDefinition), stopwatch);
                    return orderSectionDefinition;
                }

                public Skyline.DataMiner.Net.Sections.FieldDescriptor GetFieldDescriptor(Skyline.DataMiner.Net.Sections.SectionDefinition sectionDefinition, string name)
                {
                    if (sectionDefinition == null)
                        throw new System.ArgumentNullException(nameof(sectionDefinition));
                    if (string.IsNullOrWhiteSpace(name))
                        throw new System.ArgumentNullException(nameof(name));
                    var sectionDefinitionFieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
                    return System.Linq.Enumerable.FirstOrDefault(sectionDefinitionFieldDescriptors, d => d.Name.Contains(name));
                }

                public Skyline.DataMiner.Net.Sections.ReservationFieldDescriptor GetOrCreateOrderReservationIdFieldDescriptor(Skyline.DataMiner.Net.Sections.SectionDefinition sectionDefinition)
                {
                    var fieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
                    var reservationIdFieldDescriptor = System.Linq.Enumerable.FirstOrDefault(fieldDescriptors, f => f is Skyline.DataMiner.Net.Sections.ReservationFieldDescriptor);
                    if (reservationIdFieldDescriptor == null)
                        throw new System.Exception("Unable to find field descriptor");
                    return reservationIdFieldDescriptor as Skyline.DataMiner.Net.Sections.ReservationFieldDescriptor;
                /*
			VSC 04/11/2021: No longer necessary 

            var newReservationFieldDescriptor = new ReservationFieldDescriptor
            {
                FieldType = typeof(Guid),
                Name = Event.OrderReservationFieldDescriptorName,
                IsHidden = false,
                IsOptional = true
            };

            sectionDefinition.AddOrReplaceFieldDescriptor(newReservationFieldDescriptor);
            UpdateSectionDefinition(sectionDefinition);
			*/
                }

                public Skyline.DataMiner.Net.Sections.FieldDescriptor GetOrCreateOrderIsIntegrationFieldDescriptor(Skyline.DataMiner.Net.Sections.SectionDefinition sectionDefinition)
                {
                    var fieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
                    var orderIsIntegrationFieldDescriptor = System.Linq.Enumerable.FirstOrDefault(fieldDescriptors, f => f.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.OrderIsIntegrationFieldDescriptorName);
                    if (orderIsIntegrationFieldDescriptor == null)
                        throw new System.Exception("Unable to find field descriptor");
                    return orderIsIntegrationFieldDescriptor;
                /*
			VSC 04/11/2021: No longer necessary 


	        var newOrderIsIntegrationFieldDescriptor = new FieldDescriptor
	        {
		        FieldType = typeof(bool),
		        Name = Event.OrderIsIntegrationFieldDescriptorName,
		        IsHidden = false,
		        IsOptional = true
	        };

	        sectionDefinition.AddOrReplaceFieldDescriptor(newOrderIsIntegrationFieldDescriptor);
	        UpdateSectionDefinition(sectionDefinition);
			*/
                }

                protected void LogMethodStart(string nameOfMethod, out System.Diagnostics.Stopwatch stopwatch)
                {
                    Log(nameOfMethod, "Start");
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();
                }

                protected void LogMethodCompleted(string nameOfMethod, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    stopwatch?.Stop();
                    Log(nameOfMethod, $"Completed [{stopwatch?.Elapsed}]");
                }

                protected void Log(string nameOfMethod, string message, string nameOfObject = null)
                {
                    progressReporter?.LogProgress(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressLog(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.EventManager), nameOfMethod, message, nameOfObject));
                }
            }

            public class OrderSection : Skyline.DataMiner.Net.Sections.Section
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter;
                private Skyline.DataMiner.Net.Sections.ReservationFieldDescriptor reservationFieldDescriptor;
                private Skyline.DataMiner.Net.Sections.FieldDescriptor isIntegrationFieldDescriptor;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager jobManager;
                private readonly Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition;
                private System.Guid orderId;
                private bool orderIdIsValid;
                private bool orderIsIntegration;
                private bool orderIsIntegrationIsValid;
                public OrderSection(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.LiteOrder order, Skyline.DataMiner.Net.Sections.SectionDefinition orderSectionDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    if (order == null)
                        throw new System.ArgumentNullException(nameof(order));
                    this.orderSectionDefinition = orderSectionDefinition ?? throw new System.ArgumentNullException(nameof(orderSectionDefinition));
                    this.progressReporter = progressReporter;
                    this.jobManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.JobManager(Skyline.DataMiner.Automation.Engine.SLNetRaw, progressReporter);
                    GetFieldDescriptors();
                    Section = new Skyline.DataMiner.Net.Sections.Section(orderSectionDefinition);
                    OrderId = order.Id;
                    OrderIsIntegration = order.IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.None;
                }

                public OrderSection(Skyline.DataMiner.Net.Sections.Section section)
                {
                    this.Section = section;
                    foreach (var fieldValue in section.FieldValues)
                    {
                        var fieldDescriptor = fieldValue.GetFieldDescriptor();
                        if (fieldDescriptor == null)
                            continue;
                        if (fieldDescriptor is Skyline.DataMiner.Net.Sections.ReservationFieldDescriptor)
                        {
                            if (fieldValue?.Value == null || fieldValue.Value.Type != typeof(System.Guid))
                                throw new System.Exception($"fieldDescriptor {fieldDescriptor.ID} does not contain a Guid");
                            orderId = (System.Guid)fieldValue.Value.Value;
                            orderIdIsValid = true;
                        }
                        else if (fieldDescriptor.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.OrderIsIntegrationFieldDescriptorName)
                        {
                            if (fieldValue?.Value == null || fieldValue.Value.Type != typeof(bool))
                                throw new System.Exception($"fieldDescriptor {fieldDescriptor.ID} does not contain a boolean");
                            orderIsIntegration = (bool)fieldValue.Value.Value;
                            orderIsIntegrationIsValid = true;
                        }
                    }
                }

                public bool IsValid => orderIdIsValid && orderIsIntegrationIsValid;
                public Skyline.DataMiner.Net.Sections.Section Section
                {
                    get;
                }

                public System.Guid OrderId
                {
                    get => orderId;
                    set
                    {
                        orderId = value;
                        Section.AddOrReplaceFieldValue(new Skyline.DataMiner.Net.Sections.FieldValue(reservationFieldDescriptor)
                        {Value = new Skyline.DataMiner.Net.Sections.ValueWrapper<System.Guid>(orderId)});
                        orderIdIsValid = true;
                    }
                }

                public bool OrderIsIntegration
                {
                    get => orderIsIntegration;
                    set
                    {
                        orderIsIntegration = value;
                        Section.AddOrReplaceFieldValue(new Skyline.DataMiner.Net.Sections.FieldValue(isIntegrationFieldDescriptor)
                        {Value = new Skyline.DataMiner.Net.Sections.ValueWrapper<System.Boolean>(orderIsIntegration)});
                        orderIsIntegrationIsValid = true;
                    }
                }

                private void GetFieldDescriptors()
                {
                    reservationFieldDescriptor = jobManager.GetOrCreateOrderReservationIdFieldDescriptor(orderSectionDefinition);
                    if (reservationFieldDescriptor == null)
                        throw new System.Exception($"Unable to get or create reservation Id field descriptor");
                    isIntegrationFieldDescriptor = jobManager.GetOrCreateOrderIsIntegrationFieldDescriptor(orderSectionDefinition);
                    if (isIntegrationFieldDescriptor == null)
                        throw new System.Exception($"Unable to get or create order is integration field descriptor");
                }
            }
        }

        namespace Exceptions
        {
            public class AddOrUpdateEventFailedException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public AddOrUpdateEventFailedException()
                {
                }

                public AddOrUpdateEventFailedException(string message): base(message)
                {
                }

                public AddOrUpdateEventFailedException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class EdgeNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public EdgeNotFoundException()
                {
                }

                public EdgeNotFoundException(int NodeId, System.Guid serviceDefinitionId): base($"Unable to find Edge connected to node {NodeId} in service definition {serviceDefinitionId}")
                {
                }

                public EdgeNotFoundException(string message): base(message)
                {
                }

                public EdgeNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            [System.Serializable]
            public class ElementByProtocolNotFoundException : System.Exception
            {
                public ElementByProtocolNotFoundException(string protocolName): base($"Unable to find any active elements with protocol {protocolName} on the DMA.")
                {
                }
            }

            public class ElementNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public ElementNotFoundException()
                {
                }

                public ElementNotFoundException(string protocol): base($"Unable to find Element with protocol {protocol}")
                {
                }

                public ElementNotFoundException(string protocol, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM.InterplayPamElements elementName): base($"Unable to find {protocol} {elementName.ToString()} Element")
                {
                }

                public ElementNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class FunctionDefinitionNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public FunctionDefinitionNotFoundException()
                {
                }

                public FunctionDefinitionNotFoundException(string name): base($"Unable to find Function Definition with name {name}")
                {
                }

                public FunctionDefinitionNotFoundException(System.Guid ID): base($"Unable to find Function Definition with ID {ID}")
                {
                }

                public FunctionDefinitionNotFoundException(string name, System.Guid ID): base($"Unable to find Function Definition with name {name} and ID {ID}")
                {
                }

                public FunctionDefinitionNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class FunctionNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public FunctionNotFoundException()
                {
                }

                public FunctionNotFoundException(string name, bool labelInsteadOfName = false): base(labelInsteadOfName ? $"Unable to find Function with label {name}" : $"Unable to find Function with name {name}")
                {
                }

                public FunctionNotFoundException(string name, System.Collections.Generic.IEnumerable<System.String> options, bool labelInsteadOfName = false): base($"Unable to find Function with {(labelInsteadOfName ? "label" : "name")} {name} among options {string.Join(", ", options)}")
                {
                }

                public FunctionNotFoundException(System.Guid ID): base($"Unable to find Function with ID {ID}")
                {
                }

                public FunctionNotFoundException(int NodeId): base($"Unable to find Function with Node ID {NodeId}")
                {
                }

                public FunctionNotFoundException(string name, System.Guid ID): base($"Unable to find Function with name {name} and ID {ID}")
                {
                }

                public FunctionNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class FunctionParameterNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public FunctionParameterNotFoundException()
                {
                }

                public FunctionParameterNotFoundException(string name): base($"Unable to find Function Parameter with name {name}")
                {
                }

                public FunctionParameterNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            [System.Serializable]
            public abstract class MediaServicesException : System.Exception
            {
                protected MediaServicesException()
                {
                }

                protected MediaServicesException(string message): base(message)
                {
                }

                protected MediaServicesException(string message, System.Exception inner): base(message, inner)
                {
                }

                protected MediaServicesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context): base(info, context)
                {
                }
            }

            public class NodeNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public NodeNotFoundException()
                {
                }

                public NodeNotFoundException(string message): base(message)
                {
                }

                public NodeNotFoundException(System.Guid ID): base($"Unable to find Function with ID {ID}")
                {
                }

                public NodeNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class ProfileParameterNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public ProfileParameterNotFoundException()
                {
                }

                public ProfileParameterNotFoundException(string name, string functionName = null, System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> collectionThatShouldContainProfileParameter = null): base($"Unable to find Profile Parameter with name {name}{(!string.IsNullOrWhiteSpace(functionName) ? $" in function {functionName}" : string.Empty)}{(collectionThatShouldContainProfileParameter != null ? $" between {string.Join(",", System.Linq.Enumerable.Select(collectionThatShouldContainProfileParameter, p => p.Name))}" : string.Empty)}")
                {
                }

                public ProfileParameterNotFoundException(System.Guid ID, System.Collections.Generic.IEnumerable<System.Guid> options = null): base($"Unable to find Profile Parameter with ID {ID} among options '{string.Join(", ", options)}'")
                {
                }

                public ProfileParameterNotFoundException(string name, System.Guid ID): base($"Unable to find Profile Parameter with name {name} and ID {ID}")
                {
                }

                public ProfileParameterNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class ReservationNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public ReservationNotFoundException()
                {
                }

                public ReservationNotFoundException(string name): base($"Unable to find Reservation with name {name}")
                {
                }

                public ReservationNotFoundException(System.Guid ID): base($"Unable to find Reservation with ID {ID}")
                {
                }

                public ReservationNotFoundException(string message, System.Exception inner): base($"No Reservation Instance found with ID {message}", inner)
                {
                }
            }

            public class ResourcePoolNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public ResourcePoolNotFoundException()
                {
                }

                public ResourcePoolNotFoundException(string name): base($"Unable to find Resource Pool with name {name}")
                {
                }

                public ResourcePoolNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }

            public class ServiceReservationPropertyNotFoundException : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.MediaServicesException
            {
                public ServiceReservationPropertyNotFoundException()
                {
                }

                public ServiceReservationPropertyNotFoundException(string propertyName, string serviceName): base($"Unable to find property {propertyName} on service {serviceName}")
                {
                }

                public ServiceReservationPropertyNotFoundException(string message, System.Exception inner): base(message, inner)
                {
                }
            }
        }

        namespace Function
        {
            public class Function : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource resource;
                private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource initialResource;
                private System.Collections.Generic.List<Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource> selectableResources;
                public Function()
                {
                    Parameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    InterfaceParameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    SelectableResources = new System.Collections.Generic.List<Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource>();
                }

                public Function(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservation, Skyline.DataMiner.Net.ServiceManager.Objects.Node serviceDefinitionNode, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition serviceDefinition)
                {
                    if (reservation == null)
                        throw new System.ArgumentNullException(nameof(reservation));
                    if (serviceDefinitionNode == null)
                        throw new System.ArgumentNullException(nameof(serviceDefinitionNode));
                    if (serviceDefinition == null)
                        throw new System.ArgumentNullException(nameof(serviceDefinition));
                    helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function), out var stopwatch);
                    var optionsProperty = System.Linq.Enumerable.FirstOrDefault(serviceDefinitionNode.Properties, p => p.Name == "Options");
                    var isOptional = optionsProperty != null && optionsProperty.Value.Contains("Optional");
                    var configurationOrderProperty = System.Linq.Enumerable.FirstOrDefault(serviceDefinitionNode.Properties, p => p.Name == "ConfigurationOrder");
                    var configurationOrder = configurationOrderProperty != null ? System.Convert.ToInt32(configurationOrderProperty.Value) : 0;
                    var functionDefinition = System.Linq.Enumerable.FirstOrDefault(serviceDefinition.FunctionDefinitions, f => f.Label == serviceDefinitionNode.Label);
                    Id = serviceDefinitionNode.Configuration.FunctionID;
                    NodeId = serviceDefinitionNode.ID;
                    Definition = functionDefinition;
                    Name = functionDefinition.Name;
                    ConfigurationOrder = configurationOrder;
                    Parameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    InterfaceParameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    IsOptional = isOptional;
                    // update function parameters and resource
                    var functionInReservation = System.Linq.Enumerable.FirstOrDefault(Skyline.DataMiner.Library.Solutions.SRM.ReservationInstanceExtensions.GetFunctionData(reservation), f => f.Id == NodeId);
                    var resourceUsageDefinition = System.Linq.Enumerable.FirstOrDefault(reservation.ResourcesInReservationInstance, r => (r is Skyline.DataMiner.Net.ResourceManager.Objects.ServiceResourceUsageDefinition) && (r as Skyline.DataMiner.Net.ResourceManager.Objects.ServiceResourceUsageDefinition).ServiceDefinitionNodeID == NodeId);
                    UpdateFunctionProfileParametersAndResource(helpers, functionInReservation, resourceUsageDefinition);
                    helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function), null, stopwatch);
                }

                /// <summary>
                /// The id of this function, this matches the Id of the FunctionDefinition.
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>s
                public System.Guid Id
                {
                    get;
                    set;
                }

                /// <summary>
                /// The name of this function, this matches the name of the FunctionDefinition.
                /// </summary>
                public string Name
                {
                    get;
                    set;
                }

                /// <summary>
                /// The node id of this function node in the service definition
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>
                public int NodeId
                {
                    get;
                    set;
                }

                /// <summary>
                /// The configuration order property value in the service definition.
                /// </summary>
                public int ConfigurationOrder
                {
                    get;
                    set;
                }

                /// <summary>
                /// The list of parameters for the service
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> Parameters
                {
                    get;
                    set;
                }

                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> InterfaceParameters
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> NonDtrNonAudioProfileParameters
                {
                    get
                    {
                        return System.Linq.Enumerable.Where(Parameters, p => !p.IsNonInterfaceDtrParameter && !System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AllAudioChannelConfigurationGuids, p.Id));
                    }
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> Capabilities => System.Linq.Enumerable.Concat(System.Linq.Enumerable.Where(Parameters, p => p.IsCapability), System.Linq.Enumerable.Where(InterfaceParameters, p => p.IsCapability));
                /// <summary>
                /// Gets or sets the resource used for this function.
                /// Should never be passed (internal property).
                /// </summary>
                public Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource Resource
                {
                    get => resource;
                    set
                    {
                        if (resource != null && resource.Equals(value))
                            return;
                        resource = value;
                        // Clear Other Satellite Name profile parameter in case of Satellite Function and selected Resource is not Other
                        if (Name.Equals("Satellite", System.StringComparison.InvariantCultureIgnoreCase) && resource != null && !resource.Name.Equals("Other", System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter otherSatelliteNameProfileParameter = System.Linq.Enumerable.FirstOrDefault(Parameters, x => x.Name.Equals("Other Satellite Name", System.StringComparison.InvariantCultureIgnoreCase));
                            if (otherSatelliteNameProfileParameter != null)
                                otherSatelliteNameProfileParameter.Value = System.String.Empty;
                        }

                        ResourceChanged?.Invoke(this, new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function.ResourceChangedEventArgs(ResourceName));
                    }
                }

                /// <summary>
                /// Property used by UI.
                /// </summary>
                public string ResourceName => Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(Resource, Id) : "None";
                public event System.EventHandler<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function.ResourceChangedEventArgs> ResourceChanged;
                /// <summary>
                /// A collection of selectable Resources. Set by Controller and used by UI.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.List<Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource> SelectableResources
                {
                    get => selectableResources;
                    set
                    {
                        selectableResources = value;
                        SelectableResourcesChanged?.Invoke(this, new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function.SelectableResourcesChangedEventArgs(SelectableResourceNames));
                    }
                }

                /// <summary>
                /// Property used by UI.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.IEnumerable<System.String> SelectableResourceNames
                {
                    get
                    {
                        var selectableResourceNames = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(SelectableResources, r => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(r, Id)));
                        selectableResourceNames.Add("None");
                        return selectableResourceNames;
                    }
                }

                public event System.EventHandler<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function.SelectableResourcesChangedEventArgs> SelectableResourcesChanged;
                /// <summary>
                /// Gets a boolean indicating if Change Tracking is enabled.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool IsChanged => ChangeInfo.IsChanged;
                /// <summary>
                /// Gets an object containing all change info since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                /// <summary>
                /// Function Definition for this function.
                /// Used in the function sections.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition Definition
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this node is optional or not
                /// Only required when action is "NEW" or "EDIT"
                /// This can be read out from the "Options" property on the node, value will be "Optional" in case the node is optional
                /// </summary>
                public bool IsOptional
                {
                    get;
                    set;
                }

                public bool RequiresResource
                {
                    get;
                    set;
                }

                = true;
                public bool McrHasOverruledFixedTieLineLogic
                {
                    get;
                    set;
                }

                = false;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration Configuration
                {
                    get
                    {
                        var functionConfiguration = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration{Id = Id, Name = Name, ResourceId = Resource?.ID ?? System.Guid.Empty, ResourceName = ResourceName, ProfileParameters = new System.Collections.Generic.Dictionary<System.Guid, System.Object>(), RequiresResource = RequiresResource, McrHasOverruledFixedTieLineLogic = McrHasOverruledFixedTieLineLogic};
                        if (Parameters != null)
                        {
                            foreach (var functionParameter in Parameters)
                                functionConfiguration.ProfileParameters[functionParameter.Id] = functionParameter.Value;
                        }

                        if (InterfaceParameters != null)
                        {
                            foreach (var functionParameter in InterfaceParameters)
                                functionConfiguration.ProfileParameters[functionParameter.Id] = functionParameter.Value;
                        }

                        return functionConfiguration;
                    }
                }

                public void UpdateFunctionProfileParametersAndResource(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.Library.Solutions.SRM.Model.Function functionInReservation, Skyline.DataMiner.Net.Messages.ResourceUsageDefinition resourceUsageDefinition)
                {
                    if (functionInReservation == null)
                        throw new System.ArgumentNullException(nameof(functionInReservation));
                    foreach (var reservationProfileParameter in functionInReservation.Parameters)
                    {
                        var functionProfileParameter = System.Linq.Enumerable.FirstOrDefault(Parameters, p => p.Id == reservationProfileParameter.Id);
                        if (functionProfileParameter == null)
                        {
                            functionProfileParameter = helpers.ProfileManager.GetProfileParameter(reservationProfileParameter.Id);
                            Parameters.Add(functionProfileParameter);
                        }

                        functionProfileParameter.Value = reservationProfileParameter.Value;
                    }

                    foreach (var reservationInterfaceProfileParameter in System.Linq.Enumerable.SelectMany(functionInReservation.AllInterfacesInFunction, i => i.Parameters))
                    {
                        var functionInterfaceProfileParameter = System.Linq.Enumerable.FirstOrDefault(InterfaceParameters, p => p.Id == reservationInterfaceProfileParameter.Id);
                        if (functionInterfaceProfileParameter == null)
                        {
                            functionInterfaceProfileParameter = helpers.ProfileManager.GetProfileParameter(reservationInterfaceProfileParameter.Id);
                            InterfaceParameters.Add(functionInterfaceProfileParameter);
                        }

                        functionInterfaceProfileParameter.Value = reservationInterfaceProfileParameter.Value;
                    }

                    if (resourceUsageDefinition != null && resourceUsageDefinition.GUID != System.Guid.Empty)
                    {
                        Resource = (Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource)Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager.GetResource(resourceUsageDefinition.GUID);
                    }
                    else
                    {
                        Resource = null;
                    }
                }

                /// <summary>
                /// Gets the types of changes that this function underwent since construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes that this function underwent.</returns>
                /// <see cref = "IYleChangeTracking"/>
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    bool resourceWasAdded = initialResource == null && Resource != null;
                    bool resourceWasRemoved = initialResource != null && Resource == null;
                    bool resourceWasChanged = initialResource != null && Resource != null && initialResource.ID != Resource.ID;
                    if (resourceWasAdded || resourceWasChanged)
                        changeInfo.ResourceChangeInfo.MarkResourceAddedOrSwapped();
                    else if (resourceWasRemoved)
                        changeInfo.ResourceChangeInfo.MarkResourceRemoved();
                    var allProfileParameters = System.Linq.Enumerable.Concat(Parameters, InterfaceParameters);
                    foreach (var parameter in allProfileParameters)
                        changeInfo.CombineWith(parameter.ChangeInfo);
                    return changeInfo;
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialResource = Resource;
                    foreach (var parameter in System.Linq.Enumerable.Concat(Parameters, InterfaceParameters))
                        parameter.AcceptChanges();
                }

                public class ResourceChangedEventArgs : System.EventArgs
                {
                    internal ResourceChangedEventArgs(string resourceName)
                    {
                        ResourceName = resourceName;
                    }

                    public string ResourceName
                    {
                        get;
                        private set;
                    }
                }

                public class SelectableResourcesChangedEventArgs : System.EventArgs
                {
                    internal SelectableResourcesChangedEventArgs(System.Collections.Generic.IEnumerable<System.String> selectableResourceNames)
                    {
                        SelectableResourceNames = selectableResourceNames;
                    }

                    public System.Collections.Generic.IEnumerable<System.String> SelectableResourceNames
                    {
                        get;
                        private set;
                    }
                }
            }

            public class FunctionDefinition
            {
                public System.Guid Id
                {
                    get;
                    set;
                }

                public string Name
                {
                    get;
                    set;
                }

                /// <summary>
                /// The label assigned to this function definition in the service definition.
                /// </summary>
                public string Label
                {
                    get;
                    set;
                }

                public int ConfigurationOrder
                {
                    get;
                    internal set;
                }

                public string Options
                {
                    get;
                    internal set;
                }

                public string ResourcePool
                {
                    get;
                    internal set;
                }

                public bool IsHidden
                {
                    get;
                    internal set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition ProfileDefinition
                {
                    get;
                    internal set;
                }

                public System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition> InterfaceProfileDefinitions
                {
                    get;
                    internal set;
                }

                public System.Collections.Generic.List<System.Guid> Children
                {
                    get;
                    internal set;
                }

                /// <summary>
                /// Indicates if manual resource selection is allowed for this node in the service definition.
                /// Note that this is only applicable when retrieved via the service definition.
                /// </summary>
                public bool IsManualResourceSelectionAllowed
                {
                    get;
                    internal set;
                }
            }
        }

        namespace IngestExport
        {
            public enum Type
            {
                [System.ComponentModel.Description("Export")]
                Export = 0,
                [System.ComponentModel.Description("Import")]
                Import = 1,
                [System.ComponentModel.Description("Iplay Folder Creation")]
                IplayFolderCreation = 2,
                [System.ComponentModel.Description("Iplay WG Transfer")]
                IplayWgTransfer = 3,
                [System.ComponentModel.Description("Non-Interplay Project")]
                NonInterplayProject = 4
            }

            public enum State
            {
                [System.ComponentModel.Description("Preliminary")]
                Preliminary = 0,
                [System.ComponentModel.Description("Submitted")]
                Submitted = 1,
                [System.ComponentModel.Description("Work in Progress")]
                WorkInProgress = 2,
                [System.ComponentModel.Description("Change Requested")]
                ChangeRequested = 3,
                [System.ComponentModel.Description("Completed")]
                Completed = 4,
                [System.ComponentModel.Description("Cancelled")]
                Cancelled = 5
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("InteractiveAutomationToolkit.dll")]
            public abstract class NonLiveOrder
            {
                public const string DefaultOwner = "None";
                public const string OrderDescriptionTicketField = "Order Description";
                public const string DeadlineTicketField = "Deadline";
                public const string StartTimeTicketField = "Start Time";
                public const string OwnerTicketField = "Owner";
                public const string TypeTicketField = "Type";
                public const string DataTicketField = "Data";
                public const string StateTicketField = "State";
                public const string MaterialSourceTicketField = "Material Source";
                public const string ProgramNameTicketField = "Program Name";
                public const string DeliveryDateTicketField = "Delivery Date";
                public const string SourceFolderPathTicketField = "Source Folder Path";
                public const string TargetFolderPathTicketField = "Target Folder Path";
                public const string InterplayFolderPathTicketField = "Interplay Folder Path";
                public const string ExportInformationSubtitleAttachmentsPathTicketField = "Subtitle Attachments Path";
                public const string AdditionalInformationTicketField = "Additional Information";
                public const string CreatedByTicketField = "Created By";
                public const string ModifiedByTicketField = "Modified By";
                public const string LastModifiedByTicketField = "Last Modified By";
                public const string ShortDescriptionTicketField = "Short Description";
                public const string ReasonOfRejectionField = "Reason of Rejection";
                public const string TeamHkiField = "Team_HKI";
                public const string TeamNewsField = "Team_NEWS";
                public const string TeamTreField = "Team_TRE";
                public const string TeamVsaField = "Team_VSA";
                public const string TeamMgmtField = "Team_MGMT";
                /// <summary>
                /// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
                /// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
                /// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
                /// </summary>
                protected NonLiveOrder()
                {
                    DataMinerId = default(int? );
                    TicketId = default(int? );
                    State = default(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.State);
                    OrderDescription = default(string);
                    Deadline = default(System.DateTime);
                    Owner = default(string);
                    CreatedBy = default(string);
                    ModifiedBy = new System.Collections.Generic.HashSet<System.String>();
                    ReasonOfRejection = default(string);
                    TeamHki = default(bool);
                    TeamNews = default(bool);
                    TeamTre = default(bool);
                    TeamVsa = default(bool);
                    TeamMgmt = default(bool);
                }

                public abstract Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type OrderType
                {
                    get;
                }

                public abstract string ShortDescription
                {
                    get;
                }

                public int? DataMinerId
                {
                    get;
                    set;
                }

                public int? TicketId
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.State State
                {
                    get;
                    set;
                }

                public string OrderDescription
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty]
                public System.DateTime Deadline
                {
                    get;
                    set;
                }

                public System.DateTime StartTime
                {
                    get
                    {
                        return Deadline - System.TimeSpan.FromHours(1);
                    }
                }

                /// <summary>
                /// Gets or sets the user to which the ticket is assigned.
                /// </summary>
                public string Owner
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the user that created the ticket.
                /// </summary>
                public string CreatedBy
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the user that updated the ticket.
                /// </summary>
                public string LastModifiedBy
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the uses that modified the ticket.
                /// </summary>
                public System.Collections.Generic.HashSet<System.String> ModifiedBy
                {
                    get;
                    set;
                }

                public string ReasonOfRejection
                {
                    get;
                    set;
                }

                public bool TeamHki
                {
                    get;
                    set;
                }

                public bool TeamNews
                {
                    get;
                    set;
                }

                public bool TeamTre
                {
                    get;
                    set;
                }

                public bool TeamVsa
                {
                    get;
                    set;
                }

                public bool TeamMgmt
                {
                    get;
                    set;
                }

                public bool IsAssignedToSomeone => !string.IsNullOrWhiteSpace(Owner) && Owner.ToLower() != DefaultOwner.ToLower();
            }

            namespace Transfer
            {
                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class Transfer : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.NonLiveOrder
                {
                    /// <summary>
                    /// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
                    /// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
                    /// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
                    /// </summary>
                    public Transfer()
                    {
                        Source = default(string);
                        SourceFolder = default(string);
                        FileUrls = default(string[]);
                        FileType = default(string);
                        Destination = default(string);
                        ReceiverEmailAddress = default(string);
                        InterplayDestinationFolder = default(string);
                        AdditionalCustomerInformation = default(string);
                    }

                    [Newtonsoft.Json.JsonProperty]
                    public override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type OrderType
                    {
                        get
                        {
                            return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type.IplayWgTransfer;
                        }
                    }

                    [Newtonsoft.Json.JsonIgnore]
                    public override string ShortDescription
                    {
                        get => OrderDescription + " - " + Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type.IplayWgTransfer) + " - " + Source + " to " + Destination;
                    }

                    public string Source
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string SourceFolder
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string[] FileUrls
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string FileType
                    {
                        get;
                        set;
                    }

                    public string Destination
                    {
                        get;
                        set;
                    }

                    public string ReceiverEmailAddress
                    {
                        get;
                        set;
                    }

                    public string InterplayDestinationFolder
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty]
                    public string AdditionalCustomerInformation
                    {
                        get;
                        set;
                    }
                }
            }
        }

        namespace Integrations
        {
            public enum IntegrationType
            {
                [System.ComponentModel.Description("None")]
                None = 0,
                [System.ComponentModel.Description("Ceiton")]
                Ceiton = 1,
                [System.ComponentModel.Description("Plasma")]
                Plasma = 2,
                [System.ComponentModel.Description("Feenix")]
                Feenix = 3,
                [System.ComponentModel.Description("Eurovision")]
                Eurovision = 4,
                [System.ComponentModel.Description("Pebble Beach Marina")]
                PebbleBeachMarina = 5
            }

            namespace Eurovision
            {
                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class AudioChannel
                {
                    public string AudioChannelCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string AudioChannelOtherText
                    {
                        get;
                        set;
                    }
                }

                public enum Type
                {
                    [System.ComponentModel.Description("None")]
                    None,
                    [System.ComponentModel.Description("News Event")]
                    NewsEvent,
                    [System.ComponentModel.Description("Program Event")]
                    ProgramEvent,
                    [System.ComponentModel.Description("Satellite Capacity")]
                    SatelliteCapacity,
                    [System.ComponentModel.Description("Unilateral Transmission")]
                    UnilateralTransmission,
                    [System.ComponentModel.Description("OSS Transmission")]
                    OSSTransmission
                }

                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class EurovisionBookingDetails
                {
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.Type Type
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string EventId
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string MultilateralTransmissionId
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string SatelliteId
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string TransportableId
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.DateTime? Start
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.DateTime? End
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string DestinationOrganizationCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string DestinationCityCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string OriginOrganizationCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string OriginCityCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Contact
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Description
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Note
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string LineUp
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Phone
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Email
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string ContractCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string FeedpointCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string FacilityProductId
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoDefinitionCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoResolutionCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoAspectRatioCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoBitrateCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoFrameRateCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VideoBandwidthCode
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel1
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel2
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel3
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel4
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel5
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel6
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel7
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.AudioChannel AudioChannel8
                    {
                        get;
                        set;
                    }
                }
            }
        }

        namespace Locking
        {
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            public class ExternalRequestLockResponse
            {
                [Newtonsoft.Json.JsonProperty("Id", Required = Newtonsoft.Json.Required.Always)]
                public string Id
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("ObjectType", Required = Newtonsoft.Json.Required.Always)]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes ObjectType
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("ObjectId", Required = Newtonsoft.Json.Required.Always)]
                public string ObjectId
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("Username", Required = Newtonsoft.Json.Required.Always)]
                public string Username
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("ReleaseLocksAfter", Required = Newtonsoft.Json.Required.Always)]
                public System.TimeSpan ReleaseLocksAfter
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("IsLockGranted", Required = Newtonsoft.Json.Required.Always)]
                public bool IsLockGranted
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonProperty("IsLockExtended", Required = Newtonsoft.Json.Required.Always)]
                public bool IsLockExtended
                {
                    get;
                    set;
                }
            }

            public class LockManager
            {
                private readonly Skyline.DataMiner.Automation.Element orderManagerElement;
                private readonly string username;
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter;
                private readonly System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes, System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse>> locks = new System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes, System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse>>();
                /// <summary>
                /// Initializes a new LockManager.
                /// </summary>
                /// <param name = "engine">Link with DataMiner.</param>
                /// <exception cref = "ArgumentNullException">Thrown when the provided Engine is null.</exception>
                /// <exception cref = "ElementByProtocolNotFoundException">Thrown when no active Order Manager Element is found on the DMS.</exception>
                public LockManager(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    this.engine = engine ?? throw new System.ArgumentNullException(nameof(engine));
                    this.engine.SetFlag(Skyline.DataMiner.Automation.RunTimeFlags.NoKeyCaching);
                    this.progressReporter = progressReporter;
                    username = engine.UserLoginName;
                    orderManagerElement = System.Linq.Enumerable.FirstOrDefault(engine.FindElementsByProtocol(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.OrderManagerProtocol.Name)) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ElementByProtocolNotFoundException(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.OrderManagerProtocol.Name);
                    locks.Add(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes.Event, new System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse>());
                    locks.Add(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes.Order, new System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse>());
                }

                private System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse> InternalEventLocks
                {
                    get
                    {
                        return locks[Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes.Event];
                    }
                }

                private System.Collections.Generic.Dictionary<System.String, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ExternalRequestLockResponse> InternalOrderLocks
                {
                    get
                    {
                        return locks[Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.ObjectTypes.Order];
                    }
                }

                /// <summary>
                /// Returns a value indicating if all of the requested Event locks are granted or not.
                /// </summary>
                public bool AreEventLocksGranted
                {
                    get
                    {
                        return System.Linq.Enumerable.All(InternalEventLocks, x => x.Value.IsLockGranted);
                    }
                }

                /// <summary>
                /// Returns a value indicating if all of the requested Order locks are granted or not.
                /// </summary>
                public bool AreOrderLocksGranted
                {
                    get
                    {
                        return System.Linq.Enumerable.All(InternalOrderLocks, x => x.Value.IsLockGranted);
                    }
                }

                /// <summary>
                /// Returns a value indicating if all of the requested locks (regardless if they are Event or Order locks) are granted or not.
                /// </summary>
                public bool AreLocksGranted
                {
                    get
                    {
                        return AreEventLocksGranted && AreOrderLocksGranted;
                    }
                }
            }

            public enum ObjectTypes
            {
                Event = 0,
                Order = 1
            }
        }

        namespace Notes
        {
            public interface INoteManager
            {
            }

            public class NoteManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes.INoteManager
            {
                private const string TicketDomainName = "Notes";
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing.TicketingManager ticketingManager;
                public NoteManager(Skyline.DataMiner.Automation.IEngine engine)
                {
                    if (engine == null)
                        throw new System.ArgumentNullException("engine");
                    ticketingManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing.TicketingManager(engine, TicketDomainName);
                }
            }
        }

        namespace Order
        {
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            public class BillingInfo
            {
                public BillingInfo()
                {
                    BillableCompany = default(string);
                    CustomerCompany = default(string);
                }

                public string BillableCompany
                {
                    get;
                    set;
                }

                public string CustomerCompany
                {
                    get;
                    set;
                }
            }

            public enum BackupType
            {
                [System.ComponentModel.Description("None")]
                None = 0,
                [System.ComponentModel.Description("Cold Backup")]
                Cold = 1,
                [System.ComponentModel.Description("Standby Backup")]
                StandBy = 2,
                [System.ComponentModel.Description("Active Backup")]
                Active = 3
            }

            public enum OrderType
            {
                Video = 0,
                Radio = 1
            }

            public enum Status
            {
                /// <summary>
                /// Order which may or may not happen.
                /// Could be that only generic order details are filled in.
                /// Mandatory fields in service configurations are at this point not mandatory yet.
                /// </summary>
                [System.ComponentModel.Description("Preliminary")]
                Preliminary = 0,
                /// <summary>
                /// Preliminary order is completed.
                /// This means the order has at least one source and one destination.
                /// </summary>
                [System.ComponentModel.Description("Planned")]
                Planned = 1,
                /// <summary>
                /// Order which is rejected by an operator.
                /// This will be the case if the order is not possible to be produced or is wrongly configured.
                /// </summary>
                [System.ComponentModel.Description("Rejected")]
                Rejected = 2,
                /// <summary>
                /// Order which is confirmed by operator manually or is automatically generated with proper information.
                /// </summary>
                [System.ComponentModel.Description("Confirmed")]
                Confirmed = 3,
                /// <summary>
                /// Order which was previously confirmed by operator but was changed after that by customer.
                /// Order which was created automatically by integrations and set as confirmed and was changed withing 24h of the start of the order.
                /// </summary>
                [System.ComponentModel.Description("Change Requested")]
                ChangeRequested = 4,
                /// <summary>
                /// Order where at least one service is in running state.
                /// Only routing and recording services can be added at this point.
                /// </summary>
                [System.ComponentModel.Description("Running")]
                Running = 5,
                /// <summary>
                /// Order that contains running recording services and all live services are completed.
                /// </summary>
                [System.ComponentModel.Description("File Processing")]
                FileProcessing = 6,
                /// <summary>
                /// All services are in post roll or successfully completed in the order.
                /// </summary>
                [System.ComponentModel.Description("Completed")]
                Completed = 7,
                /// <summary>
                /// One or more services are completed with errors in the order.
                /// </summary>
                [System.ComponentModel.Description("Completed With Errors")]
                CompletedWithErrors = 8,
                /// <summary>
                /// Order was cancelled.
                /// </summary>
                [System.ComponentModel.Description("Cancelled")]
                Cancelled = 9,
                /// <summary>
                /// EBU Order was manually booked and a request for one or more services has been sent to EBU.
                /// When an order has this state it is saved until booked by the HandleIntegrationUpdate script with the synopsis information received from EBU.
                /// </summary>
                [System.ComponentModel.Description("Waiting for EBU")]
                WaitingOnEbu = 10,
                /// <summary>
                /// When an order has this state it is saved until the mcr operator fills in the missing details and confirms. Then the order will be booked if the timespan is within 1 week.
                /// </summary>
                [System.ComponentModel.Description("Planned Unknown Source")]
                PlannedUnknownSource = 11
            }

            public interface IOrderManager
            {
                System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order> GetAllOrders();
            }

            public class LiteOrder
            {
                public static readonly char[] OrderNameDisallowedCharacters = new[]{'<', '>', ':', '"', '/', '\\', '|', '?', '*'};
                public const string PropertyNameEventId = "EventId";
                public const string PropertyNameType = "Type";
                public const string PropertyNameIntegration = "Integration";
                public const string PropertyNameStatus = "Status";
                public const string PropertyNameUsergroups = "UserGroups";
                public const string PropertyNameInternal = "Internal";
                public const string PropertyNamePlasmaId = "PlasmaId";
                public const string PropertyNameEurovisionId = "EurovisionId";
                public const string PropertyNameEurovisionTransmissionNumber = "EurovisionTransmissionNumber";
                public const string PropertyNameOrderchanges = "OrderChanges";
                public const string PropertyNameCreatedBy = "CreatedBy";
                public const string PropertyNameLastUpdatedBy = "Last Updated By";
                public const string PropertyNameOperatorNotes = "OperatorNotes";
                public const string PropertyNameErrorDescription = "ErrorDescription";
                public const string PropertyNameReasonForCancellationOrRejection = "ReasonForCancellationOrRejection";
                public const string PropertyNameComments = "Comments";
                public const string PropertyNameShortDescription = "Short Description";
                public const string PropertyNameRecurrence = "Recurrence";
                public const string PropertyNameFromTemplate = "FromTemplate";
                public const string PropertyNameBillingInfo = "BillingInfo";
                //public const string PropertyNameStartWithoutPreroll = "StartWithoutPreroll";
                //public const string PropertyNameEndWithoutPostroll = "EndWithoutPostroll";
                public const string PropertyNameUiTeamMessiLive = "UI_Team_Messi_Live";
                public const string PropertyNameUiTeamMessiNews = "UI_Team_Messi_News";
                public const string PropertyNameUiAreena = "UI_Areena";
                public const string PropertyNameUiPlasma = "UI_Plasma";
                public const string PropertyNameUiMessiNewsRec = "UI_Messi_News_Rec";
                public const string PropertyNameUiTomTre = "UI_TRE";
                public const string PropertyNameUiTomSho = "UI_SHO";
                public const string PropertyNameUiTomNews = "UI_News";
                public const string PropertyNameUiTomSvenska = "UI_Svenska";
                public const string PropertyNameUiMcrChange = "UI_MCR_Change";
                public const string PropertyNameUiRecording = "UI_Recording";
                public const string PropertyNameUiOffice = "UI_Office";
                public const string PropertyNameUiMcrSpecialist = "UI_Fiber";
                public const string PropertyNameIsLocked = "IsLocked";
                public const string PropertyNameSportsplanningSport = "SportsPlanning_Sport";
                public const string PropertyNameSportsplanningDescr = "SportsPlanning_Description";
                public const string PropertyNameSportsplanningCommentary = "SportsPlanning_Commentary";
                public const string PropertyNameSportsplanningCommentary2 = "SportsPlanning_Commentary2";
                public const string PropertyNameSportsplanningCompetitionTime = "SportsPlanning_CompetitionTime";
                public const string PropertyNameSportsplanningJournalist1 = "SportsPlanning_JournalistOne";
                public const string PropertyNameSportsplanningJournalist2 = "SportsPlanning_JournalistTwo";
                public const string PropertyNameSportsplanningJournalist3 = "SportsPlanning_JournalistThree";
                public const string PropertyNameSportsplanningLocation = "SportsPlanning_Location";
                public const string PropertyNameSportsplanningTechResources = "SportsPlanning_TechnicalResources";
                public const string PropertyNameSportsplanningLivehighlights = "SportsPlanning_LiveHighlightsFile";
                public const string PropertyNameSportsplanningReqBroadcastTime = "SportsPlanning_RequestedBroadcastTime";
                public const string PropertyNameSportsplanningProdNrPlasmaId = "SportsPlanning_ProductionNumberPlasmaId";
                public const string PropertyNameSportsplanningProdNrCeiton = "SportsPlanning_ProductNumberCeiton";
                public const string PropertyNameSportsplanningCostDep = "SportsPlanning_CostDepartment";
                public const string PropertyNameSportsplanningAdditionalInformation = "SportsPlanning_AdditionalInformation";
                public const string PropertyNameNewsInformationNewsCameraOperator = "NewsInformation_NewsCameraOperator";
                public const string PropertyNameNewsInformationJournalist = "NewsInformation_Journalist";
                public const string PropertyNameNewsInformationVirveCommandGroupOne = "NewsInformation_VirveCommandGroupOne";
                public const string PropertyNameNewsInformationVirveCommandGroupTwo = "NewsInformation_VirveCommandGroupTwo";
                public const string PropertyNameNewsInformationAdditionalInformation = "NewsInformation_AdditionalInformation";
                public const string PropertyNameAudioReturnInfo = "AudioReturnInfo";
                public const string PropertyNameLiveUDeviceNames = "LiveUDeviceNames";
                public const string PropertyNameVidigoStreamSourceLinks = "VidigoStreamSourceLinks";
                public const string PropertyNamePlasmaIdsForArchiving = "PlasmaIdsForArchiving";
                public const string PropertyNameServiceConfigurations = "ServiceConfigurations";
                public const string PropertyNameStartnow = "StartNow";
                public const string PropertyNameFixedSourcePlasma = "Fixed_Source_Plasma";
                public const string PropertyNameYleId = "YleId";
                public const string PropertyNameSources = "Sources";
                public const string PropertyNameDestinations = "Destinations";
                public const string PropertyNameRecordings = "Recordings";
                public const string PropertyNameTransmissions = "Transmissions";
                private System.DateTime start;
                private System.DateTime end;
                /// <summary>
                /// Reservation object for the order. Saved as property for performance reasons.
                /// </summary>
                public Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance Reservation
                {
                    get;
                    set;
                }

                /// <summary>
                /// Reservation instance ID of the order.
                /// </summary>
                public System.Guid Id
                {
                    get;
                    set;
                }

                /// <summary>
                /// The event this order belongs to.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event Event
                {
                    get;
                    set;
                }

                /// <summary>
                /// Name of the order.
                /// </summary>
                public string Name
                {
                    get;
                    set;
                }

                /// <summary>
                /// Used by UI.
                /// </summary>
                public string ShortDescription => Name;
                /// <summary>
                /// Start date and time of the order.
                /// </summary>
                public System.DateTime Start
                {
                    get => start;
                    set
                    {
                        if (start != value)
                        {
                            start = value;
                            StartChanged?.Invoke(this, start);
                        }
                    }
                }

                internal event System.EventHandler<System.DateTime> StartChanged;
                /// <summary>
                /// End date and time of the order.
                /// </summary>
                public System.DateTime End
                {
                    get => end;
                    set
                    {
                        if (end != value)
                        {
                            end = value;
                            EndChanged?.Invoke(this, end);
                        }
                    }
                }

                internal event System.EventHandler<System.DateTime> EndChanged;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurringSequenceInfo RecurringSequenceInfo
                {
                    get;
                    set;
                }

                /// <summary>
                /// The status of this order.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status Status
                {
                    get;
                    set;
                }

                /// <summary>
                /// Comments for the order.
                /// </summary>
                public string Comments
                {
                    get;
                    set;
                }

                /// <summary>
                /// The type of this order.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderType Type
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates which integration created this Order.
                /// If not specified this will be set to None.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType IntegrationType
                {
                    get;
                    set;
                }

                /// <summary>
                /// Id of the Program provided by the Plasma/MediaGenix integration.
                /// This will only applicable when the IntegrationType is Plasma.
                /// </summary>
                public string PlasmaId
                {
                    get;
                    set;
                }

                /// <summary>
                /// Id of the Plasma Program or Feenix Order in YLE.
                /// </summary>
                public string YleId
                {
                    get;
                    set;
                }

                /// <summary>
                /// A list containing IDs of the User Groups that are allowed to see/edit this order.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Int32> UserGroupIds
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.SportsPlanning SportsPlanning
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.NewsInformation NewsInformation
                {
                    get;
                    set;
                }

                /// <summary>
                /// The name of the contract that should be used to create the event.
                /// </summary>
                public string Contract
                {
                    get;
                    set;
                }

                /// <summary>
                /// The name of the company that should be used to create the event.
                /// </summary>
                public string Company
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets a value indicating whether an order is part of an internal event or not.
                /// </summary>
                public bool IsInternal
                {
                    get;
                    set;
                }

                /// <summary>
                /// Login name of the user that created the Order.
                /// </summary>
                public string CreatedByUserName
                {
                    get;
                    set;
                }

                /// <summary>
                /// Login name of the user that last modified the Order.
                /// </summary>
                public string LastUpdatedBy
                {
                    get;
                    set;
                }

                /// <summary>
                /// Notes that can only be seen and updated by an MCR user from the CustomerUI.
                /// </summary>
                public string OperatorNotes
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the description that should be filled out by an MCR user when the Order was completed with errors.
                /// If the order was completed with errors and this value is not filled out, then the order should be visible in the UI_MCR_Change and UI_Recording views.
                /// </summary>
                public string ErrorDescription
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets a property containing the reason why a user has rejected or canceled an Order.
                /// </summary>
                public string ReasonForCancellationOrRejection
                {
                    get;
                    set;
                }

                /// <summary>
                /// A list of the changes that were made to the order.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderChange> OrderChanges
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a collection of Cube View IDs of views where this order is visible.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Int32> SecurityViewIds
                {
                    get;
                    set;
                }

                = new System.Collections.Generic.HashSet<System.Int32>();
                /// <summary>
                /// Indicates whether the Order should start as soon as possible or not.
                /// </summary>
                public bool StartNow
                {
                    get;
                    set;
                }

                = false;
                /// <summary>
                /// Indicates whether the Order should stop immediately.
                /// </summary>
                public bool StopNow
                {
                    get;
                    set;
                }

                = false;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BillingInfo BillingInfo
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this order can be cancelled.
                /// </summary>
                public bool CanCancel
                {
                    get
                    {
                        return Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Preliminary || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.WaitingOnEbu || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Planned || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Rejected || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Confirmed || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.ChangeRequested || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.FileProcessing || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Running;
                    }
                }

                /// <summary>
                /// Indicates if this order can be rejected.
                /// </summary>
                public bool CanReject
                {
                    get => Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Planned;
                }

                /// <summary>
                /// Indicates if this order can be confirmed.
                /// </summary>
                public bool CanConfirm
                {
                    get
                    {
                        return Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Planned || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.ChangeRequested;
                    }
                }

                /// <summary>
                /// Indicates if this order can be deleted.
                /// </summary>
                public bool CanDelete
                {
                    get
                    {
                        return Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Rejected || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Cancelled || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Completed || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.CompletedWithErrors;
                    }
                }

                public bool IsSaved => (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Preliminary || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.WaitingOnEbu || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.PlannedUnknownSource);
                /// <summary>
                /// Indication if the order is generated from a template.
                /// </summary>
                public bool IsCreatedFromTemplate
                {
                    get;
                    set;
                }

                public string ExternalJsonFilePath
                {
                    get;
                    set;
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            public class NewsInformation : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private string initialNewsCameraOperator;
                private string initialJournalist;
                private string initialVirveCommandGroupOne;
                private string initialVirveCommandGroupTwo;
                private string initialAdditionalInformation;
                public string NewsCameraOperator
                {
                    get;
                    set;
                }

                public string Journalist
                {
                    get;
                    set;
                }

                public string VirveCommandGroupOne
                {
                    get;
                    set;
                }

                public string VirveCommandGroupTwo
                {
                    get;
                    set;
                }

                public string AdditionalInformation
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a boolean indicating if Change Tracking has been enabled for this object.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public bool IsChanged => ChangeInfo.IsChanged;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                /// <summary>
                /// Gets the type of changes that have happened since object creation or since last <see cref = "IChangeTracking.AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes.</returns>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    if (initialNewsCameraOperator != NewsCameraOperator)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialJournalist != Journalist)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialVirveCommandGroupOne != VirveCommandGroupOne)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialVirveCommandGroupTwo != VirveCommandGroupTwo)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialAdditionalInformation != AdditionalInformation)
                        changeInfo.MarkCustomPropertiesChanged();
                    return changeInfo;
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialNewsCameraOperator = NewsCameraOperator;
                    initialJournalist = Journalist;
                    initialVirveCommandGroupOne = VirveCommandGroupOne;
                    initialVirveCommandGroupTwo = VirveCommandGroupTwo;
                    initialAdditionalInformation = AdditionalInformation;
                }

                public override string ToString()
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is NewsInformation other))
                        return false;
                    bool equal = true;
                    foreach (var property in typeof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.NewsInformation).GetProperties())
                    {
                        equal &= property.GetValue(this).Equals(property.GetValue(other));
                    }

                    return equal;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = hash * 23 + NewsCameraOperator.GetHashCode();
                        hash = hash * 23 + Journalist.GetHashCode();
                        hash = hash * 23 + VirveCommandGroupOne.GetHashCode();
                        hash = hash * 23 + VirveCommandGroupTwo.GetHashCode();
                        hash = hash * 23 + AdditionalInformation.GetHashCode();
                        return hash;
                    }
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            public class Order : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.LiteOrder
            {
                public const int StartNowDelayInMinutes = 30;
                public const int StartInTheFutureDelayInMinutes = 10;
                public const string ServiceBookingEventName = "Service Booking";
                public const string HandleOrderActionScriptName = "HandleOrderAction";
                public const string HandleOrderActionScriptReservationGuidParameterName = "ReservationGuid";
                public const string HandleOrderActionScriptActionParameterName = "Action";
                public const string HandleOrderActionScriptBookingManagerInfoParameterName = "Booking Manager Info";
                /// <summary>
                /// Initializes a new instance of the <see cref = "Order"/> class.
                /// </summary>
                public Order()
                {
                }

                /// <summary>
                /// Initializes a new instance of the <see cref = "Order"/> class based on the existing Order.
                /// The new Order will have new GUIDs assigned to its ID parameter and the IDs of the Services.
                /// </summary>
                /// <param name = "order">Order to copy.</param>
                public Order(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order)
                {
                    System.Reflection.PropertyInfo[] propertyInfo = typeof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order).GetProperties();
                    foreach (System.Reflection.PropertyInfo property in propertyInfo)
                    {
                        if (property.CanWrite)
                            property.SetValue(this, property.GetValue(order));
                    }

                    Id = System.Guid.Empty;
                    foreach (var service in Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManager.FlattenServices(Sources))
                    {
                        service.Id = System.Guid.NewGuid();
                        service.IsBooked = false;
                    }
                }

                /// <summary>
                /// Preroll of the order. Equal to the duration between the start of the first preroll among its services and the first start among its services. 
                /// </summary>
                public System.TimeSpan PreRoll
                {
                    get
                    {
                        if (Sources == null || !System.Linq.Enumerable.Any(Sources) || System.Linq.Enumerable.All(AllServices, s => s.IsEventLevelReception || s.IsGlobalEventLevelReception))
                            return System.TimeSpan.Zero;
                        var earliestServicePreRollStart = System.Linq.Enumerable.Min(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, s => !s.IsEventLevelReception && !s.IsGlobalEventLevelReception), s => s.StartWithPreRoll));
                        var earliestServiceStart = System.Linq.Enumerable.Min(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, s => !s.IsEventLevelReception && !s.IsGlobalEventLevelReception), s => s.Start));
                        bool earliestServicePreRollStartIsBeforeEarliestServiceStart = earliestServicePreRollStart < earliestServiceStart;
                        return earliestServicePreRollStartIsBeforeEarliestServiceStart ? earliestServiceStart - earliestServicePreRollStart : System.TimeSpan.Zero;
                    }
                }

                public System.DateTime StartWithPreRoll => Start - PreRoll;
                /// <summary>
                /// Postroll of the order. Equal to the duration between the start of the latest postroll among its services and the latest end among its services. 
                /// </summary>
                public System.TimeSpan PostRoll
                {
                    get
                    {
                        if (Sources == null || !System.Linq.Enumerable.Any(Sources) || System.Linq.Enumerable.All(AllServices, s => s.IsEventLevelReception || s.IsGlobalEventLevelReception))
                            return System.TimeSpan.Zero;
                        var latestServicePostRollEnd = System.Linq.Enumerable.Max(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, s => !s.IsEventLevelReception && !s.IsGlobalEventLevelReception), s => s.EndWithPostRoll));
                        var latestServiceEnd = System.Linq.Enumerable.Max(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, s => !s.IsEventLevelReception && !s.IsGlobalEventLevelReception), s => s.End));
                        bool latestServicePostRollEndIsLaterThanLatestServiceEnd = latestServiceEnd < latestServicePostRollEnd;
                        return latestServicePostRollEndIsLaterThanLatestServiceEnd ? latestServicePostRollEnd - latestServiceEnd : System.TimeSpan.Zero;
                    }
                }

                public System.DateTime EndWithPostRoll => End + PostRoll;
                /// <summary>
                /// The list of sources in this order.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> Sources
                {
                    get;
                    set;
                }

                /// <summary>
                /// The service definition used by the order.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition Definition
                {
                    get;
                    set;
                }

                /// <summary>
                /// Work Order Id of the eurovision service that was requested through the LiveOrderForm
                /// </summary>
                public string EurovisionWorkOrderId
                {
                    get
                    {
                        foreach (var service in AllServices)
                        {
                            if (!string.IsNullOrWhiteSpace(service.EurovisionWorkOrderId))
                                return service.EurovisionWorkOrderId;
                        }

                        return string.Empty;
                    }
                }

                /// <summary>
                /// Transmission Number of the Eurovision Synopsis.
                /// Used for uniquely identifying Eurovision Orders.
                /// Only applicable for Orders that use Eurovision Services or Orders generated by the Eurovision Integration.
                /// </summary>
                public string EurovisionTransmissionNumber
                {
                    get
                    {
                        foreach (var service in AllServices)
                        {
                            if (!string.IsNullOrWhiteSpace(service.EurovisionTransmissionNumber))
                                return service.EurovisionTransmissionNumber;
                        }

                        return string.Empty;
                    }
                }

                /// <summary>
                /// Gets a IEnumerable containing all Services in the Order.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> AllServices => FlattenServices(Sources);
                /// <summary>
                /// Gets a boolean indicating if the order contains cueing (preroll) services.
                /// </summary>
                public bool HasCueingServices
                {
                    get => System.Linq.Enumerable.Any(AllServices, x => x.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCueing);
                }

                public bool HasPostRollServices
                {
                    get => System.Linq.Enumerable.Any(AllServices, x => x.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.PostRoll);
                }

                /// <summary>
                /// Gets a boolean indicating if the order contains a fixed plasma source. Used by UI to filter.
                /// </summary>
                public bool HasFixedPlasmaSource
                {
                    get
                    {
                        var sourceService = System.Linq.Enumerable.FirstOrDefault(Sources, s => s.BackupType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType.None);
                        if (sourceService == null)
                            return false;
                        bool sourceIsFixedLineLy = sourceService.Definition.Name == "_Fixed Line RX LY";
                        bool sourceIsMadeFromPlasmaIntegration = sourceService.IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma;
                        return sourceIsMadeFromPlasmaIntegration && sourceIsFixedLineLy;
                    }
                }

                /// <summary>
                /// Booking can only be edited when Order is not running and has no cueing or post roll services.
                /// </summary>
                public bool CanEditBooking
                {
                    get => !HasCueingServices && !HasPostRollServices && Status != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Running;
                }

                public bool UI_Team_Messi_Live
                {
                    get
                    {
                        return System.Linq.Enumerable.Any(AllServices, s => s != null && s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && (s.Definition.Description == "Messi Live" || s.Definition.Description == "Messi Live Backup"));
                    }
                }

                public bool UI_Team_Messi_News
                {
                    get
                    {
                        return System.Linq.Enumerable.Any(AllServices, s => s != null && s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && s.Definition.Description == "Messi News");
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on Areena Monitoring page.
                /// </summary>
                public bool UI_Areena
                {
                    get
                    {
                        return System.Linq.Enumerable.Any(AllServices, s => s.Definition.Description == "Areena");
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on TOM News page.
                /// </summary>
                public bool UI_News
                {
                    get
                    {
                        if (AllServices == null)
                            return false;
                        foreach (var service in AllServices)
                        {
                            if (service.Definition != null && service.Definition.Description?.Contains("News") == true)
                                return true;
                            foreach (var function in System.Linq.Enumerable.Where(service.Functions, f => f != null))
                            {
                                var sourceOrDestinationLocation = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p != null && (p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.YleHelsinkiDestinationLocation));
                                if (sourceOrDestinationLocation != null && (sourceOrDestinationLocation.StringValue == "Uutisalue" || sourceOrDestinationLocation.StringValue == "Uutisstudiot" || sourceOrDestinationLocation.StringValue == "Uutisstudio ST 27/28"))
                                    return true;
                                if (HasFunctionMatchingPlasmaSource(function, new string[]{"ST-24", "ST-25", "ST-26", "ST-27", "NOPSA", "SU28O"}))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on TOM Studio Helsinki page.
                /// </summary>
                public bool UI_Sho
                {
                    get
                    {
                        if (AllServices == null)
                            return false;
                        foreach (var service in AllServices)
                        {
                            foreach (var function in System.Linq.Enumerable.Where(service.Functions, f => f != null))
                            {
                                var sourceOrDestinationLocation = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p != null && (p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.YleHelsinkiDestinationLocation));
                                if (sourceOrDestinationLocation != null && (sourceOrDestinationLocation.StringValue == "Studio Helsinki" || sourceOrDestinationLocation.StringValue == "Studio Helsinki UT"))
                                    return true;
                                if (HasFunctionMatchingPlasmaSource(function, new string[]{"SHO1", "SHO2", "SHO3", "SHO4", "SHO5"}))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on TOM Mediapolis page.
                /// </summary>
                public bool UI_Tre
                {
                    get
                    {
                        if (System.Linq.Enumerable.Any(AllServices, s => s != null && (s.Definition.Description == "YLE Mediapolis" || s.Definition.Description == "LiveU Mediapolis")))
                            return true;
                        var orderServices = AllServices;
                        if (orderServices == null)
                            return false;
                        foreach (var service in orderServices)
                        {
                            foreach (var function in System.Linq.Enumerable.Where(service.Functions, f => f != null))
                            {
                                if (HasFunctionMatchingPlasmaSource(function, new string[]{"TRE 01"}))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on TOM SVENSKA page.
                /// </summary>
                public bool UI_Svenska
                {
                    get
                    {
                        var orderServices = AllServices;
                        if (orderServices == null)
                            return false;
                        foreach (var service in orderServices)
                        {
                            foreach (var function in System.Linq.Enumerable.Where(service.Functions, f => f != null))
                            {
                                var sourceOrDestinationLocation = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p != null && (p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.YleHelsinkiDestinationLocation));
                                if (sourceOrDestinationLocation != null && (sourceOrDestinationLocation.StringValue == "Shohub"))
                                {
                                    return true;
                                }

                                if (HasFunctionMatchingPlasmaSource(function, new string[]{"SHOHUB"}))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                /// <summary>
                /// The order will be shown on the TOM News page if the integration type is Plasma.
                /// </summary>
                public bool UI_Plasma
                {
                    get
                    {
                        return IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma;
                    }
                }

                /// <summary>
                /// The order will be shown on the TOM News page if it contains one or more recording services.
                /// </summary>
                public bool UI_Messi_News_Rec
                {
                    get
                    {
                        return System.Linq.Enumerable.Any(AllServices, s => s?.Definition != null && !string.IsNullOrEmpty(s.Definition.Description) && s.Definition.Description.Contains("News"));
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on MCR Operator Task List page.
                /// </summary>
                public bool UI_McrChange
                {
                    get
                    {
                        bool orderIsStartedWithIncompletedUsertasks = System.DateTime.Now >= StartWithPreRoll && System.Linq.Enumerable.Any(AllServices, s => s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete));
                        bool containsOrderNonBookedServices = System.Linq.Enumerable.Any(AllServices, s => s != null && !s.IsBooked) && IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma;
                        bool orderPlannedMoreThanOneWeek = System.Linq.Enumerable.Any(AllServices, s => s != null && s.StartWithPreRoll.ToLocalTime().Subtract(System.DateTime.Now) > new System.TimeSpan(days: 7, hours: 0, minutes: 1, seconds: 0));
                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.PlannedUnknownSource)
                        {
                            return true;
                        }

                        if (IsSaved || (containsOrderNonBookedServices || orderPlannedMoreThanOneWeek) && (IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma || IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Feenix))
                        {
                            return false; //When non plasma orders are saved or where plasma/feenix orders contain one or more services where the start time with preroll is larger than 7 days, with other words services that aren't booked yet.
                        }

                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.CompletedWithErrors && !System.String.IsNullOrWhiteSpace(OperatorNotes))
                        {
                            return false;
                        }

                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.FileProcessing && EndWithPostRoll <= System.DateTime.Now && System.Linq.Enumerable.Any(AllServices, s => s != null && System.Linq.Enumerable.Any(s.UserTasks, u => u != null && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete && u.Description.IndexOf("File Processing", System.StringComparison.OrdinalIgnoreCase) != -1)))
                        {
                            return false;
                        }

                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Planned || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.ChangeRequested)
                        {
                            return true; // All orders where ORDER status = Planned (Use case: Operator confirms/rejects an order)
                        }

                        if (System.Linq.Enumerable.Any(AllServices, s => s != null && s.Definition?.VirtualPlatform != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && (s.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ResourceOverbooked || s.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCompletedWithErrors && System.String.IsNullOrWhiteSpace(ErrorDescription))))
                        {
                            // This caused issues when booking new orders
                            // Services were having status ResourceOverbooked
                            return true; // All orders where one or more SERVICES are having status Resources Overbooked (excluding Recordings) OR having status “Service completed with errors” and error description field (txt) is empty
                        }

                        if (orderIsStartedWithIncompletedUsertasks)
                        {
                            return true; // All orders where one or more USER TASK are INCOMPLETE when pre-roll has begun
                        }

                        if (IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma && System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite && s.Start - System.DateTime.Now < new System.TimeSpan(hours: 72, minutes: 1, seconds: 0) && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.SatelliteReception.SpaceNeeded) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                        {
                            return true; // All orders which include RX SERVICE (Satellite RX or Backup Satellite RX) related USER TASK: Satellite space needed	
                        }

                        if (IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma && System.Linq.Enumerable.Any(AllServices, s => (s.IsEbuDummyReception || s.IsEbuDummyTransmission) && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.Dummy.SelectTechnicalSystem))))
                        {
                            return true; // All orders which include DUMMY EBU SERVICE with USER TASK: Select Technical System from EBU Synopsis
                        }

                        if (IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma && System.Linq.Enumerable.Any(AllServices, s => System.Linq.Enumerable.Any(s.Functions, f => System.Linq.Enumerable.Any(f.Parameters, p => (p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FixedLineEbuSourceLocation || p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.EbuDestinationLocation) && p.StringValue == "Nordic Ring"))) && Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Planned)
                        {
                            return true; // All orders where source or destination is Fixed Line Nordic Ring and status is Planned  (Included in rule 1)
                        }

                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on Booking Office Task List page.
                /// </summary>
                public bool UI_Office
                {
                    get
                    {
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatformServiceName == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName.Satellite && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.SatelliteReception.SpaceNeeded) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include RX Service (Sat RX or Backup Sat RX) related user task: Satellite space needed
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFiber && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.FiberReception.AllocationNeeded) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include RX service (ad hoc fiber RX or backup Ad Hoc Fiber RX) related user task: fiber allocation needed
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionMicrowave && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.MicrowaveReception.EquipmentAllocation) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include RX Service (MW link RX or backup MW RX) related user task: MW equipment allocation needed
                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on MCR Specialist Task List page.
                /// </summary>
                public bool UI_McrSpecialist
                {
                    get
                    {
                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Completed || Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.CompletedWithErrors)
                            return false;
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatformServiceName == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName.Fiber && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => (u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.FiberReception.AllocationNeeded) || u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.FiberReception.EquipmentAllocation) || u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.FiberTransmission.Configure)) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include (backup) Ad Hoc Fiber RX related user tasks
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatformServiceName == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName.Microwave && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => (u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.MicrowaveReception.EquipmentAllocation) || u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.MicrowaveReception.EquipmentConfiguration) || u.Name.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.Descriptions.MicrowaveTransmission.Configure)) && u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include (backup) Microwave RX related user tasks
                        if (System.Linq.Enumerable.Any(AllServices, s => s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionIp && s.UserTasks != null && System.Linq.Enumerable.Any(s.UserTasks, u => u.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)))
                            return true; // All orders which include RX SERVICE (IP Transmission) 
                        return false;
                    }
                }

                /// <summary>
                /// Gets a boolean indicating if this order is visible on Media Operator Task List page.
                /// </summary>
                public bool UI_Recording
                {
                    get
                    {
                        bool containsOrderNonBookedServices = System.Linq.Enumerable.Any(AllServices, s => s != null && !s.IsBooked) && IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma;
                        bool orderPlannedMoreThanOneWeek = System.Linq.Enumerable.Any(AllServices, s => s != null && s.StartWithPreRoll.ToLocalTime().Subtract(System.DateTime.Now) > new System.TimeSpan(days: 7, hours: 0, minutes: 1, seconds: 0));
                        if (IsSaved || (containsOrderNonBookedServices || orderPlannedMoreThanOneWeek) && (IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma || IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Feenix))
                        {
                            return false; //When non plasma orders are saved or where plasma/feenix orders contain one or more services where the start time with preroll is larger than 7 days, with other words services that aren't booked yet.
                        }

                        if (Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.CompletedWithErrors && !System.String.IsNullOrWhiteSpace(OperatorNotes))
                        {
                            return false;
                        }

                        var recordingServiceCount = 0;
                        bool recordingServiceHasSubtitleProxy = false;
                        var orderServices = AllServices;
                        foreach (var service in orderServices)
                        {
                            bool isRecording = service.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording;
                            if (!isRecording)
                                continue;
                            recordingServiceCount++;
                            var status = service.Status;
                            // return true when there is a recording service with service completed with errors status and an empty error description
                            if (status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCompletedWithErrors && System.String.IsNullOrWhiteSpace(OperatorNotes))
                            {
                                return true;
                            }

                            // return true when there is a recording service with resource overbooked status
                            if (status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ResourceOverbooked)
                            {
                                return true;
                            }

                            // return false when recording service is part of a Plasma Live News order
                            if (IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma && service != null && service.RecordingConfiguration?.IsPlasmaLiveNews == true)
                            {
                                return false;
                            }

                            // return true when there is a recording service with a non-real time code
                            if (service.RecordingConfiguration?.RecordingFileTimeCodec == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.TimeCodec.NonReal && IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma)
                            {
                                return true;
                            }

                            // return true when there is a recording service with sub recordings
                            if (service.RecordingConfiguration?.SubRecordings != null && System.Linq.Enumerable.Any(service.RecordingConfiguration.SubRecordings) && IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma)
                            {
                                return true;
                            }

                            if (recordingServiceCount == 2 && IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.None && Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status.Completed)
                            {
                                return false;
                            }

                            // Return true if a plasma order has more than 3 recordings (more means that extra recordings are added manually), default plasma orders has 2-3 recording services
                            if (recordingServiceCount > 1 && IntegrationType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma || recordingServiceCount > 3 && IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma)
                            {
                                return true;
                            }

                            // check if the recording service is a day change and requires subtitle proxy
                            if (service.RecordingConfiguration?.SubtitleProxy == true && service.Start.Date != service.End.Date)
                                recordingServiceHasSubtitleProxy = true;
                            // return true in case of a Plasma order with a day change and more than 1 recording
                            if (recordingServiceCount >= 1 && recordingServiceHasSubtitleProxy)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }

                /// <summary>
                /// Returns a list of all resources used for this order.
                /// </summary>
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource> AllServiceResources => GetAllServiceResources();
                /// <summary>
                /// Retrieving the audio return information from the source services.
                /// Only news users can fill in this field for every type of service.
                /// Other users will only see this field when LiveU source service is used.
                /// </summary>
                public string AudioReturnInfo
                {
                    get
                    {
                        if (AllServices == null)
                            return System.String.Empty;
                        System.Collections.Generic.List<System.String> audioReturnInfos = new System.Collections.Generic.List<System.String>();
                        foreach (var sourceService in Sources)
                        {
                            if (sourceService == null)
                                continue;
                            if (!System.String.IsNullOrWhiteSpace(sourceService.AudioReturnInfo))
                            {
                                audioReturnInfos.Add(sourceService.AudioReturnInfo);
                            }
                        }

                        return string.Join(";", audioReturnInfos);
                    }
                }

                /// <summary>
                /// Additional information for reception/transmission LiveU device name.
                /// Only applicable for LiveU receptions/transmissions.
                /// Will be saved in a custom property
                /// </summary>
                public string LiveUDeviceNames
                {
                    get
                    {
                        var orderServices = AllServices;
                        if (orderServices == null)
                            return System.String.Empty;
                        System.Collections.Generic.List<System.String> liveUDeviceNamesToAdd = new System.Collections.Generic.List<System.String>();
                        foreach (var service in orderServices)
                        {
                            if (service == null)
                                continue;
                            bool isServiceLiveUReceptionOrTransmission = service.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionLiveU || service.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionLiveU;
                            if (isServiceLiveUReceptionOrTransmission && !System.String.IsNullOrEmpty(service.LiveUDeviceName))
                            {
                                liveUDeviceNamesToAdd.Add(service.LiveUDeviceName);
                            }
                        }

                        return System.String.Join(";", liveUDeviceNamesToAdd);
                    }
                }

                /// /// <summary>
                /// Returns a dot comma separated list containing the Vidigo stream source links if the order contains Messi News Recordings.
                /// </summary>
                public string VidigoStreamSourceLinks
                {
                    get
                    {
                        var vidigoStreamSourceLinks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, s => !string.IsNullOrWhiteSpace(s.VidigoStreamSourceLink)), s => s.VidigoStreamSourceLink));
                        return string.Join(";", vidigoStreamSourceLinks);
                    }
                }

                /// <summary>
                /// Returns a dot comma separated list containing all existing plasma ids for archiving from all live recording services within this order.
                /// </summary>
                public string PlasmaIdsForArchiving
                {
                    get
                    {
                        var orderServices = AllServices;
                        if (orderServices == null)
                            return System.String.Empty;
                        System.Collections.Generic.List<System.String> plasmaIdsForArchiveToAdd = new System.Collections.Generic.List<System.String>();
                        foreach (var service in orderServices)
                        {
                            if (service == null)
                                continue;
                            bool isServiceLiveRecording = service.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && service.Definition.Description != null && service.Definition.Description.Contains("Live");
                            if (isServiceLiveRecording && service.RecordingConfiguration != null && !string.IsNullOrWhiteSpace(service.RecordingConfiguration.PlasmaIdForArchive))
                            {
                                plasmaIdsForArchiveToAdd.Add(service.RecordingConfiguration.PlasmaIdForArchive);
                            }
                        }

                        return System.String.Join(";", plasmaIdsForArchiveToAdd);
                    }
                }

                /// <summary>
                /// Returns a dot comma separated list containing the Short Descriptions of the Source Services.
                /// </summary>
                public string SourceDescriptions
                {
                    get
                    {
                        if (Sources == null || !System.Linq.Enumerable.Any(Sources))
                            return System.String.Empty;
                        return System.String.Join(";", System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(Sources, x => x.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Reception), x => x.GetShortDescription(this)));
                    }
                }

                /// <summary>
                /// Returns a dot comma separated list containing the Short Descriptions of the Destination Services.
                /// </summary>
                public string DestinationDescriptions
                {
                    get
                    {
                        System.Collections.Generic.List<System.String> resultDescriptions = new System.Collections.Generic.List<System.String>();
                        string umxLineResourceName = GetUmxLineResourceNameForMessiNewsRecordings();
                        var destinationShortDescriptions = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, x => x.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Destination), x => x.GetShortDescription(this)));
                        if (!System.Linq.Enumerable.Any(destinationShortDescriptions, x => x.Replace(" ", string.Empty).Equals(umxLineResourceName, System.StringComparison.InvariantCultureIgnoreCase)))
                        {
                            resultDescriptions.Add(umxLineResourceName);
                        }

                        resultDescriptions.AddRange(destinationShortDescriptions);
                        return System.String.Join(";", System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Where(resultDescriptions, x => !string.IsNullOrWhiteSpace(x))));
                    }
                }

                /// <summary>
                /// Returns a dot comma separated list containing the Short Descriptions of the Recording Services.
                /// </summary>
                public string RecordingDescriptions
                {
                    get
                    {
                        return System.String.Join(";", System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, x => x.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Recording), x => x.GetShortDescription(this)));
                    }
                }

                /// <summary>
                /// Returns a dot comma separated list containing the Short Descriptions of the Transmission Services.
                /// </summary>
                public string TransmissionDescriptions
                {
                    get
                    {
                        return System.String.Join(";", System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllServices, x => x.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Transmission), x => x.GetShortDescription(this)));
                    }
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource> GetAllServiceResources(System.Guid? serviceToIgnoreId = null, string functionToIgnoreLabel = null)
                {
                    return System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(System.Linq.Enumerable.SelectMany(System.Linq.Enumerable.Where(AllServices, s => s.Id != serviceToIgnoreId), s => s.Functions), f => f.Definition.Label != functionToIgnoreLabel && f.Resource != null), f => f.Resource));
                }

                public static System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> FlattenServices(System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> services)
                {
                    var flattenedServices = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service>();
                    foreach (var service in services)
                    {
                        flattenedServices.Add(service);
                        flattenedServices.AddRange(FlattenServices(service.Children));
                    }

                    return flattenedServices;
                }

                private string GetUmxLineResourceNameForMessiNewsRecordings()
                {
                    var result = new System.Text.StringBuilder();
                    var allOrderServices = AllServices;
                    var newsRecordingService = System.Linq.Enumerable.FirstOrDefault(allOrderServices, s => s != null && s.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && s.Definition.Description.Contains("News"));
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service routingService = null;
                    if (newsRecordingService != null)
                    {
                        routingService = System.Linq.Enumerable.Single(allOrderServices, s => s != null && s.Children?.Contains(newsRecordingService) == true);
                        if (routingService != null && routingService.Functions != null && System.Linq.Enumerable.Any(routingService.Functions))
                        {
                            var matrixOutputFunction = System.Linq.Enumerable.Single(routingService.Functions, f => f != null && routingService.Definition?.FunctionIsFirst(f) == true);
                            var matrixOutputResource = matrixOutputFunction.Resource;
                            string matrixOutputResourceName = matrixOutputResource != null ? matrixOutputResource.Name : "None";
                            string matrixOutputResourceDisplayName = System.Linq.Enumerable.Last(matrixOutputResourceName.Split('.'));
                            if (!string.IsNullOrEmpty(matrixOutputResourceDisplayName))
                            {
                                result.Append(matrixOutputResourceDisplayName.Contains("UMX") ? matrixOutputResourceDisplayName : string.Empty);
                            }
                        }
                    }

                    return result.ToString();
                }

                /// <summary>
                /// Checks if a function uses a matching plasma source 
                /// </summary>
                /// <param name = "function">Function that contains the plasma user code profile parameter</param>
                /// <param name = "regularExpression">Regular expression to search matching plasma source</param>
                /// <returns></returns>
                private bool HasFunctionMatchingPlasmaSource(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function, string[] applicablePlasmaUserCodes)
                {
                    var sourcePlasmaUserCode = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p != null && p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FixedLineLySourcePlasmaUserCode);
                    if (sourcePlasmaUserCode == null)
                        return false;
                    if (System.Linq.Enumerable.Contains(applicablePlasmaUserCodes, sourcePlasmaUserCode.StringValue.ToUpper()))
                        return true;
                    return false;
                }

                public override string ToString()
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine(System.String.Format("Name: {0}", Name));
                    sb.AppendLine(System.String.Format("\tStatus: {0}", Status));
                    sb.AppendLine(System.String.Format("\tStart: {0}", Start));
                    sb.AppendLine(System.String.Format("\tEnd: {0}", End));
                    sb.AppendLine(System.String.Format("\tStart With Pre Roll: {0}", StartWithPreRoll));
                    sb.AppendLine(System.String.Format("\tEnd With Post Roll: {0}", EndWithPostRoll));
                    sb.AppendLine(System.String.Format("\tService Definition: {0}", Definition.Id));
                    sb.AppendLine(System.String.Format("\tIntegration Type: {0}", IntegrationType));
                    sb.AppendLine(System.String.Format("\tPlasma ID: {0}", PlasmaId));
                    sb.AppendLine(System.String.Format("\tEurovision ID: {0}", EurovisionWorkOrderId));
                    sb.AppendLine(System.String.Format("\tTransmission Number: {0}", EurovisionTransmissionNumber));
                    sb.AppendLine(System.String.Format("\tContract: {0}", Contract));
                    sb.AppendLine(System.String.Format("\tCompany: {0}", Company));
                    sb.AppendLine("\tServices:");
                    foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service in AllServices)
                    {
                        sb.AppendLine("\t" + service.ToString());
                    }

                    return sb.ToString();
                }

                /// <summary>
                /// Checks if the Order matches the (basic) requirements to be booked.
                /// </summary>
                public bool CanBeBooked
                {
                    get
                    {
                        bool hasReceptionService = false;
                        bool hasRecordingService = false;
                        bool hasTransmissionService = false;
                        bool hasDestinationService = false;
                        foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service in FlattenServices(System.Linq.Enumerable.Where(Sources, x => x.BackupType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType.None)))
                        {
                            if (service?.Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Reception)
                                hasReceptionService = true;
                            if (service?.Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Recording)
                                hasRecordingService = true;
                            if (service?.Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Transmission)
                                hasTransmissionService = true;
                            if (service?.Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Destination)
                                hasDestinationService = true;
                        }

                        return hasReceptionService && (hasRecordingService || hasTransmissionService || hasDestinationService);
                    }
                }

                /// <summary>
                /// Checks if the Order contains Eurovision services that should be manually booked from the LiveOrderForm.
                /// </summary>
                public bool HasEurovisionServices
                {
                    get
                    {
                        foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service in AllServices)
                        {
                            if (service?.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionEurovision)
                                return true;
                            if (service?.Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionEurovision)
                                return true;
                        }

                        return false;
                    }
                }
            }

            public class OrderChange
            {
                public string UserLoginName
                {
                    get;
                    set;
                }

                public System.DateTime TimeStamp
                {
                    get;
                    set;
                }

                = System.DateTime.Now;
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.PropertyChange> PropertyChanges
                {
                    get;
                    set;
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            public class OrderManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.IOrderManager
            {
                public const string OrderBookingManagerElementName = "Order Booking Manager";
                public OrderManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    Helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers Helpers
                {
                    get;
                    set;
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order> GetAllOrders()
                {
                    System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance> allOrderReservations = Skyline.DataMiner.Library.Solutions.SRM.ResourceManagerExtensions.GetReservationInstancesByProperty(Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager, "Type", "Video");
                    return System.Linq.Enumerable.Select(allOrderReservations, x => new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order{Id = x.ID, Name = x.Name, Comments = GetOrderEventGuid(x).ToString()});
                }

                /// <summary>
                /// Creates a list containing the provided services and all their underlying child services.
                /// </summary>
                /// <remarks>To get all Services in an Order, you can use the AllServices property on Order.</remarks>
                /// <param name = "services">Services to check. These services are included in the returned collection.</param>
                /// <returns>Collection containing the provided services and all of their underlying child services.</returns>
                public static System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> FlattenServices(System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> services)
                {
                    System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> flattenedServices = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service>();
                    foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service in services)
                    {
                        flattenedServices.Add(service);
                        flattenedServices.AddRange(FlattenServices(service.Children));
                    }

                    return flattenedServices;
                }

                public System.Guid GetOrderEventGuid(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    object eventId = null;
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.LiteOrder.PropertyNameEventId, out eventId))
                    {
                        return System.Guid.Empty;
                    }

                    if (!System.Guid.TryParse(eventId.ToString(), out var eventGuid))
                    {
                        // TODO: check to use custom Exception
                        throw new System.Exception($"Unable to parse GUID: '{eventId.ToString()}'");
                    }

                    return eventGuid;
                }
            }

            public enum UpdateType
            {
                Add,
                Remove,
                Change
            }

            public class PropertyChange
            {
                public string PropertyName
                {
                    get;
                    set;
                }

                public string OriginalValue
                {
                    get;
                    set;
                }

                public string UpdatedValue
                {
                    get;
                    set;
                }

                public System.Type PropertyType
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.UpdateType UpdateType
                {
                    get;
                    set;
                }

                public System.Guid ServiceId
                {
                    get;
                    set;
                }

                = System.Guid.Empty;
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            public class SportsPlanning : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private string initialSport;
                private string initialDescription;
                private string initialCommentary;
                private string initialCommentary2;
                private double initialCompetitionTime;
                private string initialJournalistOne;
                private string initialJournalistTwo;
                private string initialJournalistThree;
                private string initialLocation;
                private string initialTechnicalResources;
                private string initialLiveHighlightsFile;
                private double initialRequestedBroadcastTime;
                private string initialProductNumberCeiton;
                private string initialProductionNumberPlasmaId;
                private string initialCostDepartment;
                private string initialAdditionalInfo;
                public string Sport
                {
                    get;
                    set;
                }

                public string Description
                {
                    get;
                    set;
                }

                public string Commentary
                {
                    get;
                    set;
                }

                public string Commentary2
                {
                    get;
                    set;
                }

                /// <summary>
                /// Milliseconds since 01/01/1970
                /// </summary>
                public double CompetitionTime
                {
                    get;
                    set;
                }

                public string JournalistOne
                {
                    get;
                    set;
                }

                public string JournalistTwo
                {
                    get;
                    set;
                }

                public string JournalistThree
                {
                    get;
                    set;
                }

                public string Location
                {
                    get;
                    set;
                }

                public string TechnicalResources
                {
                    get;
                    set;
                }

                public string LiveHighlightsFile
                {
                    get;
                    set;
                }

                /// <summary>
                /// Milliseconds since 01/01/1970
                /// </summary>
                public double RequestedBroadcastTime
                {
                    get;
                    set;
                }

                public string ProductionNumberPlasmaId
                {
                    get;
                    set;
                }

                public string ProductNumberCeiton
                {
                    get;
                    set;
                }

                public string CostDepartment
                {
                    get;
                    set;
                }

                public string AdditionalInformation
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a boolean indicating if Change Tracking has been enabled for this object.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public bool IsChanged => ChangeInfo.IsChanged;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                /// <summary>
                /// Gets the type of changes that have happened since object creation or since last <see cref = "IChangeTracking.AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes.</returns>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    if (initialSport != Sport)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialDescription != Description)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialCommentary != Commentary)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialCommentary2 != Commentary2)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (System.Math.Abs(initialCompetitionTime - CompetitionTime) > 0.01)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialJournalistOne != JournalistOne)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialJournalistTwo != JournalistTwo)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialJournalistThree != JournalistThree)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialLocation != Location)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialTechnicalResources != TechnicalResources)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialLiveHighlightsFile != LiveHighlightsFile)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (System.Math.Abs(initialRequestedBroadcastTime - RequestedBroadcastTime) > 0.01)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialProductNumberCeiton != ProductNumberCeiton)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialProductionNumberPlasmaId != ProductionNumberPlasmaId)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialCostDepartment != CostDepartment)
                        changeInfo.MarkCustomPropertiesChanged();
                    if (initialAdditionalInfo != AdditionalInformation)
                        changeInfo.MarkCustomPropertiesChanged();
                    return changeInfo;
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialSport = Sport;
                    initialDescription = Description;
                    initialCommentary = Commentary;
                    initialCommentary2 = Commentary2;
                    initialCompetitionTime = CompetitionTime;
                    initialJournalistOne = JournalistOne;
                    initialJournalistTwo = JournalistTwo;
                    initialJournalistThree = JournalistThree;
                    initialLocation = Location;
                    initialTechnicalResources = TechnicalResources;
                    initialLiveHighlightsFile = LiveHighlightsFile;
                    initialRequestedBroadcastTime = RequestedBroadcastTime;
                    initialProductNumberCeiton = ProductNumberCeiton;
                    initialProductionNumberPlasmaId = ProductionNumberPlasmaId;
                    initialCostDepartment = CostDepartment;
                    initialAdditionalInfo = AdditionalInformation;
                }

                public override string ToString()
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is SportsPlanning other))
                        return false;
                    bool equal = true;
                    foreach (var property in typeof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.SportsPlanning).GetProperties())
                    {
                        equal &= property.GetValue(this).Equals(property.GetValue(other));
                    }

                    return equal;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = hash * 23 + Sport.GetHashCode();
                        hash = hash * 23 + Description.GetHashCode();
                        hash = hash * 23 + Commentary.GetHashCode();
                        hash = hash * 23 + Commentary2.GetHashCode();
                        hash = hash * 23 + CompetitionTime.GetHashCode();
                        hash = hash * 23 + JournalistOne.GetHashCode();
                        hash = hash * 23 + JournalistTwo.GetHashCode();
                        hash = hash * 23 + JournalistThree.GetHashCode();
                        hash = hash * 23 + Location.GetHashCode();
                        hash = hash * 23 + TechnicalResources.GetHashCode();
                        return hash;
                    }
                }
            }

            namespace Recurrence
            {
                public enum RecurrenceFrequencyUnit
                {
                    [System.ComponentModel.Description("day(s)")]
                    Days,
                    [System.ComponentModel.Description("week(s)")]
                    Weeks,
                    [System.ComponentModel.Description("month(s)")]
                    Months,
                    [System.ComponentModel.Description("year(s)")]
                    Years
                }

                public enum RecurrenceRepeatType
                {
                    None,
                    DaysOfTheWeek,
                    UmpteenthDayOfTheMonth,
                    UmpteenthWeekDayOfTheMonth
                }

                [System.Flags]
                public enum DaysOfTheWeek
                {
                    None = 0,
                    [System.ComponentModel.Description("Monday")]
                    Monday = 1,
                    [System.ComponentModel.Description("Tuesday")]
                    Tuesday = 2,
                    [System.ComponentModel.Description("Wednesday")]
                    Wednesday = 4,
                    [System.ComponentModel.Description("Thursday")]
                    Thursday = 8,
                    [System.ComponentModel.Description("Friday")]
                    Friday = 16,
                    [System.ComponentModel.Description("Saturday")]
                    Saturday = 32,
                    [System.ComponentModel.Description("Sunday")]
                    Sunday = 64
                }

                public enum EndingType
                {
                    [System.ComponentModel.Description("Never")]
                    Never,
                    [System.ComponentModel.Description("On a specific date")]
                    SpecificDate,
                    [System.ComponentModel.Description("After certain amount of repeats")]
                    CertainAmountOfRepeats
                }

                public enum RecurrenceAction
                {
                    New = 0,
                    [System.ComponentModel.Description("Edit this order only")]
                    ThisOrderOnly = 1,
                    [System.ComponentModel.Description("Edit all orders in recurring sequence")]
                    AllOrdersInSequence = 2
                }

                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class Recurrence
                {
                    public Recurrence()
                    {
                        StartTime = default(System.DateTime);
                        RecurrenceFrequency = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequency();
                        RecurrenceRepeat = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceRepeat();
                        RecurrenceEnding = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceEnding();
                    }

                    /// <summary>
                    /// The date time indicating the start of the recurring order sequence.
                    /// </summary>
                    public System.DateTime StartTime
                    {
                        get;
                        set;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequency RecurrenceFrequency
                    {
                        get;
                        set;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceRepeat RecurrenceRepeat
                    {
                        get;
                        set;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceEnding RecurrenceEnding
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The date time indicating the end of the recurring order sequence.
                    /// </summary>
                    public System.DateTime EffectiveEndDate
                    {
                        get
                        {
                            var effectiveEndDateOfRecurringOrder = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(System.DateTime.Now.AddYears(1), System.TimeSpan.FromMinutes(1)); // default value for never-ending recurrence
                            if (RecurrenceEnding.EndingType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.EndingType.SpecificDate)
                            {
                                effectiveEndDateOfRecurringOrder = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(RecurrenceEnding.EndingDateTime, System.TimeSpan.FromMinutes(1));
                            }
                            else if (RecurrenceEnding.EndingType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.EndingType.CertainAmountOfRepeats)
                            {
                                System.TimeSpan timespan;
                                switch (RecurrenceFrequency.FrequencyUnit)
                                {
                                    case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit.Days:
                                        timespan = System.TimeSpan.FromDays(1);
                                        break;
                                    case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit.Weeks:
                                        timespan = System.TimeSpan.FromDays(7);
                                        break;
                                    case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit.Months:
                                        timespan = System.TimeSpan.FromDays(31);
                                        break;
                                    case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit.Years:
                                        timespan = System.TimeSpan.FromDays(365);
                                        break;
                                    default:
                                        timespan = System.TimeSpan.Zero;
                                        break;
                                }

                                var multiplier = RecurrenceFrequency.Frequency * RecurrenceEnding.AmountOfRepeats;
                                effectiveEndDateOfRecurringOrder = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(StartTime, System.TimeSpan.FromMinutes(1)) + SLDataGateway.API.Tools.TimeSpanExtensions.Multiply(timespan, multiplier);
                            }

                            return effectiveEndDateOfRecurringOrder;
                        }
                    }
                }

                public class RecurrenceFrequency
                {
                    public RecurrenceFrequency()
                    {
                        Frequency = 0;
                        FrequencyUnit = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit.Days;
                    }

                    public int Frequency
                    {
                        get;
                        set;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceFrequencyUnit FrequencyUnit
                    {
                        get;
                        set;
                    }
                }

                public class RecurrenceRepeat
                {
                    public RecurrenceRepeat()
                    {
                        RepeatType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceRepeatType.None;
                        UmpteenthDayOfTheMonth = 0;
                        Day = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.DaysOfTheWeek.None;
                        UmpteenthOccurrenceOfWeekDayOfTheMonth = 0;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceRepeatType RepeatType
                    {
                        get;
                        set;
                    }

                    public int UmpteenthDayOfTheMonth
                    {
                        get;
                        set;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.DaysOfTheWeek Day
                    {
                        get;
                        set;
                    }

                    public int UmpteenthOccurrenceOfWeekDayOfTheMonth
                    {
                        get;
                        set;
                    }
                }

                public class RecurrenceEnding
                {
                    public RecurrenceEnding()
                    {
                        EndingType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.EndingType.CertainAmountOfRepeats;
                        EndingDateTime = default(System.DateTime);
                        AmountOfRepeats = 0;
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.EndingType EndingType
                    {
                        get;
                        set;
                    }

                    public System.DateTime EndingDateTime
                    {
                        get;
                        set;
                    }

                    public int AmountOfRepeats
                    {
                        get;
                        set;
                    }
                }

                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class RecurringSequenceInfo
                {
                    public RecurringSequenceInfo()
                    {
                        Name = default(string);
                        Recurrence = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.Recurrence();
                    }

                    [Newtonsoft.Json.JsonIgnore]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.RecurrenceAction RecurrenceAction
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The name of the recurring order sequence.
                    /// </summary>
                    public string Name
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The ID of the recurring order sequence in the order manager.
                    /// </summary>
                    public System.Guid Id => TemplateId;
                    /// <summary>
                    /// The recurrence info for the recurring order sequence.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence.Recurrence Recurrence
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The key of the template for the recurring order in the contract manager.
                    /// </summary>
                    public System.Guid TemplateId
                    {
                        get;
                        set;
                    }

                    public bool TemplateIsUpdated
                    {
                        get;
                        set;
                    }

                    public System.Guid EventId
                    {
                        get;
                        set;
                    }

                    public override string ToString()
                    {
                        return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                    }
                }
            }
        }

        namespace Profile
        {
            /// <summary>
            /// This class represents a group of unique Audio Channel Pairs.
            /// </summary>
            public class AudioChannelConfiguration
            {
                private readonly System.Collections.Generic.HashSet<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair> audioChannelPairs = new System.Collections.Generic.HashSet<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair>();
                private int lastDisplayedPair = -1;
                private int maxDisplayedPair = -1;
                /// <summary>
                /// Initializes a new instance of the <see cref = "AudioChannelConfiguration"/> class
                /// </summary>
                /// <param name = "profileParameters">Collection containing the audio channel profile parameters. This can list can also contain other profile parameters. Only the audio channel profile parameters are used.</param>
                public AudioChannelConfiguration(System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> profileParameters)
                {
                    System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> audioProfileParameters = System.Linq.Enumerable.Where(profileParameters, p => p != null && System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AllAudioChannelConfigurationGuids, p.Id));
                    Initialize(audioProfileParameters);
                }

                /// <summary>
                /// A boolean indicating if this Audio Channel Configuration is a copy from the source, taking into account some special copy rules.
                /// </summary>
                public bool IsCopyFromSource
                {
                    get;
                    set;
                }

                /// <summary>
                /// The parameter indicating if dolby-e decoding is required.
                /// Only applicable in case dolby-e is selected in the source.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter AudioDolbyDecodingRequiredProfileParameter
                {
                    get;
                    set;
                }

                /// <summary>
                /// The parameter indicating if embedding is required.
                /// Not applicable in the source.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter AudioEmbeddingRequiredProfileParameter
                {
                    get;
                    set;
                }

                /// <summary>
                /// The parameter indicating if deembedding is required.
                /// Not applicable in the source.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter AudioDeembeddingRequiredProfileParameter
                {
                    get;
                    set;
                }

                /// <summary>
                /// The parameter indicating if shuffling is required.
                /// Not applicable in the source.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter AudioShufflingRequiredProfileParameter
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets the list of Audio Channel Pairs managed by this AudioChannelConfiguration.
                /// </summary>
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair> AudioChannelPairs
                {
                    get
                    {
                        return audioChannelPairs;
                    }
                }

                /// <summary>
                /// Indicates if an additional Audio Channel Pair can be displayed in the UI.
                /// </summary>
                public bool CanAddAudioPair
                {
                    get
                    {
                        return lastDisplayedPair < maxDisplayedPair;
                    }
                }

                /// <summary>
                /// Indicates if an Audio Channel Pair can be removed in the UI.
                /// </summary>
                public bool CanRemoveAudioPair
                {
                    get
                    {
                        return lastDisplayedPair > 0;
                    }
                }

                /// <summary>
                /// Gets a value matching the Channel of the last Audio Channel Pair that is displayed in the UI.
                /// Returns -1 if no Audio Channel Pairs are displayed.
                /// </summary>
                public int LastDisplayedAudioPairchannel
                {
                    get
                    {
                        return lastDisplayedPair;
                    }
                }

                /// <summary>
                /// Initialize the properties linked to audio profile parameters.
                /// </summary>
                /// <param name = "audioProfileParameters">The list of audio profile parameters.</param>
                private void Initialize(System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> audioProfileParameters)
                {
                    AudioDolbyDecodingRequiredProfileParameter = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, a => a.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AudioDolbyDecodingRequired);
                    AudioEmbeddingRequiredProfileParameter = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, a => a.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AudioEmbeddingRequired);
                    AudioDeembeddingRequiredProfileParameter = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, a => a.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AudioDeembeddingRequired);
                    AudioShufflingRequiredProfileParameter = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, a => a.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AudioShufflingRequired);
                    foreach (var audioProfileParameter in audioProfileParameters)
                    {
                        // there is only 1 parameter used to indicate if audio dolby decoding is required
                        if (AudioDolbyDecodingRequiredProfileParameter != null && AudioDolbyDecodingRequiredProfileParameter.Id == audioProfileParameter.Id)
                            continue;
                        // check that this audio channel profile parameter is not yet processed
                        // every Audio Channel Pair uses a number of profile parameters 
                        if (System.Linq.Enumerable.Any(AudioChannelPairs, p => p.Contains(audioProfileParameter)))
                            continue;
                        int channel;
                        if (!System.Int32.TryParse(System.Linq.Enumerable.Last(audioProfileParameter.Name.Split(' ')), out channel) || channel % 2 == 0)
                        {
                            // an Audio Channel Pair contains the configuration of 2 audio channels
                            // we can skip this in case the channel is not uneven as it will be automatically added to the correct audio channel pair already
                            continue;
                        }

                        var firstChannel = audioProfileParameter;
                        var firstChannelDescription = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, p => p.Name == System.String.Format("{0} Description", firstChannel.Name));
                        var secondChannel = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, p => p.Name == firstChannel.Name.Replace(channel.ToString(), (channel + 1).ToString()));
                        var secondChannelDescription = System.Linq.Enumerable.FirstOrDefault(audioProfileParameters, p => p.Name == System.String.Format("{0} Description", secondChannel.Name));
                        var audioChannel = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair(firstChannel, firstChannelDescription, secondChannel, secondChannelDescription, AudioDolbyDecodingRequiredProfileParameter);
                        audioChannelPairs.Add(audioChannel);
                        if (audioChannel.ShouldDisplay)
                            lastDisplayedPair = audioChannel.Channel;
                        if (audioChannel.Channel > maxDisplayedPair)
                            maxDisplayedPair = audioChannel.Channel;
                    }
                }

                public override string ToString()
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Is copy from source: {IsCopyFromSource} | ");
                    if (AudioDeembeddingRequiredProfileParameter != null)
                        sb.AppendLine($"Audio deembedding required: {AudioDeembeddingRequiredProfileParameter.StringValue} | ");
                    if (AudioEmbeddingRequiredProfileParameter != null)
                        sb.AppendLine($"Audio embedding required: {AudioEmbeddingRequiredProfileParameter.StringValue} | ");
                    if (AudioShufflingRequiredProfileParameter != null)
                        sb.AppendLine($"Audio shuffling required: {AudioShufflingRequiredProfileParameter.StringValue} | ");
                    foreach (var audioChannelPair in AudioChannelPairs)
                    {
                        sb.AppendLine($"{audioChannelPair.ToString()} | ");
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// This class represents an Audio Channel Pair.
            /// </summary>
            public class AudioChannelPair
            {
                private System.Collections.Generic.List<System.String> allAudioChannelOptions;
                private bool isStereo;
                /// <summary>
                /// Initializes a new instance of the <see cref = "AudioChannelPair"/> class
                /// </summary>
                /// <param name = "firstChannel">ProfileParameter of the first audio channel in this pair.</param>
                /// <param name = "firstChannelDescription">ProfileParameter of the description for the first audio channel in this pair.</param>
                /// <param name = "secondChannel">ProfileParameter of the second audio channel in this pair.</param>
                /// <param name = "secondChannelDescription">ProfileParameter of the description for the second audio channel in this pair.</param>
                /// <param name = "dolbyDecoding">ProfileParameter of the Dolby Decoding parameter.</param>
                /// <exception cref = "ArgumentNullException"/>
                public AudioChannelPair(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter firstChannel, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter firstChannelDescription, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter secondChannel, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter secondChannelDescription, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter dolbyDecoding)
                {
                    FirstChannelProfileParameter = firstChannel ?? throw new System.ArgumentNullException(nameof(firstChannel));
                    FirstChannelDescriptionProfileParameter = firstChannelDescription ?? throw new System.ArgumentNullException(nameof(firstChannelDescription));
                    SecondChannelProfileParameter = secondChannel ?? throw new System.ArgumentNullException(nameof(secondChannel));
                    SecondChannelDescriptionProfileParameter = secondChannelDescription ?? throw new System.ArgumentNullException(nameof(secondChannelDescription));
                    AllAudioChannelOptions = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Concat(new[]{Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.Constants.None}, System.Linq.Enumerable.Select(FirstChannelProfileParameter.Discreets, d => d.DisplayValue)));
                    AllAudioChannelOptionsWithoutDolbyDecoding = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(AllAudioChannelOptions, o => !(o.StartsWith("Dolby") && o.Contains("&"))));
                    Channel = System.Convert.ToInt32(System.Linq.Enumerable.Last(firstChannel.Name.Split(' ')));
                    Description = $"Audio Channel {Channel}&{Channel + 1}";
                    if (FirstChannelProfileParameter.StringValue == "Other" && SecondChannelProfileParameter.StringValue == "Other")
                    {
                        IsStereo = FirstChannelDescriptionProfileParameter.StringValue == SecondChannelDescriptionProfileParameter.StringValue;
                    }
                    else
                    {
                        IsStereo = FirstChannelProfileParameter.StringValue == SecondChannelProfileParameter.StringValue;
                    }

                    if (dolbyDecoding != null)
                    {
                        DolbyDecodingProfileParameter = dolbyDecoding;
                    }
                }

                /// <summary>
                /// Indicates if this audio channel pair contains the provided profile parameter.
                /// </summary>
                /// <param name = "parameter">The profile parameter.</param>
                /// <returns>True in case the profile parameter is part of this audio channel configuration.</returns>
                public bool Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter parameter)
                {
                    return FirstChannelProfileParameter.Equals(parameter) || FirstChannelDescriptionProfileParameter.Equals(parameter) || SecondChannelProfileParameter.Equals(parameter) || SecondChannelDescriptionProfileParameter.Equals(parameter);
                }

                /// <summary>
                /// Gets all possible options for the Audio Channel profile parameters.
                /// </summary>
                public System.Collections.Generic.List<System.String> AllAudioChannelOptions
                {
                    get => allAudioChannelOptions;
                    set
                    {
                        if (AllAudioChannelOptions != value)
                        {
                            allAudioChannelOptions = value;
                            AudioChannelOptionsChanged?.Invoke(this, new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair.AudioChannelOptionsChangedEventArgs(Channel, allAudioChannelOptions));
                        }
                    }
                }

                public event System.EventHandler<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair.AudioChannelOptionsChangedEventArgs> AudioChannelOptionsChanged;
                /// <summary>
                /// Gets all options without the Dolby Decoded channels for the Audio Channel profile parameters.
                /// </summary>
                public System.Collections.Generic.List<System.String> AllAudioChannelOptionsWithoutDolbyDecoding
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The profile parameter for the first channel.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter FirstChannelProfileParameter
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The profile parameter for the description of the first channel.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter FirstChannelDescriptionProfileParameter
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The profile parameter for the second channel.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter SecondChannelProfileParameter
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The profile parameter for the description of the second channel.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter SecondChannelDescriptionProfileParameter
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The Dolby decoding profile parameter.
                /// </summary>
                /// <remarks>Possible values: "Yes", "No".</remarks>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter DolbyDecodingProfileParameter
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The channel of this audio channel pair.
                /// This matches the Audio Channel ID of the first Audio Channel in this Pair.
                /// </summary>
                public int Channel
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The description of this audio channel pair.
                /// </summary>
                public string Description
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Indicates if this audio channel pair is stereo or mono.
                /// </summary>
                public bool IsStereo
                {
                    get => isStereo;
                    set
                    {
                        if (isStereo != value)
                        {
                            isStereo = value;
                            IsStereoChanged?.Invoke(this, isStereo);
                        }
                    }
                }

                /// <summary>
                /// This event is called when the value of the IsStereo configuration parameter is updated.
                /// </summary>
                public event System.EventHandler<System.Boolean> IsStereoChanged;
                /// <summary>
                /// Used in AudioChannelPairSection.
                /// Indicates whether this pair should be visible in the UI.
                /// An Audio Channel Pair should only be displayed when it's values are not none.
                /// </summary>
                public bool ShouldDisplay => FirstChannelProfileParameter.StringValue != "None" && FirstChannelProfileParameter.Value != null || SecondChannelProfileParameter.StringValue != "None" && SecondChannelProfileParameter.Value != null;
                /// <summary>
                /// Generates a HashCode for this object.
                /// </summary>
                /// <returns>HashCode for this object.</returns>
                public override int GetHashCode()
                {
                    return FirstChannelProfileParameter.GetHashCode() ^ FirstChannelDescriptionProfileParameter.GetHashCode() ^ SecondChannelProfileParameter.GetHashCode() ^ SecondChannelDescriptionProfileParameter.GetHashCode();
                }

                /// <summary>
                /// Checks if this object matches another one.
                /// </summary>
                /// <param name = "obj">Object to check.</param>
                /// <returns>True if object matches else false.</returns>
                public override bool Equals(object obj)
                {
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair other = obj as Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelPair;
                    if (other == null)
                        return false;
                    return FirstChannelProfileParameter.Equals(other.FirstChannelProfileParameter) && FirstChannelDescriptionProfileParameter.Equals(other.FirstChannelDescriptionProfileParameter) && SecondChannelProfileParameter.Equals(other.SecondChannelProfileParameter) && SecondChannelDescriptionProfileParameter.Equals(other.SecondChannelDescriptionProfileParameter);
                }

                public override string ToString()
                {
                    return $"Channel {Channel} Profile Parameter = {FirstChannelProfileParameter.StringValue}. | Channel {Channel + 1} Profile Parameter = {SecondChannelProfileParameter.StringValue} | IsStereo = {IsStereo} | Dolby Decoding = {(DolbyDecodingProfileParameter != null ? DolbyDecodingProfileParameter.StringValue : "N/A")}.";
                }

                public class AudioChannelOptionsChangedEventArgs : System.EventArgs
                {
                    internal AudioChannelOptionsChangedEventArgs(int channel, System.Collections.Generic.IEnumerable<System.String> audioChannelOptions)
                    {
                        Channel = channel;
                        AudioChannelOptions = audioChannelOptions;
                    }

                    public int Channel
                    {
                        get;
                        private set;
                    }

                    public System.Collections.Generic.IEnumerable<System.String> AudioChannelOptions
                    {
                        get;
                        private set;
                    }
                }
            }

            public class Discreet
            {
                private readonly string discreet;
                public Discreet(string discreet)
                {
                    this.discreet = discreet;
                    IsAvailable = true;
                    InitializeLinkedParameterValues();
                }

                private void InitializeLinkedParameterValues()
                {
                    if (discreet == null)
                        return;
                    if (discreet.Contains("[") && discreet.Contains(":") && discreet.Contains("]"))
                    {
                        System.Text.RegularExpressions.Match discreetNameMatch = System.Text.RegularExpressions.Regex.Match(discreet, @".*\[");
                        System.Text.RegularExpressions.Match parameterNameMatch = System.Text.RegularExpressions.Regex.Match(discreet, @"\[.*:");
                        System.Text.RegularExpressions.Match parameterValueMatch = System.Text.RegularExpressions.Regex.Match(discreet, @":.*\]");
                        DisplayValue = discreetNameMatch.Value.Trim(' ', '[');
                        LinkedParentName = parameterNameMatch.Value.Trim('[', ':');
                        LinkedParentValue = parameterValueMatch.Value.Trim(':', ']').ToUpper();
                    }
                    else
                    {
                        DisplayValue = discreet;
                    }
                }

                public bool IsAvailable
                {
                    get;
                    set;
                }

                /// <summary>
                /// The value of the discreet as it should be displayed in the UI.
                /// In case of linked parameters this will be the first piece of the discreet.
                /// In case of a non-linked parameter this will be the same as the internal value of the discreet.
                /// </summary>
                public string DisplayValue
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Value of the Discreet as used internally. For linked discreets, this will be "DisplayValue[Linked Parent Name:Linked Parent Value]"
                /// </summary>
                public string InternalValue
                {
                    get
                    {
                        return discreet;
                    }
                }

                public string LinkedParentName
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The selected value of the linked parent discreet to which this discreet applies.
                /// This value is in uppercase.
                /// </summary>
                public string LinkedParentValue
                {
                    get;
                    private set;
                }

                public override string ToString()
                {
                    return discreet;
                }

                public override bool Equals(object obj)
                {
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet other = obj as Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet;
                    if (other == null)
                        return false;
                    return discreet.Equals(other.discreet);
                }

                public override int GetHashCode()
                {
                    return discreet.GetHashCode();
                }
            }

            public interface IProfileManager
            {
                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition GetProfileDefinition(System.Guid id);
                System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition> GetInterfaceProfileDefinitions(Skyline.DataMiner.Net.Messages.FunctionDefinition functionDefinition);
                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter GetProfileParameter(System.Guid guid);
            }

            public class ProfileDefinition
            {
                public ProfileDefinition(Skyline.DataMiner.Net.Profiles.ProfileDefinition srmProfileDefinition)
                {
                    if (srmProfileDefinition == null)
                        throw new System.ArgumentNullException(nameof(srmProfileDefinition));
                    Id = srmProfileDefinition.ID;
                    Name = srmProfileDefinition.Name;
                    ProfileParameters = GetProfileParameters(srmProfileDefinition);
                }

                public System.Guid Id
                {
                    get;
                    set;
                }

                public string Name
                {
                    get;
                    set;
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> ProfileParameters
                {
                    get;
                    set;
                }

                private System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> GetProfileParameters(Skyline.DataMiner.Net.Profiles.ProfileDefinition srmProfileDefinition)
                {
                    var profileParameters = new System.Collections.Generic.HashSet<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    if (srmProfileDefinition == null)
                        return profileParameters;
                    foreach (var profileParameter in srmProfileDefinition.Parameters)
                    {
                        var existingProfileParameter = System.Linq.Enumerable.FirstOrDefault(profileParameters, p => p.Equals(profileParameter));
                        if (existingProfileParameter == null)
                        {
                            profileParameters.Add(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter(profileParameter));
                        }
                    }

                    return profileParameters;
                }
            }

            public class ProfileManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.IProfileManager
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers;
                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.ProfileDefinition> allSrmProfileDefinitions;
                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.Parameter> allSrmProfileParameters;
                public ProfileManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    this.helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                }

                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.ProfileDefinition> AllSrmProfileDefinitions => allSrmProfileDefinitions ?? (allSrmProfileDefinitions = GetAllSrmProfileDefinitions());
                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.Parameter> AllSrmProfileParameters => allSrmProfileParameters ?? (allSrmProfileParameters = GetAllSrmProfileParameters());
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter GetProfileParameter(System.Guid guid)
                {
                    var parameter = System.Linq.Enumerable.SingleOrDefault(AllSrmProfileParameters, pp => pp.ID == guid) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ProfileParameterNotFoundException(guid);
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter(parameter);
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition GetProfileDefinition(System.Guid id)
                {
                    var srmProfileDefinition = System.Linq.Enumerable.SingleOrDefault(AllSrmProfileDefinitions, pd => pd.ID == id) ?? throw new Skyline.DataMiner.Library.Exceptions.ProfileDefinitionNotFoundException(id);
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition(srmProfileDefinition);
                }

                public System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition> GetInterfaceProfileDefinitions(Skyline.DataMiner.Net.Messages.FunctionDefinition functionDefinition)
                {
                    if (functionDefinition == null)
                        throw new System.ArgumentNullException(nameof(functionDefinition));
                    var interfaceProfileDefinitions = new System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition>();
                    var functionInterfaces = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Concat(System.Linq.Enumerable.Concat(functionDefinition.InputInterfaces, functionDefinition.OutputInterfaces), functionDefinition.InputOutputInterfaces));
                    foreach (var functionInterface in functionInterfaces)
                    {
                        if (interfaceProfileDefinitions.ContainsKey(functionInterface.ProfileDefinition))
                            continue;
                        var systemProfileDefinition = System.Linq.Enumerable.SingleOrDefault(AllSrmProfileDefinitions, pd => pd.ID == functionInterface.ProfileDefinition);
                        var profileDefinition = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition(systemProfileDefinition);
                        interfaceProfileDefinitions[profileDefinition.Id] = profileDefinition;
                    }

                    return interfaceProfileDefinitions;
                }

                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.ProfileDefinition> GetAllSrmProfileDefinitions()
                {
                    LogMethodStart(nameof(GetAllSrmProfileDefinitions), out var stopwatch);
                    var retrievedSrmProfileDefinitions = Skyline.DataMiner.Net.ManagerStore.CrudHelperComponentExtension.ReadAll(helpers.ProfileHelper.ProfileDefinitions);
                    Log(nameof(GetAllSrmProfileDefinitions), $"Retrieved {retrievedSrmProfileDefinitions.Count} profile definitions");
                    LogMethodCompleted(nameof(GetAllSrmProfileDefinitions), stopwatch);
                    return retrievedSrmProfileDefinitions;
                }

                private System.Collections.Generic.List<Skyline.DataMiner.Net.Profiles.Parameter> GetAllSrmProfileParameters()
                {
                    LogMethodStart(nameof(GetAllSrmProfileParameters), out var stopwatch);
                    var retrievedProfileParameters = Skyline.DataMiner.Net.ManagerStore.CrudHelperComponentExtension.ReadAll(helpers.ProfileHelper.ProfileParameters);
                    Log(nameof(GetAllSrmProfileParameters), $"Retrieved {retrievedProfileParameters.Count} profile parameters");
                    LogMethodCompleted(nameof(GetAllSrmProfileParameters), stopwatch);
                    return retrievedProfileParameters;
                }

                private void Log(string nameOfMethod, string message, string nameOfObject = null)
                {
                    helpers.ProgressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileManager), nameOfMethod, message, nameOfObject);
                }

                private void LogMethodStart(string nameOfMethod, out System.Diagnostics.Stopwatch stopwatch)
                {
                    Log(nameOfMethod, "Start");
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();
                }

                private void LogMethodCompleted(string nameOfMethod, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    stopwatch?.Stop();
                    Log(nameOfMethod, $"Completed [{stopwatch?.Elapsed}]");
                }
            }

            /// <summary>
            /// This class represents an SRM ProfileParameter.
            /// </summary>
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            public class ProfileParameter : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private object value;
                private object initialValue;
                private System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet> discreets = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet>();
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo valueValidation;
                public ProfileParameter()
                {
                }

                public ProfileParameter(Skyline.DataMiner.Net.Profiles.Parameter netParameter, Skyline.DataMiner.Library.Solutions.SRM.Model.Parameter srmParameter = null)
                {
                    if (netParameter == null)
                        throw new System.ArgumentNullException(nameof(netParameter));
                    Name = netParameter.Name;
                    Id = netParameter.ID;
                    Category = netParameter.Categories;
                    Type = (Skyline.DataMiner.Library.Solutions.SRM.Model.ParameterType)netParameter.Type;
                    Discreets = netParameter.Type == Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Discrete ? System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(netParameter.Discretes, x => new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet(x))) : new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet>();
                    DefaultValue = netParameter.DefaultValue;
                    Stepsize = netParameter.Stepsize;
                    Decimals = netParameter.Decimals;
                    RangeMax = netParameter.RangeMax;
                    RangeMin = netParameter.RangeMin;
                    Unit = netParameter.Units;
                    if (srmParameter != null)
                        Value = srmParameter.Value;
                    initialValue = Value;
                }

                /// <summary>
                /// The id of the profile parameter
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>
                public System.Guid Id
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the name of the ProfileParameter.
                /// </summary>
                public string Name
                {
                    get;
                    set;
                }

                /// <summary>
                /// Property set by controller and used by UI for validation.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo ValueValidation
                {
                    get
                    {
                        if (valueValidation == null)
                            valueValidation = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo();
                        return valueValidation;
                    }

                    set
                    {
                        valueValidation = value;
                    }
                }

                /// <summary>
                /// The value of the profile parameter
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>
                public object Value
                {
                    get
                    {
                        return value;
                    }

                    set
                    {
                        if (this.value != value)
                        {
                            this.value = value;
                            ValueChanged?.Invoke(this, this.value);
                        }
                    }
                }

                /// <summary>
                /// Gets the string representation of the value contained in this ProfileParameter.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public string StringValue
                {
                    get => System.Convert.ToString(value);
                }

                /// <summary>
                /// This event is called when the value of this ProfileParameter is updated.
                /// </summary>
                public event System.EventHandler<System.Object> ValueChanged;
                /// <summary>
                /// The categories of the Profile Parameter.
                /// </summary>
                public Skyline.DataMiner.Net.Profiles.ProfileParameterCategory Category
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if the Profile Parameter is a Capability.
                /// </summary>
                public bool IsCapability
                {
                    get => (Category & Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capability) == Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capability;
                }

                /// <summary>
                /// Gets a boolean indicating if Change Tracking has been enabled for this object.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool IsChanged => ChangeInfo.IsChanged;
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                public bool IsNonInterfaceDtrParameter => Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids._Matrix || Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids._OrbitalPosition;
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet> Discreets
                {
                    get
                    {
                        return discreets;
                    }

                    set
                    {
                        if (value == null)
                        {
                            discreets = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.Discreet>();
                        }
                        else
                        {
                            discreets = value;
                        }
                    }
                }

                public Skyline.DataMiner.Library.Solutions.SRM.Model.ParameterType Type
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.Net.Profiles.ParameterValue DefaultValue
                {
                    get;
                    set;
                }

                public double RangeMin
                {
                    get;
                    set;
                }

                public double RangeMax
                {
                    get;
                    set;
                }

                public double Stepsize
                {
                    get;
                    set;
                }

                public int Decimals
                {
                    get;
                    set;
                }

                public string Unit
                {
                    get;
                    set;
                }

                public override bool Equals(object obj)
                {
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter other = obj as Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter;
                    if (other == null)
                        return false;
                    return Id.Equals(other.Id);
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialValue = Value;
                }

                public override string ToString()
                {
                    return $"{Name} = {StringValue}";
                }

                /// <summary>
                /// Gets the changes made to this object since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes.</returns>
                /// <see cref = "IYleChangeTracking"/>
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    if (Value != initialValue)
                    {
                        if (System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AllAudioProcessingRequiredGuids, Id))
                            changeInfo.ProfileParameterChangeInfo.MarkAudioProcessingProfileParametersChanged();
                        else if (Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.VideoFormat)
                            changeInfo.ProfileParameterChangeInfo.MarkVideoProcessingProfileParametersChanged();
                        else if (Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.RemoteGraphics)
                            changeInfo.ProfileParameterChangeInfo.MarkGraphicsProcessingProfileParametersChanged();
                        else if (IsNonInterfaceDtrParameter)
                            changeInfo.ProfileParameterChangeInfo.MarkDtrProfileParametersChanged();
                        else
                            changeInfo.ProfileParameterChangeInfo.MarkOtherProfileParametersChanged();
                    }

                    return changeInfo;
                }
            }
        }

        namespace Resources
        {
            public interface IResourceManager
            {
            }

            /// <summary>
            /// Wrapper class around the ResourceManagerHelper.
            /// This was introduced to allow mocking of this class through the IResourceManager interface.
            /// </summary>
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            public class ResourceManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.IResourceManager
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers;
                public ResourceManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    this.helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                }
            }
        }

        namespace Service
        {
            public enum Status
            {
                /// <summary>
                /// Service which is under preliminary order.
                /// </summary>
                [System.ComponentModel.Description("Preliminary")]
                Preliminary = 0,
                /// <summary>
                /// Service which is waiting for device configuration.
                /// User tasks are generated for specific service related manual tasks.
                /// </summary>
                [System.ComponentModel.Description("Configuration Pending")]
                ConfigurationPending = 1,
                /// <summary>
                /// Service where all related user tasks are completed.
                /// </summary>
                [System.ComponentModel.Description("Configuration Completed")]
                ConfigurationCompleted = 2,
                /// <summary>
                /// Service where some component is overbooked.
                /// This is actually not possible in SRM and will more likely be like "Quaranteened" eg "Resource Missing"
                /// </summary>
                [System.ComponentModel.Description("Resource Overbooked")]
                ResourceOverbooked = 3,
                /// <summary>
                /// Service which is withing 30min of start time.
                /// </summary>
                [System.ComponentModel.Description("Service Cueing")]
                ServiceCueing = 4,
                /// <summary>
                /// Service where start time is reached and is not yet finished.
                /// </summary>
                [System.ComponentModel.Description("Service Running")]
                ServiceRunning = 5,
                /// <summary>
                /// Service in post roll.
                /// </summary>
                [System.ComponentModel.Description("Post Roll")]
                PostRoll = 6,
                /// <summary>
                /// Service where end time is reached but was only partially delivered or not delivered at all.
                /// </summary>
                [System.ComponentModel.Description("Service Completed With Errors")]
                ServiceCompletedWithErrors = 7,
                /// <summary>
                /// Service where end time is reached.
                /// </summary>
                [System.ComponentModel.Description("Service Completed")]
                ServiceCompleted = 8,
                /// <summary>
                /// Service which was cancelled.
                /// </summary>
                [System.ComponentModel.Description("Cancelled")]
                Cancelled = 9,
                /// <summary>
                /// Service which has incompleted file processing user tasks.
                /// </summary>
                [System.ComponentModel.Description("File Processing")]
                FileProcessing = 10
            }

            public enum Type
            {
                Source,
                Destination,
                Recording,
                Transmission,
                VideoProcessing,
                AudioProcessing,
                GraphicsProcessing,
                Routing
            }

            public enum FileDestination
            {
                [System.ComponentModel.Description("ARCHIVE (METRO)")]
                ArchiveMetro,
                [System.ComponentModel.Description("IPLAY HKI")]
                IplayHki,
                [System.ComponentModel.Description("IPLAY VSA")]
                IplayVsa,
                [System.ComponentModel.Description("IPLAY TRE")]
                IplayTre,
                [System.ComponentModel.Description("UA IPLAY")]
                UaIplay,
                [System.ComponentModel.Description("ISILON")]
                Isilon
            }

            public enum VideoResolution
            {
                [System.ComponentModel.Description("576i50")]
                Resolution576i50,
                [System.ComponentModel.Description("1080i50")]
                Resolution1080i50,
                [System.ComponentModel.Description("1080p50")]
                Resolution1080p50
            }

            public enum VideoCodec
            {
                [System.ComponentModel.Description("AVCi100")]
                AvcI100,
                [System.ComponentModel.Description("XDcamHD50")]
                XdCamHd50
            }

            public enum TimeCodec
            {
                [System.ComponentModel.Description("Real")]
                Real,
                [System.ComponentModel.Description("Non-Real")]
                NonReal
            }

            public enum ProxyFormat
            {
                [System.ComponentModel.Description("Both")]
                Both,
                [System.ComponentModel.Description("MPEG-1")]
                Mpeg1,
                [System.ComponentModel.Description("MPEG-4")]
                Mpeg4
            }

            public interface IServiceManager
            {
                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service GetService(System.Guid serviceId);
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            /// <summary>
            /// The recording configuration containing additional details for a recording that are not stored in profile parameters.
            /// </summary>
            public class RecordingConfiguration : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private bool initialSubrecordingsNeeded;
                private string initialNameOfServiceToRecord;
                private string initialRecordingName;
                private string initialPlasmaIdForArchive;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FileDestination initialRecordingFileDestination;
                private string initialRecordingFileDestinationPath;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.VideoResolution initialRecordingFileVideoResolution;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.TimeCodec initialRecordingFileTimeCodec;
                private bool initialSubtitleProxy;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ProxyFormat initialProxyFormat;
                private bool initialFastRerunCopy;
                private bool initialAreenaCopy;
                private bool initialBroadcastReady;
                private System.DateTime initialDeadlineForArchiving;
                public RecordingConfiguration()
                {
                    RecordingFileDestination = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FileDestination.ArchiveMetro;
                    RecordingFileVideoResolution = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.VideoResolution.Resolution1080i50;
                    RecordingFileVideoCodec = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.VideoCodec.AvcI100;
                    RecordingFileTimeCodec = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.TimeCodec.Real;
                    SubtitleProxy = false;
                    ProxyFormat = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ProxyFormat.Mpeg1;
                    FastRerunCopy = false;
                    FastAreenaCopy = false;
                    BroadcastReady = false;
                    IsPlasmaLiveNews = false;
                    SubRecordingsNeeded = false;
                    SubRecordings = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.SubRecording>();
                }

                /// <summary>
                /// The service name of the service that needs to be recorded.
                /// Only needs to be set in case the operator in the UI selected to record a specific service.
                /// </summary>
                public string NameOfServiceToRecord
                {
                    get;
                    set;
                }

                /// <summary>
                /// Name of the recording.
                /// </summary>
                public string RecordingName
                {
                    get;
                    set;
                }

                /// <summary>
                /// Plasma ID for archiving.
                /// Only applicable on Live recordings.
                /// </summary>
                public string PlasmaIdForArchive
                {
                    get;
                    set;
                }

                /// <summary>
                /// Destination of the recording file.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.FileDestination RecordingFileDestination
                {
                    get;
                    set;
                }

                /// <summary>
                /// The video resolution of the recording file.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.VideoResolution RecordingFileVideoResolution
                {
                    get;
                    set;
                }

                /// <summary>
                /// The video codec of the recording file.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.VideoCodec RecordingFileVideoCodec
                {
                    get;
                    set;
                }

                /// <summary>
                /// The time codec for the recording file.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.TimeCodec RecordingFileTimeCodec
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if subtitle proxy is required.
                /// </summary>
                public bool SubtitleProxy
                {
                    get;
                    set;
                }

                /// <summary>
                /// The proxy format.
                /// Only in case Subtitle Proxy is required.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ProxyFormat ProxyFormat
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if a fast rerun copy is required.
                /// </summary>
                public bool FastRerunCopy
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if a fast areena copy is required.
                /// </summary>
                public bool FastAreenaCopy
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this recording is broadcast ready.
                /// </summary>
                public bool BroadcastReady
                {
                    get;
                    set;
                }

                /// <summary>
                /// The destination path for this recording.
                /// </summary>
                public string RecordingFileDestinationPath
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if sub recordings are needed.
                /// </summary>
                public bool SubRecordingsNeeded
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if the recording configuration is part of a Plasma Live News order.
                /// </summary>
                public bool IsPlasmaLiveNews
                {
                    get;
                    set;
                }

                /// <summary>
                /// The deadline for archiving.
                /// </summary>
                public System.DateTime DeadLineForArchiving
                {
                    get;
                    set;
                }

                /// <summary>
                /// TV Channel name from the Transmission table in the MediaGenix WhatsOn element.
                /// Used for the short description on service level.
                /// Only applicable for plasma services.
                /// </summary>
                public string PlasmaTvChannelName
                {
                    get;
                    set;
                }

                /// <summary>
                /// Program name from the Programs table in the MediaGenix WhatsOn element.
                /// Used for the short description on service level.
                /// Only applicable for plasma services.
                /// </summary>
                public string PlasmaProgramName
                {
                    get;
                    set;
                }

                /// <summary>
                /// The list of sub recordings.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.SubRecording> SubRecordings
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public bool IsChanged => ChangeInfo.IsChanged;
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                /// <summary>
                /// Gets a boolean indicating if Change Tracking is enabled.
                /// </summary>
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets the type of changes that have happened since object creation or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes.</returns>
                /// <see cref = "IYleChangeTracking"/>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    bool propertiesHaveChanged = false;
                    propertiesHaveChanged |= initialNameOfServiceToRecord != NameOfServiceToRecord;
                    propertiesHaveChanged |= initialRecordingName != RecordingName;
                    propertiesHaveChanged |= initialRecordingFileDestination != RecordingFileDestination;
                    propertiesHaveChanged |= initialRecordingFileDestinationPath != RecordingFileDestinationPath;
                    propertiesHaveChanged |= initialPlasmaIdForArchive != PlasmaIdForArchive;
                    propertiesHaveChanged |= initialRecordingFileVideoResolution != RecordingFileVideoResolution;
                    propertiesHaveChanged |= initialRecordingFileTimeCodec != RecordingFileTimeCodec;
                    propertiesHaveChanged |= initialFastRerunCopy != FastRerunCopy;
                    propertiesHaveChanged |= initialAreenaCopy != FastAreenaCopy;
                    propertiesHaveChanged |= initialSubtitleProxy != SubtitleProxy;
                    propertiesHaveChanged |= initialProxyFormat != ProxyFormat;
                    propertiesHaveChanged |= initialBroadcastReady != BroadcastReady;
                    propertiesHaveChanged |= initialDeadlineForArchiving != DeadLineForArchiving;
                    propertiesHaveChanged |= initialSubrecordingsNeeded != SubRecordingsNeeded;
                    if (propertiesHaveChanged)
                        changeInfo.MarkCustomPropertiesChanged();
                    foreach (var subrecording in SubRecordings)
                        changeInfo.CombineWith(subrecording.ChangeInfo);
                    return changeInfo;
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialNameOfServiceToRecord = NameOfServiceToRecord;
                    initialRecordingName = RecordingName;
                    initialPlasmaIdForArchive = PlasmaIdForArchive;
                    initialRecordingFileDestination = RecordingFileDestination;
                    initialRecordingFileDestinationPath = RecordingFileDestinationPath;
                    initialRecordingFileVideoResolution = RecordingFileVideoResolution;
                    initialRecordingFileTimeCodec = RecordingFileTimeCodec;
                    initialSubtitleProxy = SubtitleProxy;
                    initialProxyFormat = ProxyFormat;
                    initialFastRerunCopy = FastRerunCopy;
                    initialAreenaCopy = FastAreenaCopy;
                    initialBroadcastReady = BroadcastReady;
                    initialSubrecordingsNeeded = SubRecordingsNeeded;
                    initialDeadlineForArchiving = DeadLineForArchiving;
                    foreach (var subrecording in SubRecordings)
                        subrecording.AcceptChanges();
                }

                /// <summary>
                /// Deserialize a json string into a RecordingConfiguration object.
                /// </summary>
                /// <param name = "recordingConfiguration">The recording configuration json string.</param>
                /// <returns>A RecordingConfiguration object.</returns>
                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration Deserialize(string recordingConfiguration)
                {
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration>(recordingConfiguration);
                    }
                    catch (System.Exception)
                    {
                        return null;
                    }
                }

                public override string ToString()
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Subtitle proxy: " + SubtitleProxy + " | ");
                    sb.AppendLine($"Fast rerun copy: " + FastRerunCopy + " | ");
                    sb.AppendLine($"Broadcast ready: " + BroadcastReady + " | ");
                    sb.AppendLine($"Sub recording needed: " + SubRecordingsNeeded + " | ");
                    foreach (var subRecording in SubRecordings)
                    {
                        sb.AppendLine($"{subRecording.ToString()} | ");
                    }

                    return sb.ToString();
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            public class Service : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private const string None = "None";
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo startValidation;
                private string name;
                private bool isCancelled;
                // Fields for change tracking
                private System.DateTime initialStart;
                //private DateTime initialStartWithPreRoll;
                private System.DateTime initialEnd;
                //private DateTime initialEndWithPostRoll;
                private bool initialIntegrationIsMaster;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration initialRecordingConfiguration;
                private string initialContactInfoName;
                private string initialContactInfoPhone;
                private string initialLiveUDeviceName;
                private string initialAudioReturnInfo;
                private string initialVidigoStreamSourceLink;
                private string initialComments;
                private System.Collections.Generic.HashSet<System.Int32> initialSecurityViewIds;
                private System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function> initialFunctions;
                private string comments;
                private System.DateTime start;
                private System.DateTime end;
                private System.TimeSpan preRoll;
                private System.TimeSpan postRoll;
                private bool hasCustomPreRoll = false;
                private bool hasCustomPostRoll = false;
                private System.Collections.Generic.Dictionary<System.String, System.Int32> selectableSecurityViewIds;
                private bool isEventLevelReception;
                private bool isGlobalEventLevelReception;
                //private DateTime startWithPreRoll;
                //private DateTime endWithPostRoll;
                public Service()
                {
                    Id = System.Guid.NewGuid();
                    IsBooked = false;
                    Functions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function>();
                    Children = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service>();
                    OrderReferences = new System.Collections.Generic.HashSet<System.Guid>();
                    Interfaces = new System.Collections.Generic.List<Skyline.DataMiner.Net.ServiceManager.Objects.InterfaceConfiguration>();
                    SecurityViewIds = new System.Collections.Generic.HashSet<System.Int32>();
                    UserTasks = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask>();
                }

                public Service(string name): this()
                {
                    this.name = name;
                }

                public Service(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service)
                {
                    // Deep copy using reflection
                    System.Reflection.PropertyInfo[] propertyInfo = typeof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service).GetProperties();
                    foreach (System.Reflection.PropertyInfo property in propertyInfo)
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(this, property.GetValue(service));
                        }
                    }

                    Id = service.Id;
                }

                /// <summary>
                /// Creates a new Service instance based on a ServiceConfiguration.
                /// </summary>
                /// <param name = "nodeLabel"></param>
                /// <param name = "configuration">ServiceConfiguration object defining the Service.</param>
                /// <param name = "helpers"></param>
                /// <param name = "nodeId"></param>
                public Service(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, int nodeId, string nodeLabel, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.ServiceConfiguration configuration)
                {
                    helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), out var stopwatch, Name);
                    Name = configuration.Name;
                    Id = configuration.Id;
                    PreRoll = configuration.PreRoll;
                    Start = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.FromServiceConfiguration(configuration.Start), System.TimeSpan.FromSeconds(1)), System.TimeSpan.FromMilliseconds(1));
                    End = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.FromServiceConfiguration(configuration.End), System.TimeSpan.FromSeconds(1)), System.TimeSpan.FromMilliseconds(1));
                    BackupType = configuration.ServiceLevel.HasValue ? configuration.ServiceLevel.Value : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType.None;
                    LinkedServiceId = configuration.LinkedServiceId;
                    LinkedEventIds = configuration.LinkedEventIds ?? new System.Collections.Generic.HashSet<System.String>();
                    IsEurovisionService = configuration.IsEurovisionService;
                    IsEurovisionMultiFeedService = configuration.IsEurovisionMultiFeedService;
                    EurovisionWorkOrderId = configuration.EurovisionWorkOrderId;
                    EurovisionTransmissionNumber = configuration.EurovisionTransmissionNumber;
                    EurovisionBookingDetails = configuration.EurovisionBookingDetails;
                    EurovisionServiceConfigurations = configuration.EurovisionServiceConfigurations;
                    EurovisionDestinationId = configuration.EurovisionDestinationId;
                    EurovisionTechnicalSystemId = configuration.EurovisionTechnicalSystemId;
                    IntegrationType = configuration.IntegrationType;
                    IntegrationIsMaster = configuration.IntegrationIsMaster;
                    Comments = configuration.Comments;
                    ContactInformationName = configuration.ContactInformationName;
                    ContactInformationTelephoneNumber = configuration.ContactInformationTelephoneNumber;
                    LiveUDeviceName = configuration.LiveUDeviceName;
                    AudioReturnInfo = configuration.AudioReturnInfo;
                    VidigoStreamSourceLink = configuration.VidigoStreamSourceLink;
                    AdditionalDescriptionUnknownSource = configuration.AdditionalDescriptionUnknownSource;
                    IsUnknownSourceService = configuration.IsUnknownSourceService;
                    IsAudioConfigurationCopiedFromSource = configuration.IsAudioConfigurationCopiedFromSource;
                    RequiresRouting = configuration.RequiresRouting;
                    RoutingConfigurationUpdateRequired = configuration.RoutingConfigurationUpdateRequired;
                    RecordingConfiguration = configuration.RecordingConfiguration;
                    IsBooked = false;
                    IsPreliminary = true;
                    NodeId = nodeId;
                    NodeLabel = nodeLabel;
                    Children = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service>();
                    Functions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function>();
                    OrderReferences = configuration.OrderReferences ?? new System.Collections.Generic.HashSet<System.Guid>();
                    UserTasks = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask>();
                    IsEventLevelReception = configuration.IsEventLevelReception;
                    IsGlobalEventLevelReception = configuration.IsGlobalEventLevelReception;
                    HasResourcesAssigned = false;
                    HasAnIssueBeenreportedManually = configuration.HasAnIssueBeenreportedManually;
                    SecurityViewIds = configuration.SecurityViewIds;
                    NameOfServiceToTransmit = configuration.NameOfServiceToTransmit;
                    ChangedByUpdateServiceScript = configuration.ChangedByUpdateServiceScript;
                    InitServiceDefinition(helpers, this, configuration.ServiceDefinitionId, string.Empty, configuration.Functions, nodeId, nodeLabel);
                    helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), $"Summary of properties on service object taken from service configuration: ID={Id} IsELR={IsEventLevelReception}, IsGlobalELR={IsGlobalEventLevelReception}, SecurityViewIds={string.Join(",", SecurityViewIds)}, RequiresRouting={RequiresRouting}, RoutingConfigurationUpdateRequired={RoutingConfigurationUpdateRequired}, HasIssueBeenReportedManually={HasAnIssueBeenreportedManually}, NameOfServiceToRecordOrTransmit={NameOfServiceToTransmitOrRecord}, IntegrationIsMaster={IntegrationIsMaster}, Timing={TimingInfoToString(this)}, ChangedByUpdateServiceScript={ChangedByUpdateServiceScript}", Name);
                    helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), null, stopwatch);
                }

                private static void InitServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, System.Guid serviceDefinitionId, string serviceDefinitionName, System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration> configuredFunctions, int nodeId, string nodeLabel)
                {
                    helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(InitServiceDefinition), out var stopwatch);
                    if (serviceDefinitionId != System.Guid.Empty)
                    {
                        service.Definition = helpers.ServiceDefinitionManager.GetServiceDefinition(serviceDefinitionId);
                        if (service.Definition != null)
                        {
                            // Set Pre- and PostRoll
                            //PreRoll = ServiceManager.GetPreRollDuration(Definition, IntegrationType);
                            service.PostRoll = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.GetPostRollDuration(service.Definition, service.IntegrationType);
                        }

                        if (service.Definition?.FunctionDefinitions != null && configuredFunctions != null)
                        {
                            foreach (var functionDefinition in service.Definition.FunctionDefinitions)
                            {
                                helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), "Handling Function Definition " + functionDefinition.Name, service.Name);
                                var functionNode = System.Linq.Enumerable.FirstOrDefault(service.Definition.Diagram.Nodes, n => n.Label == functionDefinition.Label);
                                if (functionNode == null)
                                    continue;
                                var function = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function{Id = functionDefinition.Id, Name = functionDefinition.Name, NodeId = functionNode.ID, ConfigurationOrder = functionDefinition.ConfigurationOrder, Parameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>(), InterfaceParameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>(), Definition = functionDefinition, IsOptional = functionDefinition.Options != null && functionDefinition.Options.Contains("Optional")};
                                UpdateFunctionFromServiceConfiguration(helpers, configuredFunctions, function, functionDefinition);
                                service.Functions.Add(function);
                                helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), $"Function summary: {function.Configuration.ToString()}", service.Name);
                            }

                            service.AudioChannelConfiguration = GetAudioChannelConfiguration(service.Functions);
                        }
                    }
                    else if (service.IsEurovisionService)
                    {
                        bool hasServiceName = !string.IsNullOrEmpty(service.Name);
                        bool hasServiceDefinitionName = !string.IsNullOrEmpty(serviceDefinitionName);
                        bool serviceNameContainsReception = hasServiceName && service.Name.Contains("Reception");
                        bool serviceNameContainsTransmission = hasServiceName && service.Name.Contains("Transmission");
                        bool serviceDefinitionNameContainsReception = hasServiceDefinitionName && serviceDefinitionName.Contains("Reception");
                        bool serviceDefinitionNameContainsTransmission = hasServiceDefinitionName && serviceDefinitionName.Contains("Transmission");
                        // Eurovision service that was booked from the CustomerUI
                        if (serviceNameContainsReception || serviceDefinitionNameContainsReception)
                        {
                            service.Definition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition.GenerateEurovisionReceptionServiceDefinition();
                        }
                        else if (serviceNameContainsTransmission || serviceDefinitionNameContainsTransmission)
                        {
                            service.Definition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition.GenerateEurovisionTransmissionServiceDefinition();
                        }
                        else
                        {
                            throw new System.InvalidOperationException("Unable to determine the service definition for the following Eurovision Service: " + service.Name);
                        }
                    }
                    else if (service.IsUnknownSourceService)
                    {
                        service.Definition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition.GenerateDummyUnknownReceptionServiceDefinition();
                        service.NodeId = nodeId;
                        service.NodeLabel = nodeLabel;
                    }
                    else
                    {
                        // dummy source
                        service.Definition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition.GenerateDummyReceptionServiceDefinition();
                        service.NodeId = nodeId; // node ID of a main source service should always be 1
                        service.NodeLabel = nodeLabel;
                    }

                    helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(InitServiceDefinition), null, stopwatch);
                }

                private static void UpdateFunctionFromServiceConfiguration(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration> configuredFunctions, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition functionDefinition)
                {
                    if (!configuredFunctions.TryGetValue(functionDefinition.Id, out var functionConfiguration))
                        return;
                    function.RequiresResource = functionConfiguration.RequiresResource;
                    try
                    {
                        function.McrHasOverruledFixedTieLineLogic = functionConfiguration.McrHasOverruledFixedTieLineLogic;
                    }
                    catch
                    {
                        function.McrHasOverruledFixedTieLineLogic = false;
                    }

                    if (functionConfiguration.ResourceId != System.Guid.Empty)
                    {
                        try
                        {
                            function.Resource = helpers.ResourceManagerHelper.GetResource(functionConfiguration.ResourceId) as Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource;
                            helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(UpdateFunctionFromServiceConfiguration), $"Set function {function?.Name} resource to '{function?.Resource?.Name}' based on service configuration");
                        }
                        catch (System.Exception e)
                        {
                            helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(UpdateFunctionFromServiceConfiguration), $"Exception retrieving function {function?.Name} resource '{functionConfiguration.ResourceId}': {e}");
                        }
                    }

                    if (functionDefinition.ProfileDefinition != null)
                    {
                        var parametersToAdd = GetProfileParametersFromConfiguration(helpers, functionConfiguration, function, functionDefinition.ProfileDefinition);
                        function.Parameters.AddRange(parametersToAdd);
                        helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(UpdateFunctionFromServiceConfiguration), $"Added following profile parameters from configuration to function {function.Name}: '{string.Join(", ", System.Linq.Enumerable.Select(parametersToAdd, p => $"{p.Name}={p.Value}"))}'");
                    }

                    if (functionDefinition.InterfaceProfileDefinitions != null && System.Linq.Enumerable.Any(functionDefinition.InterfaceProfileDefinitions))
                    {
                        foreach (var interfaceProfileDefinition in functionDefinition.InterfaceProfileDefinitions.Values)
                        {
                            var parametersToAdd = GetProfileParametersFromConfiguration(helpers, functionConfiguration, function, interfaceProfileDefinition);
                            function.InterfaceParameters.AddRange(parametersToAdd);
                            helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(UpdateFunctionFromServiceConfiguration), $"Added following interface profile parameters from configuration to function {function.Name}: '{string.Join(", ", System.Linq.Enumerable.Select(parametersToAdd, p => $"{p.Name}={p.Value}"))}'");
                        }
                    }

                    helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(UpdateFunctionFromServiceConfiguration), $"Note: function object should contain all of its profile parameters now.");
                }

                private static System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> GetProfileParametersFromConfiguration(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration configuration, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileDefinition profileDefinition)
                {
                    var parameters = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    foreach (var profileDefinitionParameter in profileDefinition.ProfileParameters)
                    {
                        bool parameterIsAlreadyAdded = System.Linq.Enumerable.Any(System.Linq.Enumerable.Concat(function.Parameters, function.InterfaceParameters), p => p.Id == profileDefinitionParameter.Id);
                        if (parameterIsAlreadyAdded)
                            continue;
                        var functionProfileParameter = helpers.ProfileManager.GetProfileParameter(profileDefinitionParameter.Id);
                        if (configuration.ProfileParameters != null)
                        {
                            var serviceConfigurationProfileParameter = System.Linq.Enumerable.FirstOrDefault(configuration.ProfileParameters, p => p.Key == functionProfileParameter.Id);
                            functionProfileParameter.Value = serviceConfigurationProfileParameter.Value;
                        }

                        parameters.Add(functionProfileParameter);
                    }

                    return parameters;
                }

                /// <summary>
                /// Reservation instance of the service.
                /// </summary>
                public System.Guid Id
                {
                    get;
                    set;
                }

                /// <summary>
                /// Name of the order that uses this service.
                /// Currently used for recording services
                /// </summary>
                public string OrderName
                {
                    get;
                    set;
                }

                /// <summary>
                /// The name of the service.
                /// </summary>
                public string Name
                {
                    get
                    {
                        if (name == null && Definition != null)
                        {
                            // the id used here is not the id of the reservation itself
                            // this id is generated when the service object itself is created initially
                            // the name of a contributed service cannot be changed afterwards and is only known after booking the reservation
                            // this id is included in the name because the name needs to be unique
                            name = $"{Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceType)} [{Id}]";
                        }

                        return name;
                    }

                    internal set
                    {
                        name = value;
                    }
                }

                /// <summary>
                /// Start date and time of the service.
                /// </summary>
                public System.DateTime Start
                {
                    get
                    {
                        return start;
                    }

                    set
                    {
                        if (start != value)
                        {
                            if (start == default(System.DateTime))
                                initialStart = value;
                            start = value;
                            StartChanged?.Invoke(this, start);
                            StartWithPreRollChanged?.Invoke(this, StartWithPreRoll);
                        }
                    }
                }

                /// <summary>
                /// Property set by controller and used by UI for validation.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo StartValidation
                {
                    get => startValidation ?? (startValidation = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo());
                    set => startValidation = value;
                }

                internal event System.EventHandler<System.DateTime> StartChanged;
                public System.DateTime StartWithPreRoll => Start.Subtract(PreRoll);
                internal event System.EventHandler<System.DateTime> StartWithPreRollChanged;
                /// <summary>
                /// End date and time of the service.
                /// </summary>
                public System.DateTime End
                {
                    get => end;
                    set
                    {
                        if (end != value)
                        {
                            if (end == default(System.DateTime))
                                initialEnd = value;
                            end = value;
                            EndChanged?.Invoke(this, end);
                            EndWithPostRollChanged?.Invoke(this, EndWithPostRoll);
                        }
                    }
                }

                internal event System.EventHandler<System.DateTime> EndChanged;
                public System.DateTime EndWithPostRoll => End.Add(PostRoll);
                internal event System.EventHandler<System.DateTime> EndWithPostRollChanged;
                public System.TimeSpan PreRoll
                {
                    get => hasCustomPreRoll ? preRoll : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.GetPreRollDuration(Definition, IntegrationType);
                    internal set
                    {
                        hasCustomPreRoll = true;
                        preRoll = value;
                    }
                }

                public System.TimeSpan PostRoll
                {
                    get => hasCustomPostRoll ? postRoll : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.GetPostRollDuration(Definition, IntegrationType);
                    internal set
                    {
                        hasCustomPostRoll = true;
                        postRoll = value;
                    }
                }

                /// <summary>
                /// The status of this service.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status Status => GenerateStatus();
                /// <summary>
                /// Indicates if this Service is saved or not. Used to determine the correct Service Status.
                /// </summary>
                public bool IsPreliminary
                {
                    get;
                    set;
                }

                /// <summary>
                /// The list of orders ids this service is used in.
                /// For now this doesn't contain the order itself to avoid having to retrieve all orders each time we retrieve a service.
                /// Could be improved in the future.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Guid> OrderReferences
                {
                    get;
                    set;
                }

                /// <summary>
                /// The service definition for the service.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition Definition
                {
                    get;
                    set;
                }

                /// <summary>
                /// The reservation which is retrieved during FromReservationInstance() is stored in this property.
                /// </summary>
                public Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance ReservationInstance
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The service level (backup) for the service.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType BackupType
                {
                    get;
                    set;
                }

                /// <summary>
                /// The list of functions in this service.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function> Functions
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets the configuration of the Audio Channel Profile Parameters.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelConfiguration AudioChannelConfiguration
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Is set to true when a destination or recording contains an audio configuration that is copied from source.
                /// </summary>
                public bool IsAudioConfigurationCopiedFromSource
                {
                    get;
                    set;
                }

                /// <summary>
                /// The comments for the service.
                /// </summary>
                public string Comments
                {
                    get => comments;
                    set
                    {
                        comments = value;
                        CommentsChanged?.Invoke(this, comments);
                    }
                }

                internal event System.EventHandler<System.String> CommentsChanged;
                /// <summary>
                /// The list of child services for the service.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service> Children
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates which integration created this Service.
                /// If not specified this will be set to None.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType IntegrationType
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a boolean indicating if the related integration is master or if DataMiner is the master.
                /// Used to determine which updates from which sources are allowed.
                /// Always false for manually created services.
                /// </summary>
                public bool IntegrationIsMaster
                {
                    get;
                    set;
                }

                = false;
                /// <summary>
                /// The resource linked to this service.
                /// This is an internal property.
                /// </summary>
                public Skyline.DataMiner.Net.Messages.Resource ContributingResource
                {
                    get;
                    set;
                }

                /// <summary>
                /// The resource pool for the resource linked to the service (can be found in Contributing Config property of Service Definition).
                /// This is an internal property.
                /// </summary>
                public Skyline.DataMiner.Net.Messages.ResourcePool ResourcePool
                {
                    get;
                    set;
                }

                /// <summary>
                /// The node id for this service in the order service definition.
                /// This is an internal property.
                /// </summary>
                public int NodeId
                {
                    get;
                    set;
                }

                /// <summary>
                /// The node label for this service in the order service definition.
                /// </summary>
                public string NodeLabel
                {
                    get;
                    set;
                }

                /// <summary>
                /// The available interfaces for this service in the order service definitions.
                /// This is an internal property.
                /// </summary>
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.ServiceManager.Objects.InterfaceConfiguration> Interfaces
                {
                    get;
                    set;
                }

                /// <summary>
                /// The id of the event in which this service can be used as an Event Level Reception.
                /// This is an internal property.
                /// </summary>
                public System.Collections.Generic.HashSet<System.String> LinkedEventIds
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this service can be used as an Event Level Reception.
                /// This is an internal property.
                /// </summary>
                public bool IsEventLevelReception
                {
                    get => isEventLevelReception;
                    set
                    {
                        if (value == IsEventLevelReception)
                            return;
                        isEventLevelReception = value;
                        IsEventLevelReceptionChanged?.Invoke(this, IsEventLevelReception);
                    }
                }

                public event System.EventHandler<System.Boolean> IsEventLevelReceptionChanged;
                /// <summary>
                /// Indicates if this service can be used as an global Event Level Reception.
                /// This is an internal property.
                /// </summary>
                public bool IsGlobalEventLevelReception
                {
                    get => isGlobalEventLevelReception;
                    set
                    {
                        if (value == IsGlobalEventLevelReception)
                            return;
                        isGlobalEventLevelReception = value;
                        IsGlobalEventLevelReceptionChanged?.Invoke(this, IsGlobalEventLevelReception);
                    }
                }

                public event System.EventHandler<System.Boolean> IsGlobalEventLevelReceptionChanged;
                /// <summary>
                /// Gets a boolean indicating if this service is already booked as ELR.
                /// </summary>
                public bool IsAlreadyBookedAsEventLevelReception => IsBooked && IsEventLevelReception && System.Linq.Enumerable.Any(LinkedEventIds);
                /// <summary>
                /// Gets a boolean indicating if this service is already booked as a global ELR.
                /// </summary>
                public bool IsAlreadyBookedAsGlobalEventLevelReception => IsBooked && IsGlobalEventLevelReception && System.Linq.Enumerable.Any(LinkedEventIds);
                /// <summary>
                /// Indicates if this service has any Resources assigned to it.
                /// </summary>
                public bool HasResourcesAssigned
                {
                    get;
                    set;
                }

                /// <summary>
                /// It only applies to Child Services (Destination/Recording) of a backup source when the Service Level of the Backup is Active.
                /// This is the Id of the Service to which this service is linked.
                /// </summary>
                public System.Guid LinkedServiceId
                {
                    get;
                    set;
                }

                /// <summary>
                /// Should be passed from the UI to the Library when a new Order is created where the Backup source is configured as Active backup.
                /// This property links the Source child service to it backup counterpart and vice versa.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service LinkedService
                {
                    get;
                    set;
                }

                /// <summary>
                /// Should be passed from the UI to the Library when an order is created.
                /// This property contains the type of the order this service is part of.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderType OrderType
                {
                    get;
                    set;
                }

                /// <summary>
                /// A list of User Tasks linked to this service.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask> UserTasks
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this is an Eurovision service.
                /// </summary>
                public bool IsEurovisionService
                {
                    get;
                    set;
                }

                /// <summary>
                /// This is set to true from the HandleIntegrationUpdate script if the synopsis contains a value for the ioMuxName field.
                /// The ioMuxName field is used to define multiple multiplexed feeds in the signal.
                /// If this boolean is true, you will be able to edit the Service Selection profile parameter of the Satellite Reception in the LiveOrderForm.
                /// </summary>
                public bool IsEurovisionMultiFeedService
                {
                    get;
                    set;
                }

                /// <summary>
                /// The id of the Eurovision work order that was requested through the Live Order Form.
                /// </summary>
                public string EurovisionWorkOrderId
                {
                    get;
                    set;
                }

                /// <summary>
                /// The transmission number of the Eurovision booking in case of a Eurovision service.
                /// This can be used to map the incoming Eurovision synopsis to this service.
                /// This is only applicable if this is a Eurovision service and if it was automatically created by the Eurovision integration.
                /// </summary>
                public string EurovisionTransmissionNumber
                {
                    get;
                    set;
                }

                /// <summary>
                /// Only used in the CustomerUI script.
                /// This value is not stored or retrieved from the SRM configuration.
                /// Used to determine if the Resource assigned to this Service should be included when updating the available Resources in the script.
                /// </summary>
                public bool IsDuplicate
                {
                    get;
                    set;
                }

                = false;
                /// <summary>
                /// Serialized data of the Eurovision booking details as configured in the Customer UI.
                /// This is only applicable in case of an Eurovision service.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.EurovisionBookingDetails EurovisionBookingDetails
                {
                    get;
                    set;
                }

                /// <summary>
                /// This property is only used when the Service is a Dummy Service that was generated from the EBU integration.
                /// In that case this property will contain the possible Receptions or Transmissions.
                /// </summary>
                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.ServiceConfiguration> EurovisionServiceConfigurations
                {
                    get;
                    set;
                }

                /// <summary>
                /// This property is only used when the Service is a Reception Service that was generated by the EBU integration.
                /// In that case this property will contain the id of the destination in the synopsis on which this service is based. 
                /// </summary>
                [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                public string EurovisionDestinationId
                {
                    get;
                    set;
                }

                /// <summary>
                /// This property is only used when the Service is a Reception or Transmission Service that was generated by the EBU integration.
                /// In that case this property will contain the id of the technical system in the synopsis on which this service is based. 
                /// </summary>
                [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                public string EurovisionTechnicalSystemId
                {
                    get;
                    set;
                }

                /// <summary>
                /// Whenever an order contains an unknown source the user is able to specify additional information 
                /// So that a user in a later stage can select the desired source whenever they want to book the order definitively.
                /// </summary>
                public string AdditionalDescriptionUnknownSource
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this service is unknown.
                /// </summary>
                public bool IsUnknownSourceService
                {
                    get;
                    set;
                }

                /// <summary>
                /// Contact Information Name.
                /// Only applicable for LiveU receptions.
                /// Will be saved in a custom property for LiveU receptions;
                /// </summary>
                public string ContactInformationName
                {
                    get;
                    set;
                }

                /// <summary>
                /// Contact Information Telephone Number.
                /// Only applicable for LiveU receptions.
                /// Will be saved in a custom property for LiveU receptions;
                /// </summary>
                public string ContactInformationTelephoneNumber
                {
                    get;
                    set;
                }

                /// <summary>
                /// Additional information reception/transmission LiveU device name.
                /// Only applicable for LiveU receptions or transmissions.
                /// Will be saved in a custom property for LiveU receptions or transmissions;
                /// </summary>
                public string LiveUDeviceName
                {
                    get;
                    set;
                }

                /// <summary>
                /// Normal users will fill in this field when LiveU is selected
                /// News users will be able to fill in this field for every reception service.
                /// </summary>
                public string AudioReturnInfo
                {
                    get;
                    set;
                }

                /// <summary>
                /// Additional info for IP RX Vidigo services.
                /// Will be saved in custom property on IP receptions.
                /// </summary>
                public string VidigoStreamSourceLink
                {
                    get;
                    set;
                }

                /// <summary>
                /// Contains additional recording configurations that are not stored as profile parameters.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration RecordingConfiguration
                {
                    get;
                    set;
                }

                public string NameOfServiceToTransmit
                {
                    get;
                    set;
                }

                public string NameOfServiceToTransmitOrRecord
                {
                    get
                    {
                        if (Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording)
                            return RecordingConfiguration?.NameOfServiceToRecord;
                        else if (Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Transmission)
                            return NameOfServiceToTransmit;
                        else
                            return string.Empty;
                    }

                    set
                    {
                        if (Definition?.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && RecordingConfiguration != null)
                            RecordingConfiguration.NameOfServiceToRecord = value;
                        else if (Definition?.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Transmission)
                            NameOfServiceToTransmit = value;
                    }
                }

                /// <summary>
                /// Indicates if routing is required for this service.
                /// </summary>
                public bool RequiresRouting
                {
                    get;
                    set;
                }

                = true;
                /// <summary>
                /// Indicates if routing config should be updated whenever applicable.
                /// </summary>
                public bool RoutingConfigurationUpdateRequired
                {
                    get;
                    set;
                }

                = true;
                /// <summary>
                /// Indicates if this Service is a Dummy Service.
                /// </summary>
                public bool IsDummy => Definition == null || Definition.IsDummy;
                /// <summary>
                /// Indicates if the Service is a Dummy reception service that was generated by EBU Integration.
                /// </summary>
                public bool IsEbuDummyReception
                {
                    get
                    {
                        if (Definition == null)
                            return false;
                        return IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Eurovision && Definition.IsDummy && Definition.ContributingConfig != null && Definition.ContributingConfig.ParentSystemFunction == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.SourceServiceSystemFunctionId;
                    }
                }

                /// <summary>
                /// Indicates if the Service is a Dummy transmission service that was generated by EBU Integration.
                /// </summary>
                public bool IsEbuDummyTransmission
                {
                    get
                    {
                        if (Definition == null)
                            return false;
                        return IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Eurovision && Definition.IsDummy && Definition.ContributingConfig != null && Definition.ContributingConfig.ParentSystemFunction == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.TransmissionServiceSystemFunctionId;
                    }
                }

                public bool IsBooked
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets a collection of DataMiner Cube view IDs of views where this service is visible. If empty, this service should be visible for everyone.
                /// </summary>
                public System.Collections.Generic.HashSet<System.Int32> SecurityViewIds
                {
                    get;
                    set;
                }

                = new System.Collections.Generic.HashSet<System.Int32>();
                /// <summary>
                /// Gets a collection of DataMiner Cube view IDs of views that are selected in the event of which this service is part of. Used by UI.
                /// </summary>
                public System.Collections.Generic.Dictionary<System.String, System.Int32> SelectableSecurityViewIds
                {
                    get => selectableSecurityViewIds;
                    set
                    {
                        selectableSecurityViewIds = value;
                        SelectableSecurityViewIdsChanged?.Invoke(this, SelectableSecurityViewIds);
                    }
                }

                public event System.EventHandler<System.Collections.Generic.Dictionary<System.String, System.Int32>> SelectableSecurityViewIdsChanged;
                public string DisplayName
                {
                    get;
                    set;
                }

                /// <summary>
                /// A boolean used to skip certain logic because the user input from the UpdateService script should overwrite it
                /// </summary>
                public bool ChangedByUpdateServiceScript
                {
                    get;
                    set;
                }

                = false;
                /// <summary>
                /// Gets a boolean indicating if this object has been changed since object construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool IsChanged => ChangeInfo.IsChanged;
                /// <summary>
                /// Gets a boolean indicating if Change Tracking is enabled.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                [Newtonsoft.Json.JsonIgnore]
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets an object containing all changes made since object construction or since last <see cref = "AcceptChanges"/> call. 
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                /// <summary>
                /// Gets a boolean indicating if service is backup.
                /// Used by UI.
                /// </summary>
                public bool UI_IsBackupService
                {
                    get => BackupType != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType.None;
                }

                /// <summary>
                /// Gets a boolean indicating if service has commentary audio.
                /// Used by UI.
                /// </summary>
                public bool UI_HasCommentaryAudio
                {
                    get => false;
                } // TODO implement when audio commentary is supported.

                /// <summary>
                /// If an issue has been reported manually via the UpdateService script and when the order is finished.
                /// The service will have the status Completed With Errors at the end.
                /// </summary>
                public bool HasAnIssueBeenreportedManually
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates whether the Service should stop immediately.
                /// </summary>
                public bool StopNow
                {
                    get;
                    set;
                }

                = false;
                /// <summary>
                /// Indicates if the service was automatically generated.
                /// This is determined by checking the Virtual Platform of the Service Definition.
                /// </summary>
                public bool IsAutogenerated
                {
                    get
                    {
                        if (Definition == null)
                            return false;
                        if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Routing)
                            return true;
                        if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.AudioProcessing)
                            return true;
                        if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.GraphicsProcessing)
                            return true;
                        if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.FileProcessing)
                            return true;
                        if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.VideoProcessing)
                            return true;
                        return false;
                    }
                }

                /// <summary>
                /// Gets the types of changes that this service underwent since construction or since last <see cref = "AcceptChanges"/> call.
                /// </summary>
                /// <returns>A flags enum containing the types of changes that this service underwent.</returns>
                /// <see cref = "IYleChangeTracking"/>
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    progressReporter?.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GetChanges), Name);
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    bool startIsChanged = Start != initialStart;
                    //bool startWithPreRollIsChanged = StartWithPreRoll != initialStartWithPreRoll;
                    //if (startIsChanged || startWithPreRollIsChanged) changeInfo.TimingChangeInfo.MarkStartTimingChanged();
                    if (startIsChanged)
                        changeInfo.TimingChangeInfo.MarkStartTimingChanged();
                    bool endIsChanged = End != initialEnd;
                    //bool endWithPostRollIsChanged = EndWithPostRoll != initialEndWithPostRoll;
                    //if (endIsChanged || endWithPostRollIsChanged) changeInfo.TimingChangeInfo.MarkEndTimingChanged();
                    if (endIsChanged)
                        changeInfo.TimingChangeInfo.MarkEndTimingChanged();
                    bool timingHasChanged = startIsChanged || endIsChanged;
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GetChanges), $"Timing has{(timingHasChanged ? string.Empty : " not")} changed from {TimingInfoToString(initialStart, initialEnd)}{(timingHasChanged ? $" to {TimingInfoToString(this)}" : string.Empty)}", Name);
                    if (RecordingConfiguration?.ChangeInfo != null)
                        changeInfo.CombineWith(RecordingConfiguration.ChangeInfo);
                    bool integrationIsMasterChanged = IntegrationIsMaster != initialIntegrationIsMaster;
                    bool contactInfoIsChanged = ContactInformationName != initialContactInfoName;
                    bool contactInfoPhoneIsChanged = ContactInformationTelephoneNumber != initialContactInfoPhone;
                    bool liveUDeviceNameIsChanged = LiveUDeviceName != initialLiveUDeviceName;
                    bool audioReturnInfoIsChanged = AudioReturnInfo != initialAudioReturnInfo;
                    bool vidigoStreamSourceLinkIsChanged = VidigoStreamSourceLink != initialVidigoStreamSourceLink;
                    if (integrationIsMasterChanged || contactInfoIsChanged || contactInfoPhoneIsChanged || liveUDeviceNameIsChanged || vidigoStreamSourceLinkIsChanged)
                        changeInfo.MarkCustomPropertiesChanged();
                    bool commentsChanged = Comments != initialComments;
                    if (commentsChanged)
                        changeInfo.MarkCustomPropertiesChanged();
                    bool securityViewIdsAdded = initialSecurityViewIds == null && SecurityViewIds != null;
                    bool securityViewIdsRemoved = initialSecurityViewIds != null && SecurityViewIds == null;
                    bool securityViewIdsValuesChanged = initialSecurityViewIds != null && SecurityViewIds != null && !System.Linq.Enumerable.SequenceEqual(System.Linq.Enumerable.OrderBy(initialSecurityViewIds, x => x), System.Linq.Enumerable.OrderBy(SecurityViewIds, x => x));
                    bool securityViewIdsChanged = securityViewIdsAdded || securityViewIdsRemoved || securityViewIdsValuesChanged;
                    if (securityViewIdsChanged)
                    {
                        changeInfo.SecurityViewIdsChanged = true;
                    }

                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GetChanges), $"Security View IDs have{(securityViewIdsChanged ? string.Empty : " not")} changed from {(initialSecurityViewIds == null ? "empty" : string.Join(";", initialSecurityViewIds))}{(securityViewIdsChanged ? $" to {string.Join(";", SecurityViewIds)}" : string.Empty)}", Name);
                    // TODO add eurovision booking details change check
                    if (Functions == null)
                        return changeInfo;
                    if (initialFunctions != null && Functions.Count != initialFunctions.Count)
                        changeInfo.MarkServiceDefinitionChanged();
                    foreach (var function in Functions)
                    {
                        if (initialFunctions != null)
                        {
                            var initialFunction = System.Linq.Enumerable.FirstOrDefault(initialFunctions, f => f.Id == function.Id);
                            if (initialFunction == null)
                            {
                                changeInfo.MarkServiceDefinitionChanged();
                                continue;
                            }
                        }

                        var functionChangeInfo = function.ChangeInfo;
                        changeInfo.CombineWith(functionChangeInfo);
                        bool functionResourceHasChanged = functionChangeInfo.ResourceChangeInfo.ResourcesChanged;
                        if (!functionResourceHasChanged)
                            continue;
                        int functionPositionInServiceDefinition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.GraphExtensions.GetFunctionPosition(Definition.Diagram, function);
                        bool functionIsAtBeginningOfServiceDefinition = functionPositionInServiceDefinition == 0;
                        bool functionIsAtEndOfServiceDefinition = functionPositionInServiceDefinition == Definition.Diagram.Nodes.Count - 1;
                        if (functionIsAtBeginningOfServiceDefinition)
                            changeInfo.ResourceChangeInfo.MarkResourceAtBeginningOfServiceDefinitionAddedOrSwapped();
                        if (functionIsAtEndOfServiceDefinition)
                            changeInfo.ResourceChangeInfo.MarkResourceAtEndOfServiceDefinitionAddedOrSwapped();
                    }

                    progressReporter?.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GetChanges), Name);
                    return changeInfo;
                }

                /// <summary>
                /// Resets Change Tracking.
                /// </summary>
                /// <see cref = "IYleChangeTracking"/>
                public void AcceptChanges()
                {
                    initialStart = Start;
                    initialEnd = End;
                    //initialStartWithPreRoll = StartWithPreRoll;
                    //initialEndWithPostRoll = EndWithPostRoll;
                    initialRecordingConfiguration = RecordingConfiguration;
                    RecordingConfiguration?.AcceptChanges();
                    initialIntegrationIsMaster = IntegrationIsMaster;
                    initialContactInfoName = ContactInformationName;
                    initialContactInfoPhone = ContactInformationTelephoneNumber;
                    initialVidigoStreamSourceLink = VidigoStreamSourceLink;
                    initialLiveUDeviceName = LiveUDeviceName;
                    initialAudioReturnInfo = AudioReturnInfo;
                    initialSecurityViewIds = new System.Collections.Generic.HashSet<System.Int32>(SecurityViewIds);
                    initialComments = Comments;
                    foreach (var function in Functions)
                        function.AcceptChanges();
                    initialFunctions = Functions;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Service other))
                        return false;
                    return Id.Equals(other.Id);
                // TODO saved services have no ID and their names can change when editing/merging an Order, so this Equals method does not work in that case
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }

                /// <summary>
                /// Get the service for a give reservation instance.
                /// </summary>
                /// <param name = "helpers"></param>
                /// <param name = "reservationInstance">The reservation instance.</param>
                /// <returns>The service object.</returns>
                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service FromReservationInstance(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    if (helpers == null)
                        throw new System.ArgumentNullException(nameof(helpers));
                    if (reservationInstance == null)
                        throw new System.ArgumentNullException(nameof(reservationInstance));
                    helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(FromReservationInstance), out var stopwatch, reservationInstance.Name);
                    var booking = Skyline.DataMiner.Library.Solutions.SRM.ReservationInstanceExtensions.GetBookingData(reservationInstance);
                    var service = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service(booking.Description)
                    {Children = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service>(), Functions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function>(), UserTasks = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask>(), Id = reservationInstance.ID, PreRoll = Skyline.DataMiner.Library.Solutions.SRM.ReservationInstanceExtensions.GetPreRoll(reservationInstance), PostRoll = Skyline.DataMiner.Library.Solutions.SRM.ReservationInstanceExtensions.GetPostRoll(reservationInstance)};
                    var convertedStartTime = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.FromReservation(reservationInstance.Start), System.TimeSpan.FromMinutes(1));
                    var convertedEndTime = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.Truncate(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions.FromReservation(reservationInstance.End), System.TimeSpan.FromMinutes(1));
                    service.Start = convertedStartTime.Add(service.PreRoll);
                    service.End = convertedEndTime.Subtract(service.PostRoll);
                    service.IsBooked = true;
                    service.BackupType = GetServiceLevel(reservationInstance);
                    service.LinkedEventIds = GetLinkedEventIds(reservationInstance) ?? new System.Collections.Generic.HashSet<System.String>();
                    service.IntegrationType = GetIntegrationType(reservationInstance);
                    service.IntegrationIsMaster = GetBooleanProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.IntegrationIsMasterPropertyName);
                    service.IsEventLevelReception = GetBooleanProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.IsEventLevelReceptionPropertyName);
                    service.IsGlobalEventLevelReception = GetBooleanProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName);
                    if (service.IsEventLevelReception || service.IsGlobalEventLevelReception)
                        service.VerifyContributingResourceCreation();
                    service.HasResourcesAssigned = System.Linq.Enumerable.Any(reservationInstance.ResourcesInReservationInstance);
                    service.OrderReferences = GetOrderReferences(reservationInstance);
                    service.SecurityViewIds = new System.Collections.Generic.HashSet<System.Int32>(reservationInstance.SecurityViewIDs);
                    var serviceResourceReservationInstance = reservationInstance as Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance;
                    if (serviceResourceReservationInstance == null)
                        throw new System.ArgumentException($"Unable to cast to {nameof(Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance)}", nameof(reservationInstance));
                    var serviceDefinition = helpers.ServiceDefinitionManager.GetServiceDefinition(serviceResourceReservationInstance.ServiceDefinitionID);
                    service.Definition = serviceDefinition;
                    service.Functions = helpers.ServiceManager.GetFunctions(serviceResourceReservationInstance, serviceDefinition);
                    service.LinkedServiceId = GetLinkedServiceId(serviceResourceReservationInstance);
                    service.AudioChannelConfiguration = GetAudioChannelConfiguration(service.Functions);
                    service.EurovisionWorkOrderId = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.EurovisionIdPropertyName);
                    service.EurovisionTransmissionNumber = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.EurovisionTransmissionNumberPropertyName);
                    service.EurovisionBookingDetails = GetEurovisionBookingDetails(reservationInstance);
                    if (!string.IsNullOrEmpty(service.EurovisionWorkOrderId) || service.EurovisionBookingDetails != null)
                        service.IsEurovisionService = true;
                    service.ContactInformationName = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.ContactInformationNamePropertyName);
                    service.ContactInformationTelephoneNumber = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.ContactInformationTelephoneNumberPropertyName);
                    service.VidigoStreamSourceLink = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.VidigoStreamSourceLinkPropertyName);
                    service.HasAnIssueBeenreportedManually = GetBooleanProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.ReportedIssuePropertyName);
                    if (serviceDefinition.VirtualPlatformServiceName == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName.LiveU)
                    {
                        service.LiveUDeviceName = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.LiveUDeviceNamePropertyName);
                    }

                    service.AudioReturnInfo = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.AudioReturnInfoPropertyName);
                    service.RecordingConfiguration = GetRecordingConfiguration(reservationInstance);
                    service.NameOfServiceToTransmit = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.NameOfServiceToTransmitPropertyName);
                    service.UserTasks = System.Linq.Enumerable.ToList(helpers.UserTaskManager.GetUserTasks(service));
                    service.isCancelled = GetStatus(serviceResourceReservationInstance) == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.Cancelled;
                    service.Comments = GetStringProperty(reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.CommentsPropertyName);
                    service.ReservationInstance = serviceResourceReservationInstance;
                    helpers.ProgressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(FromReservationInstance), $"Summary of properties on service object taken from reservation instance: ID={service.Id} IsELR={service.IsEventLevelReception} IsGlobalELR={service.IsGlobalEventLevelReception}, SecurityViewIds={string.Join(",", service.SecurityViewIds)}, RequiresRouting={service.RequiresRouting}, RoutingConfigurationUpdateRequired={service.RoutingConfigurationUpdateRequired}, HasIssueBeenReportedManually={service.HasAnIssueBeenreportedManually}, NameOfServiceToTransmitOrRecord={service.NameOfServiceToTransmitOrRecord}, IntegrationIsMaster={service.IntegrationIsMaster}, Timing={TimingInfoToString(service)}", service.Name);
                    helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(FromReservationInstance), reservationInstance.Name, stopwatch);
                    return service;
                }

                private static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType GetServiceLevel(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.ServiceLevelPropertyName, out var serviceLevel))
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceReservationPropertyNotFoundException(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.ServiceLevelPropertyName, reservationInstance.Name);
                    }

                    return (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType)System.Convert.ToInt32(serviceLevel);
                }

                private static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType GetIntegrationType(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    object integrationType;
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.IntegrationTypePropertyName, out integrationType))
                    {
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.None;
                    }

                    return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType>(System.Convert.ToString(integrationType));
                }

                public static bool GetBooleanProperty(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservation, string propertyName)
                {
                    string propertyValue = GetStringProperty(reservation, propertyName);
                    return !string.IsNullOrWhiteSpace(propertyValue) && System.Convert.ToBoolean(propertyValue);
                }

                public static string GetStringProperty(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservation, string propertyName)
                {
                    if (reservation == null)
                        throw new System.ArgumentNullException(nameof(reservation));
                    if (string.IsNullOrWhiteSpace(propertyName))
                        throw new System.ArgumentNullException(nameof(propertyName));
                    return reservation.Properties.Dictionary.TryGetValue(propertyName, out object propertyValue) ? System.Convert.ToString(propertyValue) : null;
                }

                private static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration GetRecordingConfiguration(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    object recordingConfigurationPropertyValue;
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.RecordingConfigurationPropertyName, out recordingConfigurationPropertyValue))
                    {
                        return null;
                    }

                    try
                    {
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration.Deserialize(System.Convert.ToString(recordingConfigurationPropertyValue));
                    }
                    catch (System.Exception)
                    {
                        return null;
                    }
                }

                private static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status GetStatus(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    object status;
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.StatusPropertyName, out status))
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceReservationPropertyNotFoundException(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.StatusPropertyName, reservationInstance.Name);
                    }

                    return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status>(System.Convert.ToString(status));
                }

                private static System.Guid GetLinkedServiceId(Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance reservationInstance)
                {
                    var linkedServiceIdProperty = System.Linq.Enumerable.FirstOrDefault(reservationInstance.Properties, p => string.Equals(p.Key, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.LinkedServiceIdPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    if (linkedServiceIdProperty.Equals(default(System.Collections.Generic.KeyValuePair<System.String, System.Object>)) || linkedServiceIdProperty.Value == null)
                        return System.Guid.Empty;
                    System.Guid linkedServiceId;
                    if (!System.Guid.TryParse(System.Convert.ToString(linkedServiceIdProperty.Value), out linkedServiceId))
                        return System.Guid.Empty;
                    return linkedServiceId;
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelConfiguration GetAudioChannelConfiguration(System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function> functions)
                {
                    System.Collections.Generic.HashSet<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter> audioChannelProfileParameters = new System.Collections.Generic.HashSet<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter>();
                    foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function in functions)
                    {
                        foreach (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileParameter parameter in function.Parameters)
                        {
                            if (parameter == null)
                                continue;
                            if (System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AllAudioChannelConfigurationGuids, parameter.Id))
                            {
                                audioChannelProfileParameters.Add(parameter);
                            }
                        }
                    }

                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.AudioChannelConfiguration(audioChannelProfileParameters);
                }

                private static System.Collections.Generic.HashSet<System.Guid> GetOrderReferences(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservation)
                {
                    var orders = new System.Collections.Generic.HashSet<System.Guid>();
                    var orderIdsProperty = System.Linq.Enumerable.FirstOrDefault(reservation.Properties, p => string.Equals(p.Key, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.OrderReferencesPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    if (orderIdsProperty.Equals(default(System.Collections.Generic.KeyValuePair<System.String, System.Object>)) || System.Convert.ToString(orderIdsProperty.Value) == string.Empty)
                    {
                        return orders;
                    }

                    try
                    {
                        var orderIds = System.Linq.Enumerable.Select(System.Convert.ToString(orderIdsProperty.Value).Split(';'), id => System.Guid.Parse(id));
                        foreach (var orderId in orderIds)
                        {
                            if (orderId == System.Guid.Empty)
                                continue;
                            orders.Add(orderId);
                        }
                    }
                    catch (System.Exception)
                    {
                        return orders;
                    }

                    return orders;
                }

                internal static System.Collections.Generic.HashSet<System.String> GetLinkedEventIds(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservation)
                {
                    var eventIds = new System.Collections.Generic.HashSet<System.String>();
                    var linkedEventIdsProperty = System.Linq.Enumerable.FirstOrDefault(reservation.Properties, p => string.Equals(p.Key, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.LinkedEventIdPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    if (linkedEventIdsProperty.Equals(default(System.Collections.Generic.KeyValuePair<System.String, System.Object>)) || System.Convert.ToString(linkedEventIdsProperty.Value) == string.Empty)
                    {
                        return eventIds;
                    }

                    try
                    {
                        var linkedEventIds = System.Convert.ToString(linkedEventIdsProperty.Value).Split(';');
                        foreach (var eventId in linkedEventIds)
                        {
                            if (eventId == string.Empty)
                                continue;
                            eventIds.Add(eventId);
                        }
                    }
                    catch (System.Exception)
                    {
                        return eventIds;
                    }

                    return eventIds;
                }

                private static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.EurovisionBookingDetails GetEurovisionBookingDetails(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance reservationInstance)
                {
                    object eurovisionBookingDetailsPropertyValue;
                    if (!reservationInstance.Properties.Dictionary.TryGetValue(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ServicePropertyNames.EurovisionBookingDetailsPropertyName, out eurovisionBookingDetailsPropertyValue))
                    {
                        return null;
                    }

                    var eurovisionBookingDetails = System.Convert.ToString(eurovisionBookingDetailsPropertyValue);
                    if (string.IsNullOrEmpty(eurovisionBookingDetails))
                    {
                        return null;
                    }

                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.EurovisionBookingDetails>(eurovisionBookingDetails);
                    }
                    catch (System.Exception)
                    {
                        return null;
                    }
                }

                /// <summary>
                /// Checks if resources are assigned to the Functions that should have resources assigned. 
                /// </summary>
                /// <remarks>
                /// Due to the way the standard SRM solution works, all Functions in the Service Definitions have been marked optional.
                /// Therefore this method is used to see if all Functions that require a resource have a resource assigned.
                /// </remarks>
                /// <returns>A boolean indicating if all resources are correctly assigned.</returns>
                private bool VerifyFunctionResources(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    if (Definition == null)
                        return false;
                    if (Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.AudioProcessing)
                    {
                        return VerifyAudioProcessingFunctionResources();
                    }

                    foreach (var function in Functions)
                    {
                        // matrix functions in a non-routing service always need to have a resource assigned
                        if (function != null && function.Name.Contains("Matrix") && Definition.VirtualPlatform != Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Routing)
                        {
                            if (function.Name.Contains("Input") && !VerifyMatrixFunctionResources(function))
                            {
                                return false;
                            }
                            else
                            {
                            // output will automatically be checked when verifying the input
                            }
                        }
                        else if (function != null && function.Resource == null)
                        {
                            progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(VerifyFunctionResources), $"Function {function.Definition.Label} requires a resource but has no resource assigned", Name);
                            return false;
                        }
                    }

                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(VerifyFunctionResources), $"All necessary functions have resources assigned", Name);
                    return true;
                }

                /// <summary>
                /// Verify that the matrix function resources are correctly assigned.
                /// </summary>
                /// <param name = "matrixInputFunction">The matrix input function.</param>
                /// <returns>Returns true if the matrix input and connected output are correctly assigned.</returns>
                private bool VerifyMatrixFunctionResources(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function matrixInputFunction, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    // if the function is a matrix and has no resource assigned, check if the resources before and after the matrix are the same device
                    // if they aren't the same device, a matrix is required as routing between those different devices 
                    // the lack of a resource for the Matrix function then leads to a ResourceOverbooked state on the Service
                    // if they are the same device, no routing is required between those functions
                    var edgeToMatrixInput = System.Linq.Enumerable.FirstOrDefault(Definition.Diagram.Edges, e => e.ToNodeID == matrixInputFunction.NodeId);
                    if (edgeToMatrixInput == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.EdgeNotFoundException($"Edge to node id {matrixInputFunction.NodeId} not found in service definition {Definition.Id}");
                    }

                    var functionToMatrixInput = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == edgeToMatrixInput.FromNodeID);
                    if (functionToMatrixInput == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException($"Function not found with node id {edgeToMatrixInput.FromNodeID}");
                    }

                    var edgeToMatrixOutput = System.Linq.Enumerable.FirstOrDefault(Definition.Diagram.Edges, e => e.FromNodeID == matrixInputFunction.NodeId);
                    if (edgeToMatrixOutput == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.EdgeNotFoundException($"Edge from node id {matrixInputFunction.NodeId} not found in service definition {Definition.Id}");
                    }

                    var matrixOutputFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == edgeToMatrixOutput.ToNodeID);
                    if (matrixOutputFunction == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException($"Function not found with node id {edgeToMatrixInput.ToNodeID}");
                    }

                    var edgeFromMatrixOutput = System.Linq.Enumerable.FirstOrDefault(Definition.Diagram.Edges, e => e.FromNodeID == edgeToMatrixOutput.ToNodeID);
                    if (edgeFromMatrixOutput == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.EdgeNotFoundException($"Edge from node id {edgeToMatrixOutput.ToNodeID} not found in service definition {Definition.Id}");
                    }

                    var functionFromMatrixOutput = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == edgeFromMatrixOutput.ToNodeID);
                    if (functionFromMatrixOutput == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException($"Function not found with node id {edgeFromMatrixOutput.ToNodeID}");
                    }

                    if (functionToMatrixInput.Resource != null && functionFromMatrixOutput.Resource != null && functionToMatrixInput.Resource.MainDVEDmaID == functionFromMatrixOutput.Resource.MainDVEDmaID && functionToMatrixInput.Resource.MainDVEElementID == functionFromMatrixOutput.Resource.MainDVEElementID)
                    {
                        // in case the resources connected to the input and the output matrix are from the same device
                        // then no matrix resources are needed
                        progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(VerifyMatrixFunctionResources), $"Resource {functionToMatrixInput.Resource.Name} from function {functionToMatrixInput.Definition.Label} and resource {functionFromMatrixOutput.Resource.Name} from function {functionFromMatrixOutput} are the same device, matrix functions are OK", Name);
                        return true;
                    }

                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(VerifyMatrixFunctionResources), $"Function {matrixInputFunction.Definition.Label} has resource {matrixInputFunction.Resource?.Name}, function {matrixOutputFunction.Definition.Label} has resource {matrixOutputFunction.Resource?.Name}", Name);
                    // if we reach this code then both the input and output matrix function should have a resource assigned
                    return matrixInputFunction.Resource != null && matrixOutputFunction.Resource != null;
                }

                /// <summary>
                /// Checks if all audio processing functions have a correct resource assigned.
                /// </summary>
                /// <returns>Returns true in case all resources are ok.</returns>
                private bool VerifyAudioProcessingFunctionResources()
                {
                    var audioDeembeddingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.AudioDeembeddingFunctionDefinitionName);
                    var audioDeembeddingRequiredParameter = System.Linq.Enumerable.FirstOrDefault(audioDeembeddingFunction.Parameters, p => p.Name.Contains("Required"));
                    var audioDeembeddingRequired = System.Convert.ToString(audioDeembeddingRequiredParameter.Value) != "Yes";
                    var audioDolbyDecodingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.AudioDolbyDecodingFunctionDefinitionName);
                    var audioDolbyDecodingRequiredParameter = System.Linq.Enumerable.FirstOrDefault(audioDolbyDecodingFunction.Parameters, p => p.Name.Contains("Required"));
                    var audioDolbyDecodingRequired = System.Convert.ToString(audioDolbyDecodingRequiredParameter.Value) != "Yes";
                    var audioShufflingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.AudioShufflingFunctionDefinitionName);
                    var audioShufflingRequiredParameter = System.Linq.Enumerable.FirstOrDefault(audioShufflingFunction.Parameters, p => p.Name.Contains("Required"));
                    var audioShufflingRequired = System.Convert.ToString(audioShufflingRequiredParameter.Value) != "Yes";
                    var audioEmbeddingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.Name == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.AudioEmbeddingFunctionDefinitionName);
                    var audioEmbeddingRequiredParameter = System.Linq.Enumerable.FirstOrDefault(audioEmbeddingFunction.Parameters, p => p.Name.Contains("Required"));
                    var audioEmbeddingRequired = System.Convert.ToString(audioEmbeddingRequiredParameter.Value) != "Yes";
                    var matrixSdiInputAudioDeembeddingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiInputAudioDeembeddingFunctionNodeId);
                    var audioDeembeddingCheckMatrixInputResource = audioDeembeddingRequired && (audioDolbyDecodingRequired || audioShufflingRequired || audioEmbeddingRequired);
                    var audioDeembeddingCheckMatrixOutputResource = false;
                    if (!VerifyAudioProcessingFunctionResource(audioDeembeddingFunction, matrixSdiInputAudioDeembeddingFunction, null, audioDeembeddingCheckMatrixInputResource, audioDeembeddingCheckMatrixOutputResource))
                        return false;
                    var matrixSdiInputAudioDolbyDecodingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiInputAudioDolbyDecodingFunctionNodeId);
                    var matrixSdiOutputAudioDolbyDecodingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiOutputAudioDolbyDecodingFunctionNodeId);
                    var audioDolbyDecodingCheckMatrixInputResource = audioDolbyDecodingRequired && (audioShufflingRequired || audioEmbeddingRequired);
                    var audioDolbyDecodingCheckMatrixOutputResource = audioDolbyDecodingRequired && audioDeembeddingRequired;
                    if (!VerifyAudioProcessingFunctionResource(audioDolbyDecodingFunction, matrixSdiInputAudioDolbyDecodingFunction, matrixSdiOutputAudioDolbyDecodingFunction, audioDolbyDecodingCheckMatrixInputResource, audioDolbyDecodingCheckMatrixOutputResource))
                        return false;
                    var matrixSdiInputAudioShufflingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiInputAudioShufflingFunctionNodeId);
                    var matrixSdiOutputAudioShufflingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiOutputAudioShufflingFunctionNodeId);
                    var audioShufflingCheckMatrixInputResource = audioShufflingRequired && audioEmbeddingRequired;
                    var audioShufflingCheckMatrixOutputResource = audioShufflingRequired && (audioDeembeddingRequired || audioDolbyDecodingRequired);
                    if (!VerifyAudioProcessingFunctionResource(audioShufflingFunction, matrixSdiInputAudioShufflingFunction, matrixSdiOutputAudioShufflingFunction, audioShufflingCheckMatrixInputResource, audioShufflingCheckMatrixOutputResource))
                        return false;
                    var matrixSdiOutputAudioEmbeddingFunction = System.Linq.Enumerable.FirstOrDefault(Functions, f => f.NodeId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService.MatrixSdiOutputAudioEmbeddingFunctionNodeId);
                    var audioEmbeddingCheckMatrixInputResource = false;
                    var audioEmbeddingCheckMatrixOutputResource = audioEmbeddingRequired && (audioDeembeddingRequired || audioDolbyDecodingRequired || audioShufflingRequired);
                    if (!VerifyAudioProcessingFunctionResource(audioEmbeddingFunction, null, matrixSdiOutputAudioEmbeddingFunction, audioEmbeddingCheckMatrixInputResource, audioEmbeddingCheckMatrixOutputResource))
                        return false;
                    return true;
                }

                private bool VerifyAudioProcessingFunctionResource(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function audioProcessingFunction, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function matrixInputFunction, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function matrixOutputFunction, bool checkInput = true, bool checkOutput = true)
                {
                    if (audioProcessingFunction == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException("Audio processing function could not be found");
                    }

                    var requiredParameter = System.Linq.Enumerable.FirstOrDefault(audioProcessingFunction.Parameters, p => p.Name.Contains("Required"));
                    if (requiredParameter == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionParameterNotFoundException("Audio processing Required");
                    }

                    // if this function is not required then we don't need to check the actual resource(s)
                    if (System.Convert.ToString(requiredParameter.Value) != "Yes")
                        return true;
                    if (audioProcessingFunction.Resource == null)
                        return false;
                    if (checkInput)
                    {
                        if (matrixInputFunction == null)
                        {
                            throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException("Matrix SDI Input audio processing function could not be found");
                        }

                        if (matrixInputFunction.Resource == null)
                            return false;
                    }

                    if (checkOutput)
                    {
                        if (matrixOutputFunction == null)
                        {
                            throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionNotFoundException("Matrix SDI Output audio processing function could not be found");
                        }

                        if (matrixOutputFunction.Resource == null)
                            return false;
                    }

                    return true;
                }

                private void VerifyContributingResourceCreation()
                {
                    // check if the contributing resource was correctly created
                    // the resource should have the same id as the service
                    var contributingFunctionResource = Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager.GetResource(Id);
                    if (contributingFunctionResource == null)
                    {
                        // Catching faulty event level reception resources
                        ContributingResource = null;
                        ResourcePool = null;
                        return;
                    }

                    // the contributing resource will only be added to 1 resource pool
                    var resourcePoolId = System.Linq.Enumerable.FirstOrDefault(contributingFunctionResource.PoolGUIDs);
                    var resourcePool = Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager.GetResourcePool(resourcePoolId) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ResourcePoolNotFoundException($"Resource pool not found {resourcePoolId}");
                    ContributingResource = contributingFunctionResource;
                    ResourcePool = resourcePool;
                }

                public override string ToString()
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Short Description: {GetShortDescription()}");
                    sb.AppendLine($"\tStatus: {Status}");
                    sb.AppendLine($"\tBackup Type: {BackupType}");
                    sb.AppendLine($"\tID: {Id}");
                    sb.AppendLine($"\tNode ID: {NodeId}");
                    sb.AppendLine($"\tStart: {Start}");
                    sb.AppendLine($"\tEnd: {End}");
                    sb.AppendLine($"\tStart With Pre Roll: {StartWithPreRoll}");
                    sb.AppendLine($"\tEnd With Post Roll: {EndWithPostRoll}");
                    sb.AppendLine($"\tIntegration Type: {IntegrationType}");
                    sb.AppendLine($"\tEurovision ID: {EurovisionWorkOrderId}");
                    sb.AppendLine($"\tEurovision Transmission Number: {EurovisionTransmissionNumber}");
                    sb.AppendLine($"\tSD Virtual Platform: {Definition.VirtualPlatform}");
                    sb.AppendLine($"\tSD ID: {Definition.Id}");
                    sb.AppendLine($"\tSD Name: {Definition.Name}");
                    sb.AppendLine($"\tIs Eurovision Service: {IsEurovisionService}");
                    return sb.ToString();
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status GenerateStatus(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    var now = System.DateTime.Now;
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), $"Evaluating status at: {now.ToString("dd/MM/yyyy HH:mm:ss")}", Name);
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), $"Service timing: {TimingInfoToString(this)}", Name);
                    if (isCancelled)
                    {
                        progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), "Service is cancelled", Name);
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.Cancelled;
                    }

                    if (IsPreliminary || !IsBooked)
                    {
                        progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), "Service is preliminary", Name);
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.Preliminary;
                    }

                    // check if resources are overbooked
                    bool isResourceOverbooked = !VerifyFunctionResources();
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), $"{(isResourceOverbooked ? "Not all" : "All")} expected resources are assigned", Name);
                    // check user tasks
                    var hasIncompleteConfigurationUserTasks = false;
                    var hasIncompleteFileProcessingUserTasks = false;
                    if (UserTasks != null)
                    {
                        foreach (var userTask in UserTasks)
                        {
                            progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), $"User Task {userTask.Name} is {userTask.Status}", Name);
                            if (userTask.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Complete)
                                continue;
                            if (userTask.Description.IndexOf("File Processing", System.StringComparison.OrdinalIgnoreCase) != -1)
                                hasIncompleteFileProcessingUserTasks = true;
                            else
                                hasIncompleteConfigurationUserTasks = true;
                        }
                    }

                    if (hasIncompleteConfigurationUserTasks)
                        progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), "Service has incomplete configuration user tasks", Name);
                    if (hasIncompleteFileProcessingUserTasks)
                        progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service), nameof(GenerateStatus), "Service has incomplete file processing user tasks", Name);
                    // return correct status based on timing, resources and user tasks
                    if (EndWithPostRoll <= now)
                    {
                        if (HasAnIssueBeenreportedManually || isResourceOverbooked || hasIncompleteConfigurationUserTasks)
                            return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCompletedWithErrors;
                        if (hasIncompleteFileProcessingUserTasks)
                            return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.FileProcessing;
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCompleted;
                    }

                    if (isResourceOverbooked)
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ResourceOverbooked;
                    if (hasIncompleteConfigurationUserTasks)
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ConfigurationPending;
                    if (StartWithPreRoll <= now && now < Start)
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceCueing;
                    if (Start <= now && now < End)
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ServiceRunning;
                    if (End <= now && now < EndWithPostRoll)
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.PostRoll;
                    return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ConfigurationCompleted;
                }

                /// <summary>
                /// Gets the short description for this service.
                /// </summary>
                /// <param name = "order">The order that contains this service. Can be null in case of non-recording services. Cannot be null in case of recording services.</param>
                /// <exception cref = "ArgumentNullException">Thrown in case this service is a recording and <paramref name = "order"/> is null.</exception>
                public string GetShortDescription(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order = null, Skyline.DataMiner.Automation.IEngine engine = null)
                {
                    switch (Definition.VirtualPlatform)
                    {
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite:
                            return GetSatelliteReceptionDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionSatellite:
                            return GetSatelliteTransmissionDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionFiber:
                            return GetFiberTranmissionDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionLiveU:
                            return GetLiveUReceptionDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFiber:
                            return GetFiberReceptionDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording:
                            if (order == null)
                                throw new System.ArgumentNullException(nameof(order), "Order cannot be null for a recording service.");
                            return GetRecordingDescription(order);
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Routing:
                            return GetRoutingDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.AudioProcessing:
                            return GetAudioProcessingDescription();
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFixedService:
                            return GetFixedServiceDescription();
                        default:
                            return GetStandardShortDescription();
                    }
                }

                private string GetStandardShortDescription()
                {
                    var result = new System.Text.StringBuilder();
                    bool isServiceDestination = Definition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Destination;
                    if (!isServiceDestination)
                    {
                        string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                        result.Append(virtualPlatformName);
                    }

                    if (Functions.Count == 1)
                    {
                        var function = System.Linq.Enumerable.Single(Functions);
                        var resource = function.Resource;
                        string resourceName = resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(resource, function.Id) : None;
                        string resourceDescription = !isServiceDestination ? " - " + resourceName : resourceName;
                        result.Append(resourceDescription);
                    }

                    return result.ToString();
                }

                private string GetFiberReceptionDescription()
                {
                    var description = new System.Text.StringBuilder();
                    bool isFullCapacity = Definition.Description.Contains("Full Capacity");
                    description.Append(isFullCapacity ? GetFiberReceptionFullCapacityDescription() : GetFiberReceptionLimitedCapacityDescription());
                    return description.ToString();
                }

                private string GetFiberReceptionFullCapacityDescription()
                {
                    var description = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    description.Append($"{virtualPlatformName}");
                    var function = System.Linq.Enumerable.Single(Functions);
                    string fiberResourceName = function.Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(function.Resource, function.Id) : None;
                    if (!string.IsNullOrEmpty(fiberResourceName))
                    {
                        description.Append($" - {fiberResourceName}");
                    }

                    string nameValue = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FeedName).StringValue;
                    if (!string.IsNullOrEmpty(nameValue))
                    {
                        description.Append($" - {nameValue}");
                    }

                    return description.ToString();
                }

                private string GetFiberReceptionLimitedCapacityDescription()
                {
                    var description = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    description.Append($"{virtualPlatformName}");
                    var fiberSourceFunction = System.Linq.Enumerable.SingleOrDefault(Functions, f => f != null && f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.FiberSource);
                    string fiberResourceName = fiberSourceFunction.Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(fiberSourceFunction.Resource, fiberSourceFunction.Id) : None;
                    if (!string.IsNullOrEmpty(fiberResourceName))
                    {
                        description.Append($" - {fiberResourceName} --> ");
                    }

                    var function = System.Linq.Enumerable.Single(Functions, f => f != null && f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.FiberDecoding);
                    fiberResourceName = function.Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(function.Resource, function.Id) : None;
                    if (!string.IsNullOrEmpty(fiberResourceName))
                    {
                        description.Append(fiberResourceName);
                    }

                    string nameValue = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FeedName).StringValue;
                    if (!string.IsNullOrEmpty(nameValue))
                    {
                        description.Append($" - {nameValue}");
                    }

                    return description.ToString();
                }

                private string GetFiberTranmissionDescription()
                {
                    var description = new System.Text.StringBuilder();
                    bool isLimitedCapacity = Definition.Description.Contains("Limited Capacity");
                    if (isLimitedCapacity)
                    {
                        string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                        string virtualPlatformType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceType);
                        description.Append($"{virtualPlatformName} {virtualPlatformType}");
                        var genericEncodingfunction = System.Linq.Enumerable.SingleOrDefault(Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.GenericEncoding);
                        string fiberResourceName = genericEncodingfunction?.Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(genericEncodingfunction.Resource, genericEncodingfunction.Id) : None;
                        if (!string.IsNullOrEmpty(fiberResourceName))
                        {
                            description.Append($" - {fiberResourceName} --> ");
                        }

                        var fiberSourceFunction = System.Linq.Enumerable.SingleOrDefault(Functions, f => f != null && f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.FiberDestination);
                        fiberResourceName = fiberSourceFunction?.Resource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(fiberSourceFunction.Resource, fiberSourceFunction.Id) : None;
                        if (!string.IsNullOrEmpty(fiberResourceName))
                        {
                            description.Append(fiberResourceName);
                        }

                        string nameValue = System.Linq.Enumerable.SingleOrDefault(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FeedName)?.StringValue;
                        if (!string.IsNullOrEmpty(nameValue))
                        {
                            description.Append($" - {nameValue}");
                        }

                        return description.ToString();
                    }
                    else
                    {
                        string standardDescription = GetStandardShortDescription();
                        description.Append(standardDescription);
                        string feedNameValue = System.Linq.Enumerable.SingleOrDefault(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.FeedName)?.StringValue;
                        if (!string.IsNullOrEmpty(feedNameValue))
                        {
                            description.Append($" - {feedNameValue}");
                        }

                        return description.ToString();
                    }
                }

                private string GetSatelliteReceptionDescription()
                {
                    var result = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    result.Append($"{virtualPlatformName}");
                    var allProfileParams = System.Linq.Enumerable.ToList(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters));
                    var modulationStandardProfileParameter = System.Linq.Enumerable.FirstOrDefault(allProfileParams, p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.ModulationStandard) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ProfileParameterNotFoundException(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.ModulationStandard.ToString(), null, allProfileParams);
                    bool serviceUsesNs3OrNs4Modulation = modulationStandardProfileParameter.StringValue == "NS3" || modulationStandardProfileParameter.StringValue == "NS4";
                    if (serviceUsesNs3OrNs4Modulation)
                    {
                        var demodulatingFunction = System.Linq.Enumerable.Single(Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.Demodulating);
                        var demodulatingResource = demodulatingFunction.Resource;
                        string demodulatingResourceName = demodulatingResource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(demodulatingResource, demodulatingFunction.Id) : string.Empty;
                        result.Append(" - " + demodulatingResourceName);
                    }

                    var decodingFunction = System.Linq.Enumerable.Single(Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.Decoding);
                    var decodingResource = decodingFunction.Resource;
                    string decodingResourceName = decodingResource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(decodingResource, decodingFunction.Id) : string.Empty;
                    if (!string.IsNullOrEmpty(decodingResourceName))
                    {
                        if (serviceUsesNs3OrNs4Modulation)
                            result.Append(" > ");
                        else
                            result.Append(" - ");
                        result.Append(decodingResourceName);
                    }

                    string serviceSelection = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.ServiceSelection)?.StringValue;
                    if (!string.IsNullOrWhiteSpace(serviceSelection))
                        result.Append(" - " + serviceSelection);
                    return result.ToString();
                }

                private string GetSatelliteTransmissionDescription()
                {
                    var result = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    string virtualPlatformType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceType);
                    result.Append($"{virtualPlatformName} {virtualPlatformType}");
                    result.Append(" - ");
                    var encodingFunction = System.Linq.Enumerable.Single(Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.GenericEncoding);
                    var encodingResource = encodingFunction.Resource;
                    string encodingResourceName = encodingResource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(encodingResource, encodingFunction.Id) : "None";
                    result.Append(encodingResourceName);
                    result.Append(" --> ");
                    var modulatingFunction = System.Linq.Enumerable.Single(Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.GenericModulating);
                    var modulatingResource = modulatingFunction.Resource;
                    string modulatingResourceName = modulatingResource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(modulatingResource, modulatingFunction.Id) : "None";
                    result.Append(modulatingResourceName);
                    return result.ToString();
                }

                private string GetLiveUReceptionDescription()
                {
                    var result = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    result.Append($"{virtualPlatformName}");
                    var function = System.Linq.Enumerable.Single(Functions);
                    var functionResource = function.Resource;
                    string resourceName = functionResource != null ? Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.ResourceExtensions.GetDisplayName(functionResource, function.Id) : string.Empty;
                    if (!string.IsNullOrEmpty(resourceName))
                        result.Append(" - " + resourceName);
                    bool isPasila = Definition.Description.Contains("Pasila");
                    bool isAudioReturn = System.Linq.Enumerable.Any(System.Linq.Enumerable.SelectMany(Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.AudioReturnChannel && p.StringValue == "Required");
                    if (isPasila && isAudioReturn)
                        result.Append(" - Audio Return");
                    return result.ToString();
                }

                private string GetRoutingDescription()
                {
                    string virtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatform);
                    var firstResource = System.Linq.Enumerable.First(Functions).Resource;
                    string firstResourceName = firstResource != null ? firstResource.Name : None;
                    string firstResourceDisplayName = System.Linq.Enumerable.Last(firstResourceName.Split('.'));
                    var lastResource = System.Linq.Enumerable.Last(Functions).Resource;
                    string lastResourceName = lastResource != null ? lastResource.Name : None;
                    string lastResourceDisplayName = System.Linq.Enumerable.Last(lastResourceName.Split('.'));
                    if (firstResource != null || lastResource != null)
                    {
                        string matrix = string.Empty;
                        bool isEduskuntaMatrix = firstResourceName.Contains("EDUSKUNTA") || lastResourceName.Contains("EDUSKUNTA");
                        bool isHmxMatrix = firstResourceName.Contains("HMX") || lastResourceName.Contains("HMX");
                        bool isNmxMatrix = firstResourceName.Contains("NMX") || lastResourceName.Contains("NMX");
                        if (isEduskuntaMatrix)
                            matrix = "EDU";
                        else if (isHmxMatrix)
                            matrix = "HMX";
                        else if (isNmxMatrix)
                            matrix = "NEWS";
                        return $"{matrix} {virtualPlatform} - {firstResourceDisplayName} --> {lastResourceDisplayName}";
                    }

                    if (!(string.IsNullOrEmpty(firstResourceName) && string.IsNullOrEmpty(lastResourceName)))
                        return $"{virtualPlatform} - {firstResourceDisplayName} --> {lastResourceDisplayName}";
                    else
                        return virtualPlatform;
                }

                private string GetRecordingDescription(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order)
                {
                    var description = new System.Text.StringBuilder();
                    switch (Definition.Description)
                    {
                        case "Messi Live":
                            description.Append("Live Rec.");
                            description.Append(GetLiveRecordingDescriptionResourcePart(order));
                            break;
                        case "Messi Live Backup":
                            description.Append("Live Backup Rec.");
                            description.Append(GetLiveRecordingDescriptionResourcePart(order));
                            break;
                        case "Messi News":
                            description.Append(GetNewsRecordingDescription());
                            description.Append(GetNewsRecordingDetailsPart(order));
                            break;
                        default:
                            return "Recording";
                    }

                    if (RecordingConfiguration != null)
                    {
                        if (IntegrationType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma)
                        {
                            if (!string.IsNullOrEmpty(RecordingConfiguration.PlasmaTvChannelName))
                                description.Append(" - " + RecordingConfiguration.PlasmaTvChannelName);
                            if (!string.IsNullOrEmpty(RecordingConfiguration.PlasmaProgramName))
                                description.Append(" - " + RecordingConfiguration.PlasmaProgramName);
                        }
                        else if (!string.IsNullOrEmpty(RecordingConfiguration.RecordingName))
                            description.Append(" - " + RecordingConfiguration.RecordingName);
                    }

                    return description.ToString();
                }

                private string GetLiveRecordingDescriptionResourcePart(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order)
                {
                    var result = new System.Text.StringBuilder();
                    var routingService = System.Linq.Enumerable.Single(order.AllServices, s => s.Children.Contains(this));
                    var matrixOutputFunction = System.Linq.Enumerable.LastOrDefault(routingService.Functions); // Not all services have functions, for example the Dummy Services or Eurovision Dummy Services.
                    var matrixOutputResource = matrixOutputFunction?.Resource;
                    string matrixOutputResourceName = matrixOutputResource?.Name ?? None;
                    string matrixOutputResourceDisplayName = System.Linq.Enumerable.Last(matrixOutputResourceName.Split('.'));
                    result.Append(" - " + matrixOutputResourceDisplayName);
                    string functionResourceName = System.Linq.Enumerable.Single(Functions).Resource?.Name ?? None;
                    if (!string.IsNullOrEmpty(functionResourceName))
                        result.Append(" --> " + functionResourceName);
                    return result.ToString();
                }

                private string GetNewsRecordingDescription()
                {
                    var newsRecordingFunction = System.Linq.Enumerable.FirstOrDefault(Functions);
                    if (newsRecordingFunction == null)
                        return "News Rec.";
                    var feedTypeProfileParameter = System.Linq.Enumerable.FirstOrDefault(newsRecordingFunction.Parameters, p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids._FeedType);
                    if (feedTypeProfileParameter == null || string.IsNullOrEmpty(feedTypeProfileParameter.StringValue) || feedTypeProfileParameter.StringValue == "None")
                        return "News Rec.";
                    return $"News {feedTypeProfileParameter.StringValue} Rec.";
                }

                private string GetNewsRecordingDetailsPart(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order)
                {
                    var result = new System.Text.StringBuilder();
                    var routingService = System.Linq.Enumerable.Single(order.AllServices, s => s != null && s.Children?.Contains(this) == true);
                    if (routingService.Functions != null && System.Linq.Enumerable.Any(routingService.Functions))
                    {
                        var matrixOutputFunction = System.Linq.Enumerable.Single(routingService.Functions, f => f != null && routingService.Definition?.FunctionIsFirst(f) == true);
                        var matrixOutputResource = matrixOutputFunction.Resource;
                        string matrixOutputResourceName = matrixOutputResource != null ? matrixOutputResource.Name : None;
                        string matrixOutputResourceDisplayName = System.Linq.Enumerable.Last(matrixOutputResourceName.Split('.'));
                        if (!string.IsNullOrEmpty(matrixOutputResourceDisplayName))
                        {
                            result.Append(matrixOutputResourceDisplayName.Contains("UMX") ? " - " + matrixOutputResourceDisplayName : string.Empty);
                        }
                    }

                    string recordingFunctionResourceName = System.Linq.Enumerable.Single(Functions).Resource?.Name ?? None;
                    if (!string.IsNullOrEmpty(recordingFunctionResourceName))
                        result.Append(" --> " + recordingFunctionResourceName);
                    return result.ToString();
                }

                private string GetAudioProcessingDescription()
                {
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    var nonRoutingResourceNames = System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(Functions, f => !System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.AllMatrixGuids, f.Id) && f.Resource != null), f => f.Resource.Name);
                    var stringBuilder = new System.Text.StringBuilder(virtualPlatformName);
                    foreach (var resourceName in nonRoutingResourceNames)
                        stringBuilder.Append(" - " + resourceName);
                    return stringBuilder.ToString();
                }

                private string GetFixedServiceDescription()
                {
                    var result = new System.Text.StringBuilder();
                    string virtualPlatformName = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
                    result.Append(virtualPlatformName);
                    var function = System.Linq.Enumerable.FirstOrDefault(Functions);
                    if (function == null)
                        return result.ToString();
                    var channelParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id.Equals(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.Channel));
                    if (channelParameter != null && !string.IsNullOrEmpty(channelParameter.StringValue))
                        result.Append(" - " + channelParameter.StringValue);
                    var linkedIrdProperty = function.Resource != null ? System.Linq.Enumerable.FirstOrDefault(function.Resource.Properties, p => p.Name.Equals("LinkedIRD", System.StringComparison.InvariantCultureIgnoreCase)) : null;
                    if (linkedIrdProperty != null && !string.IsNullOrEmpty(linkedIrdProperty.Value))
                        result.Append(" - " + linkedIrdProperty.Value);
                    string serviceSelection = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.ServiceSelection)?.StringValue;
                    if (!string.IsNullOrWhiteSpace(serviceSelection))
                        result.Append(" - " + serviceSelection);
                    return result.ToString();
                }

                public static string TimingInfoToString(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service)
                {
                    return $"Start:{service.Start.ToString("dd/MM/yyyy HH:mm:ss")}, Start with preroll:{service.StartWithPreRoll.ToString("dd/MM/yyyy HH:mm:ss")}, End:{service.End.ToString("dd/MM/yyyy HH:mm:ss")}, End with postroll:{service.EndWithPostRoll.ToString("dd/MM/yyyy HH:mm:ss")}";
                }

                public static string TimingInfoToString(System.DateTime start, System.DateTime end)
                {
                    return $"Start:{start.ToUniversalTime()}, End:{end.ToUniversalTime()}";
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            public class ServiceManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.IServiceManager
            {
                public static readonly string SourceServiceSystemFunctionId = "c7e8648e-7522-4724-99a8-74e48a45f380";
                public static readonly string TransmissionServiceSystemFunctionId = "147f77f6-74d6-4802-8603-5042a6e0ad5d";
                private static readonly System.TimeSpan PlasmaServicePreRoll = System.TimeSpan.Zero;
                private static readonly System.TimeSpan PlasmaServicePostRoll = System.TimeSpan.Zero;
                private static readonly System.TimeSpan FeenixServicePreRoll = System.TimeSpan.FromMinutes(5);
                private static readonly System.TimeSpan FeenixServicePostRoll = System.TimeSpan.FromMinutes(5);
                private static readonly System.TimeSpan MessiLiveRecordingPreRoll = System.TimeSpan.FromMinutes(10);
                private static readonly System.TimeSpan MessiNewsRecordingPreRoll = System.TimeSpan.Zero;
                private static readonly System.TimeSpan MessiLiveRecordingPostRoll = System.TimeSpan.FromMinutes(20);
                private static readonly System.TimeSpan MessiNewsRecordingPostRoll = System.TimeSpan.Zero;
                private static readonly System.TimeSpan Preroll = System.TimeSpan.FromMinutes(5);
                private static readonly System.TimeSpan PostRoll = System.TimeSpan.FromMinutes(5);
                private static readonly System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform, System.TimeSpan> preRollDurations = new System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform, System.TimeSpan>{{Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFiber, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionMicrowave, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionIp, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionLiveU, new System.TimeSpan(hours: 0, minutes: 15, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFixedLine, new System.TimeSpan(hours: 0, minutes: 15, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Routing, new System.TimeSpan(hours: 0, minutes: 15, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.VideoProcessing, new System.TimeSpan(hours: 0, minutes: 15, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.AudioProcessing, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.GraphicsProcessing, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionSatellite, new System.TimeSpan(hours: 0, minutes: 30, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionIp, new System.TimeSpan(hours: 0, minutes: 60, seconds: 0)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Destination, new System.TimeSpan(hours: 0, minutes: 15, seconds: 0)}};
                private static readonly System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform, System.TimeSpan> postRollDurations = new System.Collections.Generic.Dictionary<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform, System.TimeSpan>{{Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFiber, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionMicrowave, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionIp, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionLiveU, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionFixedLine, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Routing, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.VideoProcessing, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.AudioProcessing, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.GraphicsProcessing, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionSatellite, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionIp, System.TimeSpan.FromMinutes(60)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionLiveU, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionMicrowave, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionEurovision, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionFiber, System.TimeSpan.FromMinutes(30)}, {Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Destination, System.TimeSpan.FromMinutes(30)}, };
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                public ServiceManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    Helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                    this.engine = helpers.Engine;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers Helpers
                {
                    get;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service GetService(System.Guid serviceId)
                {
                    var reservationInstance = GetReservation(serviceId);
                    if (reservationInstance == null)
                    {
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ReservationNotFoundException(serviceId);
                    }

                    var service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service.FromReservationInstance(Helpers, reservationInstance);
                    return service;
                }

                public Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance GetReservation(System.Guid id)
                {
                    return Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager.GetReservationInstance(id);
                }

                public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function> GetFunctions(Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance reservationInstance, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition serviceDefinition)
                {
                    if (reservationInstance == null)
                        throw new System.ArgumentNullException(nameof(reservationInstance));
                    if (serviceDefinition == null)
                        throw new System.ArgumentNullException(nameof(serviceDefinition));
                    LogMethodStarted(nameof(GetFunctions), out var stopwatch);
                    if (reservationInstance.IsQuarantined)
                    {
                        // When the service reservation instance is quarantined, no valid function data is available.
                        return new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function>();
                    }

                    var functions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function>();
                    foreach (var node in serviceDefinition.Diagram.Nodes)
                    {
                        var function = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function(Helpers, reservationInstance, node, serviceDefinition);
                        functions.Add(function);
                    }

                    LogMethodCompleted(nameof(GetFunctions), null, stopwatch);
                    return functions;
                }

                /// <summary>
                /// Gets the Pre Roll duration for the provided Service Definition.
                /// </summary>
                /// <param name = "serviceDefinition">Service Definition for which the pre roll is requested.</param>
                /// <param name = "integrationType">Optional IntegrationType of the service. Affects the pre roll for Plasma Services.</param>
                /// <param name = "primarySourcePreRollDuration">Pre Roll of the Primary Source Service. In case the Pre Roll of the Primary Source Service itself is requested, this value is ignored.</param>
                /// <returns>TimeSpan of the Pre Roll.</returns>
                public static System.TimeSpan GetPreRollDuration(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition serviceDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType integrationType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.None, System.TimeSpan primarySourcePreRollDuration = default(System.TimeSpan))
                {
                    System.TimeSpan originalPreroll;
                    switch (integrationType)
                    {
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma:
                            originalPreroll = PlasmaServicePreRoll;
                            break;
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Feenix:
                            originalPreroll = FeenixServicePreRoll;
                            break;
                        default:
                            if (serviceDefinition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && serviceDefinition.Description.ToUpper().Contains("MESSI LIVE"))
                            {
                                // MESSI LIVE REC Pre Roll = 10 min
                                originalPreroll = MessiLiveRecordingPreRoll;
                            }
                            else if (serviceDefinition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && serviceDefinition.Description.ToUpper().Equals("MESSI NEWS"))
                            {
                                // MESSI NEWS REC Pre Roll = 5 min
                                originalPreroll = MessiNewsRecordingPreRoll;
                            }
                            else if (preRollDurations.ContainsKey(serviceDefinition.VirtualPlatform))
                            {
                                originalPreroll = preRollDurations[serviceDefinition.VirtualPlatform];
                            }
                            else
                            {
                                originalPreroll = primarySourcePreRollDuration;
                            }

                            break;
                    }

                    // Temporary workaround as YLE would temporarily like to see smaller prerolls
                    if (originalPreroll < Preroll)
                        return originalPreroll;
                    return Preroll;
                }

                /// <summary>
                /// Gets the Post Roll duration for the provided Service Definition.
                /// </summary>
                /// <param name = "serviceDefinition">Service Definition for which the post roll is requested.</param>
                /// <param name = "integrationType">Optional IntegrationType of the service. Affects the post roll for Plasma Services.</param>
                /// <param name = "primarySourcePostRollDuration">Post Roll of the Primary Source Service. In case the Post Roll of the Primary Source Service itself is requested, this value is ignored.</param>
                /// <returns>TimeSpan of the Post Roll.</returns>
                public static System.TimeSpan GetPostRollDuration(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition serviceDefinition, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType integrationType = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.None, System.TimeSpan primarySourcePostRollDuration = default(System.TimeSpan))
                {
                    System.TimeSpan originalPostRoll;
                    switch (integrationType)
                    {
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Plasma:
                            originalPostRoll = PlasmaServicePostRoll;
                            break;
                        case Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType.Feenix:
                            originalPostRoll = FeenixServicePostRoll;
                            break;
                        default:
                            if (serviceDefinition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && serviceDefinition.Description.ToUpper().Contains("MESSI LIVE"))
                            {
                                // MESSI LIVE REC Post Roll = 20 min
                                originalPostRoll = MessiLiveRecordingPostRoll;
                            }
                            else if (serviceDefinition.VirtualPlatform == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.Recording && serviceDefinition.Description.ToUpper().Equals("MESSI NEWS"))
                            {
                                // MESSI NEWS  REC Post Roll = 10min
                                originalPostRoll = MessiNewsRecordingPostRoll;
                            }
                            else if (postRollDurations.ContainsKey(serviceDefinition.VirtualPlatform))
                            {
                                originalPostRoll = postRollDurations[serviceDefinition.VirtualPlatform];
                            }
                            else
                            {
                                originalPostRoll = primarySourcePostRollDuration;
                            }

                            break;
                    }

                    // Temporary workaround as YLE would temporarily like to see smaller postrolls
                    if (originalPostRoll < PostRoll)
                        return originalPostRoll;
                    return PostRoll;
                }

                private void LogMethodStarted(string methodName, out System.Diagnostics.Stopwatch stopwatch, string objectName = null)
                {
                    Helpers.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager), methodName, out stopwatch, objectName);
                }

                private void LogMethodCompleted(string methodName, string objectName = null, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    Helpers.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager), methodName, objectName, stopwatch);
                }
            }

            /// <summary>
            /// A sub recording for a specific recording.
            /// </summary>
            public class SubRecording : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.IYleChangeTracking
            {
                private string initialName;
                private string initialAdditionalInfo;
                private System.DateTime initialEstimatedTimeSlotStart;
                private System.DateTime initialEstimatedTimeSlotEnd;
                private string initialTimeSlotDescription;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo nameValidation;
                public SubRecording()
                {
                    Id = System.Guid.NewGuid();
                    EstimatedTimeSlotStart = System.DateTime.Now.AddDays(1);
                    EstimatedTimeSlotEnd = System.DateTime.Now.AddDays(1);
                }

                /// <summary>
                /// Used to identify the object by a controller.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public System.Guid Id
                {
                    get;
                }

                /// <summary>
                /// The name of this sub recording.
                /// </summary>
                public string Name
                {
                    get;
                    set;
                }

                /// <summary>
                /// Property set by controller and used by UI for validation.
                /// </summary>
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo NameValidation
                {
                    get
                    {
                        if (nameValidation == null)
                            nameValidation = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ValidationInfo();
                        return nameValidation;
                    }

                    set
                    {
                        nameValidation = value;
                    }
                }

                /// <summary>
                /// Additional information about this sub recording.
                /// </summary>
                public string AdditionalInformation
                {
                    get;
                    set;
                }

                /// <summary>
                /// The start of the estimated time slot for this sub recording.
                /// </summary>
                public System.DateTime EstimatedTimeSlotStart
                {
                    get;
                    set;
                }

                /// <summary>
                /// The end of the estimated time slot for this sub recording.
                /// </summary>
                public System.DateTime EstimatedTimeSlotEnd
                {
                    get;
                    set;
                }

                /// <summary>
                /// The description for this time slot.
                /// </summary>
                public string TimeslotDescription
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonIgnore]
                public bool IsChanged => ChangeInfo.IsChanged;
                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo ChangeInfo => GetChanges();
                [Newtonsoft.Json.JsonIgnore]
                public bool ChangeTrackingEnabled
                {
                    get;
                    private set;
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo GetChanges()
                {
                    var changeInfo = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.ChangeInfo();
                    bool nameIsChanged = Name != initialName;
                    bool additionalInfoIsChanged = AdditionalInformation != initialAdditionalInfo;
                    bool estimatedTimeSlotStartIsChanged = EstimatedTimeSlotStart != initialEstimatedTimeSlotStart;
                    bool estimatedTimeSlotEndIsChanged = EstimatedTimeSlotEnd != initialEstimatedTimeSlotEnd;
                    bool timeSlotDescriptionIsChanged = TimeslotDescription != initialTimeSlotDescription;
                    if (nameIsChanged || additionalInfoIsChanged || estimatedTimeSlotStartIsChanged || estimatedTimeSlotEndIsChanged || timeSlotDescriptionIsChanged)
                        changeInfo.MarkCustomPropertiesChanged();
                    return changeInfo;
                }

                public void AcceptChanges()
                {
                    initialName = Name;
                    initialAdditionalInfo = AdditionalInformation;
                    initialEstimatedTimeSlotStart = EstimatedTimeSlotStart;
                    initialEstimatedTimeSlotEnd = EstimatedTimeSlotEnd;
                    initialTimeSlotDescription = TimeslotDescription;
                }

                public override string ToString()
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Sub recording name: " + Name + "|");
                    sb.AppendLine($"Sub recording start: " + System.Convert.ToString(EstimatedTimeSlotStart, System.Globalization.CultureInfo.InvariantCulture) + "|");
                    sb.AppendLine($"Sub recording end: " + System.Convert.ToString(EstimatedTimeSlotEnd, System.Globalization.CultureInfo.InvariantCulture) + "|");
                    sb.AppendLine($"Sub recording time slot description: " + TimeslotDescription);
                    return sb.ToString();
                }
            }

            namespace Auto_Generation
            {
                public class AudioProcessingService : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService
                {
                    private static readonly System.Guid InputAudioChannel1ProfileParameterId = System.Guid.Parse("b60f8740-f593-431b-8043-474ccb8f9566");
                    private static readonly System.Guid InputAudioChannel2ProfileParameterId = System.Guid.Parse("1fe71070-44d5-4ad0-95db-103413935506");
                    private static readonly System.Guid InputAudioChannel3ProfileParameterId = System.Guid.Parse("00d823aa-4363-4d49-9eb6-fa2a2712bf9c");
                    private static readonly System.Guid InputAudioChannel4ProfileParameterId = System.Guid.Parse("bc63cd0f-1886-45ff-a675-659a2f840166");
                    private static readonly System.Guid InputAudioChannel5ProfileParameterId = System.Guid.Parse("7d8a7d82-a328-4f5c-9c4d-a1f634fdefc5");
                    private static readonly System.Guid InputAudioChannel6ProfileParameterId = System.Guid.Parse("81352369-5ece-4c1e-98d6-bd908589e2c4");
                    private static readonly System.Guid InputAudioChannel7ProfileParameterId = System.Guid.Parse("cb840609-c68c-4636-9225-b3667685e29c");
                    private static readonly System.Guid InputAudioChannel8ProfileParameterId = System.Guid.Parse("11ea232f-511e-463a-927a-d4ed4fac52b6");
                    private static readonly System.Guid InputAudioChannel9ProfileParameterId = System.Guid.Parse("96837519-a1be-4415-aef9-ec7c148a8ce0");
                    private static readonly System.Guid InputAudioChannel10ProfileParameterId = System.Guid.Parse("6701766b-60d4-499c-8e84-ee91f2afbc65");
                    private static readonly System.Guid InputAudioChannel11ProfileParameterId = System.Guid.Parse("e39f61ce-515f-4134-b308-32fb6c3df9c0");
                    private static readonly System.Guid InputAudioChannel12ProfileParameterId = System.Guid.Parse("b7e5efb5-8c48-4670-9d3b-c8315beeb0f8");
                    private static readonly System.Guid InputAudioChannel13ProfileParameterId = System.Guid.Parse("4da8b4de-178c-4bbe-b48c-69f25dba46c9");
                    private static readonly System.Guid InputAudioChannel14ProfileParameterId = System.Guid.Parse("c1f16b91-a429-4e09-b315-34900b6a0906");
                    private static readonly System.Guid InputAudioChannel15ProfileParameterId = System.Guid.Parse("b305ea6d-9028-44e3-8b72-e394ade2ac0c");
                    private static readonly System.Guid InputAudioChannel16ProfileParameterId = System.Guid.Parse("01133e53-fa18-48d8-bb3e-073326618f3c");
                    private static readonly System.Guid OutputAudioChannel1ProfileParameterId = System.Guid.Parse("c18bcf3e-7985-45f4-b968-56826861b761");
                    private static readonly System.Guid OutputAudioChannel2ProfileParameterId = System.Guid.Parse("3c8bc1f0-8509-4ca8-91a9-e3528cd69bc6");
                    private static readonly System.Guid OutputAudioChannel3ProfileParameterId = System.Guid.Parse("edbc7e6d-0823-41b4-a0ac-89e0b0bb70a4");
                    private static readonly System.Guid OutputAudioChannel4ProfileParameterId = System.Guid.Parse("9031245f-6c36-4437-a22d-b2b65fdedb17");
                    private static readonly System.Guid OutputAudioChannel5ProfileParameterId = System.Guid.Parse("26f31192-01ab-45a1-b214-a56dab65072f");
                    private static readonly System.Guid OutputAudioChannel6ProfileParameterId = System.Guid.Parse("f6e684f5-3627-4fb8-9406-2e6898655440");
                    private static readonly System.Guid OutputAudioChannel7ProfileParameterId = System.Guid.Parse("447bb387-397d-4565-88ff-972488e1feba");
                    private static readonly System.Guid OutputAudioChannel8ProfileParameterId = System.Guid.Parse("cdfb4027-3ebb-4167-bb87-b94e732f1461");
                    private static readonly System.Guid OutputAudioChannel9ProfileParameterId = System.Guid.Parse("0f5ecba8-3c17-4831-b7f0-cca2a80e001f");
                    private static readonly System.Guid OutputAudioChannel10ProfileParameterId = System.Guid.Parse("fee87c0a-bb43-4819-ad66-1d366f7435ed");
                    private static readonly System.Guid OutputAudioChannel11ProfileParameterId = System.Guid.Parse("de03ea6a-029e-4bf2-ad57-646a14417c36");
                    private static readonly System.Guid OutputAudioChannel12ProfileParameterId = System.Guid.Parse("9f964476-2a3b-4c49-8e75-deadfc91bc04");
                    private static readonly System.Guid OutputAudioChannel13ProfileParameterId = System.Guid.Parse("65929b68-e642-4ed6-a669-27455690137e");
                    private static readonly System.Guid OutputAudioChannel14ProfileParameterId = System.Guid.Parse("8de81be4-8daf-4e30-9a5f-2ae6fd1a22dd");
                    private static readonly System.Guid OutputAudioChannel15ProfileParameterId = System.Guid.Parse("a4e3c96b-7c15-43a8-944d-61ddcc7c2905");
                    private static readonly System.Guid OutputAudioChannel16ProfileParameterId = System.Guid.Parse("517be756-d2d3-4ce1-b9e7-cee150264c0c");
                    public static readonly string AudioEmbeddingFunctionDefinitionName = "Audio Embedding";
                    public static readonly string AudioDeembeddingFunctionDefinitionName = "Audio Deembedding";
                    public static readonly string AudioShufflingFunctionDefinitionName = "Audio Shuffling";
                    public static readonly string AudioDolbyDecodingFunctionDefinitionName = "Audio Dolby Decoding";
                    public static readonly int MatrixSdiInputAudioDeembeddingFunctionNodeId = 3;
                    public static readonly int MatrixSdiOutputAudioDolbyDecodingFunctionNodeId = 2;
                    public static readonly int MatrixSdiInputAudioDolbyDecodingFunctionNodeId = 7;
                    public static readonly int MatrixSdiOutputAudioShufflingFunctionNodeId = 6;
                    public static readonly int MatrixSdiInputAudioShufflingFunctionNodeId = 10;
                    public static readonly int MatrixSdiOutputAudioEmbeddingFunctionNodeId = 9;
                    private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource inputResource;
                    private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource outputResource;
                    private string inputAudioChannel1;
                    private string inputAudioChannel2;
                    private string inputAudioChannel3;
                    private string inputAudioChannel4;
                    private string inputAudioChannel5;
                    private string inputAudioChannel6;
                    private string inputAudioChannel7;
                    private string inputAudioChannel8;
                    private string inputAudioChannel9;
                    private string inputAudioChannel10;
                    private string inputAudioChannel11;
                    private string inputAudioChannel12;
                    private string inputAudioChannel13;
                    private string inputAudioChannel14;
                    private string inputAudioChannel15;
                    private string inputAudioChannel16;
                    private string outputAudioChannel1;
                    private string outputAudioChannel2;
                    private string outputAudioChannel3;
                    private string outputAudioChannel4;
                    private string outputAudioChannel5;
                    private string outputAudioChannel6;
                    private string outputAudioChannel7;
                    private string outputAudioChannel8;
                    private string outputAudioChannel9;
                    private string outputAudioChannel10;
                    private string outputAudioChannel11;
                    private string outputAudioChannel12;
                    private string outputAudioChannel13;
                    private string outputAudioChannel14;
                    private string outputAudioChannel15;
                    private string outputAudioChannel16;
                    public AudioProcessingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        UpdateInputOutputResource(service.Functions);
                        foreach (var function in service.Functions)
                        {
                            foreach (var parameter in function.Parameters)
                            {
                                if (parameter.Id == InputAudioChannel1ProfileParameterId)
                                    inputAudioChannel1 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel2ProfileParameterId)
                                    inputAudioChannel2 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel3ProfileParameterId)
                                    inputAudioChannel3 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel4ProfileParameterId)
                                    inputAudioChannel4 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel5ProfileParameterId)
                                    inputAudioChannel5 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel6ProfileParameterId)
                                    inputAudioChannel6 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel7ProfileParameterId)
                                    inputAudioChannel7 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel8ProfileParameterId)
                                    inputAudioChannel8 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel9ProfileParameterId)
                                    inputAudioChannel9 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel10ProfileParameterId)
                                    inputAudioChannel10 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel11ProfileParameterId)
                                    inputAudioChannel11 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel12ProfileParameterId)
                                    inputAudioChannel12 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel13ProfileParameterId)
                                    inputAudioChannel13 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel14ProfileParameterId)
                                    inputAudioChannel14 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel15ProfileParameterId)
                                    inputAudioChannel15 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == InputAudioChannel16ProfileParameterId)
                                    inputAudioChannel16 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel1ProfileParameterId)
                                    outputAudioChannel1 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel2ProfileParameterId)
                                    outputAudioChannel2 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel3ProfileParameterId)
                                    outputAudioChannel3 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel4ProfileParameterId)
                                    outputAudioChannel4 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel5ProfileParameterId)
                                    outputAudioChannel5 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel6ProfileParameterId)
                                    outputAudioChannel6 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel7ProfileParameterId)
                                    outputAudioChannel7 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel8ProfileParameterId)
                                    outputAudioChannel8 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel9ProfileParameterId)
                                    outputAudioChannel9 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel10ProfileParameterId)
                                    outputAudioChannel10 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel11ProfileParameterId)
                                    outputAudioChannel11 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel12ProfileParameterId)
                                    outputAudioChannel12 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel13ProfileParameterId)
                                    outputAudioChannel13 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel14ProfileParameterId)
                                    outputAudioChannel14 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel15ProfileParameterId)
                                    outputAudioChannel15 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == OutputAudioChannel16ProfileParameterId)
                                    outputAudioChannel16 = System.Convert.ToString(parameter.Value);
                            }
                        }

                        LiveVideoServices = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                    }

                    public override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type Type => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type.AudioProcessing;
                    public string InputAudioChannel1
                    {
                        get
                        {
                            return inputAudioChannel1;
                        }

                        set
                        {
                            inputAudioChannel1 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel1ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel2
                    {
                        get
                        {
                            return inputAudioChannel2;
                        }

                        set
                        {
                            inputAudioChannel2 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel2ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel3
                    {
                        get
                        {
                            return inputAudioChannel3;
                        }

                        set
                        {
                            inputAudioChannel3 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel3ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel4
                    {
                        get
                        {
                            return inputAudioChannel4;
                        }

                        set
                        {
                            inputAudioChannel4 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel4ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel5
                    {
                        get
                        {
                            return inputAudioChannel5;
                        }

                        set
                        {
                            inputAudioChannel5 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel5ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel6
                    {
                        get
                        {
                            return inputAudioChannel6;
                        }

                        set
                        {
                            inputAudioChannel6 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel6ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel7
                    {
                        get
                        {
                            return inputAudioChannel7;
                        }

                        set
                        {
                            inputAudioChannel7 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel7ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel8
                    {
                        get
                        {
                            return inputAudioChannel8;
                        }

                        set
                        {
                            inputAudioChannel8 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel8ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel9
                    {
                        get
                        {
                            return inputAudioChannel9;
                        }

                        set
                        {
                            inputAudioChannel9 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel9ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel10
                    {
                        get
                        {
                            return inputAudioChannel10;
                        }

                        set
                        {
                            inputAudioChannel10 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel10ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel11
                    {
                        get
                        {
                            return inputAudioChannel11;
                        }

                        set
                        {
                            inputAudioChannel11 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel11ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel12
                    {
                        get
                        {
                            return inputAudioChannel12;
                        }

                        set
                        {
                            inputAudioChannel12 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel12ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel13
                    {
                        get
                        {
                            return inputAudioChannel13;
                        }

                        set
                        {
                            inputAudioChannel13 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel13ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel14
                    {
                        get
                        {
                            return inputAudioChannel14;
                        }

                        set
                        {
                            inputAudioChannel14 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel14ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel15
                    {
                        get
                        {
                            return inputAudioChannel15;
                        }

                        set
                        {
                            inputAudioChannel15 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel15ProfileParameterId, value);
                        }
                    }

                    public string InputAudioChannel16
                    {
                        get
                        {
                            return inputAudioChannel16;
                        }

                        set
                        {
                            inputAudioChannel16 = value;
                            UpdateServiceAudioChannelParameter(InputAudioChannel16ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel1
                    {
                        get
                        {
                            return outputAudioChannel1;
                        }

                        set
                        {
                            outputAudioChannel1 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel1ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel2
                    {
                        get
                        {
                            return outputAudioChannel2;
                        }

                        set
                        {
                            outputAudioChannel2 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel2ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel3
                    {
                        get
                        {
                            return outputAudioChannel3;
                        }

                        set
                        {
                            outputAudioChannel3 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel3ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel4
                    {
                        get
                        {
                            return outputAudioChannel4;
                        }

                        set
                        {
                            outputAudioChannel4 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel4ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel5
                    {
                        get
                        {
                            return outputAudioChannel5;
                        }

                        set
                        {
                            outputAudioChannel5 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel5ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel6
                    {
                        get
                        {
                            return outputAudioChannel6;
                        }

                        set
                        {
                            outputAudioChannel6 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel6ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel7
                    {
                        get
                        {
                            return outputAudioChannel7;
                        }

                        set
                        {
                            outputAudioChannel7 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel7ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel8
                    {
                        get
                        {
                            return outputAudioChannel8;
                        }

                        set
                        {
                            outputAudioChannel8 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel8ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel9
                    {
                        get
                        {
                            return outputAudioChannel9;
                        }

                        set
                        {
                            outputAudioChannel9 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel9ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel10
                    {
                        get
                        {
                            return outputAudioChannel10;
                        }

                        set
                        {
                            outputAudioChannel10 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel10ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel11
                    {
                        get
                        {
                            return outputAudioChannel11;
                        }

                        set
                        {
                            outputAudioChannel11 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel11ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel12
                    {
                        get
                        {
                            return outputAudioChannel12;
                        }

                        set
                        {
                            outputAudioChannel12 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel12ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel13
                    {
                        get
                        {
                            return outputAudioChannel13;
                        }

                        set
                        {
                            outputAudioChannel13 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel13ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel14
                    {
                        get
                        {
                            return outputAudioChannel14;
                        }

                        set
                        {
                            outputAudioChannel14 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel14ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel15
                    {
                        get
                        {
                            return outputAudioChannel15;
                        }

                        set
                        {
                            outputAudioChannel15 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel15ProfileParameterId, value);
                        }
                    }

                    public string OutputAudioChannel16
                    {
                        get
                        {
                            return outputAudioChannel16;
                        }

                        set
                        {
                            outputAudioChannel16 = value;
                            UpdateServiceAudioChannelParameter(OutputAudioChannel16ProfileParameterId, value);
                        }
                    }

                    /// <summary>
                    /// The list of live video services using this Audio Processing service.
                    /// </summary>
                    public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService> LiveVideoServices
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The input resource for this Audio Processing Service.
                    /// This overrides the input resource set when initializing the Live Video Services as an Audio Processing service doesn't require all functions.
                    /// </summary>
                    public override Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource InputResource
                    {
                        get
                        {
                            return inputResource;
                        }
                    }

                    /// <summary>
                    /// The output resource for this Audio Processing Service.
                    /// This overrides the output resource set when initializing the Live Video Services as an Audio Processing service doesn't require all functions.
                    /// </summary>
                    public override Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource OutputResource
                    {
                        get
                        {
                            return outputResource;
                        }
                    }

                    /// <summary>
                    /// Update the input and output resource based on what functions of the Audio Processing service are required.
                    /// </summary>
                    /// <param name = "functions">The functions of the Audio Processing service.</param>
                    private void UpdateInputOutputResource(System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function> functions)
                    {
                        if (AudioDeembeddingRequired)
                        {
                            var audioDeembeddingFunction = System.Linq.Enumerable.FirstOrDefault(functions, f => f.Name == AudioDeembeddingFunctionDefinitionName);
                            if (audioDeembeddingFunction != null)
                                inputResource = audioDeembeddingFunction.Resource;
                            if (!AudioDolbyDecodingRequired && !AudioShufflingRequired && !AudioEmbeddingRequired && audioDeembeddingFunction != null)
                            {
                                outputResource = audioDeembeddingFunction.Resource;
                                return;
                            }
                        }

                        if (AudioDolbyDecodingRequired)
                        {
                            var audioDolbyDecodingFunction = System.Linq.Enumerable.FirstOrDefault(functions, f => f.Name == AudioDolbyDecodingFunctionDefinitionName);
                            if (!AudioDeembeddingRequired && audioDolbyDecodingFunction != null)
                                inputResource = audioDolbyDecodingFunction.Resource;
                            if (!AudioShufflingRequired && !AudioEmbeddingRequired && audioDolbyDecodingFunction != null)
                            {
                                outputResource = audioDolbyDecodingFunction.Resource;
                                return;
                            }
                        }

                        if (AudioShufflingRequired)
                        {
                            var audioShufflingFunction = System.Linq.Enumerable.FirstOrDefault(functions, f => f.Name == AudioShufflingFunctionDefinitionName);
                            if (!AudioDeembeddingRequired && !AudioDolbyDecodingRequired && audioShufflingFunction != null)
                                inputResource = audioShufflingFunction.Resource;
                            if (!AudioEmbeddingRequired && audioShufflingFunction != null)
                            {
                                outputResource = audioShufflingFunction.Resource;
                                return;
                            }
                        }

                        if (AudioEmbeddingRequired)
                        {
                            var audioEmbeddingFunction = System.Linq.Enumerable.FirstOrDefault(functions, f => f.Name == AudioEmbeddingFunctionDefinitionName);
                            if (!AudioDeembeddingRequired && !AudioDolbyDecodingRequired && !AudioShufflingRequired && audioEmbeddingFunction != null)
                                inputResource = audioEmbeddingFunction.Resource;
                            if (audioEmbeddingFunction != null)
                                outputResource = audioEmbeddingFunction.Resource;
                        }
                    }

                    private void UpdateServiceAudioChannelParameter(System.Guid parameterId, string value)
                    {
                        if (Service == null)
                        {
                            return;
                        }

                        foreach (var function in Service.Functions)
                        {
                            if (function.Parameters == null)
                            {
                                continue;
                            }

                            var parameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == parameterId);
                            if (parameter != null)
                            {
                                parameter.Value = value;
                                return;
                            }
                        }
                    }
                }

                /// <summary>
                /// Represents a service of type Graphics Processing.
                /// </summary>
                public class GraphicsProcessingService : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService
                {
                    public GraphicsProcessingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        VideoFormat = "1080i50";
                        RemoteGraphics = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.SelectMany(service.Functions, f => f.Parameters), p => p.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.RemoteGraphics)?.StringValue;
                        DestinationsAndTransmissions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                    }

                    public GraphicsProcessingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, string graphicsProcessingEngine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        VideoFormat = "1080i50";
                        RemoteGraphics = graphicsProcessingEngine;
                        DestinationsAndTransmissions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                    }

                    /// <summary>
                    /// Indicates the type of this service.
                    /// </summary>
                    public override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type Type => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type.GraphicsProcessing;
                    /// <summary>
                    /// The list of Destination services that use this Graphics Processing service.
                    /// </summary>
                    public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService> DestinationsAndTransmissions
                    {
                        get;
                        set;
                    }
                }

                /// <summary>
                /// Base class for any Live Video service.
                /// </summary>
                public abstract class LiveVideoService : System.IEquatable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>
                {
                    public readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order Order;
                    protected static readonly System.Guid VideoFormatProfileParameterId = System.Guid.Parse("8dc6df35-c574-4412-bf52-de7e3c78201c");
                    protected static readonly System.Guid RecordingVideoFormatProfileParameterId = System.Guid.Parse("105baed9-4031-4d7c-a436-d26fe608af7b");
                    protected static readonly System.Guid AudioEmbeddingRequiredProfileParameterId = System.Guid.Parse("a2726fd1-cc4b-4b23-b464-3684a0db4dd6");
                    protected static readonly System.Guid AudioDeembeddingRequiredProfileParameterId = System.Guid.Parse("1d51086a-ca66-4e1d-b84f-6fd05a24ee68");
                    protected static readonly System.Guid AudioShufflingRequiredProfileParameterId = System.Guid.Parse("01682ff9-2cc4-4cd5-8252-d5956424479a");
                    protected static readonly System.Guid AudioDolbyDecodingRequiredProfileParameterId = System.Guid.Parse("35215bb4-0faa-423f-beed-58ada9b604b3");
                    protected static readonly string AudioProcessingFunctionRequiredTrueProfileParameterValue = "Yes";
                    protected static readonly string AudioProcessingFunctionRequiredFalseProfileParameterValue = "No";
                    private readonly System.Guid AudioChannel1ProfileParameterId = System.Guid.Parse("d4f1e0fe-1a72-4ec3-9fd8-aefd4785ebdf");
                    private readonly System.Guid AudioChannel2ProfileParameterId = System.Guid.Parse("bce7eff1-a884-47af-a182-559e0cfa4379");
                    private readonly System.Guid AudioChannel3ProfileParameterId = System.Guid.Parse("471d52de-b90c-4d8d-9cb5-b11916a06d08");
                    private readonly System.Guid AudioChannel4ProfileParameterId = System.Guid.Parse("5f014774-c3ce-492e-96c3-cd7402dbe171");
                    private readonly System.Guid AudioChannel5ProfileParameterId = System.Guid.Parse("fad366e6-2f29-466b-a286-434446cf6437");
                    private readonly System.Guid AudioChannel6ProfileParameterId = System.Guid.Parse("5c68fb8a-5c8f-4970-aeb4-b04b5f465724");
                    private readonly System.Guid AudioChannel7ProfileParameterId = System.Guid.Parse("83b3d476-39dd-4660-854f-27bbe6436914");
                    private readonly System.Guid AudioChannel8ProfileParameterId = System.Guid.Parse("e9aeb156-a027-4898-b41e-6af88169a3ff");
                    private readonly System.Guid AudioChannel9ProfileParameterId = System.Guid.Parse("5eadca8d-f96a-464d-81a4-139dc6da0fba");
                    private readonly System.Guid AudioChannel10ProfileParameterId = System.Guid.Parse("7178133a-d8a8-485d-99fa-b909ef73d848");
                    private readonly System.Guid AudioChannel11ProfileParameterId = System.Guid.Parse("052babfe-b396-4d3f-a305-21442b3e0fd1");
                    private readonly System.Guid AudioChannel12ProfileParameterId = System.Guid.Parse("acb1bef4-f80b-424b-abd1-a51a05586a6d");
                    private readonly System.Guid AudioChannel13ProfileParameterId = System.Guid.Parse("fa83f575-0935-4952-b561-ce349c7e59fb");
                    private readonly System.Guid AudioChannel14ProfileParameterId = System.Guid.Parse("f60e0b3e-840a-47f1-b521-9e1dbdc28562");
                    private readonly System.Guid AudioChannel15ProfileParameterId = System.Guid.Parse("2712a2f9-273a-439f-b85a-5101789782cf");
                    private readonly System.Guid AudioChannel16ProfileParameterId = System.Guid.Parse("cc76cdc0-925c-472f-985f-c0c5e477639c");
                    private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.GraphicsProcessingService graphicsProcessing;
                    private readonly System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService> children;
                    private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.VideoProcessingService videoProcessingService;
                    private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService audioProcessingService;
                    private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.RoutingService inputRoutingService;
                    private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.RoutingService outputRoutingService;
                    private string audioEmbedding;
                    private string audioDeembedding;
                    private string audioShuffling;
                    private string audioDolbyDecoding;
                    protected LiveVideoService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order)
                    {
                        Helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                        Order = order;
                        Service = service;
                        // firstFunction en lastFunction is used to select the first or last resource from the service definition (used to connect to matrix input or output)
                        int? firstFunctionPosition = null, lastFunctionPosition = null;
                        foreach (var function in service.Functions)
                        {
                            var functionPositionInServiceDefinition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.GraphExtensions.GetFunctionPosition(service.Definition.Diagram, function);
                            if (!firstFunctionPosition.HasValue || firstFunctionPosition > functionPositionInServiceDefinition)
                            {
                                InputResource = function.Resource;
                                firstFunctionPosition = functionPositionInServiceDefinition;
                            }

                            if (!lastFunctionPosition.HasValue || lastFunctionPosition < functionPositionInServiceDefinition)
                            {
                                OutputResource = function.Resource;
                                lastFunctionPosition = functionPositionInServiceDefinition;
                            }

                            foreach (var parameter in function.Parameters)
                            {
                                if (parameter.Id == VideoFormatProfileParameterId || parameter.Id == RecordingVideoFormatProfileParameterId)
                                    VideoFormat = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioEmbeddingRequiredProfileParameterId)
                                    audioEmbedding = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioDeembeddingRequiredProfileParameterId)
                                    audioDeembedding = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioShufflingRequiredProfileParameterId)
                                    audioShuffling = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioDolbyDecodingRequiredProfileParameterId)
                                    audioDolbyDecoding = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel1ProfileParameterId)
                                    AudioChannel1 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel2ProfileParameterId)
                                    AudioChannel2 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel3ProfileParameterId)
                                    AudioChannel3 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel4ProfileParameterId)
                                    AudioChannel4 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel5ProfileParameterId)
                                    AudioChannel5 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel6ProfileParameterId)
                                    AudioChannel6 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel7ProfileParameterId)
                                    AudioChannel7 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel8ProfileParameterId)
                                    AudioChannel8 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel9ProfileParameterId)
                                    AudioChannel9 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel10ProfileParameterId)
                                    AudioChannel10 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel11ProfileParameterId)
                                    AudioChannel11 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel12ProfileParameterId)
                                    AudioChannel12 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel13ProfileParameterId)
                                    AudioChannel13 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel14ProfileParameterId)
                                    AudioChannel14 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel15ProfileParameterId)
                                    AudioChannel15 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == AudioChannel16ProfileParameterId)
                                    AudioChannel16 = System.Convert.ToString(parameter.Value);
                                else if (parameter.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.ProfileParameterGuids.RemoteGraphics)
                                    RemoteGraphics = parameter.StringValue;
                            }
                        }

                        children = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                        Children = children.AsReadOnly();
                    }

                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers Helpers
                    {
                        get;
                    }

                    /// <summary>
                    /// Indicates the type of this Live Video service.
                    /// </summary>
                    public abstract Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type Type
                    {
                        get;
                    }

                    /// <summary>
                    /// The service object of this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service Service
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// The configured video format for this Live Video service.
                    /// </summary>
                    public string VideoFormat
                    {
                        get;
                        protected set;
                    }

                    /// <summary>
                    /// Indicates if audio embedding is required.
                    /// </summary>
                    public bool AudioEmbeddingRequired
                    {
                        get => audioEmbedding == AudioProcessingFunctionRequiredTrueProfileParameterValue;
                        protected set
                        {
                            audioEmbedding = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
                            if (Service != null)
                            {
                                foreach (var function in Service.Functions)
                                {
                                    var audioEmbeddingParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == AudioEmbeddingRequiredProfileParameterId);
                                    if (audioEmbeddingParameter != null)
                                    {
                                        audioEmbeddingParameter.Value = audioEmbedding;
                                    }
                                }
                            }
                        }
                    }

                    /// <summary>
                    /// Indicates if audio deembedding is required.
                    /// </summary>
                    public bool AudioDeembeddingRequired
                    {
                        get => audioDeembedding == AudioProcessingFunctionRequiredTrueProfileParameterValue;
                        protected set
                        {
                            audioDeembedding = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
                            if (Service == null)
                                return;
                            foreach (var function in Service.Functions)
                            {
                                var audioDeembeddingParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == AudioDeembeddingRequiredProfileParameterId);
                                if (audioDeembeddingParameter != null)
                                {
                                    audioDeembeddingParameter.Value = audioDeembedding;
                                }
                            }
                        }
                    }

                    /// <summary>
                    /// Indicates if audio shuffling is required.
                    /// </summary>
                    public bool AudioShufflingRequired
                    {
                        get => audioShuffling == AudioProcessingFunctionRequiredTrueProfileParameterValue;
                        set
                        {
                            audioShuffling = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
                            if (Service == null)
                                return;
                            foreach (var function in Service.Functions)
                            {
                                var audioShufflingParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == AudioShufflingRequiredProfileParameterId);
                                if (audioShufflingParameter != null)
                                {
                                    audioShufflingParameter.Value = audioShuffling;
                                }
                            }
                        }
                    }

                    /// <summary>
                    /// Indicates if audio dolby decoding is required.
                    /// </summary>
                    public bool AudioDolbyDecodingRequired
                    {
                        get => audioDolbyDecoding == AudioProcessingFunctionRequiredTrueProfileParameterValue;
                        protected set
                        {
                            audioDolbyDecoding = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
                            if (Service == null)
                                return;
                            foreach (var function in Service.Functions)
                            {
                                var audioDolbyDecodingParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == AudioDolbyDecodingRequiredProfileParameterId);
                                if (audioDolbyDecodingParameter != null)
                                {
                                    audioDolbyDecodingParameter.Value = audioDolbyDecoding;
                                }
                            }
                        }
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 1.
                    /// </summary>
                    public string AudioChannel1
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 2.
                    /// </summary>
                    public string AudioChannel2
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 3.
                    /// </summary>
                    public string AudioChannel3
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 4.
                    /// </summary>
                    public string AudioChannel4
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 5.
                    /// </summary>
                    public string AudioChannel5
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 6.
                    /// </summary>
                    public string AudioChannel6
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 7.
                    /// </summary>
                    public string AudioChannel7
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 8.
                    /// </summary>
                    public string AudioChannel8
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 9.
                    /// </summary>
                    public string AudioChannel9
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 10.
                    /// </summary>
                    public string AudioChannel10
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 11.
                    /// </summary>
                    public string AudioChannel11
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 12.
                    /// </summary>
                    public string AudioChannel12
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 13.
                    /// </summary>
                    public string AudioChannel13
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 14.
                    /// </summary>
                    public string AudioChannel14
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 15.
                    /// </summary>
                    public string AudioChannel15
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the configuration for Audio Channel 16.
                    /// </summary>
                    public string AudioChannel16
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates the parent of this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService Parent
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// The list of children of this Live Video service.
                    /// </summary>
                    public System.Collections.ObjectModel.ReadOnlyCollection<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService> Children
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// Indicates what remote graphics are required.
                    /// </summary>
                    public string RemoteGraphics
                    {
                        get;
                        protected set;
                    }

                    /// <summary>
                    /// Reference to the Graphics Processing service used for this Destination.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.GraphicsProcessingService GraphicsProcessing
                    {
                        get => graphicsProcessing;
                        set
                        {
                            if (value == null && graphicsProcessing == null)
                                return;
                            if (value != null && graphicsProcessing != null && graphicsProcessing.Equals(value))
                                return;
                            if (value == null)
                            {
                                graphicsProcessing.DestinationsAndTransmissions.Remove(this);
                            }
                            else
                            {
                                value.DestinationsAndTransmissions.Add(this);
                            }

                            graphicsProcessing = value;
                        }
                    }

                    /// <summary>
                    /// Indicates if the current Graphics Processing configuration is valid.
                    /// </summary>
                    public bool HasValidGraphicsProcessingConfiguration
                    {
                        get
                        {
                            if (IsGraphicsProcessingRequired)
                            {
                                if (GraphicsProcessing == null)
                                    return false;
                                return HasValidGraphicsProcessingService;
                            }
                            else
                            {
                                return GraphicsProcessing == null;
                            }
                        }
                    }

                    /// <summary>
                    /// Indicates if Graphics Processing is required.
                    /// </summary>
                    private bool IsGraphicsProcessingRequired => RemoteGraphics != null && RemoteGraphics != "None";
                    /// <summary>
                    /// Indicates if the current Graphics Processing service is valid.
                    /// </summary>
                    private bool HasValidGraphicsProcessingService => GraphicsProcessing.RemoteGraphics == RemoteGraphics && GraphicsProcessing.Service.Start.ToUniversalTime() <= Service.Start.ToUniversalTime() && GraphicsProcessing.Service.End.ToUniversalTime() >= Service.End.ToUniversalTime();
                    /// <summary>
                    /// The Video Processing service used by this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.VideoProcessingService VideoProcessingService
                    {
                        get => videoProcessingService;
                        set
                        {
                            if (value == null && videoProcessingService == null)
                                return;
                            if (value != null && videoProcessingService != null && videoProcessingService.Equals(value))
                                return;
                            if (value == null)
                            {
                                videoProcessingService.LiveVideoServices.Remove(this);
                            }
                            else
                            {
                                value.LiveVideoServices.Add(this);
                            }

                            videoProcessingService = value;
                        }
                    }

                    /// <summary>
                    /// The Audio Processing service used by this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.AudioProcessingService AudioProcessingService
                    {
                        get => audioProcessingService;
                        set
                        {
                            if (value == null && audioProcessingService == null)
                                return;
                            if (value != null && audioProcessingService != null && audioProcessingService.Equals(value))
                                return;
                            if (value == null)
                            {
                                audioProcessingService.LiveVideoServices.Remove(this);
                            }
                            else
                            {
                                value.LiveVideoServices.Add(this);
                            }

                            audioProcessingService = value;
                        }
                    }

                    /// <summary>
                    /// The Input Routing service for this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.RoutingService InputRoutingService
                    {
                        get => inputRoutingService;
                        set
                        {
                            if (value == null && inputRoutingService == null)
                                return;
                            if (value != null && inputRoutingService != null && inputRoutingService.Equals(value))
                                return;
                            inputRoutingService = value;
                        }
                    }

                    /// <summary>
                    /// The Output Routing service for this Live Video service.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.RoutingService OutputRoutingService
                    {
                        get => outputRoutingService;
                        set
                        {
                            if (value == null && outputRoutingService == null)
                                return;
                            if (value != null && outputRoutingService != null && outputRoutingService.Equals(value))
                                return;
                            outputRoutingService = value;
                        }
                    }

                    /// <summary>
                    /// The input resource for this Live Video service.
                    /// </summary>
                    public virtual Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource InputResource
                    {
                        get;
                    }

                    /// <summary>
                    /// The output resource for this Live Video service.
                    /// </summary>
                    public virtual Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource OutputResource
                    {
                        get;
                    }

                    /// <summary>
                    /// Check if the provided object is the same as this service.
                    /// </summary>
                    /// <param name = "obj">The object to check.</param>
                    /// <returns>True if both objects are the same.</returns>
                    public override bool Equals(object obj)
                    {
                        if (obj == null)
                        {
                            return false;
                        }

                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService other = obj as Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService;
                        if (other == null)
                        {
                            return false;
                        }

                        return Equals(other);
                    }

                    /// <summary>
                    /// Check if the provided Live Video service object is the same as this service.
                    /// </summary>
                    /// <param name = "other">The Live Video service to check.</param>
                    /// <returns>True if both services are the same.</returns>
                    public bool Equals(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService other)
                    {
                        return other != null && other.Service.Equals(Service);
                    }

                    /// <summary>
                    /// Get the hash code for this Live Video service.
                    /// </summary>
                    /// <returns>The hash code.</returns>
                    public override int GetHashCode()
                    {
                        return Service.GetHashCode();
                    }

                    protected void Log(string nameOfMethod, string message)
                    {
                        Helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService), nameOfMethod, message, Service.Name);
                    }
                }

                public class RoutingService : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService
                {
                    private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource matrixInputSdi;
                    private Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource matrixOutputSdi;
                    public RoutingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        foreach (var function in service.Functions)
                        {
                            if (function.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.MatrixInputSdi)
                            {
                                matrixInputSdi = function.Resource;
                                function.ResourceChanged += (o, e) => matrixInputSdi = function.Resource;
                            }
                            else if (function.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.MatrixOutputSdi)
                            {
                                matrixOutputSdi = function.Resource;
                                function.ResourceChanged += (o, e) => matrixOutputSdi = function.Resource;
                            }
                        }
                    }

                    public override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type Type => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type.Routing;
                    public Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource MatrixInputSdi
                    {
                        get => matrixInputSdi;
                        set
                        {
                            if (matrixInputSdi == null && value == null)
                                return;
                            if (matrixInputSdi != null && matrixInputSdi.Equals(value))
                                return;
                            if (MatrixInputSdiIsValid)
                            {
                                Log($"{MatrixInputSdi}.Set", $"Matrix Input SDI resource is already set to '{MatrixInputSdi?.Name}' and should not be changed anymore.");
                                throw new System.InvalidOperationException($"Service {Service?.Name} matrix input SDI resource is already set to '{MatrixInputSdi?.Name}' and should not be changed anymore to '{value?.Name}'");
                            }

                            matrixInputSdi = value;
                            if (Service == null)
                                return;
                            var function = System.Linq.Enumerable.FirstOrDefault(Service.Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.MatrixInputSdi);
                            if (function != null)
                            {
                                function.Resource = value;
                                MatrixInputSdiIsValid = true;
                                Log($"{MatrixInputSdi}.Set", $"Set service {Service.Name} function {function.Name} resource to '{function.ResourceName}'.");
                            }
                        }
                    }

                    public bool MatrixInputSdiIsValid
                    {
                        get;
                        set;
                    }

                    = false;
                    public Skyline.DataMiner.Net.ResourceManager.Objects.FunctionResource MatrixOutputSdi
                    {
                        get => matrixOutputSdi;
                        set
                        {
                            if (matrixOutputSdi == null && value == null)
                                return;
                            if (matrixOutputSdi != null && matrixOutputSdi.Equals(value))
                                return;
                            if (MatrixOutputSdiIsValid)
                            {
                                Log($"{MatrixOutputSdi}.Set", $"Matrix Output SDI resource is already set to '{MatrixOutputSdi?.Name}' and should not be changed anymore.");
                                throw new System.InvalidOperationException($"Service {Service?.Name} matrix output SDI resource is already set to '{MatrixOutputSdi?.Name}' and should not be changed anymore to '{value?.Name}'");
                            }

                            matrixOutputSdi = value;
                            if (Service == null)
                                return;
                            var function = System.Linq.Enumerable.FirstOrDefault(Service.Functions, f => f.Id == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.MatrixOutputSdi);
                            if (function != null)
                            {
                                function.Resource = value;
                                MatrixOutputSdiIsValid = true;
                                Log($"{MatrixOutputSdi}.Set", $"Set service {Service.Name} function {function.Name} resource to '{function.ResourceName}'.");
                            }
                        }
                    }

                    public bool MatrixOutputSdiIsValid
                    {
                        get;
                        set;
                    }

                    = false;
                    public override string ToString()
                    {
                        return $"{Service.Name} ({MatrixInputSdi?.Name},{MatrixOutputSdi?.Name})";
                    }
                }

                /// <summary>
                /// Represents a service of type Video Processing.
                /// </summary>
                public class VideoProcessingService : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService
                {
                    private static readonly System.Guid InputVideoFormatProfileParameterId = System.Guid.Parse("79a7d1f5-d750-400f-b5b9-b5e6d198ad25");
                    private static readonly System.Guid OutputVideoFormatProfileParameterId = System.Guid.Parse("209d06c7-3b7a-49ae-a11f-b6d9f5920fca");
                    public VideoProcessingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        foreach (var function in service.Functions)
                        {
                            var inputVideoFormatParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == InputVideoFormatProfileParameterId);
                            if (inputVideoFormatParameter != null)
                            {
                                InputVideoFormat = System.Convert.ToString(inputVideoFormatParameter.Value);
                            }

                            var outputVideoFormatParameter = System.Linq.Enumerable.FirstOrDefault(function.Parameters, p => p.Id == OutputVideoFormatProfileParameterId);
                            if (outputVideoFormatParameter != null)
                            {
                                OutputVideoFormat = System.Convert.ToString(outputVideoFormatParameter.Value);
                            }

                            if (InputVideoFormat != null && OutputVideoFormat != null)
                            {
                                break;
                            }
                        }

                        LiveVideoServices = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                    }

                    public VideoProcessingService(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, string inputVideoFormat, string outputVideoFormat, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Order order): base(helpers, service, order)
                    {
                        InputVideoFormat = inputVideoFormat;
                        OutputVideoFormat = outputVideoFormat;
                        LiveVideoServices = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService>();
                    }

                    /// <summary>
                    /// Indicates the type of this service.
                    /// </summary>
                    public override Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type Type => Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Type.VideoProcessing;
                    /// <summary>
                    /// The input video format of this Video Processing service.
                    /// </summary>
                    public string InputVideoFormat
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// The output video format of this Video Processing service.
                    /// </summary>
                    public string OutputVideoFormat
                    {
                        get;
                        private set;
                    }

                    /// <summary>
                    /// The list of live video services using this Video Processing service.
                    /// </summary>
                    public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Auto_Generation.LiveVideoService> LiveVideoServices
                    {
                        get;
                        set;
                    }
                }
            }

            namespace Configuration
            {
                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class FunctionConfiguration
                {
                    /// <summary>
                    /// The ID of the function.
                    /// </summary>
                    public System.Guid Id
                    {
                        get;
                        set;
                    }

                    public string Name
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The profile parameters configured for this function.
                    /// The key is the id of the profile parameter and the value is the actual value that was configured.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Collections.Generic.Dictionary<System.Guid, System.Object> ProfileParameters
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The resource assigned to this function
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Guid ResourceId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The resource name assigned to this function
                    /// </summary>
                    public string ResourceName
                    {
                        get;
                        set;
                    }

                    public bool RequiresResource
                    {
                        get;
                        set;
                    }

                    public bool McrHasOverruledFixedTieLineLogic
                    {
                        get;
                        set;
                    }

                    = false;
                    public override string ToString()
                    {
                        return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                    }
                }

                [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
                public class ServiceConfiguration
                {
                    /// <summary>
                    /// The ID of the service.
                    /// </summary>
                    public System.Guid Id
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The name of the service.
                    /// </summary>
                    public string Name
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Start time configured for this service.
                    /// </summary>
                    public System.DateTime Start
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The preroll of the Service.
                    /// This was added in case it doesn't match the predefined prerolls when booked very fast.
                    /// </summary>
                    public System.TimeSpan PreRoll
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// End time configured for this service.
                    /// </summary>
                    public System.DateTime End
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The service level (backup) for the service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.BackupType? ServiceLevel
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Linked service Id, only applicable when active backup applies on the order this service belongs.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Guid LinkedServiceId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Linked event Ids, only applicable for every type of event level reception service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Collections.Generic.HashSet<System.String> LinkedEventIds
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The function configurations for each function in this service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.FunctionConfiguration> Functions
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The id of the service definition that is used for this service.
                    /// </summary>
                    public System.Guid ServiceDefinitionId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates which integration created this Service.
                    /// If not specified this will be set to None.
                    /// </summary>
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.IntegrationType IntegrationType
                    {
                        get;
                        set;
                    }

                    public bool IntegrationIsMaster
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if this is an Eurovision service.
                    /// </summary>
                    public bool IsEurovisionService
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// This is set to true from the HandleIntegrationUpdate script if the synopsis contains a value for the ioMuxName field.
                    /// The ioMuxName field is used to define multiple multiplexed feeds in the signal.
                    /// If this boolean is true, you will be able to edit the Service Selection profile parameter of the Satellite Reception in the LiveOrderForm.
                    /// </summary>
                    public bool IsEurovisionMultiFeedService
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if this service is an Event Level Reception or not.
                    /// </summary>
                    public bool IsEventLevelReception
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if this service is an global Event Level Reception or not.
                    /// </summary>
                    public bool IsGlobalEventLevelReception
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The id of the work order that has been requested from EBU.
                    /// This can be used to map the incoming Eurovision synopsis to this service correctly.
                    /// This is only applicable for EBU services that were requested through the LiveOrderForm.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string EurovisionWorkOrderId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The transmission number of the Eurovision synopsis in case of a Eurovision service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string EurovisionTransmissionNumber
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Serialized data of the Eurovision booking details as configured in the Customer UI.
                    /// This is only applicable in case of an Eurovision service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision.EurovisionBookingDetails EurovisionBookingDetails
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// This property is only used when the Service was generated by the EBU integration.
                    /// In that case this property will contain the possible Receptions or Transmissions.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration.ServiceConfiguration> EurovisionServiceConfigurations
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// This property is only used when the Service is a Reception Service that was generated by the EBU integration.
                    /// In that case this property will contain the id of the destination in the synopsis on which this service is based. 
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string EurovisionDestinationId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// This property is only used when the Service is a Reception or Transmission Service that was generated by the EBU integration.
                    /// In that case this property will contain the id of the technical system in the synopsis on which this service is based. 
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string EurovisionTechnicalSystemId
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// If an issue has been reported manually via the UpdateService script and when the order is finished.
                    /// The service will have the status Completed With Errors at the end.
                    /// </summary>
                    public bool HasAnIssueBeenreportedManually
                    {
                        get;
                        set;
                    }

                    = false;
                    /// <summary>
                    /// Comments for this service.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string Comments
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// <summary>
                    /// Contact Information Name.
                    /// Only applicable for LiveU receptions.
                    /// Will be saved in a custom property for LiveU receptions;
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string ContactInformationName
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Contact Information Telephone Number.
                    /// Only applicable for LiveU receptions.
                    /// Will be saved in a custom property for LiveU receptions;
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string ContactInformationTelephoneNumber
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Additional information LiveU device name.
                    /// Only applicable for LiveU receptions or transmissions.
                    /// Will be saved in a custom property for LiveU receptions or transmissions;
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string LiveUDeviceName
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Normal users will fill in this field when LiveU is selected
                    /// News users will be able to fill in this field for every reception service.
                    /// </summary>
                    public string AudioReturnInfo
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Additional information Vidigo Stream Source Link.
                    /// Only applicable for IP Receptions.
                    /// Will be saved in a custom property.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string VidigoStreamSourceLink
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Additional information about the desired source that should be selected whenever they want to book an order definitively.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string AdditionalDescriptionUnknownSource
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if a source service is unknown.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public bool IsUnknownSourceService
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if the audio configuration is copied from source.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public bool IsAudioConfigurationCopiedFromSource
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if this service requires routing.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public bool RequiresRouting
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if routing config should be updated whenever applicable.
                    /// </summary>
                    public bool RoutingConfigurationUpdateRequired
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// The recording configuration with additional recording details that are not stored as profile parameters.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.RecordingConfiguration RecordingConfiguration
                    {
                        get;
                        set;
                    }

                    public System.Collections.Generic.HashSet<System.Int32> SecurityViewIds
                    {
                        get;
                        set;
                    }

                    /// <summary>
                    /// Indicates if this service requires routing.
                    /// </summary>
                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public System.Collections.Generic.HashSet<System.Guid> OrderReferences
                    {
                        get;
                        set;
                    }

                    [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
                    public string NameOfServiceToTransmit
                    {
                        get;
                        set;
                    }

                    public bool ChangedByUpdateServiceScript
                    {
                        get;
                        set;
                    }

                    = false;
                }
            }
        }

        namespace ServiceDefinition
        {
            public class ContributingConfig
            {
                public string ParentSystemFunction
                {
                    get;
                    set;
                }

                public string ResourcePool
                {
                    get;
                    set;
                }

                /// <summary>
                /// The appendix name for the contributed resources created for each service
                /// Only required when action is "NEW" or "EDIT" and only for a service (not for the order itself) (can be found in Contributing Config property of SD)
                /// </summary>
                public string ReservationAppendixName
                {
                    get;
                    set;
                }

                public string LifeCycle
                {
                    get;
                    set;
                }

                public bool ConvertToContributing
                {
                    get;
                    set;
                }
            }

            public enum VirtualPlatform
            {
                [System.ComponentModel.Description("None")]
                None,
                [System.ComponentModel.Description("Reception.Satellite")]
                ReceptionSatellite,
                [System.ComponentModel.Description("Reception.LiveU")]
                ReceptionLiveU,
                [System.ComponentModel.Description("Reception.Fiber")]
                ReceptionFiber,
                [System.ComponentModel.Description("Reception.Fixed Line")]
                ReceptionFixedLine,
                [System.ComponentModel.Description("Reception.Fixed Service")]
                ReceptionFixedService,
                [System.ComponentModel.Description("Reception.IP")]
                ReceptionIp,
                [System.ComponentModel.Description("Reception.Microwave")]
                ReceptionMicrowave,
                [System.ComponentModel.Description("Reception.Eurovision")]
                ReceptionEurovision,
                [System.ComponentModel.Description("Reception.Unknown")]
                ReceptionUnknown,
                [System.ComponentModel.Description("Reception.Commentary Audio")]
                ReceptionCommentaryAudio,
                [System.ComponentModel.Description("Recording")]
                Recording,
                [System.ComponentModel.Description("Routing")]
                Routing,
                [System.ComponentModel.Description("Destination")]
                Destination,
                [System.ComponentModel.Description("Audio Processing")]
                AudioProcessing,
                [System.ComponentModel.Description("Video Processing")]
                VideoProcessing,
                [System.ComponentModel.Description("Graphics Processing")]
                GraphicsProcessing,
                [System.ComponentModel.Description("Transmission.Satellite")]
                TransmissionSatellite,
                [System.ComponentModel.Description("Transmission.LiveU")]
                TransmissionLiveU,
                [System.ComponentModel.Description("Transmission.Fiber")]
                TransmissionFiber,
                [System.ComponentModel.Description("Transmission.IP")]
                TransmissionIp,
                [System.ComponentModel.Description("Transmission.Microwave")]
                TransmissionMicrowave,
                [System.ComponentModel.Description("Transmission.Eurovision")]
                TransmissionEurovision,
                // TODO: is this still being used?
                [System.ComponentModel.Description("File Playout")]
                FilePlayout,
                [System.ComponentModel.Description("File Processing")]
                FileProcessing
            }

            public enum VirtualPlatformType
            {
                [System.ComponentModel.Description("None")]
                None,
                [System.ComponentModel.Description("Reception")]
                Reception,
                [System.ComponentModel.Description("Recording")]
                Recording,
                [System.ComponentModel.Description("Destination")]
                Destination,
                [System.ComponentModel.Description("Routing")]
                Routing,
                [System.ComponentModel.Description("Transmission")]
                Transmission,
                [System.ComponentModel.Description("Audio Processing")]
                AudioProcessing,
                [System.ComponentModel.Description("Video Processing")]
                VideoProcessing,
                [System.ComponentModel.Description("Graphics Processing")]
                GraphicsProcessing
            }

            public enum VirtualPlatformName
            {
                [System.ComponentModel.Description("None")]
                None,
                [System.ComponentModel.Description("Satellite")]
                Satellite,
                [System.ComponentModel.Description("LiveU")]
                LiveU,
                [System.ComponentModel.Description("Fiber")]
                Fiber,
                [System.ComponentModel.Description("Fixed Line")]
                FixedLine,
                [System.ComponentModel.Description("Fixed Service")]
                FixedService,
                [System.ComponentModel.Description("IP")]
                IP,
                [System.ComponentModel.Description("Microwave")]
                Microwave,
                [System.ComponentModel.Description("Eurovision")]
                Eurovision,
                [System.ComponentModel.Description("Unknown")]
                Unknown,
                [System.ComponentModel.Description("Recording")]
                Recording,
                [System.ComponentModel.Description("Destination")]
                Destination,
                [System.ComponentModel.Description("Routing")]
                Routing,
                [System.ComponentModel.Description("Audio Processing")]
                AudioProcessing,
                [System.ComponentModel.Description("Video Processing")]
                VideoProcessing,
                [System.ComponentModel.Description("Graphics Processing")]
                GraphicsProcessing,
                [System.ComponentModel.Description("Commentary Audio")]
                CommentaryAudio
            }

            public static class GraphExtensions
            {
                /// <summary>
                /// Gets the 0-based position of the function in the (straight-line) service definition.
                /// </summary>
                /// <remarks>The NodeId of a function does not always correspond to the position of the function in the service definition, hence this method.</remarks>
                public static int GetFunctionPosition(this Skyline.DataMiner.Net.ServiceManager.Objects.Graph graph, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function)
                {
                    bool nodeExistsWithMoreThanTwoEdges = System.Linq.Enumerable.Any(graph.Nodes, n => System.Linq.Enumerable.Count(graph.Edges, e => e.ToNode.Equals(n) || e.FromNode.Equals(n)) > 2);
                    if (nodeExistsWithMoreThanTwoEdges)
                        throw new System.ArgumentException("Graph is not a straight line", nameof(graph));
                    Skyline.DataMiner.Net.ServiceManager.Objects.Node nodeOfFunction = System.Linq.Enumerable.FirstOrDefault(graph.Nodes, n => n.Configuration.FunctionID == function.Id);
                    if (nodeOfFunction == null)
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.NodeNotFoundException(function.Id);
                    int position = 0;
                    Skyline.DataMiner.Net.ServiceManager.Objects.Node node = nodeOfFunction;
                    while (System.Linq.Enumerable.Any(graph.Edges, e => e.ToNode == node))
                    {
                        Skyline.DataMiner.Net.ServiceManager.Objects.Edge edge = System.Linq.Enumerable.FirstOrDefault(graph.Edges, e => e.ToNode == node);
                        if (node == null)
                            throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.EdgeNotFoundException();
                        if (edge.FromNode != null)
                        {
                            node = edge.FromNode;
                        }
                        else
                        {
                            break;
                        }

                        position++;
                    }

                    return position;
                }
            }

            public interface IServiceDefinitionManager
            {
                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition RoutingServiceDefinition
                {
                    get;
                }

                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition AudioProcessingServiceDefinition
                {
                    get;
                }

                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GraphicsProcessingServiceDefinition
                {
                    get;
                }

                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition VideoProcessingServiceDefinition
                {
                    get;
                }

                Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GetServiceDefinition(System.Guid serviceDefinitionGuid);
                Skyline.DataMiner.Automation.Element GetBookingManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform virtualPlatform);
            }

            public class ServiceDefinition
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform virtualPlatform;
                public ServiceDefinition()
                {
                }

                public ServiceDefinition(string virtualPlatform)
                {
                    if (!System.Linq.Enumerable.Contains(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumDescriptions<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform>(), virtualPlatform))
                        throw new System.ArgumentException(System.String.Format("Unknown Virtual Platform {0}", virtualPlatform), "virtualPlatform");
                    this.virtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform>(virtualPlatform);
                }

                public ServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform virtualPlatform)
                {
                    this.virtualPlatform = virtualPlatform;
                }

                /// <summary>
                /// The id (GUID) of the service definition
                /// Only required when action is "NEW" or "EDIT"
                /// </summary>
                public System.Guid Id
                {
                    get;
                    set;
                }

                public string Name
                {
                    get;
                    set;
                }

                public string Description
                {
                    get;
                    set;
                }

                public bool IsDefault
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if an order with a service using this service definition only allows a source (no destinations can be added).
                /// </summary>
                public bool IsSourceOnly
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this service definition can only be selected by MCR users.
                /// </summary>
                public bool IsMcrOnly
                {
                    get;
                    set;
                }

                /// <summary>
                /// Indicates if this service definition can only be used by integration orders.
                /// </summary>
                public bool IsIntegrationOnly
                {
                    get;
                    set;
                }

                /// <summary>
                /// The booking manager element name that will be used for the order bookings
                /// Always required
                /// </summary>
                public string BookingManagerElementName
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig ContributingConfig
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform VirtualPlatform
                {
                    get
                    {
                        return virtualPlatform;
                    }
                }

                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType VirtualPlatformServiceType
                {
                    get
                    {
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType>(System.Linq.Enumerable.First(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(virtualPlatform).Split('.')));
                    }
                }

                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName VirtualPlatformServiceName
                {
                    get
                    {
                        return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName>(System.Linq.Enumerable.Last(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(virtualPlatform).Split('.')));
                    }
                }

                [Newtonsoft.Json.JsonIgnore]
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition> FunctionDefinitions
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonIgnore]
                public Skyline.DataMiner.Net.ServiceManager.Objects.Graph Diagram
                {
                    get;
                    set;
                }

                [Newtonsoft.Json.JsonIgnore]
                public bool IsDummy
                {
                    get
                    {
                        return VirtualPlatformServiceName == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformName.None;
                    }
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GenerateDummyReceptionServiceDefinition()
                {
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.None)
                    {Name = "Dummy Reception", Id = System.Guid.Empty, ContributingConfig = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig{ParentSystemFunction = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception"}, FunctionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition>(), IsDefault = true, IsSourceOnly = false, IsMcrOnly = false, IsIntegrationOnly = false};
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GenerateDummyUnknownReceptionServiceDefinition()
                {
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionUnknown)
                    {Name = "Unknown Reception", Id = System.Guid.Empty, ContributingConfig = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig{ParentSystemFunction = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception"}, FunctionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition>(), IsDefault = true, IsSourceOnly = false, IsMcrOnly = false, IsIntegrationOnly = false};
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GenerateEurovisionReceptionServiceDefinition()
                {
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.ReceptionEurovision)
                    {Name = "Eurovision Reception", Id = System.Guid.Empty, ContributingConfig = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig{ParentSystemFunction = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.SourceServiceSystemFunctionId, ResourcePool = "Reception"}, FunctionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition>(), IsDefault = true, IsSourceOnly = false, IsMcrOnly = false, IsIntegrationOnly = false};
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GenerateEurovisionTransmissionServiceDefinition()
                {
                    return new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform.TransmissionEurovision)
                    {Name = "Eurovision Transmission", Id = System.Guid.Empty, ContributingConfig = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig{ParentSystemFunction = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager.TransmissionServiceSystemFunctionId, ResourcePool = "Transmission"}, FunctionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition>(), IsDefault = true, IsSourceOnly = false, IsMcrOnly = false, IsIntegrationOnly = false};
                }

                public override string ToString()
                {
                    if (Diagram == null)
                        return "Diagram is null";
                    var sb = new System.Text.StringBuilder();
                    foreach (var node in Diagram.Nodes)
                    {
                        var parentEdge = System.Linq.Enumerable.SingleOrDefault(Diagram.Edges, e => e.ToNode.ID == node.ID);
                        var parentNode = parentEdge != null ? parentEdge.FromNode.ToString() : string.Empty;
                        var childEdges = System.Linq.Enumerable.Where(Diagram.Edges, e => e.FromNode.ID == node.ID);
                        var childNodes = string.Join(",", System.Linq.Enumerable.Select(childEdges, e => e.ToNode.ID));
                        var nodeInfo = $"Node {node.ID} (parent ={parentNode}, children={childNodes})";
                        sb.Append($"{nodeInfo} ; ");
                    }

                    return sb.ToString();
                }

                public override bool Equals(object obj)
                {
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition other = obj as Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition;
                    if (other == null)
                        return false;
                    return Id.Equals(other.Id);
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }

                public bool FunctionIsFirst(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function function)
                {
                    return Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.GraphExtensions.GetFunctionPosition(Diagram, function) == 0;
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Newtonsoft.Json.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLNetTypes.dll")]
            public class ServiceDefinitionManager : Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.IServiceDefinitionManager
            {
                private const string IsDefaultPropertyName = "IsDefault";
                private const string IsSourceOnlyPropertyName = "IsSourceOnly";
                private const string IsMcrOnlyPropertyName = "IsMcrOnly";
                private const string IsIntegrationOnlyPropertyName = "IsIntegrationOnly";
                private const string VirtualPlatformPropertyName = "Virtual Platform";
                private const string ContributiongConfigPropertyName = "Contributing Config";
                private const string BookingManagerProtocolName = "Skyline Booking Manager";
                private const int BookingManagerProtocolVirtualPlatformParameterId = 123;
                // cached to improve performance (don't use these fields, use the properties)
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition routingServiceDefinition;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition graphicsProcessingServiceDefinition;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition videoProcessingServiceDefinition;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition audioProcessingServiceDefinition;
                private System.Collections.Generic.List<Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition> allNonOrderSrmServiceDefinitions;
                private System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.Messages.FunctionDefinition> allProtocolFunctionDefinitions; // initialized by some methods to improve performance
                public ServiceDefinitionManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers helpers)
                {
                    Helpers = helpers ?? throw new System.ArgumentNullException(nameof(helpers));
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers Helpers
                {
                    get;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition RoutingServiceDefinition => routingServiceDefinition ?? (routingServiceDefinition = GetServiceDefinition("Routing"));
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GraphicsProcessingServiceDefinition => graphicsProcessingServiceDefinition ?? (graphicsProcessingServiceDefinition = GetServiceDefinition("Graphics Processing"));
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition VideoProcessingServiceDefinition => videoProcessingServiceDefinition ?? (videoProcessingServiceDefinition = GetServiceDefinition("Video Processing"));
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition AudioProcessingServiceDefinition => audioProcessingServiceDefinition ?? (audioProcessingServiceDefinition = GetServiceDefinition("Audio Processing"));
                private System.Collections.Generic.List<Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition> AllNonOrderSrmServiceDefinitions => allNonOrderSrmServiceDefinitions ?? (allNonOrderSrmServiceDefinitions = GetAllNonOrderSrmServiceDefinitions());
                private System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.Messages.FunctionDefinition> AllProtocolFunctionDefinitions => allProtocolFunctionDefinitions ?? (allProtocolFunctionDefinitions = GetAllProtocolFunctionDefinitions());
                /// <remarks>ONLY TO BE USED FOR NON-ORDER SERVICE DEFINITIONS</remarks>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GetServiceDefinition(System.Guid serviceDefinitionGuid)
                {
                    try
                    {
                        LogMethodStart(nameof(GetServiceDefinition), out var stopwatch);
                        var srmServiceDefinition = System.Linq.Enumerable.SingleOrDefault(AllNonOrderSrmServiceDefinitions, sd => sd.ID == serviceDefinitionGuid);
                        if (srmServiceDefinition == null)
                            throw new Skyline.DataMiner.Library.Exceptions.ServiceDefinitionNotFoundException(serviceDefinitionGuid);
                        var virtualPlatformProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, VirtualPlatformPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                        string virtualPlatformName = virtualPlatformProperty.Value;
                        var contributingConfig = GetContributingConfig(srmServiceDefinition);
                        if (contributingConfig == null)
                        {
                            // Service Definition should always contain a Contributing Config
                            return null;
                        }

                        var serviceDefinition = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(virtualPlatformName)
                        {Id = srmServiceDefinition.ID, Name = srmServiceDefinition.Name, Description = srmServiceDefinition.Description ?? string.Empty, BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition), ContributingConfig = contributingConfig, FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition), Diagram = srmServiceDefinition.Diagram, IsDefault = IsDefault(srmServiceDefinition), IsSourceOnly = IsSourceOnly(srmServiceDefinition), IsMcrOnly = IsMcrOnly(srmServiceDefinition), IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)};
                        LogMethodCompleted(nameof(GetServiceDefinition), stopwatch);
                        return serviceDefinition;
                    }
                    catch (System.Exception e)
                    {
                        Log(nameof(GetServiceDefinition), $"Something went wrong while getting service definition {serviceDefinitionGuid}: {e}");
                        return null;
                    }
                }

                public Skyline.DataMiner.Automation.Element GetBookingManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform virtualPlatform)
                {
                    var bookingManagers = Helpers.Engine.FindElementsByProtocol(BookingManagerProtocolName);
                    var bookingManager = System.Linq.Enumerable.FirstOrDefault(bookingManagers, b => b != null && b.IsActive && System.Convert.ToString(b.GetParameter(BookingManagerProtocolVirtualPlatformParameterId)).Equals(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(virtualPlatform)));
                    return bookingManager;
                }

                private bool IsDefault(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
                {
                    var isDefaultProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, IsDefaultPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    return isDefaultProperty != null && System.Convert.ToBoolean(isDefaultProperty.Value);
                }

                private bool IsSourceOnly(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
                {
                    var isSourceOnlyProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, IsSourceOnlyPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    return isSourceOnlyProperty != null && System.Convert.ToBoolean(isSourceOnlyProperty.Value);
                }

                private bool IsMcrOnly(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
                {
                    var isMcrOnlyProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, IsMcrOnlyPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    return isMcrOnlyProperty != null && System.Convert.ToBoolean(isMcrOnlyProperty.Value);
                }

                private bool IsIntegrationOnly(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
                {
                    var isIntegrationOnlyProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, IsIntegrationOnlyPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                    return isIntegrationOnlyProperty != null && System.Convert.ToBoolean(isIntegrationOnlyProperty.Value);
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition GetServiceDefinition(string nameFilter)
                {
                    foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
                    {
                        if (!srmServiceDefinition.Name.Contains(nameFilter) || srmServiceDefinition.Properties == null)
                            continue;
                        var virtualPlatformProperty = System.Linq.Enumerable.FirstOrDefault(srmServiceDefinition.Properties, p => System.String.Equals(p.Name, VirtualPlatformPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                        if (virtualPlatformProperty == null || System.String.IsNullOrWhiteSpace(virtualPlatformProperty.Value))
                            continue;
                        string virtualPlatformName = virtualPlatformProperty.Value;
                        var contributingConfig = GetContributingConfig(srmServiceDefinition);
                        if (contributingConfig == null)
                            continue;
                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition serviceDefinition = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition(virtualPlatformName)
                        {Id = srmServiceDefinition.ID, Name = srmServiceDefinition.Name, Description = srmServiceDefinition.Description, BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition), ContributingConfig = contributingConfig, FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition), Diagram = srmServiceDefinition.Diagram, IsDefault = IsDefault(srmServiceDefinition), IsSourceOnly = IsSourceOnly(srmServiceDefinition), IsMcrOnly = IsMcrOnly(srmServiceDefinition), IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)};
                        return serviceDefinition;
                    }

                    return null;
                }

                /// <summary>
                /// Retrieve the function definitions for a given service definition.
                /// </summary>
                /// <param name = "serviceDefinition">The service definition.</param>
                /// <returns>Returns the function definitions for each function in the service definition in the order as they should be configured.</returns>
                private System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition> GetFunctionDefinitions(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
                {
                    var functionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition>();
                    var nodes = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(serviceDefinition.Diagram.Nodes, x => !System.Linq.Enumerable.Any(serviceDefinition.Diagram.Edges, y => y.ToNodeID == x.ID)));
                    while (System.Linq.Enumerable.Any(nodes))
                    {
                        var childNodes = new System.Collections.Generic.List<Skyline.DataMiner.Net.ServiceManager.Objects.Node>();
                        foreach (var node in nodes)
                        {
                            int order = -1;
                            string options = System.String.Empty;
                            string resourcePool = System.String.Empty;
                            bool isManualResourceSelectionAllowed = false;
                            bool isHidden = false;
                            foreach (var property in node.Properties)
                            {
                                switch (property.Name)
                                {
                                    case "ConfigurationOrder":
                                        if (System.Int32.TryParse(property.Value, out var configurationOrder))
                                            order = configurationOrder;
                                        break;
                                    case "Options":
                                        options = property.Value;
                                        break;
                                    case "Resource Pool":
                                        resourcePool = property.Value;
                                        break;
                                    case "IsManualResourceSelectionAllowed":
                                        isManualResourceSelectionAllowed = !System.String.IsNullOrEmpty(property.Value) && System.Convert.ToBoolean(property.Value);
                                        break;
                                    case "IsHidden":
                                        isHidden = !System.String.IsNullOrEmpty(property.Value) && System.Convert.ToBoolean(property.Value);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            functionDefinitions.Add(GetFunctionDefinition(node.Configuration.FunctionID, node.Label, order, options, resourcePool, isManualResourceSelectionAllowed, isHidden));
                            childNodes.AddRange(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(serviceDefinition.Diagram.Edges, x => x.FromNodeID == node.ID), x => x.ToNode));
                        }

                        // this is used to make sure the function definitions are ordered the same as the nodes in the SD
                        nodes.Clear();
                        nodes.AddRange(childNodes);
                    }

                    return functionDefinitions;
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition GetFunctionDefinition(System.Guid functionId, string label, int order, string options, string resourcePool, bool isManualResourceSelectionAllowed, bool isHidden)
                {
                    var srmFunctionDefinition = System.Linq.Enumerable.SingleOrDefault(AllProtocolFunctionDefinitions, fd => fd.GUID == functionId);
                    if (srmFunctionDefinition == null)
                        throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.FunctionDefinitionNotFoundException(functionId);
                    var functionDefinition = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.FunctionDefinition{Id = srmFunctionDefinition.GUID, Name = srmFunctionDefinition.Name, Label = label, ConfigurationOrder = order, Options = options, IsHidden = isHidden, ProfileDefinition = Helpers.ProfileManager.GetProfileDefinition(srmFunctionDefinition.ProfileDefinition), InterfaceProfileDefinitions = Helpers.ProfileManager.GetInterfaceProfileDefinitions(srmFunctionDefinition), ResourcePool = resourcePool, Children = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(AllProtocolFunctionDefinitions, fd => fd.ParentFunctionGUID == srmFunctionDefinition.GUID), fd => fd.GUID)), IsManualResourceSelectionAllowed = isManualResourceSelectionAllowed};
                    return functionDefinition;
                }

                private System.Collections.Generic.List<Skyline.DataMiner.Net.Messages.FunctionDefinition> GetAllProtocolFunctionDefinitions()
                {
                    LogMethodStart(nameof(GetAllProtocolFunctionDefinitions), out var stopwatch);
                    var functionDefinitions = new System.Collections.Generic.List<Skyline.DataMiner.Net.Messages.FunctionDefinition>();
                    var protocolFunctions = Helpers.ProtocolFunctionHelper.GetAllProtocolFunctions();
                    foreach (var protocolFunction in protocolFunctions)
                    {
                        foreach (var protocolFunctionVersion in protocolFunction.ProtocolFunctionVersions)
                        {
                            if (!protocolFunctionVersion.Active)
                                continue; // only consider active function definitions
                            foreach (var functionDefinition in protocolFunctionVersion.FunctionDefinitions)
                            {
                                if (!System.Linq.Enumerable.Any(functionDefinitions, fd => fd.GUID.Equals(functionDefinition.GUID)))
                                {
                                    functionDefinitions.Add(functionDefinition);
                                }
                            }
                        }
                    }

                    Log(nameof(GetAllProtocolFunctionDefinitions), $"Retrieved {functionDefinitions.Count} protocol function definitions.");
                    LogMethodCompleted(nameof(GetAllProtocolFunctionDefinitions), stopwatch);
                    return functionDefinitions;
                }

                private string GetBookingManagerElementName(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
                {
                    var virtualPlatformProperty = System.Linq.Enumerable.FirstOrDefault(serviceDefinition.Properties, p => p.Name == "Virtual Platform");
                    if (virtualPlatformProperty == null)
                    {
                        // TODO: check to use custom exceptions
                        throw new System.Exception(System.String.Format("ServiceDefinition with ID {0} does not contain a Virtual Platform property", serviceDefinition.ID));
                    }

                    var bookingManager = GetBookingManager(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform>(virtualPlatformProperty.Value));
                    return bookingManager?.ElementName;
                }

                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig GetContributingConfig(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
                {
                    Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig contributingConfig = null;
                    try
                    {
                        var contributingConfigProperty = System.Linq.Enumerable.FirstOrDefault(serviceDefinition.Properties, p => System.String.Equals(p.Name, ContributiongConfigPropertyName, System.StringComparison.InvariantCultureIgnoreCase));
                        contributingConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ContributingConfig>(contributingConfigProperty.Value);
                    }
                    catch (System.Exception)
                    {
                    }

                    return contributingConfig;
                }

                private void Log(string nameOfMethod, string message, string nameOfObject = null)
                {
                    Helpers.Log(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinitionManager), nameOfMethod, message, nameOfObject);
                }

                protected void LogMethodStart(string nameOfMethod, out System.Diagnostics.Stopwatch stopwatch)
                {
                    Log(nameOfMethod, "Start");
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();
                }

                protected void LogMethodCompleted(string nameOfMethod, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    stopwatch?.Stop();
                    Log(nameOfMethod, $"Completed [{stopwatch?.Elapsed}]");
                }

                private System.Collections.Generic.List<Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinition> GetAllNonOrderSrmServiceDefinitions()
                {
                    LogMethodStart(nameof(GetAllNonOrderSrmServiceDefinitions), out var stopwatch);
                    var result = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(Helpers.ServiceManagerHelper.GetServiceDefinitions(Skyline.DataMiner.Net.Messages.SLDataGateway.ExposerExtensions.NotEqual(Skyline.DataMiner.Net.Messages.SLDataGateway.ReflectiveExposer.DictStringField(Skyline.DataMiner.Net.ServiceManager.Objects.ServiceDefinitionExposers.Properties, "Virtual Platform"), "Order")), sd => sd.Name.StartsWith("_")));
                    Log(nameof(GetAllNonOrderSrmServiceDefinitions), $"Retrieved {result.Count} Service Definitions");
                    LogMethodCompleted(nameof(GetAllNonOrderSrmServiceDefinitions), stopwatch);
                    return result;
                }
            }
        }

        namespace Ticketing
        {
            [Skyline.DataMiner.Library.Common.Attributes.DllImport("Skyline.DataMiner.Storage.Types.dll")]
            public class TicketingManager
            {
                public const string ServiceIdTicketFieldName = "Service ID";
                public const string DeadLineTicketFieldName = "Deadline";
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly Skyline.DataMiner.Net.Ticketing.TicketingGatewayHelper ticketingHelper;
                private readonly Skyline.DataMiner.Net.Ticketing.Helpers.TicketFieldResolver ticketFieldResolver;
                public TicketingManager(Skyline.DataMiner.Automation.IEngine engine, string domain)
                {
                    this.engine = engine;
                    ticketingHelper = new Skyline.DataMiner.Net.Ticketing.TicketingGatewayHelper{HandleEventsAsync = false};
                    ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Skyline.DataMiner.Automation.Engine.SLNet.SendSingleResponseMessage(args.requestMessage);
                    ticketFieldResolver = System.Linq.Enumerable.FirstOrDefault(ticketingHelper.GetTicketFieldResolvers(Skyline.DataMiner.Net.Ticketing.Helpers.TicketFieldResolver.Factory.CreateEmptyResolver(domain)));
                }

                public Skyline.DataMiner.Net.Ticketing.Helpers.TicketFieldResolver TicketFieldResolver
                {
                    get
                    {
                        return ticketFieldResolver;
                    }
                }

                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.Net.Ticketing.Ticket> GetTicketsForService(System.Guid serviceId)
                {
                    // This is the preferred way of retrieving the ticket, but didn't work because of a bug in Software
                    // return ticketingHelper.GetTickets(new[] { TicketLink.Create(new ReservationInstanceID(serviceId)) });
                    return ticketingHelper.GetTickets(filter: Skyline.DataMiner.Net.Messages.SLDataGateway.ExposerExtensions.Equal(Skyline.DataMiner.Net.Messages.SLDataGateway.ReflectiveExposer.DictStringField(Skyline.DataMiner.Net.Ticketing.TicketingExposers.CustomTicketFields, ServiceIdTicketFieldName), serviceId.ToString()));
                }
            }
        }

        namespace UI
        {
            public class ValidationInfo
            {
                public ValidationInfo()
                {
                    State = Skyline.DataMiner.Automation.UIValidationState.NotValidated;
                    Text = System.String.Empty;
                }

                public Skyline.DataMiner.Automation.UIValidationState State
                {
                    get;
                    set;
                }

                public string Text
                {
                    get;
                    set;
                }

                public bool IsValid
                {
                    get
                    {
                        return State != Skyline.DataMiner.Automation.UIValidationState.Invalid;
                    }
                }
            }
        }

        namespace UserTasks
        {
            public static class Descriptions
            {
                public static class Dummy
                {
                    public const string SelectTechnicalSystem = "Select Technical System from EBU Synopsis";
                }

                public static class FiberReception
                {
                    public const string AllocationNeeded = "Fiber Allocation Needed";
                    public const string EquipmentAllocation = "Fiber Equipment Allocation Needed + Configuration";
                }

                public static class FiberTransmission
                {
                    public const string Configure = "Configure Ad Hoc Fiber return feed";
                }

                public static class MicrowaveReception
                {
                    public const string EquipmentAllocation = "Microwave Equipment Allocation Needed";
                    public const string EquipmentConfiguration = "Microwave Equipment Installation + Configuration Needed";
                }

                public static class MicrowaveTransmission
                {
                    public const string Configure = "Configure Microwave Return Channel";
                }

                public static class SatelliteReception
                {
                    public const string SelectAntenna = "Select Steerable Antenna";
                    public const string SpaceNeeded = "Satellite Space Needed";
                    public const string SteerAntenna = "Steer Steerable Antenna to Correct Satellite";
                    public const string ConfigureNs3 = "Configure NS3 Demodulator";
                    public const string ConfigureNs4 = "Configure NS4 Demodulator";
                    public const string ConfigureIrd = "Configure IRD";
                    public const string VerifySatellite = "Verify Satellite";
                }
            }

            public enum UserTaskSource
            {
                LiveService,
                NonLiveOrder
            }

            public enum UserTaskStatus
            {
                Incomplete = 1,
                Complete = 2
            }

            public enum UserGroup
            {
                [System.ComponentModel.Description("Booking Office")]
                BookingOffice = 1,
                [System.ComponentModel.Description("MCR Operator")]
                McrOperator = 2,
                [System.ComponentModel.Description("Fiber Specialist")]
                FiberSpecialist = 3,
                [System.ComponentModel.Description("MW Specialist")]
                MwSpecialist = 4,
                [System.ComponentModel.Description("Media Operator")]
                MediaOperator = 5,
                [System.ComponentModel.Description("Audio MCR Operator")]
                AudioMcrOperator = 6,
                [System.ComponentModel.Description("Messi Specific User Group")]
                MessiSpecific = 7,
                [System.ComponentModel.Description("Mediamylly Specific User Group")]
                MediamyllySpecific = 8,
                [System.ComponentModel.Description("Mediaputiikki Specific User Group")]
                MediaputiikkiSpecific = 9,
                [System.ComponentModel.Description("UA Specific User Group")]
                UaSpecific = 10
            }

            public class UserTask
            {
                public const string TicketFieldDescription = "Description";
                public const string TicketFieldState = "State";
                private const string TicketFieldName = "Name";
                private const string TicketFieldUserGroup = "User Group";
                private const string TicketFieldServiceId = "Service ID";
                private const string TicketFieldIngestExportFK = "Ingest Export FK";
                private const string TicketFieldOwner = "Owner";
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly System.Guid ticketFieldResolverId;
                private Skyline.DataMiner.Net.Ticketing.Ticket ticket;
                /// <summary>
                /// Constructor used to create a service-based user task from scratch.
                /// </summary>
                public UserTask(Skyline.DataMiner.Automation.IEngine engine, System.Guid ticketFieldResolverId, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, string description, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup userGroup, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus.Incomplete)
                {
                    this.engine = engine;
                    this.ticketFieldResolverId = ticketFieldResolverId;
                    UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.LiveService;
                    ServiceId = service.Id;
                    Status = status;
                    Description = description;
                    UserGroup = userGroup;
                    Service = service;
                    //Service.UserTasks = new[] { this }.Concat(Service.UserTasks.Where(x => !x.Name.Equals(this.Name))).ToList(); // replacing the existing User Task instance with this instance
                    ServiceStartTime = Service.Start;
                    Name = GenerateName(Service, description);
                }

                /// <summary>
                /// Constructor used to create a service-based user task from a ticket.
                /// </summary>
                public UserTask(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, Skyline.DataMiner.Net.Ticketing.Ticket ticket)
                {
                    this.engine = engine;
                    this.ticket = ticket;
                    UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.LiveService;
                    ID = ticket.ID.ToString();
                    ServiceId = System.Guid.Parse(System.Convert.ToString(ticket.CustomTicketFields[TicketFieldServiceId]));
                    Name = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldName]);
                    Description = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldDescription]);
                    UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup>(System.Convert.ToString(ticket.CustomTicketFields[TicketFieldUserGroup]));
                    ticket.CustomTicketFields.TryGetValue(TicketFieldOwner, out var owner);
                    Owner = System.Convert.ToString(owner);
                    var statusTicketField = ticket.GetTicketField(TicketFieldState) as Skyline.DataMiner.Net.Ticketing.Validators.GenericEnumEntry<int>;
                    if (statusTicketField != null)
                        Status = (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus)statusTicketField.Value;
                    Service = service;
                    //Service.UserTasks = new[] { this }.Concat(Service.UserTasks.Where(x => !x.Name.Equals(this.Name))).ToList(); // replacing the existing User Task instance with this instance
                    ServiceStartTime = service.Start;
                }

                /// <summary>
                /// Constructor used to make a user task from scratch for a Non-Live Transfer order.
                /// </summary>
                public UserTask(Skyline.DataMiner.Automation.IEngine engine, System.Guid ticketFieldResolverId, string ingestExportForeignKey, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus status, string name, string description, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup userGroup)
                {
                    this.engine = engine;
                    this.ticketFieldResolverId = ticketFieldResolverId;
                    UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.NonLiveOrder;
                    IngestExportForeignKey = ingestExportForeignKey;
                    Status = status;
                    Name = name;
                    Description = description;
                    UserGroup = userGroup;
                }

                /// <summary>
                /// Constructor used to make a user task from a ticket for a Non-Live Transfer order.
                /// </summary>
                public UserTask(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer.Transfer transfer, Skyline.DataMiner.Net.Ticketing.Ticket ticket)
                {
                    this.engine = engine;
                    this.ticket = ticket;
                    UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.NonLiveOrder;
                    ID = ticket.ID.ToString();
                    IngestExportForeignKey = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldIngestExportFK]);
                    Name = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldName]);
                    Description = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldDescription]);
                    UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup>(System.Convert.ToString(ticket.CustomTicketFields[TicketFieldUserGroup]));
                    ticket.CustomTicketFields.TryGetValue(TicketFieldOwner, out var owner);
                    Owner = System.Convert.ToString(owner);
                    var statusTicketField = ticket.GetTicketField(TicketFieldState) as Skyline.DataMiner.Net.Ticketing.Validators.GenericEnumEntry<int>;
                    if (statusTicketField != null)
                        Status = (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus)statusTicketField.Value;
                }

                public UserTask(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.Net.Ticketing.Ticket ticket)
                {
                    this.engine = engine;
                    this.ticket = ticket;
                    ID = ticket.ID.ToString();
                    Name = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldName]);
                    Description = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldDescription]);
                    UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetEnumValueFromDescription<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup>(System.Convert.ToString(ticket.CustomTicketFields[TicketFieldUserGroup]));
                    if (ticket.CustomTicketFields.TryGetValue(TicketFieldOwner, out var owner))
                    {
                        Owner = System.Convert.ToString(owner);
                    }

                    var statusTicketField = ticket.GetTicketField(TicketFieldState) as Skyline.DataMiner.Net.Ticketing.Validators.GenericEnumEntry<int>;
                    if (statusTicketField != null)
                        Status = (Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus)statusTicketField.Value;
                    if (ticket.CustomTicketFields.ContainsKey(TicketFieldServiceId))
                    {
                        UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.LiveService;
                        ServiceId = System.Guid.Parse(System.Convert.ToString(ticket.CustomTicketFields[TicketFieldServiceId]));
                        var serviceManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers(engine));
                        Service = serviceManager.GetService(ServiceId);
                        Service.UserTasks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Concat(new[]{this}, System.Linq.Enumerable.Where(Service.UserTasks, x => !x.Name.Equals(this.Name)))); // replacing the existing User Task instance with this instance
                        ServiceStartTime = Service.Start;
                    }
                    else if (ticket.CustomTicketFields.ContainsKey(TicketFieldIngestExportFK))
                    {
                        UserTaskSource = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource.NonLiveOrder;
                        IngestExportForeignKey = System.Convert.ToString(ticket.CustomTicketFields[TicketFieldIngestExportFK]);
                    }
                }

                /// <summary>
                /// The ID of the ticket in the ticketing domain.
                /// Format: [dataminer ID]/[ticket ID] .
                /// </summary>
                public string ID
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Status of the User Task.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskStatus Status
                {
                    get;
                    set;
                }

                /// <summary>
                /// ID of the Service this user task is linked to.
                /// </summary>
                public System.Guid ServiceId
                {
                    get;
                    private set;
                }

                /// <summary>
                /// ID of the ingest/export ticket this user task is linked to.
                /// </summary>
                public string IngestExportForeignKey
                {
                    get;
                    set;
                }

                public string Name
                {
                    get;
                    set;
                }

                public string Owner
                {
                    get;
                    set;
                }

                public string Description
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup UserGroup
                {
                    get;
                    set;
                }

                public System.DateTime ServiceStartTime
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Indicates whether the user task is made based on a live service or on a non-live order.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskSource UserTaskSource
                {
                    get;
                    private set;
                }

                /// <summary>
                /// The Service object this User Task is linked to.
                /// </summary>
                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service Service
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Generates the correct name for the service-based user task.
                /// </summary>
                private string GenerateName(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, string description)
                {
                    if (service.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Reception || service.Definition.VirtualPlatformServiceType == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatformType.Transmission)
                    {
                        return System.String.Format("{0} {1}: {2}", Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EnumExtensions.GetDescriptionFromEnumValue(service.Definition.VirtualPlatformServiceName), service.Name, description);
                    }
                    else
                    {
                        return System.String.Format("{0}: {1}", service.Name, description);
                    }
                }
            }

            public class UserTaskManager
            {
                public const string TicketingDomain = "User Tasks";
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter;
                public UserTaskManager(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter = null)
                {
                    this.engine = engine;
                    this.progressReporter = progressReporter;
                    TicketingManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing.TicketingManager(engine, TicketingDomain);
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing.TicketingManager TicketingManager
                {
                    get;
                }

                /// <summary>
                /// Get user tasks that are applicable for the given live service.
                /// </summary>
                public System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask> GetUserTasks(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service)
                {
                    if (service == null)
                        throw new System.ArgumentNullException(nameof(service));
                    LogMethodStarted(nameof(GetUserTasks), out var stopwatch, service.Name);
                    var tickets = TicketingManager.GetTicketsForService(service.Id);
                    var userTasks = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(tickets, x => new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTask(engine, service, x)));
                    Log(nameof(GetUserTasks), $"Retrieved following user tasks from database: {string.Join(";", System.Linq.Enumerable.Select(userTasks, u => $"{u.Name}({u.ID})={u.Status}"))}", service.Name);
                    LogMethodCompleted(nameof(GetUserTasks), service.Name, stopwatch);
                    return userTasks;
                }

                private void Log(string nameOfMethod, string message, string nameOfObject = null)
                {
                    progressReporter?.LogProgress(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager), nameOfMethod, message, nameOfObject);
                }

                private void LogMethodStarted(string methodName, out System.Diagnostics.Stopwatch stopwatch, string objectName = null)
                {
                    progressReporter?.LogMethodStart(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager), methodName, objectName);
                    stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                }

                private void LogMethodCompleted(string methodName, string objectName = null, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    progressReporter?.LogMethodCompleted(nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager), methodName, objectName, stopwatch);
                }
            }
        }

        namespace Utilities
        {
            public static class DateTimeExtensions
            {
                /// <summary>
                /// Used to convert a DateTime from a reservationInstance to the correct Local Time.
                /// </summary>
                /// <param name = "dateTime">DateTime as retrieved from a reservation instance.</param>
                /// <returns>Local Time from reservation instance.</returns>
                public static System.DateTime FromReservation(this System.DateTime dateTime)
                {
                    return new System.DateTime(dateTime.Ticks, System.DateTimeKind.Utc).ToLocalTime();
                }

                /// <summary>
                /// Used to convert a DateTime from a service configuration to the correct Local Time.
                /// </summary>
                /// <param name = "dateTime">DateTime as retrieved from a service configuration.</param>
                /// <returns>Local Time from service configuration.</returns>
                public static System.DateTime FromServiceConfiguration(this System.DateTime dateTime)
                {
                    return new System.DateTime(dateTime.Ticks, System.DateTimeKind.Utc).ToLocalTime();
                }

                /// <summary>
                /// Truncate to the nearest whole value of given <see cref = "timeSpan"/>
                /// </summary>
                public static System.DateTime Truncate(this System.DateTime dt, System.TimeSpan timeSpan)
                {
                    if (timeSpan == System.TimeSpan.Zero)
                        return dt;
                    return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
                }
            }

            public static class EngineExtensions
            {
                /// <summary>
                /// Logs to SLAutomation using the following template: <paramref name = "nameOfClass"/>|<paramref name = "nameOfMethod"/>|<paramref name = "nameOfOrderOrService"/>|<paramref name = "message"/>
                /// </summary>
                public static void Log(this Skyline.DataMiner.Automation.IEngine engine, string nameOfClass, string nameOfMethod, string message, string nameOfOrderOrService = null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(nameOfClass);
                    sb.Append("|");
                    sb.Append(nameOfMethod);
                    sb.Append("|");
                    if (!string.IsNullOrEmpty(nameOfOrderOrService))
                    {
                        sb.Append(nameOfOrderOrService);
                        sb.Append("|");
                    }

                    sb.Append(message);
                    engine.Log(sb.ToString());
                }
            }

            public static class EnumExtensions
            {
                public static string GetDescription(this System.Enum value)
                {
                    return GetDescriptionFromEnumValue(value);
                }

                public static string GetDescriptionFromEnumValue(System.Enum value)
                {
                    System.ComponentModel.DescriptionAttribute attribute = System.Linq.Enumerable.SingleOrDefault(value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)) as System.ComponentModel.DescriptionAttribute;
                    return attribute == null ? value.ToString() : attribute.Description;
                }

                public static T GetEnumValueFromDescription<T>(string description)
                {
                    var type = typeof(T);
                    if (!type.IsEnum)
                        throw new System.ArgumentException();
                    System.Reflection.FieldInfo[] fields = type.GetFields();
                    var field = System.Linq.Enumerable.SingleOrDefault(System.Linq.Enumerable.SelectMany(fields, f => f.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false), (f, a) => new
                    {
                    Field = f, Att = a
                    }

                    ), a => ((System.ComponentModel.DescriptionAttribute)a.Att).Description == description);
                    return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
                }

                public static System.Collections.Generic.IEnumerable<System.String> GetEnumDescriptions<T>()
                {
                    var attributes = System.Linq.Enumerable.ToList(System.Linq.Enumerable.SelectMany(typeof(T).GetMembers(), member => System.Linq.Enumerable.Cast<System.ComponentModel.DescriptionAttribute>(member.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true))));
                    return System.Linq.Enumerable.Select(attributes, x => x.Description);
                }
            }

            public class FixedFileLogger : System.IDisposable
            {
                public const string SkylineDataFilePath = @"C:\Skyline_Data\";
                public const string TextFileExtension = ".txt";
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly string logfilePath;
                private readonly System.Collections.Generic.List<System.String> buffer = new System.Collections.Generic.List<System.String>();
                public FixedFileLogger(Skyline.DataMiner.Automation.IEngine engine, string logFilePath)
                {
                    this.engine = engine;
                    this.logfilePath = logFilePath ?? throw new System.ArgumentNullException(nameof(logFilePath));
                    if (!logfilePath.StartsWith(SkylineDataFilePath))
                        throw new System.ArgumentException($"Argument does not start with {SkylineDataFilePath}", nameof(logFilePath));
                    if (!logfilePath.EndsWith(TextFileExtension))
                        throw new System.ArgumentException($"Argument does not end with {TextFileExtension}", nameof(logFilePath));
                    try
                    {
                        bool logFileExists = System.IO.File.Exists(logfilePath);
                        if (!logFileExists)
                        {
                            var newFile = System.IO.File.Create(logfilePath);
                            newFile.Close();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EngineExtensions.Log(engine, nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger), nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger), $"Something went wrong: {e}");
                        throw;
                    }
                }

                public bool PrintOneLine
                {
                    get;
                    set;
                }

                = false;
                internal void Log(string message)
                {
                    buffer.Add(message);
                }

                public static string GenerateLogFilePath(string fileName)
                {
                    return $"{SkylineDataFilePath}{fileName}{TextFileExtension}";
                }

                public void Dispose()
                {
                    try
                    {
                        if (PrintOneLine)
                        {
                            System.IO.File.AppendAllText(logfilePath, string.Join(",", buffer));
                        }
                        else
                        {
                            System.IO.File.AppendAllLines(logfilePath, buffer);
                        }

                        buffer.Clear();
                    }
                    catch (System.Exception e)
                    {
                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EngineExtensions.Log(engine, nameof(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger), nameof(Dispose), $"Something went wrong: {e}");
                        throw;
                    }
                }
            }

            public class Helpers
            {
                private readonly Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger performanceLogger;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.EventManager eventManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.OrderLogger orderLogger;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.IServiceDefinitionManager serviceDefinitionManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager serviceManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManager orderManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.ResourceManager resourceManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.LockManager lockManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts.ContractManager contractManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager userTaskManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.IProfileManager profileManager;
                private Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes.INoteManager noteManager;
                public Helpers(Skyline.DataMiner.Automation.IEngine engine)
                {
                    Engine = engine ?? throw new System.ArgumentNullException(nameof(engine));
                    performanceLogger = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger(Engine, @"C:\Skyline_Data\PerformanceLogging.txt")
                    {PrintOneLine = true};
                    performanceLogger.Log($"START SCRIPT at {System.DateTime.UtcNow}(utc) by user {Engine.UserDisplayName}");
                }

                public Helpers(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter progressReporter): this(engine)
                {
                    ProgressReporter = progressReporter;
                }

                public Helpers(Skyline.DataMiner.Automation.IEngine engine, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.OrderLogger orderLogger): this(engine, orderLogger?.ProgressReporter)
                {
                    OrderLogger = orderLogger;
                }

                public Skyline.DataMiner.Automation.IEngine Engine
                {
                    get;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.OrderLogger OrderLogger
                {
                    get => orderLogger;
                    set
                    {
                        orderLogger = value;
                        ProgressReporter = OrderLogger?.ProgressReporter;
                    }
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter ProgressReporter
                {
                    get;
                    set;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManager OrderManager
                {
                    get => orderManager ?? (orderManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManager(this));
                    set => orderManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager ServiceManager
                {
                    get => serviceManager ?? (serviceManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceManager(this));
                    set => serviceManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.IServiceDefinitionManager ServiceDefinitionManager
                {
                    get => serviceDefinitionManager ?? (serviceDefinitionManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinitionManager(this));
                    set => serviceDefinitionManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.EventManager EventManager
                {
                    get => eventManager ?? (eventManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.EventManager(this));
                    set => eventManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.ResourceManager ResourceManager
                {
                    get => resourceManager ?? (resourceManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources.ResourceManager(this));
                    set => resourceManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.IProfileManager ProfileManager
                {
                    get => profileManager ?? (profileManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile.ProfileManager(this));
                    set => profileManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts.ContractManager ContractManager
                {
                    get => contractManager ?? (contractManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts.ContractManager(Engine));
                    set => contractManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.LockManager LockManager
                {
                    get => lockManager ?? (lockManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking.LockManager(Engine, ProgressReporter));
                    set => lockManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager UserTaskManager
                {
                    get => userTaskManager ?? (userTaskManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserTaskManager(Engine, ProgressReporter));
                    set => userTaskManager = value;
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes.INoteManager NoteManager
                {
                    get => noteManager ?? (noteManager = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes.NoteManager(Engine));
                    set => noteManager = value;
                }

                public Skyline.DataMiner.Library.Solutions.SRM.Helpers.ResourceManagerHelperExtended ResourceManagerHelper => Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager;
                public Skyline.DataMiner.Library.Solutions.SRM.Helpers.ServiceManagerHelperExtended ServiceManagerHelper => Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ServiceManager;
                public Skyline.DataMiner.Net.Profiles.ProfileHelper ProfileHelper => Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ProfileHelper;
                public Skyline.DataMiner.Library.Solutions.SRM.Helpers.ProtocolFunctionHelperExtended ProtocolFunctionHelper => Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ProtocolFunctionManager;
                public void Log(string nameOfClass, string nameOfMethod, string message, string nameOfObject = null)
                {
                    if (OrderLogger != null)
                    {
                        OrderLogger.Log(nameOfClass, nameOfMethod, message, nameOfObject);
                    }
                    else if (ProgressReporter != null)
                    {
                        ProgressReporter.LogProgress(nameOfClass, nameOfMethod, message, nameOfObject);
                    }
                    else
                    {
                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.EngineExtensions.Log(Engine, nameOfClass, nameOfMethod, message, nameOfObject);
                    }
                }

                public void LogMethodStart(string nameOfClass, string nameOfMethod, out System.Diagnostics.Stopwatch stopwatch, string nameOfObject = null)
                {
                    Log(nameOfClass, nameOfMethod, "Start", nameOfObject);
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();
                }

                public void LogMethodCompleted(string nameOfClass, string nameOfMethod, string nameOfObject = null, System.Diagnostics.Stopwatch stopwatch = null)
                {
                    stopwatch?.Stop();
                    performanceLogger.Log($"{nameOfClass}.{nameOfMethod}={stopwatch?.Elapsed}");
                    Log(nameOfClass, nameOfMethod, $"Completed [{stopwatch?.Elapsed}]", nameOfObject);
                }
            }

            public static class JobExtensions
            {
                public static Skyline.DataMiner.Net.Sections.Section GetCustomEventSection(this Skyline.DataMiner.Net.Jobs.Job job, System.Guid customEventSectionDefinitionId)
                {
                    if (job == null)
                        throw new System.ArgumentNullException(nameof(job));
                    return System.Linq.Enumerable.SingleOrDefault(job.Sections, s => s.GetSectionDefinition().GetID().Id == customEventSectionDefinitionId);
                }

                public static System.Collections.Generic.IEnumerable<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection> GetOrderSections(this Skyline.DataMiner.Net.Jobs.Job job)
                {
                    var orderSections = new System.Collections.Generic.List<Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection>();
                    foreach (var section in job.Sections)
                    {
                        foreach (var fieldValue in section.FieldValues)
                        {
                            bool sectionContainsOrderId = fieldValue.Value.Type == typeof(System.Guid);
                            if (!sectionContainsOrderId)
                                continue;
                            orderSections.Add(new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection(section));
                        }
                    }

                    return orderSections;
                }

                public static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection GetOrderSection(this Skyline.DataMiner.Net.Jobs.Job job, System.Guid orderId)
                {
                    return System.Linq.Enumerable.FirstOrDefault(GetOrderSections(job), s => s.OrderId == orderId);
                }

                public static void AddOrUpdateOrderSection(this Skyline.DataMiner.Net.Jobs.Job job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.OrderSection orderSection)
                {
                    var existingSection = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetOrderSection(job, orderSection.OrderId);
                    if (existingSection != null)
                    {
                        job.Sections.Remove(existingSection.Section);
                    }

                    job.Sections.Add(orderSection.Section);
                }

                public static System.DateTime GetStartTime(this Skyline.DataMiner.Net.Jobs.Job job)
                {
                    var start = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameStartTime);
                    if (start == null)
                        return default(System.DateTime);
                    return System.DateTime.TryParse(start.ToString(), out var startTime) ? startTime : default(System.DateTime);
                }

                public static System.DateTime GetEndTime(this Skyline.DataMiner.Net.Jobs.Job job)
                {
                    var start = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.JobExtensions.GetFieldValue(job, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Event.PropertyNameEndTime);
                    if (start == null)
                        return default(System.DateTime);
                    return System.DateTime.TryParse(start.ToString(), out var startTime) ? startTime : default(System.DateTime);
                }

                public static object GetFieldValue(this Skyline.DataMiner.Net.Jobs.Job job, string fieldDescriptorName)
                {
                    foreach (var section in job.Sections)
                    {
                        foreach (var fieldValue in section.FieldValues)
                        {
                            var fieldDescriptor = fieldValue.GetFieldDescriptor();
                            if (fieldDescriptor == null)
                                continue;
                            if (System.String.Equals(fieldDescriptor.Name, fieldDescriptorName))
                            {
                                return fieldValue.Value.Value;
                            }
                        }
                    }

                    return null;
                }
            }

            [Skyline.DataMiner.Library.Common.Attributes.DllImport("SLSRMLibrary.dll")]
            public class OrderLogger : System.IDisposable
            {
                private readonly Skyline.DataMiner.Automation.IEngine engine;
                private readonly Skyline.DataMiner.Library.Solutions.SRM.BookingManager orderBookingManager;
                private readonly string userName;
                private readonly System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.Library.Solutions.SRM.Logging.SrmLogHandler> srmLogHandlers = new System.Collections.Generic.Dictionary<System.Guid, Skyline.DataMiner.Library.Solutions.SRM.Logging.SrmLogHandler>();
                private readonly System.Text.StringBuilder automationLoggerBuffer = new System.Text.StringBuilder();
                private bool orderReferenced = false;
                /// <summary>
                /// Initialize a new OrderLogger object.
                /// This constructor should only be used in case the order has not been booked.
                /// Make sure as soon as the order has been booked that AddOrderReference is executed to make sure the logging is correctly generated.
                /// </summary>
                /// <param name = "engine">The engine object.</param>
                /// <param name = "initializeEmptySrmLogHandler">Optional, set to false if you don't want to initialize a new SRM logger from this constructor.</param>
                /// <param name = "fileName">Optional, name of the file to which the logging will be written in the C:\Skyline_Data folder if the logger is not linked to a reservations.</param>
                public OrderLogger(Skyline.DataMiner.Automation.IEngine engine, bool initializeEmptySrmLogHandler = true, string fileName = null)
                {
                    this.engine = engine ?? throw new System.ArgumentNullException(nameof(engine));
                    this.userName = engine.UserLoginName;
                    this.FileName = fileName;
                    ProgressReporter = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter();
                    orderBookingManager = new Skyline.DataMiner.Library.Solutions.SRM.BookingManager((Skyline.DataMiner.Automation.Engine)engine, engine.FindElement(Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManager.OrderBookingManagerElementName));
                    if (initializeEmptySrmLogHandler)
                        InitializeEmptySrmLogHandler();
                }

                /// <summary>
                /// Initialize a new OrderLogger object.
                /// </summary>
                /// <param name = "engine">The engine object.</param>
                /// <param name = "orderIds">The ids of the orders for which the logging will need to be added to their respective log file.</param>
                public OrderLogger(Skyline.DataMiner.Automation.IEngine engine, string fileName = null, params System.Guid[] orderIds): this(engine, false, fileName)
                {
                    if (orderIds == null || !System.Linq.Enumerable.Any(orderIds) || (orderIds.Length == 1 && System.Linq.Enumerable.Single(orderIds) == System.Guid.Empty))
                    {
                        InitializeEmptySrmLogHandler();
                        return;
                    }

                    foreach (var orderId in orderIds)
                    {
                        var orderReservation = Skyline.DataMiner.Library.Solutions.SRM.SrmManagers.ResourceManager.GetReservationInstance(orderId);
                        InitializeSrmLogHandler(orderReservation);
                    }
                }

                /// <summary>
                /// Initialize a new OrderLogger object.
                /// </summary>
                /// <param name = "engine">The engine object.</param>
                /// <param name = "orderReservations">The reservations of the orders for which the logging will need to be added to their respective log file.</param>
                public OrderLogger(Skyline.DataMiner.Automation.IEngine engine, string fileName = null, params Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance[] orderReservations): this(engine, false, fileName)
                {
                    if (orderReservations == null || !System.Linq.Enumerable.Any(orderReservations))
                    {
                        InitializeEmptySrmLogHandler();
                        return;
                    }

                    foreach (var orderReservation in orderReservations)
                    {
                        InitializeSrmLogHandler(orderReservation);
                    }
                }

                public Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ProgressReporter ProgressReporter
                {
                    get;
                    set;
                }

                public string FileName
                {
                    get;
                    set;
                }

                public void Log(string nameOfClass, string nameOfMethod, string message, string nameOfObject = null)
                {
                    ProgressReporter.LogProgress(nameOfClass, nameOfMethod, message, nameOfObject);
                }

                public void Dispose()
                {
                    if (orderReferenced)
                    {
                        foreach (var srmLogHandler in srmLogHandlers.Values)
                            srmLogHandler.Dispose();
                    }
                    else if (!System.String.IsNullOrWhiteSpace(FileName))
                    {
                        string path = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger.GenerateLogFilePath(FileName);
                        Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger fixedFileLogger = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.FixedFileLogger(engine, path);
                        fixedFileLogger.Log(automationLoggerBuffer.ToString());
                        fixedFileLogger.Dispose();
                    }
                    else
                    {
                        engine.Log(automationLoggerBuffer.ToString());
                    }
                }

                private void InitializeSrmLogHandler(Skyline.DataMiner.Net.ResourceManager.Objects.ReservationInstance orderReservation)
                {
                    if (orderReservation == null)
                        throw new System.ArgumentNullException(nameof(orderReservation));
                    orderReferenced = true;
                    if (srmLogHandlers.TryGetValue(orderReservation.ID, out var orderSrmLogHandler))
                        return;
                    try
                    {
                        orderSrmLogHandler = Skyline.DataMiner.Library.Solutions.SRM.Logging.SrmLogHandler.Create((Skyline.DataMiner.Automation.Engine)engine, orderBookingManager, orderReservation);
                        srmLogHandlers.Add(orderReservation.ID, orderSrmLogHandler);
                        ProgressReporter.ProgressLogging += (sender, e) =>
                        {
                            var logging = $"[{System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")}] {e.Progress}";
                            Skyline.DataMiner.Library.Solutions.SRM.Logging.LoggerHelper.BufferMessage(orderSrmLogHandler, logging, Skyline.DataMiner.Library.Solutions.SRM.LogFileType.Debug);
                        }

                        ;
                    }
                    catch (System.Exception e)
                    {
                        engine.Log($"OrderLogger|InitializeSrmLogHandler|Exception initializing SrmLogHandler for order {orderReservation.ID}: {e}");
                    }
                }

                private void InitializeEmptySrmLogHandler()
                {
                    if (srmLogHandlers.TryGetValue(System.Guid.Empty, out var orderSrmLogHandler))
                        return;
                    try
                    {
                        orderSrmLogHandler = Skyline.DataMiner.Library.Solutions.SRM.Logging.SrmLogHandler.Create((Skyline.DataMiner.Automation.Engine)engine, orderBookingManager, null);
                        srmLogHandlers.Add(System.Guid.Empty, orderSrmLogHandler);
                        ProgressReporter.ProgressLogging += (sender, e) =>
                        {
                            var logging = $"[{System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")}] {userName} {e.Progress}";
                            automationLoggerBuffer.AppendLine(logging);
                            Skyline.DataMiner.Library.Solutions.SRM.Logging.LoggerHelper.BufferMessage(orderSrmLogHandler, logging, Skyline.DataMiner.Library.Solutions.SRM.LogFileType.Debug);
                        }

                        ;
                    }
                    catch (System.Exception e)
                    {
                        engine.Log($"OrderLogger|InitializeEmptySrmLogHandler|Exception initializing empty SrmLogHandler: {e}");
                    }
                }
            }

            public static class ResourceExtensions
            {
                public static string GetDisplayName(this Skyline.DataMiner.Net.Messages.Resource resource, System.Guid functionId)
                {
                    bool functionIsMatrix = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.IsMatrixFunction(functionId);
                    bool functionIsSatellite = functionId == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration.FunctionGuids.Antenna;
                    return functionIsMatrix || functionIsSatellite ? resource.Name : System.Linq.Enumerable.First(resource.Name.Split('.'));
                }
            }

            public static class StringExtensions
            {
                public static T GetEnumValue<T>(this string value)
                {
                    var type = typeof(T);
                    if (!type.IsEnum)
                        throw new System.ArgumentException();
                    System.Reflection.FieldInfo[] fields = type.GetFields();
                    var field = System.Linq.Enumerable.SingleOrDefault(System.Linq.Enumerable.SelectMany(fields, f => f.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false), (f, a) => new
                    {
                    Field = f, Att = a
                    }

                    ), a => ((System.ComponentModel.DescriptionAttribute)a.Att).Description == value);
                    return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
                }
            }
        }
    }
}