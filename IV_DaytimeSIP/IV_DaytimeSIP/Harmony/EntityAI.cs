using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace EntityAI
{
    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.Update))]
    public class EntityAIPatch
    {
        private static bool hasChecked = false;
        private static System.Random rnd = new System.Random();
        private static Vector3 returnPosition = Vector3.zero;
        private static List<Vector3> sleeperSpawnPositions = new List<Vector3>();
        private static IsUnderSky isUnderSkyCheck = new IsUnderSky();

        [HarmonyPostfix]
        public static void Postfix(EntityAlive __instance)
        {
            Log.Out("[DEBUG] Postfix method called");

            if (__instance is EntityPlayer)
            {
                Log.Out("[DEBUG] Skipping player entity");
                return;
            }

            if (!__instance.IsSleeper)
            {
                Log.Out("[DEBUG] Entity is not a sleeper, skipping");
                return;
            }

            Log.Out($"[DEBUG] Processing entity: {__instance.EntityName}");

            bool isUnderSky = isUnderSkyCheck.IsValid(new MinEventParams { Self = __instance });
            Log.Out($"[DEBUG] {__instance.EntityName} is under sky: {isUnderSky}");

            // Assign the spawn position only during the first check
            if (!hasChecked)
            {
                Log.Out($"[DEBUG] First check for entity: {__instance.EntityName}");
                hasChecked = true;
                Vector3 spawnPosition = __instance.SleeperSpawnPosition;
                Log.Out($"[DEBUG] Spawn position for {__instance.EntityName}: {spawnPosition}");

                if (!isUnderSky)
                {
                    Log.Out($"[DEBUG] {__instance.EntityName} is indoors at spawn position. Adding to indoors list.");
                    sleeperSpawnPositions.Add(spawnPosition);
                    returnPosition = spawnPosition;
                }
                else
                {
                    Vector3? indoorPosition = FindNearbyIndoorPosition(__instance);

                    if (indoorPosition.HasValue)
                    {
                        Log.Out($"[DEBUG] Found indoor position for {__instance.EntityName}: {indoorPosition.Value}. Adding to indoors list.");
                        sleeperSpawnPositions.Add(indoorPosition.Value);
                        returnPosition = indoorPosition.Value;
                    }
                    else
                    {
                        Log.Out($"[DEBUG] No indoor position found for {__instance.EntityName}. Using spawn position: {spawnPosition}");
                        returnPosition = spawnPosition;
                    }
                }
            }

            // NIGHT: Wake up the entity if it is sleeping.
            if (!__instance.world.IsDaytime() && __instance.IsSleeping)
            {
                Log.Out($"[DEBUG] {__instance.EntityName} is sleeping at night. Waking up.");
                __instance.ConditionalTriggerSleeperWakeUp();
                return;
            }

            // DAY: Check if the entity is close to the spawn position before resuming sleep
            if (__instance.world.IsDaytime() && !__instance.IsSleeping)
            {
                float distanceToSpawn = Vector3.Distance(__instance.position, returnPosition);
                Log.Out($"[DEBUG] {__instance.EntityName} distance to spawn position: {distanceToSpawn}");

                if (distanceToSpawn <= 5.0f && !isUnderSky)
                {
                    Log.Out($"[DEBUG] {__instance.EntityName} is close to spawn position and indoors during the day. Resuming sleeper pose.");
                    hasChecked = false; // Allow for rechecking later
                    __instance.ResumeSleeperPose();
                }
                else if (isUnderSky)
                {
                    Log.Out($"[DEBUG] {__instance.EntityName} is outdoors during the day. Searching for an indoor position.");
                    Vector3? indoorPosition = FindNearbyIndoorPosition(__instance);

                    if (indoorPosition.HasValue)
                    {
                        Log.Out($"[DEBUG] Found indoor position for {__instance.EntityName}: {indoorPosition.Value}. Moving to it.");
                        __instance.ConditionalTriggerSleeperWakeUp();
                        __instance.SetInvestigatePosition(indoorPosition.Value, 1000);
                        ForceInvestigate(__instance);
                    }
                    else
                    {
                        Log.Out($"[DEBUG] No indoor position found for {__instance.EntityName}. Resuming original behavior.");
                        if (sleeperSpawnPositions.Count > 0)
                        {
                            returnPosition = sleeperSpawnPositions[rnd.Next(sleeperSpawnPositions.Count)];
                            Log.Out($"[DEBUG] {__instance.EntityName} found new spawn position {returnPosition}");
                            __instance.SetInvestigatePosition(returnPosition, 1000);
                            ForceInvestigate(__instance);
                        }
                    }
                }
            }

            // Ensure the sleeperSpawnPositions list is populated
            if (sleeperSpawnPositions.Count == 0)
            {
                Log.Warning($"[DEBUG] Warning: sleeperSpawnPositions list is empty for {__instance.EntityName}");
            }
        }

        // Helper method to find a nearby indoor position
        private static Vector3? FindNearbyIndoorPosition(EntityAlive entity)
        {
            Log.Out($"[DEBUG] Searching for nearby indoor position for {entity.EntityName}");
            foreach (Vector3 pos in sleeperSpawnPositions)
            {
                if (!isUnderSkyCheck.IsValid(new MinEventParams { Self = entity, Position = pos }))
                {
                    Log.Out($"[DEBUG] Found indoor position at {pos}");
                    return pos;
                }
            }

            Log.Out($"[DEBUG] No indoor position found for {entity.EntityName}");
            return null; // No indoor position found
        }

        // Ensure the entity AI investigates the position
        private static void ForceInvestigate(EntityAlive entity)
        {
            Log.Out($"[DEBUG] Forcing {entity.EntityName} to investigate position {entity.InvestigatePosition}");
            entity.SetInvestigatePosition(entity.InvestigatePosition, 1000);
            entity.navigator.SetPath(null, 0); // Forces the AI to recalculate its path
        }
    }
}
