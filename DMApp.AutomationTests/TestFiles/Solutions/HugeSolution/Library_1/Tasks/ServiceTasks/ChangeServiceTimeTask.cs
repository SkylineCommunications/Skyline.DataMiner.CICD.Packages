using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
    using Service;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

    using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Library.Solutions.SRM;

	public class ChangeServiceTimeTask : Task
    {
        private readonly Service service;

        private readonly Service oldService;

        public ChangeServiceTimeTask(Helpers helpers, Service service, Service oldService = null)
            : base(helpers)
        {
            this.service = service;
            this.oldService = oldService ?? helpers.ServiceManager.GetService(service.Id);
            IsBlocking = true;
        }

        public override string Description => "Changing time for Service " + service.Name;

        public override Task CreateRollbackTask()
        {
            return new ChangeServiceTimeTask(helpers, oldService);
        }

        protected override void InternalExecute()
        {
            var serviceReservation = helpers.ResourceManager.GetReservationInstance(service.Id) ?? throw new ReservationNotFoundException(service.Id);

            var reservationPreRoll = serviceReservation.GetPreRoll();
            var reservationStart = serviceReservation.Start.FromReservation().Add(reservationPreRoll);
            var reservationPostRoll = serviceReservation.GetPostRoll();
            var reservationEnd = serviceReservation.End.FromReservation().Subtract(reservationPostRoll);

            Log(nameof(InternalExecute), $"Service preroll={service.PreRoll}, start={service.Start.ToFullDetailString()}, end={service.End.ToFullDetailString()}, postroll={service.PostRoll}. Service reservation preroll={reservationPreRoll}, start={reservationStart.ToFullDetailString()}, end={reservationEnd.ToFullDetailString()}, postroll={reservationPostRoll}.");

            bool isServiceTimingValidToBeExtended = service.Start == reservationStart && service.PreRoll == reservationPreRoll && service.PostRoll == reservationPostRoll && service.End > reservationEnd;
            if (isServiceTimingValidToBeExtended && !service.IsOrShouldBeRunning)
            {
                Log(nameof(InternalExecute), $"Only service extension required");

                var timeToAdd = service.End - reservationEnd;

                if (!helpers.ServiceManager.TryExtendService(service, timeToAdd)) throw new ChangeTimingFailedException(service.Name);
            }
            else
            {
                Log(nameof(InternalExecute), $"Full service timing change required");

                helpers.ServiceManager.TryChangeServiceTime(service);
            }

            bool lessThanTwelveHoursUntilServiceStart = oldService.Start < DateTime.Now + TimeSpan.FromHours(12);
            if (lessThanTwelveHoursUntilServiceStart)
            {
                service.TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.LateChange, true.ToString() } });
            }
        }
    }
}