namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Net.Ticketing;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NoteManager : INoteManager
    {
        private readonly Helpers helpers;
        private const string TicketDomainName = "Notes";

        private readonly TicketingManager ticketingManager;

        public NoteManager(Helpers helpers)
        {
            this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
            ticketingManager = new TicketingManager(helpers, TicketDomainName);
        }

		public Guid TicketDomainId => ticketingManager.TicketFieldResolver.ID;

        /// <summary>
        /// Add or Update the given Note.
        /// </summary>
        /// <param name="note">The Note to add or update.</param>
        /// <param name="fullTicketId">The full ticket ID of the Note.</param>
        /// <returns>A boolean indicating if the Note was successfully added or updated.</returns>
        /// 
        public bool AddOrUpdateNote(Note note, out string fullTicketId)
        {
            Ticket ticket;
            if (note.DataMinerId.HasValue && note.TicketId.HasValue)
            {
                ticket = ticketingManager.GetTicket(note.DataMinerId.Value, note.TicketId.Value);
            }
            else
            {
                ticket = new Ticket { CustomFieldResolverID = ticketingManager.TicketFieldResolver.ID };
            }

            ticket.CustomTicketFields[Note.TitleTicketField] = note.Title;
            ticket.CustomTicketFields[Note.DescriptionTicketField] = note.Description;
            ticket.CustomTicketFields[Note.DueTicketField] = note.DueDate;
            ticket.CustomTicketFields[Note.PageTicketField] = EnumExtensions.GetDescriptionFromEnumValue(note.Page);

            // these fields are only set for Eurovision notes
            if (note.StartDate.HasValue) ticket.CustomTicketFields[Note.StartTicketfield] = note.StartDate.Value;
            if (!String.IsNullOrEmpty(note.EurovisionId)) ticket.CustomTicketFields[Note.EurovisionIdTicketField] = note.EurovisionId;

            var stateTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int>();
            stateTicketField.Value = (int)note.Status;
            stateTicketField.Name = note.Status.GetDescription();
            ticket.CustomTicketFields[Note.StateTicketField] = stateTicketField;

            return ticketingManager.AddOrUpdateTicket(ticket, out fullTicketId);
        }

        public void DeleteNote(Note note)
        {
            var ticket = ticketingManager.GetTicket((int)note.DataMinerId, (int)note.TicketId);
            if (ticket is null) return;

            ticketingManager.DeleteTicket(ticket);
        }

        /// <summary>
        /// Tries to get the Note for the given IDs.
        /// </summary>
        public bool TryGetNote(int dataminerId, int ticketId, out Note note)
        {
            try
            {
                note = GetNote(dataminerId, ticketId);
                return true;
            }
            catch (Exception e)
            {
				helpers.Log(nameof(NoteManager), nameof(TryGetNote), $"Exception while trying to get Note {dataminerId}/{ticketId}: {e}");
                note = null;
                return false;
            }
        }

		/// <summary>
		/// Tries to get the Note for the given IDs.
		/// </summary>
		public bool TryGetNote(Guid uniqueId, out Note note)
		{
			try
			{
				note = GetNote(uniqueId);
				return true;
			}
			catch (Exception)
			{
				note = null;
				return false;
			}
		}

		/// <summary>
		/// Retrieves all Notes.
		/// </summary>
		public List<Note> AllNotesOlderThan(TimeSpan time)
        {
            var allNotes = ticketingManager.AllTicketsOlderThan(time);
            List<Note> notes = allNotes.Select(x => new Note(x)).ToList();
            return notes;
        }

        /// <summary>
        /// Get the note associated with the give eurovision id.
        /// </summary>
        /// <param name="eurovisionId">The eurovision id.</param>
        /// <param name="note">The note.</param>
        /// <returns>True if the note was found.</returns>
        public bool TryGetEurovisionNote(string eurovisionId, out Note note)
        {
            try
            {
                note = GetEurovisionNote(eurovisionId);
                return note != null;
            }
            catch (Exception)
            {
                note = null;
                return false;
            }
        }

        /// <summary>
        /// Get the Note for the given IDs.
        /// </summary>
        public Note GetNote(int dataminerId, int ticketId)
        {
            Ticket ticket = ticketingManager.GetTicket(dataminerId, ticketId);
            if (ticket == null) throw new TicketNotFoundException(dataminerId, ticketId);

            return new Note(ticket);
        }

        /// <summary>
		/// Get the Note for the given IDs.
		/// </summary>
		public Note GetNote(Guid uniqueId)
        {
            Ticket ticket = ticketingManager.GetTicket(uniqueId);
            if (ticket == null) throw new TicketNotFoundException(uniqueId);

            return new Note(ticket);
        }

        /// <summary>
        /// Get the note for the give eurovision id.
        /// </summary>
        /// <param name="eurovisionId">The eurovision id.</param>
        /// <returns>The note.</returns>
        public Note GetEurovisionNote(string eurovisionId)
        {
            var ticket = ticketingManager.GetTicketWithFieldValue(Note.EurovisionIdTicketField, eurovisionId);
            if (ticket == null) return null;

            return new Note(ticket);
        }

        /// <summary>
        /// Set the Status for the passed notes to Closed.
        /// </summary>
        /// <param name="notes">A collections of notes to complete.</param>
        public void CompleteNotes(IEnumerable<Note> notes)
        {
            if (notes == null) throw new ArgumentNullException("notes");

            foreach (Note note in notes)
            {
                if (note.Status == Status.Closed) continue;

                note.Status = Status.Closed;
                AddOrUpdateNote(note, out string fullTicketId);
            }
        }

        /// <summary>
        /// Set the Status for the alarm notes to Acknowledge.
        /// </summary>
        /// <param name="notes">A collections of notes to complete.</param>
        public void AcknowledgeAlarmNotes(IEnumerable<Note> notes)
        {
            if (notes == null) throw new ArgumentNullException(nameof(notes));

            foreach (Note note in notes)
            {
                if (note.Status == Status.Closed || note.Status == Status.Open) continue;

                note.Status = Status.AcknowledgedAlarm;
                AddOrUpdateNote(note, out string fullTicketId);
            }
        }

        /// <summary>
        /// Set the Status for the alarm notes to Alarm.
        /// </summary>
        /// <param name="notes">A collections of notes to set to alarm state.</param>
        public void SetNotesToAlarm(IEnumerable<Note> notes)
        {
            if (notes == null) throw new ArgumentNullException(nameof(notes));

            foreach (Note note in notes)
            {
                if (note.Status == Status.Closed || note.Status == Status.Open) continue;

                note.Status = Status.Alarm;
                AddOrUpdateNote(note, out string fullTicketId);
            }
        }

        /// <summary>
        /// Set the Status for the passed note to Closed.
        /// </summary>
        /// <param name="note">The note to complete.</param>
        public void CompleteNote(Note note)
        {
            CompleteNotes(new[] { note });
        }

        /// <summary>
        /// Set the Status for the alarm note to Acknowledge.
        /// </summary>
        /// <param name="note">The note to complete.</param>
        public void AcknowledgeAlarmNote(Note note)
        {
            AcknowledgeAlarmNotes(new[] { note });
        }

        /// <summary>
        /// Set the Status for the alarm note to Alarm.
        /// </summary>
        /// <param name="note">The note to complete.</param>
        public void SetNoteAsAlarm(Note note)
        {
            SetNotesToAlarm(new[] { note });
        }
    }
}