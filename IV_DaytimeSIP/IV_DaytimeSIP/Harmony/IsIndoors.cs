using HarmonyLib;

namespace EntityAI
{
    // This static class contains a Harmony patch for the IsIndoors class
    [HarmonyPatch(typeof(IsIndoors))]
    public static class IsIndoorsPatch
    {
        // Patch the IsValid method in IsIndoors class
        [HarmonyPatch(nameof(IsIndoors.IsValid))]
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, MinEventParams _params, IsIndoors __instance)
        {
            // Access the AmountEnclosed stat directly from the target entity's stats.
            float amountEnclosed = _params.Self.Stats.AmountEnclosed;

            // Print the current AmountEnclosed value for debugging
            Log.Out($"[Patched] IsIndoors::IsValid - AmountEnclosed: {amountEnclosed}");

            // Custom logic for determining if the entity is indoors.
            if (!__instance.invert)
            {
                __result = amountEnclosed > 0f;
                Log.Out($"[Patched] IsIndoors::IsValid - Entity is indoors: {__result}");
            }
            else
            {
                __result = amountEnclosed <= 0f;
                Log.Out($"[Patched] IsIndoors::IsValid - Entity is indoors: {__result}");
            }

            // Print final result for debugging
            Log.Out($"[Patched] IsIndoors::IsValid - Final determination: {__result}");
        }
    }
}