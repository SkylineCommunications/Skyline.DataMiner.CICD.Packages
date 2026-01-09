namespace UpdateService_4
{
	using System.ComponentModel;

	public enum ScriptAction
	{
		[Description("Edit")]
		Edit,
		[Description("ResourceChange")]
		ResourceChange,
		[Description("View")]
		View,
		[Description("ResourceChange_FromRecordingApp")]
		ResourceChange_FromRecordingApp,
		[Description("UpdateTiming")]
		UpdateTiming,
		[Description("Delete")]
		Delete,
		[Description("UseSharedSource")]
		UseSharedSource
	}
}