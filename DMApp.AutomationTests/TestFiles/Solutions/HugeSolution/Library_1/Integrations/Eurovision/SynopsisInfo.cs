using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	/// <summary>
	/// Represents a row in the Synopses table from an EBU Synopsis Web Service element.
	/// </summary>
	public class SynopsisInfo
	{
		public SynopsisInfo(object[] synopsisTableRow)
		{
			if (synopsisTableRow == null) throw new ArgumentNullException("synopsisTableRow");
			if (synopsisTableRow.Length < 20)
				throw new ArgumentException("The synopsisTableRow should contain at least 20 values.");

			DisplayKey = Convert.ToString(synopsisTableRow[0]);
			TransmissionId = Convert.ToString(synopsisTableRow[1]);
			TransmissionNumber = Convert.ToString(synopsisTableRow[2]);
			StatusCode = Convert.ToString(synopsisTableRow[4]);
			EventNumber = Convert.ToString(synopsisTableRow[8]);
			RequestId = Convert.ToString(synopsisTableRow[9]);
			FilePath = Convert.ToString(synopsisTableRow[10]);
			ProductCode = Convert.ToString(synopsisTableRow[11]);
			LineUp = Convert.ToString(synopsisTableRow[12]);
			Place = Convert.ToString(synopsisTableRow[13]);
			Nature1 = Convert.ToString(synopsisTableRow[14]);
			Nature2 = Convert.ToString(synopsisTableRow[15]);
			TextFilePath = Convert.ToString(synopsisTableRow[19]);

			if (Double.TryParse(Convert.ToString(synopsisTableRow[3]), NumberStyles.Any, CultureInfo.InvariantCulture, out double version))
			{
				Version = version;
			}
			else
			{
				throw new FormatException("Unable to parse Version to a double (" + synopsisTableRow[3] + ")");
			}

			if (DateTime.TryParse(Convert.ToString(synopsisTableRow[6]), out var startTime))
			{
				StartTime = startTime.ToLocalTime().TimeOfDay;
			}
			else
			{
				StartTime = null;
			}

			if (DateTime.TryParse(Convert.ToString(synopsisTableRow[7]), out var endTime))
			{
				EndTime = endTime.ToLocalTime().TimeOfDay;
			}
			else
			{
				EndTime = null;
			}

			try
			{
				BeginDate = DateTime.FromOADate((double)synopsisTableRow[5]).ToLocalTime();
			}
			catch (Exception)
			{
				BeginDate = null;
			}

			try
			{
				EntryCreationTime = DateTime.FromOADate((double)synopsisTableRow[16]).ToLocalTime();
			}
			catch (Exception)
			{
				EntryCreationTime = null;
			}
		}

		public string DisplayKey { get; private set; }

		public string TransmissionId { get; private set; }

		public string TransmissionNumber { get; private set; }

		public double Version { get; private set; }

		public string StatusCode { get; private set; }

		public DateTime? BeginDate { get; private set; }

		public TimeSpan? StartTime { get; private set; }

		public TimeSpan? EndTime { get; private set; }

		public string EventNumber { get; private set; }

		public string RequestId { get; private set; }

		public string FilePath { get; private set; }

		public string ProductCode { get; private set; }

		public string LineUp { get; private set; }

		public string Place { get; private set; }

		public string Nature1 { get; private set; }

		public string Nature2 { get; private set; }

		public DateTime? EntryCreationTime { get; private set; }

		public string TextFilePath { get; private set; }
	}
}