using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace TinusDLL.Zeepkist.ReplayMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Zeepkist.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Harmony Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource LogSource = new ManualLogSource(PluginInfo.PLUGIN_NAME);
        private static string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeepkistReplays");
        private static NumberFormatInfo FormatInfo = new NumberFormatInfo();

        public static bool IsPlaying = false;
        public static PlayerManager GameManager;
        public static ReadyToReset ResetManager;
        public static GameObject SoapBox;
        public static Rigidbody RigidBodyManager;
        public static GetInput InputManager;
        public static List<KeyValuePair<int, Vector3[]>> PhysicsHistory = new List<KeyValuePair<int, Vector3[]>>();

        public static bool IsReplay = false;
        public static bool IsRunning = false;
        public static int FrameId = 0;
        public static List<KeyValuePair<int, Vector3[]>> FrameData = new List<KeyValuePair<int, Vector3[]>>();
        public static int ReplayId = 0;
        public static List<string> CachedReplays = new List<string>();
        
        private static string VectorToString(Vector3 StartVector)
        {
            return $"{Math.Round(StartVector.x, 4).ToString().Replace(",", ".")},{Math.Round(StartVector.y, 4).ToString().Replace(",", ".")},{Math.Round(StartVector.z, 4).ToString().Replace(",", ".")}";
        }

        private static Vector3 StringToVector(string StartString)
        {
            string[] SplittedString = StartString.Split(',');

            float.TryParse(SplittedString[0], NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, FormatInfo, out float X);
            float.TryParse(SplittedString[1], NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, FormatInfo, out float Y);
            float.TryParse(SplittedString[2], NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, FormatInfo, out float Z);

            return new Vector3(X, Y, Z);
        }

        private static void RespawnCharacter()
        {
            ResetManager.GetMaster().RestartLevel();
        }
        
        private void Awake()
        {
            Harmony.PatchAll();

            BepInEx.Logging.Logger.Sources.Add(LogSource);

            FormatInfo.NumberDecimalSeparator = ".";
            FormatInfo.NegativeSign = "-";

            LogSource.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void FixedUpdate()
        {
            if (IsPlaying)
            {
                FrameId += 1;

                if (!SoapBox || !SoapBox.scene.IsValid())
                {
                    SoapBox = GameObject.Find("Soapbox(Clone)");
                } 
                else
                {
                    if (!ResetManager || !ResetManager.gameObject.scene.IsValid())
                    {
                        ResetManager = SoapBox.GetComponent<ReadyToReset>();
                    } 
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.P))
                        {
                            IsReplay = !IsReplay;

                            if (IsReplay)
                            {
                                CachedReplays.Clear();

                                if (!GameManager || !GameManager.gameObject.scene.IsValid())
                                {
                                    GameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
                                }

                                DirectoryInfo ReplaysFolder = new DirectoryInfo(Path.Combine(DocumentsPath, $"{GameManager.currentZeepLevel.name} - {GameManager.currentZeepLevel.author}"));
                                FileInfo[] ReplayFiles = ReplaysFolder.GetFiles("*.zeepreplay");

                                if (ReplayFiles.Length <= 0)
                                {
                                    IsReplay = false;
                                }
                                else
                                {
                                    foreach (FileInfo ReplayFile in ReplayFiles)
                                    {
                                        CachedReplays.Add(ReplayFile.Name);
                                    }

                                    ReplayId = CachedReplays.Count - 1;

                                    LoadReplay();
                                }
                            }
                            else
                            {
                                if (!InputManager || !InputManager.gameObject.scene.IsValid())
                                {
                                    InputManager = SoapBox.GetComponent<GetInput>();
                                }

                                InputManager.allowControl = true;
                                RespawnCharacter();
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.LeftBracket))
                        {
                            if (ReplayId - 1 >= 0)
                            {
                                ReplayId -= 1;
                            }
                            else
                            {
                                ReplayId = CachedReplays.Count - 1;
                            }

                            LoadReplay();
                        }
                        else if (Input.GetKeyDown(KeyCode.RightBracket))
                        {
                            if (ReplayId + 1 < CachedReplays.Count)
                            {
                                ReplayId += 1;
                            }
                            else
                            {
                                ReplayId = 0;
                            }

                            LoadReplay();
                        }

                        if (IsReplay)
                        {
                            if (!InputManager || !InputManager.gameObject.scene.IsValid())
                            {
                                InputManager = SoapBox.GetComponent<GetInput>();
                            }

                            ResetManager.screenPointer.checkpoints.SetText(CachedReplays[ReplayId]);
                            ResetManager.screenPointer.checkpoints.color = Color.red;
                            InputManager.allowControl = false;

                            if (IsRunning)
                            {
                                if (!RigidBodyManager || !RigidBodyManager.gameObject.scene.IsValid())
                                {
                                    RigidBodyManager = SoapBox.GetComponent<Rigidbody>();
                                }

                                try
                                {
                                    KeyValuePair<int, Vector3[]> CurrentFrameData = FrameData.First(Pair => Pair.Key == FrameId);

                                    SoapBox.transform.position = CurrentFrameData.Value[0];
                                    SoapBox.transform.eulerAngles = CurrentFrameData.Value[1];
                                    RigidBodyManager.velocity = CurrentFrameData.Value[2];
                                }
                                catch (Exception)
                                {
                                    LogSource.LogWarning("Errored on replay! Possibly done?");
                                    RespawnCharacter();
                                }
                            }
                        }
                        else
                        {
                            if (!RigidBodyManager || !RigidBodyManager.gameObject.scene.IsValid())
                            {
                                RigidBodyManager = SoapBox.GetComponent<Rigidbody>();
                            }

                            Vector3 Position = SoapBox.transform.position;
                            Vector3 Rotation = SoapBox.transform.eulerAngles;
                            Vector3 Velocity = RigidBodyManager.velocity;

                            PhysicsHistory.Add(new KeyValuePair<int, Vector3[]>(FrameId, new Vector3[] { Position, Rotation, Velocity }));
                        }
                    }
                }
            }
        }

        public static void SaveReplay()
        {
            if (!IsReplay && PhysicsHistory.Count > 200)
            {
                LogSource.LogInfo("Saving Replay!");

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

                    foreach (KeyValuePair<int, Vector3[]> KeySet in PhysicsHistory)
                    {
                        ReplayFile.WriteLine($"{KeySet.Key}:{VectorToString(KeySet.Value[0])}:{VectorToString(KeySet.Value[1])}:{VectorToString(KeySet.Value[2])}");
                    }

                    ReplayFile.Close();
                }
            }

            PhysicsHistory.Clear();
        }

        public static void LoadReplay()
        {
            if (IsReplay)
            {
                LogSource.LogInfo("Loading Replay!");

                FrameData.Clear();

                if (!GameManager || !GameManager.gameObject.scene.IsValid())
                {
                    GameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
                }

                string[] AllLines = File.ReadAllLines(Path.Combine(DocumentsPath, $"{GameManager.currentZeepLevel.name} - {GameManager.currentZeepLevel.author}", CachedReplays[ReplayId]));

                for (int Index = 0; Index < AllLines.Length; Index++)
                {
                    string[] InputSplit = AllLines[Index].Split(':');
                    int InputFrame;

                    if (int.TryParse(InputSplit[0], out InputFrame))
                    {
                        Vector3 Position = StringToVector(InputSplit[1]);
                        Vector3 Rotation = StringToVector(InputSplit[2]);
                        Vector3 Velocity = StringToVector(InputSplit[3]);

                        FrameData.Add(new KeyValuePair<int, Vector3[]>(InputFrame, new Vector3[] { Position, Rotation, Velocity }));
                    }
                }

                IsRunning = true;
                RespawnCharacter();
            }
        }
    }
}
