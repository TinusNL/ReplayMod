using HarmonyLib;

namespace TinusDLL.Zeepkist.ReplayMod.Patches
{
    [HarmonyPatch(typeof(DamageCharacterScript), nameof(DamageCharacterScript.IsDead))]
    public class IsDeadPatch
    {
        internal static void Postfix(DamageCharacterScript __instance, ref bool __result)
        {
            if (Plugin.NeedsKill)
            {
                Plugin.NeedsKill = false;
                __instance.KillCharacter(true, PluginInfo.PLUGIN_GUID);
            }

            if (__result && Plugin.IsPlaying)
            {
                Plugin.LogSource.LogInfo("IsDeadPatch - PostFix");
                Plugin.IsPlaying = false;
                Plugin.SaveHistory();
            }
        }
    }
}
