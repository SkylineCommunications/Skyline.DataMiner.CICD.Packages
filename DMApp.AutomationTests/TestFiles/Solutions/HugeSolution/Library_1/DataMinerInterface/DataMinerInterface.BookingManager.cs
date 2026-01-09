namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;

	public static partial class DataMinerInterface
	{
		public static class BookingManager
		{
			[WrappedMethod("BookingManager", "CreateNewBooking", 20000)]
			public static Net.ResourceManager.Objects.ReservationInstance CreateNewBooking(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Booking booking, IEnumerable<Library.Solutions.SRM.Model.Function> functions, IEnumerable<Library.Solutions.SRM.Model.Events.Event> events, IEnumerable<Library.Solutions.SRM.Model.Properties.Property> properties)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, booking.Description);

				var reservation = bookingManager.CreateNewBooking((Automation.Engine)helpers.Engine, booking, functions, events, properties);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "EditBooking", 10000)]
			public static Net.ResourceManager.Objects.ReservationInstance EditBooking(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Guid id, Booking booking, IEnumerable<Library.Solutions.SRM.Model.Function> functions, IEnumerable<Library.Solutions.SRM.Model.Events.Event> events, IEnumerable<Library.Solutions.SRM.Model.Properties.Property> properties)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, booking.Description);

				var reservation = bookingManager.EditBooking((Automation.Engine)helpers.Engine, id, booking, functions, events, properties);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "ChangeStateToConfirmed", 20000)] // YLE prod has seen execution times of up to 16 seconds
			public static Net.ResourceManager.Objects.ReservationInstance ChangeStateToConfirmed(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var reservation = bookingManager.ChangeStateToConfirmed((Automation.Engine)helpers.Engine, reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "TryChangeStateToConfirmed")]
			public static bool TryChangeStateToConfirmed(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, ref Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var result = bookingManager.TryChangeStateToConfirmed((Automation.Engine)helpers.Engine, ref reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("BookingManager", "ChangeTime")]
			public static Net.ResourceManager.Objects.ReservationInstance ChangeTime(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance, ChangeTimeInputData changeTimeInputData)
			{
				// 24/08/2022 Peter Vanpoucke: DateTime arguments must be in Local time 

				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				if (changeTimeInputData.StartDate.Kind != DateTimeKind.Local || changeTimeInputData.EndDate.Kind != DateTimeKind.Local)
				{
					Log(helpers, MethodBase.GetCurrentMethod(), "WARNING: DateTime arguments should be in local time");
				}

				var reservation = bookingManager.ChangeTime((Automation.Engine)helpers.Engine, reservationInstance, changeTimeInputData);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "TryChangeTime")]
			public static bool TryChangeTime(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, ref Net.ResourceManager.Objects.ReservationInstance reservationInstance, ChangeTimeInputData changeTimeInputData)
			{
				// 24/08/2022 Peter Vanpoucke: DateTime arguments must be in Local time 

				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				if (changeTimeInputData.StartDate.Kind != DateTimeKind.Local || changeTimeInputData.EndDate.Kind != DateTimeKind.Local)
				{
					Log(helpers, MethodBase.GetCurrentMethod(), "WARNING: DateTime arguments should be in local time");
				}

				var success = bookingManager.TryChangeTime((Automation.Engine)helpers.Engine, ref reservationInstance, changeTimeInputData);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return success;
			}

			[WrappedMethod("BookingManager", "TryExtend")]
			public static bool TryExtend(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, ref Net.ResourceManager.Objects.ReservationInstance reservationInstance, ExtendBookingInputData extendBookingInputData)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var success = bookingManager.TryExtend((Automation.Engine)helpers.Engine, ref reservationInstance, extendBookingInputData);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return success;
			}

			[WrappedMethod("BookingManager", "Extend")]
			public static Net.ResourceManager.Objects.ReservationInstance Extend(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance, ExtendBookingInputData extendBookingInputData)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var reservation = bookingManager.Extend((Automation.Engine)helpers.Engine, reservationInstance, extendBookingInputData);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "TryStart")]
			public static bool TryStart(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, ref Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var success = bookingManager.TryStart((Automation.Engine)helpers.Engine, ref reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return success;
			}

			[WrappedMethod("BookingManager", "Finish")]
			public static Net.ResourceManager.Objects.ReservationInstance Finish(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var reservation = bookingManager.Finish((Automation.Engine)helpers.Engine, reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "TryFinish")]
			public static bool TryFinish(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, ref Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var success = bookingManager.TryFinish((Automation.Engine)helpers.Engine, ref reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return success;
			}

			[WrappedMethod("BookingManager", "Cancel")]
			public static Net.ResourceManager.Objects.ReservationInstance Cancel(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				var reservation = bookingManager.Cancel((Automation.Engine)helpers.Engine, reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("BookingManager", "Delete")]
			public static void Delete(Helpers helpers, Library.Solutions.SRM.BookingManager bookingManager, Net.ResourceManager.Objects.ReservationInstance reservationInstance)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				bookingManager.Delete((Automation.Engine)helpers.Engine, reservationInstance);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("BookingManager", "ChangeName")]
			public static Net.ResourceManager.Objects.ServiceReservationInstance ChangeName(Helpers helpers, Net.ResourceManager.Objects.ServiceReservationInstance reservationInstance, string name)
			{
				if (helpers == null) throw new ArgumentNullException(nameof(helpers));
				if (reservationInstance == null) throw new ArgumentNullException(nameof(reservationInstance));
				if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Description);

				try
                {
					// Dirty hack to fix behaviour with running edits on yledminertst01
					// https://collaboration.dataminer.services/task/205193
					Net.ResourceManager.Objects.ServiceReservationInstance updatedReservation = reservationInstance;
					int retries = 0;
					do
					{
						reservationInstance.Name = name;
						ResourceManager.AddOrUpdateReservationInstances(helpers, reservationInstance);

						updatedReservation = ResourceManager.GetReservationInstance(helpers, reservationInstance.ID) as Net.ResourceManager.Objects.ServiceReservationInstance;
						helpers.Log(nameof(BookingManager), nameof(ChangeName), $"Retry: {retries}, name to set: {name}, updated name: {updatedReservation.Name}");
						if (String.Equals(name, updatedReservation.Name)) break;

						retries++;
						helpers.Engine.Sleep(100);
					}
					while (retries < 20);

					return updatedReservation;
				}
				catch (Exception)
                {
					throw;
				}
				finally
				{
					LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
				}
			}
		}
	}
}