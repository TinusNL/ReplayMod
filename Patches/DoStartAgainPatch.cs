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

            Plugin.IsPlaying = true;
            Plugin.FrameId = 0;
        }
    }
}
