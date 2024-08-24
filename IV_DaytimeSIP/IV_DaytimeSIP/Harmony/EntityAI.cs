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

        [HarmonyPostfix]
        public static void Postfix(EntityAlive __instance)
        {
            if (__instance is EntityPlayer || !__instance.IsSleeper)
            {
                return;
            }

            bool isUnderSky = Physics.Raycast(__instance.transform.position, Vector3.up, 256f, 2130706431);

            // If this is the first update, add the entity spawn point to a list if it's indoors.
            if (!hasChecked)
            {
                hasChecked = true;
                Vector3 spawnPosition = __instance.SleeperSpawnPosition;
                Log.Out($"Initial Check: Spawn Position: {spawnPosition}, Entity: {__instance.EntityName}");

                if (!isUnderSky)
                {
                    Log.Out($"Entity {__instance.EntityName} is indoors at spawn position {spawnPosition}. Adding to indoors list.");
                    sleeperSpawnPositions.Add(spawnPosition);
                    returnPosition = spawnPosition;
                }
                else
                {
                    Log.Out($"Entity {__instance.EntityName} is NOT indoors at spawn position {spawnPosition}.");
                }

                return;
            }

            // NIGHT: Wake up the entity if it is sleeping.
            if (!__instance.world.IsDaytime() && __instance.IsSleeping)
            {
                Log.Out($"Entity {__instance.EntityName} is sleeping at night. Waking up.");
                __instance.ConditionalTriggerSleeperWakeUp();
                return;
            }

            // DAY: Return sleeper to the spawn position if it has one and the entity is not indoors.
            if (__instance.world.IsDaytime() && !__instance.IsSleeping
               && __instance.SleeperSpawnPosition != null
               && !__instance.SleeperSpawnPosition.Equals(Vector3.zero))
            {
                if (isUnderSky)
                {
                    if (!returnPosition.Equals(Vector3.zero))
                    {
                        Log.Out($"Returning Entity {__instance.EntityName} to spawn position {returnPosition}");
                        __instance.SetInvestigatePosition(returnPosition, 1000);
                        return;
                    }

                    if (sleeperSpawnPositions.Count > 0)
                    {
                        returnPosition = sleeperSpawnPositions[rnd.Next(sleeperSpawnPositions.Count)];
                        Log.Out($"Entity {__instance.EntityName} found new spawn position {returnPosition}");
                        return;
                    }
                }
                else
                {
                    Log.Out($"Entity {__instance.EntityName} is indoors during the day. Resuming sleeper pose.");
                    __instance.ResumeSleeperPose();
                }
            }
        }
    }
}
