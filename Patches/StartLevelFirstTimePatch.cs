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
            Plugin.PhysicsHistory.Add(new KeyValuePair<int, Vector3[]>(0, new Vector3[0]));
            Plugin.IsPlaying = true;
            Plugin.FrameId = 0;
        }
    }
}
