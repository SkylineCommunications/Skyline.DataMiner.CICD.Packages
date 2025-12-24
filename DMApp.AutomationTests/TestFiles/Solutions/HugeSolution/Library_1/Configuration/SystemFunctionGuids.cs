using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	public static class SystemFunctionGuids
	{
		public static readonly Guid Destination = new Guid("6918e61e-975b-4c82-9740-77f043943300");

		public static readonly Guid VideoProcessing = new Guid("db640afb-2022-4731-a5dc-06ea31f44b62");

		public static readonly Guid Recording = new Guid("89378eee-a8b7-433c-afea-7ec68d5df029");

		public static readonly Guid AudioProcessing = new Guid("5b50d8af-8ccf-4aa9-87da-5e3f223639f0");
		
		public static readonly Guid GraphicsProcessing = new Guid("03834a2b-3f70-4f2a-8c0d-a0eb9b42cd8c");

		public static readonly Guid Routing = new Guid("78738536-7c34-40eb-9b13-44a8ed4c5fec");

		public static readonly Guid Source = new Guid("c7e8648e-7522-4724-99a8-74e48a45f380");
		
		public static readonly Guid Transmission = new Guid("147f77f6-74d6-4802-8603-5042a6e0ad5d");
		
		public static readonly Guid VizremFarm = new Guid("c8f0bfd2-c0e0-4235-ad76-00930e111cd5");
		
		public static readonly Guid VizremConverter = new Guid("72fdfb96-b134-4493-896c-030857947a68");
		
		public static readonly Guid VizremSt26 = new Guid("65f23f55-81d9-4090-b848-bbc3cff3aac8");

		public static readonly Guid VizremStudio = new Guid("6f0727a9-f2fc-48e0-aeea-4dd710c298c1");

		public static List<Guid> All()
		{
			return typeof(SystemFunctionGuids).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(p => (Guid)p.GetValue(null)).ToList();
		}
	}
}
