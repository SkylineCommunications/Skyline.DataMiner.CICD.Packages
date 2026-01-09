namespace LiveOrderForm_6.Dialogs
{
	using System;

	[Flags]
	public enum ValidationOptions
	{
		None = 0,
		SaveOrder = 1,
		IsRunning = 2,
		MergeOrder = 4,
		ConfirmOrder = 8,
		RequestEventLock = 16
	}
}