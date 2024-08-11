using System.Reflection;
using UnityEngine;

namespace Test
{
    public class HarmonyInit : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            // Reduce extra logging stuff
            //Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            //Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}