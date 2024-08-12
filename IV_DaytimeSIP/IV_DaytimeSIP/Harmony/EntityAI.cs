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

            // DAY: Return sleeper to the spawn position if it has one.
            if (__instance.world.IsDaytime()
                && __instance.SleeperSpawnPosition != null
                && !__instance.SleeperSpawnPosition.Equals(Vector3.zero))
            {
                __instance.SetInvestigatePosition(__instance.SleeperSpawnPosition, 1000);
                return;
            }

            // NIGHT: Wake up the zombie if it is sleeping.
            if (!__instance.world.IsDaytime() && __instance.IsSleeping)
            {
                __instance.ConditionalTriggerSleeperWakeUp();
                return;
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