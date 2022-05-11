using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace TinusDLL.Zeepkist.ReplayMod.Patches
{
    [HarmonyPatch(typeof(SetupGame), nameof(SetupGame.DoStartAgain))]
    public class DoStartAgainPatch
    {
        internal static void Postfix()
        {
            Plugin.LogSource.LogInfo("DoStartAgainPatch - PostFix");
            
            if (Plugin.IsPlaying)
            {
                Plugin.SaveHistory();
            }

            Plugin.KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "StartReplay"));
            Plugin.IsPlaying = true;
        }
    }
}
