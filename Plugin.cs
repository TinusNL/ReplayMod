using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace TinusDLL.Zeepkist.ReplayMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Zeepkist.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeepkistReplays");

        public static ManualLogSource LogSource = new ManualLogSource(PluginInfo.PLUGIN_NAME);
        public static bool IsPlaying = false;
        public static List<KeyValuePair<float, string>> KeyHistory = new List<KeyValuePair<float, string>>();
        
        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(LogSource);

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            LogSource.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void FixedUpdate()
        {
            if (IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "UpArrow"));
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "DownArrow"));
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "LeftArrow"));
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "RightArrow"));
                }
            }
        }

        public static void SaveHistory()
        {
            PlayerManager GameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
            LogSource.LogInfo($"{GameManager.currentZeepLevel.name} - {GameManager.currentZeepLevel.author}");

            string TimeFormat = $"{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
            string FileName = $"{GameManager.currentZeepLevel.name}_{GameManager.currentZeepLevel.author}_{TimeFormat}.zeepreplay";
            string FilePath = Path.Combine(DocumentsPath, FileName);

            if (!Directory.Exists(DocumentsPath))
            {
                Directory.CreateDirectory(DocumentsPath);
            }

            using (StreamWriter ReplayFile = new StreamWriter(FilePath))
            {
                float FirstTime = 0f;

                ReplayFile.WriteLine("Zeepkist ReplayMod By Tinus#4202");

                foreach (KeyValuePair<float, string> KeySet in KeyHistory)
                {
                    if (FirstTime == 0f)
                    {
                        FirstTime = KeySet.Key;
                    } else
                    {
                        ReplayFile.WriteLine($"{KeySet.Key - FirstTime}:{KeySet.Value}");
                    }
                }

                ReplayFile.Close();
            }

            KeyHistory = new List<KeyValuePair<float, string>>();
        }
    }
}
