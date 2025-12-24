namespace UpdateTicketStatus_4
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class NoteHandler
	{
		public static bool TryUpdateNote(Helpers helpers, int[] convertedSplitTicketId, Guid parsedId)
		{
			var noteManager = (NoteManager)helpers.NoteManager;
			if (parsedId != Guid.Empty && noteManager.TryGetNote(parsedId, out Note noteFromUniqueId))
			{
				if (noteFromUniqueId.Status == Status.Open) noteManager.CompleteNote(noteFromUniqueId);
				else if (noteFromUniqueId.Status == Status.Alarm) noteManager.AcknowledgeAlarmNote(noteFromUniqueId);
				else if (noteFromUniqueId.Status == Status.AcknowledgedAlarm) noteManager.SetNoteAsAlarm(noteFromUniqueId);

				return true;
			}
			else if (convertedSplitTicketId.Length == 2 && noteManager.TryGetNote(convertedSplitTicketId[0], convertedSplitTicketId[1], out var noteFromFullTicketId))
			{
				if (noteFromFullTicketId.Status == Status.Open) noteManager.CompleteNote(noteFromFullTicketId);
				else if (noteFromFullTicketId.Status == Status.Alarm) noteManager.AcknowledgeAlarmNote(noteFromFullTicketId);
				else if (noteFromFullTicketId.Status == Status.AcknowledgedAlarm) noteManager.SetNoteAsAlarm(noteFromFullTicketId);

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}