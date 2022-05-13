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
            if (Plugin.IsPlaying)
            {
                // Save
            }

            Plugin.PhysicsHistory.Add(new KeyValuePair<float, int[]>(Time.time, new int[0]));
            Plugin.IsPlaying = true;
        }
    }
}
