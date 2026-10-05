using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>
/// Mirrors how MelonLoader's Harmony finds a patch target: AccessTools.DeclaredMethod
/// on the target type, so inherited methods do not count.
/// </summary>
internal static class PatchTargetResolver
{
    /// <returns>null when the target resolves to exactly one method, else why not.</returns>
    public static string Problem(PatchDeclaration patch)
    {
        var type = TryResolve(patch.Target);
        if (type == null)
            return $"target type {patch.Target.FullName} not found";

        var candidates = type.Methods.Where(m => m.Name == patch.Method).ToList();
        if (candidates.Count == 0)
            return $"{type.FullName} declares no method {patch.Method}";
        if (patch.ArgumentTypes == null)
            return candidates.Count == 1 ? null : $"{type.FullName}.{patch.Method} has {candidates.Count} overloads; give argument types";

        return candidates.Any(m => ParametersMatch(m, patch.ArgumentTypes))
            ? null
            : $"{type.FullName}.{patch.Method}({string.Join(", ", patch.ArgumentTypes)}) not found; overloads: {Describe(candidates)}";
    }

    private static TypeDefinition TryResolve(TypeReference reference)
    {
        try { return reference.Resolve(); }
        catch (AssemblyResolutionException) { return null; }
    }

    private static bool ParametersMatch(MethodDefinition method, IReadOnlyList<string> argumentTypes) =>
        method.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(argumentTypes);

    private static string Describe(IEnumerable<MethodDefinition> methods) =>
        string.Join(" | ", methods.Select(m => $"({string.Join(", ", m.Parameters.Select(p => p.ParameterType.FullName))})"));
}
