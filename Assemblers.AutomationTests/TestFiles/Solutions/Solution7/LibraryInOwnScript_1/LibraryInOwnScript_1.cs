using MyUtils;

using Skyline.DataMiner.Automation;

public class Script
{
	public void Run(Engine engine)
	{
		engine.GenerateInformation(Utils.MakeUppercase("test"));
	}
}