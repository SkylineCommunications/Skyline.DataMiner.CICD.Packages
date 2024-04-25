namespace Skyline.DataMiner.CICD.DMApp.Common
{
	using System.Diagnostics;

	/// <summary>
	/// Creates instances of <see cref="IDotnet"/>.
	/// </summary>
	public static class DotnetFactory
	{
		/// <summary>
		/// Creates a single instance of <see cref="IDotnet"/>.
		/// </summary>
		/// <returns>An instance of <see cref="IDotnet"/>.</returns>
		public static IDotnet Create(DataReceivedEventHandler onOutput, DataReceivedEventHandler onError)
		{
			return new Dotnet(onOutput, onError);
		}
	}
}