namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    /// <summary>
    /// Combines code of multiple C# code files into a single file.
    /// </summary>
    public static class CSharpCodeCombiner
    {
        /// <summary>
        /// Combines the code of the specified C# code files into a single file.
        /// </summary>
        /// <param name="files">The C# code files to combine.</param>
        /// <returns>The string that contains the combined code.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="files"/> is null.</exception>
        public static string CombineFiles(IList<ProjectFile> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            StringBuilder newCode = new StringBuilder();

            if (files.Count > 1)
            {
                var syntaxTrees = files.Select(f => CSharpSyntaxTree.ParseText(f.Content, path: f.Name)).ToList();
                var compilationUnitRoots = syntaxTrees.Where(t => t.HasCompilationUnitRoot).Select(t => t.GetCompilationUnitRoot()).ToList();

                var usings = GetCombinedUsings(compilationUnitRoots);
                newCode.AppendLine(SyntaxFactory.List(usings).ToFullString());

                for (int i = 0; i < compilationUnitRoots.Count; i++)
                {
                    var root = compilationUnitRoots[i];

                    if (i != 0)
                    {
                        newCode.AppendLine();
                    }

                    newCode.AppendLine("//---------------------------------");
                    newCode.AppendLine("// " + root.SyntaxTree.FilePath);
                    newCode.AppendLine("//---------------------------------");

                    string code = root.Members.ToFullString();

                    newCode.Append(code);
                }
            }
            else if (files.Count > 0)
            {
                newCode.Append(files[0].Content);
            }
            else
            {
                // No files.
            }

            return newCode.ToString();
        }

        private static IEnumerable<UsingDirectiveSyntax> GetCombinedUsings(IList<CompilationUnitSyntax> compilationUnitRoots)
        {
            var usings = new HashSet<(string, string)>();

            foreach (var r in compilationUnitRoots)
            {
                foreach (var u in r.Usings)
                {
                    string name = u.Name.ToString();
                    string alias = u.Alias?.Name?.ToString();

                    if (!usings.Add((alias, name)))
                    {
                        continue;
                    }

                    yield return u;
                }
            }
        }
    }
}