namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class AsciiTable
    {
        private readonly List<string[]> rows = new List<string[]>();
        const int spacing = 4;

        public void AddRow(params string[] fields)
        {
            rows.Add(fields);
        }

        private IDictionary<int, int> CalculateColumnOffsets()
        {
            var result = new Dictionary<int, int>();

            int offset = 0;
            result.Add(0, offset);

            if (rows.Count > 0)
            {
                int numColumns = rows.Max(r => r.Length);

                for (int col = 0; col < numColumns; col++)
                {
                    int maxLength = 0;

                    foreach (var row in rows)
                    {
                        if (row.Length > col && row[col] != null)
                        {
                            maxLength = Math.Max(row[col].Length, maxLength);
                        }
                    }

                    offset += maxLength + spacing;

                    result.Add(col + 1, offset);
                }
            }

            return result;
        }

        public override string ToString()
        {
            var columnOffsets = CalculateColumnOffsets();

            StringBuilder sb = new StringBuilder();

            foreach (var row in rows)
            {
                for (int col = 0; col < row.Length; col++)
                {
                    string field = row[col] ?? "";
                    sb.Append(field);

                    if (col != row.Length - 1)
                    {
                        int offset = columnOffsets[col];
                        int nextOffset = columnOffsets[col + 1];
                        int width = nextOffset - offset;

                        int spaces = width - field.Length;
                        sb.Append(new String(' ', spaces));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
