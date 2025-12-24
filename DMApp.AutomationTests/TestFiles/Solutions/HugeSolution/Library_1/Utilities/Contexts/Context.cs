namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public abstract class Context
	{
		public Context(IEngine engine, Scripts script)
		{
			Script = script;

			for (int i = 0; i < 10; i++)
			{
				var scriptInput = engine.GetScriptParam(i);

				if (scriptInput != null)
				{
					ScriptParameters.Add(scriptInput);
				}
			}
		}

		public Scripts Script { get; set; }

		public List<ScriptParam> ScriptParameters { get; } = new List<ScriptParam>();

		public static Context Factory(IEngine engine, Scripts script)
		{
			switch(script)
			{
				case Scripts.UpdateService:
					return new UpdateServiceContext(engine);

				default:
					return new DefaultContext(engine, script);
			}	
		}
	}
}
