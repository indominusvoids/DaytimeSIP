using HarmonyLib;
using System.Reflection;
using UnityEngine;

#nullable enable
namespace EntityAI
{
    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.Update))]

    public class EntityAIPatch
    {

        // SleeperSpawnPosition
        // ChaseReturnLocation

        [HarmonyPostfix]
        public static void Postfix(EntityAlive __instance)
        {
            Log.Out($"EntityAlive::Update - entity: {__instance}");

            if (__instance is EntityPlayer)
            {
                Log.Out("Entity is a player.");
                return;
            }

            Log.Out("Entity is a zombie.");

            if (!__instance.IsSleeper)
            {
                Log.Out("Entity is NOT a sleeper.");
                return;
            }

            Log.Out("Entity is a sleeper.");


            if (__instance.IsSleeping)
            {
                 if (GameManager.Instance.World.IsDaytime())
                 {
                     return; // Exit early, so no further actions are taken.
                 }
                 else
                 {
                     __instance.ConditionalTriggerSleeperWakeUp(); // Wake up the zombie
                 }
             }
             else
            {
                if (GameManager.Instance.World.IsDaytime())
                {
                    // If the zombie is not sleeping and it's daytime, set its investigation position.
                    if (__instance.SleeperSpawnPosition != null && !__instance.SleeperSpawnPosition.Equals(Vector3.zero))
                    {
                        __instance.SetInvestigatePosition(__instance.SleeperSpawnPosition, 1000);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.setHomeArea))]
    public class Entity_setHomeArea_Patch
    {
        public static bool Prefix(EntityAlive __instance, Vector3i _pos, int _maxDistance)
        {
            Log.Out($"EntityAlive::setHomeArea Prefix - {__instance}, pos: {_pos}, _maxDistance: {_maxDistance}");
            return true;
        }
    }
}