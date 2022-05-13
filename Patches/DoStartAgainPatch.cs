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
                Plugin.SaveReplay();
            }

            Plugin.PhysicsHistory.Add(new KeyValuePair<float, Vector3[]>(Time.time, new Vector3[0]));
            Plugin.IsPlaying = true;
        }
    }
}
