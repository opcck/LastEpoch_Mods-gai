using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>One [HarmonyPatch(typeof(T), "Method", argTypes?)] found in the built mod.</summary>
internal sealed record PatchDeclaration(string PatchClass, TypeReference Target, string Method, IReadOnlyList<string> ArgumentTypes)
{
    public string Id => $"patch:{PatchClass}";
}

/// <summary>Reads Harmony patch attributes from the mod DLL without loading it.</summary>
internal static class PatchScanner
{
    private const string HarmonyPatchAttribute = "HarmonyLib.HarmonyPatch";

    public static IReadOnlyList<PatchDeclaration> Scan(ModuleDefinition module) =>
        module.GetTypes()
            .SelectMany(type => type.CustomAttributes
                .Where(attribute => attribute.AttributeType.FullName == HarmonyPatchAttribute)
                .Select(attribute => ToDeclaration(type, attribute)))
            .OfType<PatchDeclaration>()
            .ToList();

    // Only the (Type, string) and (Type, string, Type[]) forms carry a full target; others return null.
    private static PatchDeclaration ToDeclaration(TypeDefinition patchClass, CustomAttribute attribute)
    {
        var args = attribute.ConstructorArguments;
        if (args.Count < 2 || args[0].Value is not TypeReference target || args[1].Value is not string method)
            return null;

        var argumentTypes = args.Count > 2 && args[2].Value is CustomAttributeArgument[] types
            ? types.Select(t => ((TypeReference)t.Value).FullName).ToList()
            : null;
        return new PatchDeclaration(patchClass.FullName.Replace('/', '+'), target, method, argumentTypes);
    }
}
