namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
    using System;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    public class UpdateMcrDescriptionTask : Task
    {
        private readonly Service service;
        private readonly Order order;

        public UpdateMcrDescriptionTask(Helpers helpers, Service service, Order order) : base(helpers)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            IsBlocking = false;
        }

        public override string Description => $"Update MCR Description for {service.Name}";

        public override Task CreateRollbackTask()
        {
            return new UpdateMcrDescriptionTask(helpers, service, order);
        }

        protected override void InternalExecute()
        {
            helpers.ServiceManager.UpdateMcrDescription(service, order);
        }
    }
}
