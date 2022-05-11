using HarmonyLib;
using UnityEngine;

namespace TinusDLL.Zeepkist.ReplayMod.Patches
{
    [HarmonyPatch(typeof(ReadyToReset), nameof(ReadyToReset.HeyYouHitATrigger))]
    public class HeyYouHitATriggerPatch
    {
        internal static void Prefix(ref bool finishOrCheckpoint)
        {
            if (Plugin.IsReview) {
                GameObject SoapBox = GameObject.Find("Soapbox(Clone)");

                if (SoapBox)
                {
                    ReadyToReset ResetManager = SoapBox.GetComponent<ReadyToReset>();
                    GameMaster MasterManager = ResetManager.GetMaster();
                    
                    if (finishOrCheckpoint)
                    {
                        MasterManager.countFinishCrossing = false;
                        Plugin.NeedsKill = true;
                    } 
                    else
                    {
                        MasterManager.countFinishCrossing = true;
                    }
                }
            }
        }
    }
}
