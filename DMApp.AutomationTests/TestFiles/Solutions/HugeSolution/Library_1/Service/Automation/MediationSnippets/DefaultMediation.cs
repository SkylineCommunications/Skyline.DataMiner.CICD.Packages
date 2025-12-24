using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using Skyline.DataMiner.MediationSnippets;

	public class DefaultMediation<T> : IMediator
	{
		public ParameterValue ConvertDeviceToProfile(IMediation mediation, ParameterSet setValue)
		{
			if (typeof(T) == typeof(double))
			{
				return new DoubleParameterValue(setValue.Value.GetDoubleValue());
			}
			else if (typeof(T) == typeof(string))
			{
				return new StringParameterValue(setValue.Value.GetStringValue());
			}
			else throw new NotSupportedException($"Type {typeof(T).Name} is not supported");
		}

		public ProfileToDeviceResult ConvertProfileToDevice(IMediation mediation, ParameterValue value)
		{
			return new ProfileToDeviceResult(value);
		}
	}
}
