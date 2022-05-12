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
        public static List<KeyValuePair<float, string>> KeyHistory = new List<KeyValuePair<float, string>>();
        public static List<bool> PressedHistory = new List<bool>();

        public static bool IsReplay = false;
        public static int CurrentReplayId = 0;
        public static List<string> CachedReplays = new List<string>();
        public static bool NeedsKill = false;
        public static bool WaitingForRespawn = false;
        public static List<Byte> ActiveKeys = new List<Byte>();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public static void SimKeyDown(byte KeyByte)
        {
            keybd_event(KeyByte, 0, 0x0001 | 0, 0);
        }

        public static void SimKeyUp(byte KeyByte)
        {
            keybd_event(KeyByte, 0, 0x0001 | 0x0002, 0);
        }

        public static void SimKeyHold(byte KeyByte)
        {
            try
            {
                while (ActiveKeys.Contains(KeyByte))
                {
                    LogSource.LogInfo("KeyByte: " + KeyByte.ToString());

                    SimKeyDown(KeyByte);
                    Thread.Sleep(10);
                    SimKeyUp(KeyByte);

                    Thread.Sleep(37);
                }
            }
            catch (Exception Error)
            {
                LogSource.LogError(Error);
            }
        }

        private void Awake()
        {
            PressedHistory.Add(true);
            PressedHistory.Add(true);
            PressedHistory.Add(true);
            PressedHistory.Add(true);

            BepInEx.Logging.Logger.Sources.Add(LogSource);

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            LogSource.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void FixedUpdate()
        {
            if (IsPlaying)
            {
                if (IsReplay)
                {
                    if (WaitingForRespawn)
                    {
                        WaitingForRespawn = false;
                        StartReplay(true);
                    }

                    GameObject SoapBox = GameObject.Find("Soapbox(Clone)");
                    if (SoapBox)
                    {
                        ReadyToReset ResetManager = SoapBox.GetComponent<ReadyToReset>();
                        ResetManager.screenPointer.checkpoints.SetText(CachedReplays[CurrentReplayId]);
                        ResetManager.screenPointer.checkpoints.color = Color.red;

                        GameMaster MasterManager = ResetManager.GetMaster();
                        MasterManager.countFinishCrossing = false;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftBracket))
                    {
                        if (CurrentReplayId - 1 >= 0)
                        {
                            CurrentReplayId -= 1;
                        } 
                        else
                        {
                            CurrentReplayId = CachedReplays.Count - 1;
                        }

                        StartReplay(false);
                    } 
                    else if (Input.GetKeyDown(KeyCode.RightBracket))
                    {
                        if (CurrentReplayId + 1 < CachedReplays.Count)
                        {
                            CurrentReplayId += 1;
                        } 
                        else
                        {
                            CurrentReplayId = 0;
                        }

                        StartReplay(false);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && PressedHistory[0] == false)
                    {
                        PressedHistory[0] = true;
                        KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "38:True"));
                    }
                    else
                    {
                        if (PressedHistory[0] == true)
                        {
                            PressedHistory[0] = false;
                            KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "38:False"));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow) && PressedHistory[1] == false)
                    {
                        PressedHistory[1] = true;
                        KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "40:True"));
                    }
                    else
                    {
                        if (PressedHistory[1] == true)
                        {
                            PressedHistory[1] = false;
                            KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "40:False"));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow) && PressedHistory[2] == false)
                    {
                        PressedHistory[2] = true;
                        KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "37:True"));
                    }
                    else
                    {
                        if (PressedHistory[2] == true)
                        {
                            PressedHistory[2] = false;
                            KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "37:False"));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow) && PressedHistory[3] == false)
                    {
                        PressedHistory[3] = true;
                        KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "39:True"));
                    }
                    else
                    {
                        if (PressedHistory[3] == true)
                        {
                            PressedHistory[3] = false;
                            KeyHistory.Add(new KeyValuePair<float, string>(Time.time, "39:False"));
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    IsReplay = !IsReplay;

                    if (!IsReplay)
                    {
                        GameObject SoapBox = GameObject.Find("Soapbox(Clone)");
                        if (SoapBox)
                        {
                            ReadyToReset ResetManager = SoapBox.GetComponent<ReadyToReset>();
                            ResetManager.screenPointer.checkpoints.SetText("Killing...");
                            ResetManager.screenPointer.checkpoints.color = Color.red;

                            GameMaster MasterManager = ResetManager.GetMaster();
                            MasterManager.countFinishCrossing = true;
                        }

                        NeedsKill = true;
                    }
                    else
                    {
                        CurrentReplayId = 0;
                        CachedReplays.Clear();

                        DirectoryInfo ZeepkistReplaysFolder = new DirectoryInfo(DocumentsPath);
                        FileInfo[] ReplayFiles = ZeepkistReplaysFolder.GetFiles("*.zeepreplay");

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

                            StartReplay(false);
                        }
                    }
                }
            }
        }

        public static void RunReplay()
        {
            string[] AllInputs = File.ReadAllLines(Path.Combine(DocumentsPath, CachedReplays[CurrentReplayId]));
            float LastTime = 0;

            for (int Index = 0; Index < AllInputs.Length; Index++)
            {
                string[] InputSplit = AllInputs[Index].Split(':');
                float InputTime;

                if (float.TryParse(InputSplit[0], out InputTime))
                {
                    if (IsPlaying && IsReplay)
                    {
                        float WaitTime = InputTime - LastTime;
                        LogSource.LogInfo("Waiting: " + WaitTime);
                        Thread.Sleep((int)Math.Round(WaitTime * 1000));
                        LastTime = InputTime;

                        if (bool.Parse(InputSplit[2]))
                        {
                            LogSource.LogInfo("Pressing: " + InputSplit[1]);

                            byte KeyByte = byte.Parse(InputSplit[1]);
                            ActiveKeys.Add(KeyByte);
                            SimKeyDown(KeyByte);

                            Thread KeyThread = new Thread(new ThreadStart(() => { SimKeyHold(KeyByte); }));
                            KeyThread.Start();
                        }
                        else
                        {
                            LogSource.LogInfo("Releasing: " + InputSplit[1]);

                            byte KeyByte = byte.Parse(InputSplit[1]);
                            ActiveKeys.Remove(KeyByte);
                            SimKeyUp(KeyByte);
                        }
                    }
                }
            }

            ActiveKeys.Clear();
            SimKeyUp(37);
            SimKeyUp(38);
            SimKeyUp(39);
            SimKeyUp(40);
        }

        public static void StartReplay(bool Waited)
        {
            if (Waited)
            {
                Thread RunThread = new Thread(new ThreadStart(RunReplay));
                RunThread.Start();
            }
            else
            {
                WaitingForRespawn = true;
                NeedsKill = true;
            }
        }

        public static void SaveHistory()
        {
            if (!IsReplay)
            {
                PlayerManager GameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();

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
                        }
                        else
                        {
                            ReplayFile.WriteLine($"{KeySet.Key - FirstTime}:{KeySet.Value}");
                        }
                    }

                    ReplayFile.Close();
                }
            }

            KeyHistory.Clear();
        }
    }
}
