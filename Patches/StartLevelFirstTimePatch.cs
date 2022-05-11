using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace TinusDLL.Zeepkist.ReplayMod.Patches
{
    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.StartLevelFirstTime))]
    public class StartLevelFirstTimePatch
    {
        internal static void Postfix()
        {
            Plugin.LogSource.LogInfo("StartLevelFirstTimePatch - PostFix");
            Plugin.KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "StartReplay"));
            Plugin.IsPlaying = true;
        }
    }
}
