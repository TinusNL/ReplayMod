using HarmonyLib;
using UnityEngine;

namespace TinusDLL.Zeepkist.ReplayMod.Patches
{
    [HarmonyPatch(typeof(DamageCharacterScript), nameof(DamageCharacterScript.IsDead))]
    public class IsDeadPatch
    {
        internal static void Postfix(ref bool __result)
        {
            if (__result && Plugin.IsPlaying)
            {
                Plugin.IsPlaying = false;
            }
        }
    }
}
