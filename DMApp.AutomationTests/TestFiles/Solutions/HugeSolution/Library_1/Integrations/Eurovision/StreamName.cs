using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	/// <summary>
	/// This class is used to parse the technicalSystem.systemName property of a Eurovision Synopsis.
	/// </summary>
	public class StreamName
	{
		public StreamName(string streamName)
		{
			if (String.IsNullOrWhiteSpace(streamName))
				throw new ArgumentException("streamName cannot be null or empty");

			// Examples: 
			// DVBS2 8PSK 4.9373Ms/s FEC 3/4,Pilot:On  Roll-off 0.2  (11_SD MPEG2 422 10.7514 Mbps)
			// NS4 16APSK 35.294118 Ms/s FEC 32/45, Pilot: On  Roll-Off 0.02 (MPEG4/H.264 422 95.92 Mbps) NS4 NLC Mode: OFF

			// Clean streamName
			streamName = streamName.ToUpper();
			streamName = streamName.Replace("MS/S", String.Empty);
			streamName = streamName.Replace("FEC", String.Empty);
			streamName = streamName.Replace("PILOT", String.Empty);
			streamName = streamName.Replace("ROLL-OFF", String.Empty);

			int endIndex = streamName.IndexOf('(');
			if (endIndex == -1) endIndex = streamName.Length;

			string[] splitStreamName = streamName.Substring(0, endIndex).Split(' ', ',', ':').Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();

			ModulationStandard = splitStreamName[0];
			Modulation = splitStreamName[1];
			if (Double.TryParse(splitStreamName[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double symbolRate))
			{
				SymbolRate = symbolRate;
			}
			else
			{
				throw new ArgumentException(String.Format("Unable to parse {0} to a double", splitStreamName[2]));
			}

			FEC = splitStreamName[3];

			Pilot = splitStreamName[4].Equals("ON", StringComparison.InvariantCultureIgnoreCase);

			if (Double.TryParse(splitStreamName[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double rollOff))
			{
				RollOff = rollOff;
			}
			else
			{
				throw new ArgumentException(String.Format("Unable to parse {0} to a double", splitStreamName[7]));
			}

			string encodingInfo = streamName.Substring(streamName.IndexOf('('));
			if (encodingInfo.Contains("MPEG2"))
			{
				Encoding = "MPEG2";
			}
			else if (encodingInfo.Contains("MPEG4") || encodingInfo.Contains("H.264"))
			{
				Encoding = "MPEG4/H.264";
			}
			else if (encodingInfo.Contains("HEVC") || encodingInfo.Contains("H.265"))
			{
				Encoding = "HEVC/H.265";
			}
			else
			{
				// If no encoding information is known, you can assume it is MPEG4
				Encoding = "MPEG4/H.264";
			}
		}

		/// <summary>
		/// Gets the modulation standard that is described in the stream name.
		/// </summary>
		public string ModulationStandard { get; private set; }

		/// <summary>
		/// Gets the modulation that is described in the stream name.
		/// </summary>
		public string Modulation { get; private set; }

		/// <summary>
		/// Gets the symbol rate that is described in the stream name.
		/// </summary>
		public double SymbolRate { get; private set; }

		/// <summary>
		/// Gets the FEC that is described in the stream name.
		/// </summary>
		public string FEC { get; private set; }

		/// <summary>
		/// Gets the pilot value that is described in the stream name.
		/// </summary>
		public bool Pilot { get; private set; }

		/// <summary>
		/// Gets the roll-off factor that is described in the stream name.
		/// </summary>
		public double RollOff { get; private set; }

		/// <summary>
		/// Gets the type of encoding described in the stream name.
		/// </summary>
		public string Encoding { get; private set; }
	}
}