namespace LastEpoch_Hud.Tests.Support;

/// <summary>Paths to the repo, the built mod and the installed game.</summary>
internal static class GameEnvironment
{
    private const string DefaultGameDir = @"C:\Program Files (x86)\Steam\steamapps\common\Last Epoch";

    public static string RepoRoot { get; } = FindRepoRoot();

    public static string GameDir => Environment.GetEnvironmentVariable("LAST_EPOCH_PATH") ?? DefaultGameDir;

    public static string Il2CppDir => Path.Combine(GameDir, "MelonLoader", "Il2CppAssemblies");

    public static string ModProjectDir => Path.Combine(RepoRoot, "LastEpoch_Hud");

    public static string ModDll => Path.Combine(RepoRoot, "Build", "Keyboard", "net6.0", "LastEpoch_Hud.dll");

    public static void SkipWithoutGame() =>
        Assert.SkipUnless(Directory.Exists(Il2CppDir), $"No Il2Cpp assemblies at {Il2CppDir}; set LAST_EPOCH_PATH");

    public static void SkipWithoutModBuild() =>
        Assert.SkipUnless(File.Exists(ModDll), $"No mod build at {ModDll}; build the Keyboard config first");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "LastEpoch_Hud.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("LastEpoch_Hud.sln not found above the test output");
    }
}
