namespace LastEpoch_Hud.Tests.Support;

/// <summary>Reads a list file copied to the test output: one entry per line, <c>#</c> comments and blank lines skipped.</summary>
internal static class ListFile
{
    public static HashSet<string> Load(string fileName) =>
        File.ReadLines(Path.Combine(AppContext.BaseDirectory, fileName))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);
}
