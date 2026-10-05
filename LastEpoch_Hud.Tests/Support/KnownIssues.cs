namespace LastEpoch_Hud.Tests.Support;

/// <summary>
/// Ratchet list of bugs that already exist (KnownIssues.txt). Tests skip these ids,
/// and fail once a listed bug is fixed so the entry gets removed.
/// </summary>
internal static class KnownIssues
{
    private static readonly HashSet<string> Ids = ListFile.Load("KnownIssues.txt");

    public static bool Contains(string id) => Ids.Contains(id);

    public static IEnumerable<string> WithPrefix(string prefix) => Ids.Where(id => id.StartsWith(prefix, StringComparison.Ordinal));
}
