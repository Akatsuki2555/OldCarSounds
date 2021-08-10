using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HutongGames.PlayMaker;
using MSCLoader;
using OldCarSounds.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldCarSounds
{
    public class OldCarSounds : Mod
    {
        public override string ID => nameof(OldCarSounds);
        public override string Name => "Old Car Sounds";
        public override string Author => "MLDKYT";
        public override string Version => "1.4.4";

        public override void MenuOnLoad()
        {
            if (loadGameOnMenu)
            {
                loadGameOnMenu = false;
                Application.LoadLevel(3);
            }
        }

        public override void OnLoad()
        {
            if (File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(this), "log.log")))
            {
                File.Delete(Path.Combine(ModLoader.GetModSettingsFolder(this), "log.log"));
            }

            // Called once, when mod is loading after game is fully loaded
            PrintF("Starting Loading of OldCarSounds...", "load");

            // Load asset bundle
            PrintF("Loading AssetBundle", "load");

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream("OldCarSounds.Resources.oldsound.unity3d");
            byte[] shit = new byte[stream.Length];
            stream.Read(shit, 0, shit.Length);

            AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(shit);

            if (engineSoundsTypeSettings.Value == 2)
            {
                PrintF("Loading audio files for old builds...", "load");
                clip2 = assetBundle.LoadAsset<AudioClip>("idle_sisa");
                clip1 = assetBundle.LoadAsset<AudioClip>("idle");
            }

            // Assemble sounds
            if (assembleSounds.Value)
            {
                PrintF("Loading audio files for assembly sounds...", "load");
                clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
            }

            _noSel = assetBundle.LoadAsset<Material>("nosel");

            // Music
            if (oldRadioSongsSettings.Value)
            {
                PrintF("Loading radio songs...", "load");
                radio1 = assetBundle.LoadAsset("oldradio") as GameObject;

                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("mustamies"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("oldradiosong"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song2"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song3"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song4"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song5"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song6"));
                RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song7"));

                // Import custom songs
                string path = Path.Combine(ModLoader.GetModAssetsFolder(this), "radiosongs");
                if (File.Exists(path))
                    foreach (string name in Directory.GetFiles(path))
                    {
                        PrintF("Loading: " + name);
                        WWW www = new WWW("file:///" + name);
                        RadioCore.Clips.Add(www.GetAudioClip(true, false));
                    }
            }

            // Dashboard texture
            if (oldDashTexturesSettings.Value)
            {
                PrintF("Loading black material for dashboard");
                material1 = assetBundle.LoadAsset<Material>("black");
            }

            // Selection textures if chosen to
            if (selectionSelectionSettings.Value == 1)
            {
                PrintF("Loading selection material");
                selMaterial = assetBundle.LoadAsset<Material>("selection");
            }


            // Unload the asset bundle to reduce memory usage
            // assets will remain loaded
            PrintF("Unloading AssetBundle", "load");
            assetBundle.Unload(false);

            // Get Satsuma.
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            // Add the component that does the load stuff
            PrintF("Adding component for Satsuma", "load");
            satsuma.AddComponent<SatsumaOcs>();

            // Old RPM Gauge
            if (oldRpmGaugeSettings.Value)
            {
                GameObject object1 = Object.FindObjectsOfType<GameObject>()
                    .First(sdf => sdf.name.ToLower().Contains("rpm gauge"));
                if (object1 != null)
                {
                    object1.SetActive(true);
                    object1.AddComponent<RPMGauge>();
                }
                else
                {
                    PrintF("Old RPM gauge failed to load.");
                }
            }


            // Create a new instance of stopwatch
            PrintF("Starting stopwatch for info text", "load");
            _stopwatch = new Stopwatch();
            // Start a stopwatch for the lake time info thing
            _stopwatch.Start();
            PrintF("Fully loaded!", "load", true);
        }

        public override void ModSettings()
        {
            modSettings.AddButton("nodeath", "How to get No death", "NoDeath",
                () => { Process.Start("https://mldkyt.github.io/stuff/OldCarSounds/health-mod-setup.html"); });

            assembleSounds = modSettings.AddToggle("assemble", "Assemble sounds", true);
            oldRadioSongsSettings = modSettings.AddToggle("radio", "Old radio songs", true);
            oldDashTexturesSettings = modSettings.AddToggle("dash", "Old dash textures", true);
            infoTextSettings = modSettings.AddToggle("info", "Info text", true);
            disableKnobSoundsSettings = modSettings.AddToggle("sounds1", "Disable Knob sounds", true);
            disableFootSoundsSettings = modSettings.AddToggle("sounds2", "Disable Foot sounds", false);
            disableDoorSoundsSettings = modSettings.AddToggle("sounds3", "Disable Door sounds", false);
            oldRpmGaugeSettings = modSettings.AddToggle("rpm", "Old RPM gauge", true);
            oldCrashSounds = modSettings.AddToggle("crash1", "Old crash sound", false);
            oldRevving = modSettings.AddToggle("oldrev", "Old engine revving", true);

            shiftDelaySelectionSettings =
                modSettings.AddRadioButtons("shiftDelay", "Shift delay", 1, "Default", "Old", "None");
            keySoundSelectionSettings =
                modSettings.AddRadioButtons("keysound", "Key sounds", 2, "Default", "2016", "None");
            selectionSelectionSettings =
                modSettings.AddRadioButtons("selection", "Selection visual", 1, "Default", "Green boxes");
            engineSoundsTypeSettings = modSettings.AddRadioButtons("engineSounds", "Engine sounds", 2, "Unchanged",
                "Old (2016)", "Old (2014)");

            settingNumber1 = modSettings.AddBoolean("viewed", false);

            modSettings.AddButton("reload", "Reload settings", "Reload if in game", () =>
            {
                loadGameOnMenu = true;
                ModPrompt.CreateContinueAbortPrompt("Your mod loader might bug out if you reload it this way.",
                    "Warning", () => { Application.LoadLevel(1); });
            });

            modSettings.AddButton("musicButton", "How to add custom songs", () =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("If you want to add custom songs, ");
                sb.AppendLine("create a folder called \"radiosongs\"");
                sb.AppendLine("in the assets folder");
                sb.AppendLine("of the mod");

                ModPrompt.CreatePrompt(sb.ToString());
            });
        }

        public override void OnGUI()
        {
            // Use GUI statements
            // Not anything else
            // Called every render

            // Render a box with information about settings.
            if (ModLoader.CurrentScene == CurrentScene.MainMenu)
            {
                GUI.Box(new Rect(Screen.width - 210, 10, 200, 40), "OldCarSounds 1.4");
                GUI.Label(new Rect(Screen.width - 205, 25, 190, 20), "Moved to Mod Settings");
            }

            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                if (infoTextSettings.Value)
                {
                    float fps = (float) Math.Round(1f / Time.unscaledDeltaTime, 2);
                    float wrenchSize = FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value;
                    GUI.Label(new Rect(0, 0, 1000, 20), $"FPS: {fps}");
                    GUI.Label(new Rect(0, 20, 1000, 20), $"Wrench size: {wrenchSize}");
                    GUI.Label(new Rect(0, 40, 1000, 20),
                        $"Lake run current time: {_stopwatch.Elapsed.Minutes}:{_stopwatch.Elapsed.Seconds}:{_stopwatch.Elapsed.Milliseconds}");
                    GUI.Label(new Rect(0, 60, 1000, 20), "Lake run last time: ");
                }
            }
        }

        public override void Update()
        {
            // Color buttons green if looking at them

            if (!(Camera.main is null))
            {
                foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f))
                {
                    // Power knob
                    if (!(SatsumaOcs.powerKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOcs.powerKnob.name)
                        {
                            if (selectionSelectionSettings.Value == 0)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            if (selectionSelectionSettings.Value == 1)
                                SatsumaOcs.powerKnob.GetComponent<Renderer>().material = selMaterial;

                            break;
                        }

                        SatsumaOcs.powerKnob.GetComponent<Renderer>().material = _noSel;
                    }

                    // Volume knob
                    if (!(SatsumaOcs.volumeKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOcs.volumeKnob.name)
                        {
                            if (selectionSelectionSettings.Value == 0)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            if (selectionSelectionSettings.Value == 1)
                                SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = selMaterial;

                            float wheel = Input.GetAxis("Mouse ScrollWheel");
                            if (wheel >= 0.01f) SatsumaOcs.radioCoreInstance.IncreaseVolume();

                            if (wheel <= -0.01f) SatsumaOcs.radioCoreInstance.DecreaseVolume();

                            break;
                        }

                        SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = _noSel;
                    }

                    // Switch knob
                    if (!(SatsumaOcs.switchKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOcs.switchKnob.name)
                        {
                            if (selectionSelectionSettings.Value == 1)
                                SatsumaOcs.switchKnob.GetComponent<Renderer>().material = selMaterial;

                            if (selectionSelectionSettings.Value == 0)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            break;
                        }

                        SatsumaOcs.switchKnob.GetComponent<Renderer>().material = _noSel;
                    }

                    if (!(SatsumaOcs.knobChoke is null) && !(SatsumaOcs.triggerChoke is null))
                    {
                        // Check other knobs
                        if (hit.collider.gameObject == SatsumaOcs.triggerChoke)
                        {
                            if (selectionSelectionSettings.Value == 1)
                            {
                                // Color is now green
                                SatsumaOcs.knobChoke.GetComponent<Renderer>().material = selMaterial;
                                // Disable the little subtitle and stuff in center
                                // of the screen.
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                                // This "break" will exit forEach statement.
                                // Not triggering any code under }
                            }

                            break;
                        }

                        // If we don't exit in the last if statement,
                        // this will run setting the color back to normal
                        SatsumaOcs.knobChoke.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOcs.knobHazards is null) && !(SatsumaOcs.triggerHazard is null))
                    {
                        if (hit.collider.gameObject == SatsumaOcs.triggerHazard)
                        {
                            if (selectionSelectionSettings.Value == 1)
                            {
                                SatsumaOcs.knobHazards.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOcs.knobHazards.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOcs.knobLights is null) && !(SatsumaOcs.triggerLightModes is null))
                    {
                        if (hit.collider.gameObject == SatsumaOcs.triggerLightModes)
                        {
                            if (selectionSelectionSettings.Value == 1)
                            {
                                SatsumaOcs.knobLights.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOcs.knobLights.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOcs.knobWipers is null) && !(SatsumaOcs.triggerButtonWiper is null))
                    {
                        if (hit.collider.gameObject == SatsumaOcs.triggerButtonWiper)
                        {
                            if (selectionSelectionSettings.Value == 1)
                            {
                                SatsumaOcs.knobWipers.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOcs.knobWipers.GetComponent<Renderer>().material = material1;
                    }
                }


                // If we click
                if (Input.GetMouseButtonDown(0))
                    // and we're aiming at the buttons
                    foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),
                        3f))
                        if (!(SatsumaOcs.radioCoreInstance is null))
                        {
                            // Aiming at the power knob
                            if (hit.collider.gameObject.name == "trigger_ocs_power1")
                            {
                                // Toggle radio
                                if (SatsumaOcs.radioCoreInstance.on) SatsumaOcs.radioCoreInstance.DisableRadio();
                                else SatsumaOcs.radioCoreInstance.EnableRadio();
                            }

                            // Aiming at the switch song knob
                            if (hit.collider.gameObject.name == "trigger_ocs_switch1")
                                // Change song
                                SatsumaOcs.radioCoreInstance.NextClip();
                        }
            }
        }

        /// <summary>
        ///     Write to logs.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="module">Where the message is coming from. By default it's SYSTEM.</param>
        /// <param name="console">
        ///     If it should be displayed in the console even if it's not
        ///     debugging mode.
        /// </param>
        /// <exception cref="IOException">Cannot write to logs.</exception>
        public static void PrintF(string text, string module = "SYSTEM", bool console = false)
        {
            try
            {
                string modConfigFolder =
                    ModLoader.GetModSettingsFolder(ModLoader.LoadedMods.First(x => x.ID == nameof(OldCarSounds)));
                StreamWriter writer = new StreamWriter(Path.Combine(modConfigFolder, "log.log"), true);
                StringBuilder builder = new StringBuilder().Append(DateTime.Now).Append(" [").Append(module.ToUpper())
                    .Append("]: ").Append(text);
                writer.WriteLine(builder.ToString());
                writer.Close();

                if (console)
                {
                    if (module.ToUpper() == "ERROR" || module.ToUpper() == "ERR")
                        ModConsole.LogError(builder.ToString());
                    else if (module.ToUpper() == "WARN" || module.ToUpper() == "WARNING")
                        ModConsole.LogWarning(builder.ToString());
                    else
                        ModConsole.Log(builder.ToString());
                }
#if DEBUG
                else
                {
                    if (module.ToUpper() == "ERROR" || module.ToUpper() == "ERR")
                    {
                        ModConsole.LogError(builder.ToString());
                    }
                    else if (module.ToUpper() == "WARN" || module.ToUpper() == "WARNING")
                    {
                        ModConsole.LogWarning(builder.ToString());
                    }
                    else
                    {
                        ModConsole.Log(builder.ToString());
                    }
                }
#endif
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static string GameObjectPath(GameObject go)
        {
            string s = "";
            GameObject temp1 = go;
            while (true)
            {
                s = temp1.name + "/" + s;
                if (temp1.transform.parent == null)
                    break;
                temp1 = temp1.transform.parent.gameObject;
            }

            return s;
        }

        #region Variables

        public static AudioClip clip1;

        public static AudioClip clip2;

        public static AudioClip clip3;

        public static SettingToggle assembleSounds;
        public static SettingToggle oldRadioSongsSettings;
        public static SettingToggle oldDashTexturesSettings;
        public static SettingToggle infoTextSettings;
        public static SettingToggle disableKnobSoundsSettings;
        public static SettingToggle disableDoorSoundsSettings;
        public static SettingToggle disableFootSoundsSettings;
        public static SettingToggle oldRpmGaugeSettings;
        public static SettingToggle oldCrashSounds;
        public static SettingToggle oldRevving;

        public static SettingRadioButtons shiftDelaySelectionSettings;
        public static SettingRadioButtons keySoundSelectionSettings;
        public static SettingRadioButtons selectionSelectionSettings;
        public static SettingRadioButtons engineSoundsTypeSettings;

        public static SettingBoolean settingNumber1;

        public static GameObject radio1, satsuma;

        public static Material material1, selMaterial;

        public static bool loadGameOnMenu;

        private Stopwatch _stopwatch;
        private Material _noSel;

        #endregion
    }
}