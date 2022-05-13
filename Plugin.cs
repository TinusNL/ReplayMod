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
        public static Harmony Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource LogSource = new ManualLogSource(PluginInfo.PLUGIN_NAME);
        private static string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeepkistReplays");

        public static bool IsPlaying = false;
        public static PlayerManager GameManager;
        public static GameObject SoapBox;
        public static List<KeyValuePair<float, Vector3[]>> PhysicsHistory = new List<KeyValuePair<float, Vector3[]>>();

        public static bool IsReplay = false;
        public static int ReplayId = 0;
        public static List<string> CachedReplays = new List<string>();
        
        private static string VectorToString(Vector3 StartVector)
        {
            return $"{StartVector.x.ToString().Replace(",", ".")},{StartVector.y.ToString().Replace(",", ".")},{StartVector.z.ToString().Replace(",", ".")}";
        }
        
        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(LogSource);

            Harmony.PatchAll();

            LogSource.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void OnDestroy()
        {
            Harmony.UnpatchAll();
        }

        private void FixedUpdate()
        {
            if (IsPlaying)
            {
                if (!SoapBox || !SoapBox.scene.IsValid())
                {
                    SoapBox = GameObject.Find("Soapbox(Clone)");
                } 
                else
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        IsReplay = !IsReplay;
                    }

                    if (IsReplay)
                    {

                    }
                    else
                    {
                        Rigidbody RigidBody = SoapBox.GetComponent<Rigidbody>();
                        Vector3 Position = SoapBox.transform.position;
                        Vector3 Rotation = SoapBox.transform.rotation.eulerAngles;
                        Vector3 Velocity = RigidBody.velocity;

                        PhysicsHistory.Add(new KeyValuePair<float, Vector3[]>(Time.time, new Vector3[] { Position, Rotation, Velocity }));
                    }
                }
            }
        }

        public static void SaveReplay()
        {
            if (!IsReplay)
            {
                if (!GameManager || !GameManager.gameObject.scene.IsValid())
                {
                    GameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
                }

                if (!Directory.Exists(DocumentsPath))
                {
                    Directory.CreateDirectory(DocumentsPath);
                }

                string LevelPath = Path.Combine(DocumentsPath, $"{GameManager.currentZeepLevel.name} - {GameManager.currentZeepLevel.author}");
                if (!Directory.Exists(LevelPath))
                {
                    Directory.CreateDirectory(LevelPath);
                }

                string FileName = $"{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.zeepreplay";
                string FilePath = Path.Combine(LevelPath, FileName);

                using (StreamWriter ReplayFile = new StreamWriter(FilePath))
                {
                    ReplayFile.WriteLine("Zeepkist ReplayMod By Tinus#4202");

                    float FirstTime = 0f;

                    foreach (KeyValuePair<float, Vector3[]> KeySet in PhysicsHistory)
                    {
                        if (FirstTime == 0f)
                        {
                            FirstTime = KeySet.Key;
                        } 
                        else
                        {
                            ReplayFile.WriteLine($"{KeySet.Key - FirstTime}:{VectorToString(KeySet.Value[0])}:{VectorToString(KeySet.Value[1])}:{VectorToString(KeySet.Value[2])}");
                        }
                    }

                    ReplayFile.Close();
                }
            }

            PhysicsHistory.Clear();
        }
    }
}
