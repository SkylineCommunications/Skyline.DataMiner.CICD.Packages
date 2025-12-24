namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes
{
	using System;
	using System.Collections.Generic;

	public interface INoteManager
	{
		Guid TicketDomainId { get; }

		bool AddOrUpdateNote(Note note, out string fullTicketId);

		void DeleteNote(Note note);

		bool TryGetNote(int dataminerId, int ticketId, out Note note);

		bool TryGetEurovisionNote(string eurovisionId, out Note note);

		Note GetNote(int dataminerId, int ticketId);

		Note GetEurovisionNote(string eurovisionId);

        List<Note> AllNotesOlderThan(TimeSpan time);

        void CompleteNotes(IEnumerable<Note> notes);

		void CompleteNote(Note note);
	}
}
