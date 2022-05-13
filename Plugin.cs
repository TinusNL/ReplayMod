using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace TinusDLL.Zeepkist.ReplayMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Zeepkist.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource = new ManualLogSource(PluginInfo.PLUGIN_NAME);
        private static string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeepkistReplays");

        public static bool IsPlaying = false;
        public static List<KeyValuePair<float, int[]>> PhysicsHistory = new List<KeyValuePair<float, int[]>>();

        public static bool IsReplay = false;

        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(LogSource);

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            LogSource.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void FixedUpdate()
        {
        
        }
    }
}
