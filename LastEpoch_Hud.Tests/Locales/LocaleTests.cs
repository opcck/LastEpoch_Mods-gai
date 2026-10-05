using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Locales;

/// <summary>Menu text: every key in base.json is translated, and every locale file ships.</summary>
public sealed partial class LocaleTests
{
    private static readonly string LocalesDir = Path.Combine(GameEnvironment.ModProjectDir, "LastEpoch_Hud", "Locales");
    private static readonly string[] TranslatedLanguages = ["fr", "zh"];
    private static readonly string[] Languages = ["en", .. TranslatedLanguages];
    private static readonly HashSet<string> KeepEnglish = ListFile.Load("Locales/KeepEnglish.txt");

    public static TheoryData<string> LanguageData => new(Languages);
    public static TheoryData<string> TranslatedLanguageData => new(TranslatedLanguages);

    [Theory]
    [MemberData(nameof(LanguageData))]
    public void Language_TranslatesEveryBaseKey(string language)
    {
        var translations = Read(language);
        var missing = Read("base").Keys.Where(key => string.IsNullOrWhiteSpace(translations.GetValueOrDefault(key))).ToList();

        Assert.True(missing.Count == 0, $"{language}.json misses: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(TranslatedLanguageData))]
    public void Language_HasNoEnglishLeftovers(string language)
    {
        var english = Read("en");
        var leftovers = Read(language)
            .Where(pair => SameAsEnglish(english.GetValueOrDefault(pair.Key), pair.Value))
            .Select(pair => pair.Key)
            .Where(key => !KeepEnglish.Contains($"{language}:{key}"))
            .ToList();

        Assert.True(leftovers.Count == 0, $"{language}.json still English ({leftovers.Count}): {string.Join(", ", leftovers)}");
    }

    [Fact]
    public void KeepEnglish_EntriesAreStillEnglishAndExist()
    {
        var baseKeys = Read("base").Keys.ToHashSet();
        var english = Read("en");
        var translations = TranslatedLanguages.ToDictionary(language => language, Read);
        var broken = KeepEnglish.Where(entry => !IsValidKeepEnglish(entry, baseKeys, english, translations)).ToList();

        Assert.True(broken.Count == 0, "Malformed, unknown or stale KeepEnglish.txt entries (<lang>:<key>, lang is fr or zh); fix or remove them:\n" + string.Join("\n", broken));
    }

    [Fact]
    public void SettingLabels_HaveBaseKeys()
    {
        var source = File.ReadAllText(Path.Combine(GameEnvironment.ModProjectDir, "Scripts", "ModUI", "ModSettings.cs"));
        var keys = Read("base").Keys.ToHashSet();
        var missing = LabelPattern().Matches(source).Select(m => m.Groups[1].Value).Where(label => !keys.Contains(label)).Distinct().ToList();

        Assert.True(missing.Count == 0, $"base.json misses labels: {string.Join(", ", missing)}");
    }

    [Fact]
    public void LocaleFiles_AreCopiedToBuildOutput()
    {
        var copied = CopiedFiles();
        var missing = Directory.GetFiles(LocalesDir, "*.json")
            .Select(path => $@"LastEpoch_Hud\Locales\{Path.GetFileName(path)}")
            .Where(file => !copied.Contains(file))
            .Where(file => !KnownIssues.Contains($"locale-copy:{Path.GetFileName(file)}"))
            .ToList();

        Assert.True(missing.Count == 0, $"Not copied by LastEpoch_Hud.csproj: {string.Join(", ", missing)}");
    }

    [Fact]
    public void KnownUncopiedLocales_AreStillUncopied()
    {
        var copied = CopiedFiles();
        var fixedOnes = KnownIssues.WithPrefix("locale-copy:")
            .Where(id => copied.Contains($@"LastEpoch_Hud\Locales\{id["locale-copy:".Length..]}"))
            .ToList();

        Assert.True(fixedOnes.Count == 0, "Fixed; remove from KnownIssues.txt:\n" + string.Join("\n", fixedOnes));
    }

    private static bool SameAsEnglish(string english, string translated) => string.Equals(english, translated, StringComparison.OrdinalIgnoreCase);

    private static bool IsValidKeepEnglish(string entry, HashSet<string> baseKeys, Dictionary<string, string> english, Dictionary<string, Dictionary<string, string>> translations)
    {
        var parts = entry.Split(':', 2);
        if (parts.Length != 2 || !translations.TryGetValue(parts[0], out var language))
            return false;

        return baseKeys.Contains(parts[1]) && SameAsEnglish(english.GetValueOrDefault(parts[1]), language.GetValueOrDefault(parts[1]));
    }

    private static Dictionary<string, string> Read(string language) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(LocalesDir, $"{language}.json")));

    private static HashSet<string> CopiedFiles() =>
        XDocument.Load(Path.Combine(GameEnvironment.ModProjectDir, "LastEpoch_Hud.csproj"))
            .Descendants()
            .Where(e => e.Name.LocalName == "None" && e.Elements().Any(c => c.Name.LocalName == "CopyToOutputDirectory"))
            .Select(e => (string)e.Attribute("Update") ?? (string)e.Attribute("Include") ?? "")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    [GeneratedRegex("label:\\s*\"([^\"]+)\"")]
    private static partial Regex LabelPattern();
}
