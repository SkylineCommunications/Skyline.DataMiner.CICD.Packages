namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Core.InterAppCalls.Common.Shared;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// The recording configuration containing additional details for a recording that are not stored in profile parameters.
	/// </summary>
	public class RecordingConfiguration : DisplayedObject, IYleChangeTracking
	{
		private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();
		private string recordingName;
		private List<string> selectableRecordingFileDestinations = new List<string>();
		private FileDestination recordingFileDestination;
		private string evsMessiNewsTarget;
		private List<EvsMessiNewsTarget> selectableEvsMessiNewsTargets = new List<EvsMessiNewsTarget>();
		private string plasmaIdForArchive;

		public RecordingConfiguration()
		{
			AcceptChanges(null);
			DeadLineForArchiving = DateTime.Now.AddHours(1);
		}

		private RecordingConfiguration(RecordingConfiguration other)
		{
			SubRecordings = other.SubRecordings.Select(sr => sr.Clone()).Cast<SubRecording>().ToList();

			CloneHelper.CloneProperties(other, this);
		}

		public bool IsConfigured { get; set; } = false;

		/// <summary>
		/// The service name of the service that needs to be recorded.
		/// Only needs to be set in case the operator in the UI selected to record a specific service.
		/// </summary>
		[ChangeTracked]
		public string NameOfServiceToRecord { get; set; }

		public event EventHandler<string> RecordingNameChanged;

		/// <summary>
		/// Name of the recording.
		/// </summary>
		[ChangeTracked]
		public string RecordingName
		{
			get => recordingName;
			set
			{
				recordingName = value;
				RecordingNameChanged?.Invoke(this, value);
			}
		}

		/// <summary>
		/// Plasma ID for archiving.
		/// Only applicable on Live recordings.
		/// </summary>
		[ChangeTracked]
		public string PlasmaIdForArchive
		{
			get => plasmaIdForArchive;
			set
			{
				plasmaIdForArchive = value;
				PlasmaIdForArchiveChanged?.Invoke(this, value);
			}
		}

		public event EventHandler<string> PlasmaIdForArchiveChanged;

		public bool CopyPlasmaIdFromOrder { get; set; }

		/// <summary>
		/// Destination of the recording file.
		/// </summary>
		[ChangeTracked]
		public FileDestination RecordingFileDestination
		{
			get => recordingFileDestination;
			set
			{
				recordingFileDestination = value;
				RecordingFileDestinationChanged?.Invoke(this, recordingFileDestination);
			}
		}

		public event EventHandler<FileDestination> RecordingFileDestinationChanged;

		[JsonIgnore]
		public List<string> SelectableRecordingFileDestinations
		{
			get => selectableRecordingFileDestinations;
			set
			{
				selectableRecordingFileDestinations = value;
				SelectableRecordingFileDestinationsChanged?.Invoke(this, selectableRecordingFileDestinations);
			}
		}

		public event EventHandler<List<string>> SelectableRecordingFileDestinationsChanged;

		/// <summary>
		/// The video resolution of the recording file.
		/// </summary>
		[ChangeTracked]
		public VideoResolution RecordingFileVideoResolution { get; set; } = VideoResolution.Resolution1080i50;

		/// <summary>
		/// The video codec of the recording file.
		/// </summary>
		[ChangeTracked]
		public VideoCodec RecordingFileVideoCodec { get; set; } = VideoCodec.AvcI100;

		/// <summary>
		/// The time codec for the recording file.
		/// </summary>
		[ChangeTracked]
		public TimeCodec RecordingFileTimeCodec { get; set; } = TimeCodec.Real;

		/// <summary>
		/// Indicates if subtitle proxy is required.
		/// </summary>
		[ChangeTracked]
		public bool SubtitleProxy { get; set; }

		/// <summary>
		/// The proxy format.
		/// Only in case Subtitle Proxy is required.
		/// </summary>
		[ChangeTracked]
		public ProxyFormat ProxyFormat { get; set; }

		/// <summary>
		/// Indicates if a fast rerun copy is required.
		/// </summary>
		[ChangeTracked]
		public bool FastRerunCopy { get; set; }

		/// <summary>
		/// Indicates if a fast areena copy is required.
		/// </summary>
		[ChangeTracked]
		public bool FastAreenaCopy { get; set; }

		/// <summary>
		/// Indicates if this recording is broadcast ready.
		/// </summary>
		[ChangeTracked]
		public bool BroadcastReady { get; set; }

		/// <summary>
		/// The EVS Target for this recording. Only applies to Messi News Recordings.
		/// </summary>
		[ChangeTracked]
		[JsonProperty("RecordingFileDestinationPath")]
		public string EvsMessiNewsTarget
		{
			get => evsMessiNewsTarget;
			set
			{
				evsMessiNewsTarget = value;
				EvsMessiNewsTargetChanged?.Invoke(this, evsMessiNewsTarget);
			}
		}

		public event EventHandler<string> EvsMessiNewsTargetChanged;

		[JsonIgnore]
		public List<EvsMessiNewsTarget> SelectableEvsMessiNewsTargets
		{
			get => selectableEvsMessiNewsTargets;
			set
			{
				selectableEvsMessiNewsTargets = value;
				SelectableEvsMessiNewsTargetsChanged?.Invoke(this, selectableEvsMessiNewsTargets);
			}
		}

		public event EventHandler<List<EvsMessiNewsTarget>> SelectableEvsMessiNewsTargetsChanged;

		[JsonIgnore]
		public EvsMessiNewsTarget SelectedEvsMessiNewsTarget => SelectableEvsMessiNewsTargets.FirstOrDefault(x => x.Target.Equals(EvsMessiNewsTarget)) ?? DefaultEvsMessiNewsTarget;

		[JsonIgnore]
		private EvsMessiNewsTarget DefaultEvsMessiNewsTarget => SelectableEvsMessiNewsTargets.FirstOrDefault(x => x.IsDefault);

		/// <summary>
		/// Indicates if sub recordings are needed.
		/// </summary>
		[ChangeTracked]
		public bool SubRecordingsNeeded { get; set; }

		/// <summary>
		/// Indicates if the recording configuration is part of a Plasma Live News order.
		/// </summary>
		[ChangeTracked]
		public bool IsPlasmaLiveNews { get; set; }

		/// <summary>
		/// The deadline for archiving.
		/// </summary>
		[ChangeTracked]
		public DateTime DeadLineForArchiving { get; set; }

		/// <summary>
		/// TV Channel name from the Transmission table in the MediaGenix WhatsOn element.
		/// Used for the short description on service level.
		/// Only applicable for plasma services.
		/// </summary>
		[ChangeTracked]
		public string PlasmaTvChannelName { get; set; }

		/// <summary>
		/// Program name from the Programs table in the MediaGenix WhatsOn element.
		/// Used for the short description on service level.
		/// Only applicable for plasma services.
		/// </summary>
		[ChangeTracked]
		public string PlasmaProgramName { get; set; }

		/// <summary>
		/// The list of sub recordings.
		/// </summary>
		[ChangeTracked]
		public List<SubRecording> SubRecordings { get; set; } = new List<SubRecording>();

		/// <summary>
		/// Gets a boolean indicating if Change Tracking is enabled.
		/// </summary>
		[JsonIgnore]
		public bool ChangeTrackingStarted { get; private set; }

		[JsonIgnore]
		public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ClassChange(nameof(RecordingConfiguration))) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

		[JsonIgnore]
		public string UniqueIdentifier => nameof(RecordingConfiguration);

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		public event EventHandler<SubRecording> SubRecordingsAdded;

		public event EventHandler<IEnumerable<Guid>> SubRecordingsDeleted;

		public void ClearSubRecordings()
		{
			List<Guid> allSubRecordingIds = SubRecordings.Select(r => r.Id).ToList();
			SubRecordings.Clear();
			SubRecordingsDeleted?.Invoke(this, allSubRecordingIds);
		}

		public void AddSubRecording(SubRecording subRecording)
		{
			SubRecordings.Add(subRecording);
			SubRecordingsAdded?.Invoke(this, subRecording);
		}

		public void DeleteSubRecording(Guid subRecordingId)
		{
			SubRecordings.RemoveAll(r => r.Id == subRecordingId);
			SubRecordingsDeleted?.Invoke(this, new[] { subRecordingId });
		}

		/// <summary>
		/// Resets Change Tracking.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		public void AcceptChanges(Helpers helpers = null)
		{
			ChangeTrackingStarted = true;
			ChangeTrackingHelper.AcceptChanges(this, initialPropertyValues, helpers);
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is RecordingConfiguration oldRecordingConfiguration)) throw new ArgumentException($"Argument is not of type {nameof(RecordingConfiguration)}", nameof(oldObjectInstance));

			return ChangeTrackingHelper.GetChangeComparedTo(this, oldRecordingConfiguration, new ClassChange(nameof(RecordingConfiguration)), helpers);
		}

		/// <summary>
		/// Serialize this object into a json string.
		/// </summary>
		/// <returns>json string.</returns>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		/// <summary>
		/// Deserialize a json string into a RecordingConfiguration object.
		/// </summary>
		/// <param name="recordingConfiguration">The recording configuration json string.</param>
		/// <returns>A RecordingConfiguration object.</returns>
		public static RecordingConfiguration Deserialize(string recordingConfiguration)
		{
			try
			{
				return JsonConvert.DeserializeObject<RecordingConfiguration>(recordingConfiguration);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

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
}