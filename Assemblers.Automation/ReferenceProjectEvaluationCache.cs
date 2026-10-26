namespace Skyline.DataMiner.CICD.Assemblers.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Skyline.DataMiner.CICD.Assemblers.Common;

    internal sealed class ReferenceProjectEvaluationCache
    {
        private readonly Dictionary<(string Path, string Framework, string Configuration), ReferencedProjectInfo> projects =
            new Dictionary<(string Path, string Framework, string Configuration), ReferencedProjectInfo>();
        private readonly Func<string, string, string, ReferencedProjectInfo> evaluate;

        internal ReferenceProjectEvaluationCache()
            : this(MSBuildHelpers.EvaluateReferenceProject)
        {
        }

        internal ReferenceProjectEvaluationCache(Func<string, string, string, ReferencedProjectInfo> evaluate)
        {
            this.evaluate = evaluate;
        }

        internal ReferencedProjectInfo Evaluate(string path, string framework, string configuration)
        {
            var key = (Path.GetFullPath(path), framework ?? string.Empty, configuration ?? string.Empty);
            if (!projects.TryGetValue(key, out var project))
            {
                project = evaluate(key.Item1, framework, configuration);
                projects.Add(key, project);
            }

            return project;
        }

        internal void Clear()
        {
            projects.Clear();
        }
    }
}
