namespace Skyline.DataMiner.CICD.Assemblers.Automation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Automation.VisualStudio;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    /// <summary>
    /// Builds the scripts of an Automation script solution.
    /// </summary>
    public class AutomationScriptSolutionBuilder
    {
        private readonly AutomationScriptSolution _automationScriptSolution;
        private readonly ILogCollector logCollector;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptSolutionBuilder"/> class.
        /// </summary>
        /// <param name="automationScriptSolution">The Automation script solution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="automationScriptSolution"/> is <see langword="null"/>.</exception>
        public AutomationScriptSolutionBuilder(AutomationScriptSolution automationScriptSolution)
        {
            _automationScriptSolution = automationScriptSolution ?? throw new ArgumentNullException(nameof(automationScriptSolution));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptSolutionBuilder"/> class.
        /// </summary>
        /// <param name="automationScriptSolution">The Automation script solution.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <exception cref="ArgumentNullException"><paramref name="automationScriptSolution"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> is <see langword="null"/>.</exception>
        public AutomationScriptSolutionBuilder(AutomationScriptSolution automationScriptSolution, ILogCollector logCollector)
            : this(automationScriptSolution)
        {
            this.logCollector = logCollector ?? throw new ArgumentNullException(nameof(logCollector));
        }

        /// <summary>
        /// Builds the scripts of an Automation script solution.
        /// </summary>
        /// <returns>A list of the scripts with its build results.</returns>
        public async Task<List<KeyValuePair<Script, BuildResultItems>>> BuildAsync()
        {
            var allScripts = new ConcurrentBag<Script>(_automationScriptSolution.Scripts.Select(s => s.Script));

            ConcurrentBag<KeyValuePair<Script, BuildResultItems>> results = new ConcurrentBag<KeyValuePair<Script, BuildResultItems>>();
            var tasks = _automationScriptSolution.Scripts.Select(async item =>
            {
                (Script script, SolutionFolder folder) = item;

                var projects = new Dictionary<string, Project>();

                var actionsFolder = folder.GetSubFolder("Actions");
                if (actionsFolder != null)
                {
                    foreach (var p in actionsFolder.GetDescendantProjects())
                    {
                        projects[p.Name] = _automationScriptSolution.LoadProject(p);
                    }
                }

                AutomationScriptBuilder builder;
                if (logCollector == null)
                {
                    builder = new AutomationScriptBuilder(script, projects, allScripts, _automationScriptSolution.SolutionDirectory);
                }
                else
                {
                    builder = new AutomationScriptBuilder(script, projects, allScripts, logCollector, _automationScriptSolution.SolutionDirectory);
                }

                var buildResult = await builder.BuildAsync().ConfigureAwait(false);
                results.Add(new KeyValuePair<Script, BuildResultItems>(script, buildResult));
            });

            await Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}