using System.IO.Enumeration;

namespace RayGui_cs
{
    // One entry of a WinForms style filter string, such as "Text files (*.txt)|*.txt".
    internal sealed class FileFilter
    {
        private static readonly char[] WildcardChars = ['*', '?', '<', '>', '"'];

        private readonly string[] expressions;

        public FileFilter(string description, string patterns)
        {
            Description = description;
            Patterns = patterns.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            expressions = Patterns.Select(FileSystemName.TranslateWin32Expression).ToArray();
            Extension = Patterns.Select(GetExtension).FirstOrDefault(e => e is not null);
        }

        public string Description { get; }

        public string[] Patterns { get; }

        // The extension of the first pattern that names one, such as "txt" for "*.txt"; null for patterns like "*.*".
        public string? Extension { get; }

        public bool Matches(string fileName)
        {
            if (expressions.Length == 0)
            {
                return true;
            }

            foreach (string expression in expressions)
            {
                if (FileSystemName.MatchesWin32Expression(expression, fileName, ignoreCase: true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <exception cref="ArgumentException">The filter doesn't have a pattern for every description.</exception>
        public static FileFilter[] Parse(string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return [];
            }

            string[] parts = filter.Split('|');
            if (parts.Length % 2 != 0)
            {
                throw new ArgumentException(
                    "The filter string is not valid. It must contain a description, followed by a vertical bar (|) and the filter pattern, " +
                    "with each description and pattern pair separated by a vertical bar, such as \"Text files|*.txt|All files|*.*\".",
                    nameof(filter));
            }

            var filters = new FileFilter[parts.Length / 2];
            for (int i = 0; i < filters.Length; i++)
            {
                filters[i] = new FileFilter(parts[i * 2], parts[i * 2 + 1]);
            }
            return filters;
        }

        private static string? GetExtension(string pattern)
        {
            if (!pattern.StartsWith("*.", StringComparison.Ordinal))
            {
                return null;
            }

            string extension = pattern[2..];
            return extension.Length == 0 || extension.IndexOfAny(WildcardChars) >= 0 ? null : extension;
        }
    }
}
