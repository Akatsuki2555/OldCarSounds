using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using MSCLoader;
using OldCarSounds_Old.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldCarSounds_Old
{
    public class OldCarSounds_Old : Mod
    {
        public static Settings noDeathSetting;
        public static Settings assembleSounds;
        public static Settings noEngineOverrev;
        public static Settings oldRadioSongsSettings;
        public static Settings oldDashTexturesSettings;
        public static Settings infoTextSettings;
        public static Settings disableKnobSoundsSettings;
        public static Settings disableDoorSoundsSettings;
        public static Settings disableFootSoundsSettings;
        public static Settings oldRPMGaugeSettings;
        public static Settings shiftDelaySelectionSettings;
        public static Settings keySoundSelectionSettings;
        public static Settings selectionSelectionSettings;
        public static Settings engineSoundsTypeSettings;
        public static Settings changeableWrenchSizeSettings;
        public static Settings hasViewedPrompt;
        public static bool loadAssembleSound;
        public static bool noEngineOverRev;
        public static bool oldRadioSongs;
        public static bool oldDashTextures;
        public static bool infoText;
        public static bool disableKnobSounds;
        public static bool disableDoorSounds;
        public static bool disableFootSounds;
        public static bool noDeath;
        public static bool oldRPMGauge;
        public static bool changeableWrenchSize;
        public static bool viewedPrompt;
        public static int shiftDelaySelection;
        public static int keySoundSelection;
        public static int selectionSelection;
        public static int engineSoundsType;

        public static AudioClip clip1;

        public static AudioClip clip2;

        public static AudioClip clip3;

        public static GameObject radio1, satsuma;

        public static Material material1, selMaterial;
        public static bool oldDelay;

        public static bool loadGameOnMenu;

        public List<PlayMakerFSM> fsmsAntiDeath;
        private Material noSel;

        private Stopwatch stopwatch;

        public override string ID => nameof(OldCarSounds_Old); //Your mod ID (unique)
        public override string Name => "Old Car Sounds (OLD MODLoader)"; //You mod name
        public override string Author => "MLDKYT"; //Your Username
        public override string Version => "1.4.0"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
        }

        public override void OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            PrintF("Starting Loading of OldCarSounds...", "load");

            // Load asset bundle
            PrintF("Loading AssetBundle", "load");
            AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(Storage.AssetBundle1);

            if (engineSoundsType == 2)
            {
                PrintF("Loading audio files for old builds...", "load");
                clip1 = assetBundle.LoadAsset("car_idle") as AudioClip;
                clip2 = assetBundle.LoadAsset("idle_sisa") as AudioClip;
            }

            // Assemble sounds
            if (loadAssembleSound)
            {
                PrintF("Loading audio files for assembly sounds...", "load");
                clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
            }

            // Music
            if (oldRadioSongs)
            {
                PrintF("Loading radio songs...", "load");
                radio1 = assetBundle.LoadAsset("oldradio") as GameObject;
                RadioCore.Clips.Add(assetBundle.LoadAsset("mustamies") as AudioClip);
                RadioCore.Clips.Add(assetBundle.LoadAsset("oldradiosong") as AudioClip);
                RadioCore.Clips.Add(assetBundle.LoadAsset("song2") as AudioClip);
                RadioCore.Clips.Add(assetBundle.LoadAsset("song3") as AudioClip);
                RadioCore.Clips.Add(assetBundle.LoadAsset("song4") as AudioClip);
                RadioCore.Clips.Add(assetBundle.LoadAsset("song5") as AudioClip);

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
            if (oldDashTextures)
            {
                PrintF("Loading black material for dashboard");
                material1 = assetBundle.LoadAsset<Material>("black");
            }

            // Selection textures if chosen to
            if (selectionSelection == 1)
            {
                PrintF("Loading selection material");
                selMaterial = assetBundle.LoadAsset<Material>("selection");
            }

            noSel = assetBundle.LoadAsset<Material>("nosel");

            // Unload the asset bundle to reduce memory usage
            // assets will remain loaded
            PrintF("Unloading AssetBundle", "load");
            assetBundle.Unload(false);

            // Get Satsuma.
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            // Add the component that does the load stuff
            PrintF("Adding component for Satsuma", "load");
            satsuma.AddComponent<SatsumaOCS>();

            // Disable death if checked
            if (noDeath)
            {
                PrintF("Disabling death...", "load");
                var fsms = new List<PlayMakerFSM>(Object.FindObjectsOfType<PlayMakerFSM>());
                var filteredFsms = new List<PlayMakerFSM>();
                foreach (PlayMakerFSM playMakerFsm in fsms)
                {
                    if (playMakerFsm == null) continue;
                    if (playMakerFsm.FsmName.ToLower().Contains("death") ||
                        playMakerFsm.FsmName.ToLower().Contains("die")) filteredFsms.Add(playMakerFsm);
                }

                foreach (PlayMakerFSM playMakerFsm in filteredFsms)
                {
                    PrintF(
                        $"Disabled death at {GameObjectPath(playMakerFsm.gameObject)} with name {playMakerFsm.FsmName}");
                    playMakerFsm.enabled = false;
                    fsmsAntiDeath.Add(playMakerFsm);
                }
            }

            // Old RPM Gauge
            if (oldRPMGauge) GameObject.Find("rpm gauge").AddComponent<RPMGauge>();


            // Create a new instance of stopwatch
            PrintF("Starting stopwatch for info text", "load");
            stopwatch = new Stopwatch();
            // Start a stopwatch for the lake time info thing
            stopwatch.Start();
            PrintF("Fully loaded!", "load", true);
        }

        public override void ModSettings()
        {
            noDeathSetting = new Settings("noDeath", "No Death", false, UpdateAllSettings);
            assembleSounds = new Settings("assembleSounds", "Assemble Sounds", false, UpdateAllSettings);
            disableDoorSoundsSettings = new Settings("doorSounds", "Disable Door Sounds", false, UpdateAllSettings);
            disableFootSoundsSettings = new Settings("footSounds", "Disable Foot Sounds", false, UpdateAllSettings);
            disableKnobSoundsSettings = new Settings("knobSounds", "Disable Knob Sounds", false, UpdateAllSettings);
            noEngineOverrev = new Settings("noEngineOverrev", "No Engine Overreving", false, UpdateAllSettings);
            oldDashTexturesSettings = new Settings("oldDash", "Old Dashboard", false, UpdateAllSettings);
            infoTextSettings = new Settings("info", "Information Text", false, UpdateAllSettings);
            oldRadioSongsSettings = new Settings("radio", "Old Radio", false, UpdateAllSettings);
            shiftDelaySelectionSettings = new Settings("shiftDelay", "Shift Delay Selection", 0, UpdateAllSettings);
            keySoundSelectionSettings = new Settings("keySound", "Key Sound Selection", 0, UpdateAllSettings);
            selectionSelectionSettings = new Settings("selection", "Selections Settings", 0, UpdateAllSettings);
            engineSoundsTypeSettings = new Settings("sounds", "Engine Sounds Type", 0, UpdateAllSettings);
            oldRPMGaugeSettings = new Settings("rpmgauge", "Old RPM Gauge", false, UpdateAllSettings);
            changeableWrenchSizeSettings = new Settings("wrenchsizechange", "Changeable Wrench Size", false, UpdateAllSettings);
            UpdateAllSettings();
            Settings.AddSlider(this, shiftDelaySelectionSettings, 0, 2, new[]
            {
                "No change",
                "Build 172",
                "No delay"
            });
            Settings.AddSlider(this, keySoundSelectionSettings, 0, 2, new[]
            {
                "No change",
                "Old key sounds (2016)",
                "No key sounds (2014)"
            });
            Settings.AddSlider(this, selectionSelectionSettings, 0, 2, new[]
            {
                "No change in selections",
                "Green box selections",
                "Newer selections"
            });
            Settings.AddSlider(this, engineSoundsTypeSettings, 0, 2, new[]
            {
                "No engine sound change",
                "Lower pitch (2016)",
                "Old alpha (2014)"
            });
            Settings.AddCheckBox(this, noDeathSetting);
            Settings.AddCheckBox(this, assembleSounds);
            Settings.AddCheckBox(this, disableDoorSoundsSettings);
            Settings.AddCheckBox(this, disableFootSoundsSettings);
            Settings.AddCheckBox(this, disableKnobSoundsSettings);
            Settings.AddCheckBox(this, noEngineOverrev);
            Settings.AddCheckBox(this, oldDashTexturesSettings);
            Settings.AddCheckBox(this, infoTextSettings);
            Settings.AddCheckBox(this, oldRadioSongsSettings);
            Settings.AddCheckBox(this, oldRPMGaugeSettings);
            Settings.AddCheckBox(this, changeableWrenchSizeSettings);
        }

        private void UpdateAllSettings()
        {
            noDeath = (bool) noDeathSetting.GetValue();
            loadAssembleSound = (bool) assembleSounds.GetValue();
            disableDoorSounds = (bool) disableDoorSoundsSettings.GetValue();
            disableFootSounds = (bool) disableFootSoundsSettings.GetValue();
            disableKnobSounds = (bool) disableKnobSoundsSettings.GetValue();
            noEngineOverRev = (bool) noEngineOverrev.GetValue();
            oldDashTextures = (bool) oldDashTexturesSettings.GetValue();
            infoText = (bool) infoTextSettings.GetValue();
            oldRadioSongs = (bool) oldRadioSongsSettings.GetValue();
            shiftDelaySelection = int.Parse(shiftDelaySelectionSettings.GetValue().ToString());
            keySoundSelection = int.Parse(keySoundSelectionSettings.GetValue().ToString());
            selectionSelection = int.Parse(selectionSelectionSettings.GetValue().ToString());
            engineSoundsType = int.Parse(engineSoundsTypeSettings.GetValue().ToString());
            oldRPMGauge = (bool) oldRPMGaugeSettings.GetValue();
            changeableWrenchSize = (bool) changeableWrenchSizeSettings.GetValue();
        }

        public override void OnSave()
        {
            // Called once, when save and quit
            // Serialize your save file here.
        }

        public override void OnGUI()
        {
            // Use GUI statements
            // Not anything else
            // Called every render

            // Render a box with information about settings.
            if (ModLoader.GetCurrentScene() == CurrentScene.MainMenu)
            {
                GUI.Box(new Rect(Screen.width - 210, 10, 200, 40), "OldCarSounds 1.4");
                GUI.Label(new Rect(Screen.width - 205, 25, 190, 20), "Moved to Mod Settings");
            }

            if (ModLoader.GetCurrentScene() == CurrentScene.Game)
            {
                float fps = (float) Math.Round(1f / Time.unscaledDeltaTime, 2);
                float wrenchSize = FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value;
                GUI.Label(new Rect(0, 0, 500, 20), $"FPS: {fps}");
                GUI.Label(new Rect(0, 20, 500, 20), $"Wrench size: {wrenchSize}");
                GUI.Label(new Rect(0, 40, 500, 20),
                    $"Lake run current time: {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}");
                GUI.Label(new Rect(0, 60, 500, 20), "Lake run last time: ");
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
                    if (!(SatsumaOCS.PowerKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOCS.PowerKnob.name)
                        {
                            if (selectionSelection == 1)
                                SatsumaOCS.PowerKnob.GetComponent<Renderer>().material = selMaterial;

                            if (selectionSelection == 2)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            break;
                        }

                        SatsumaOCS.PowerKnob.GetComponent<Renderer>().material = noSel;
                    }

                    // Volume knob
                    if (!(SatsumaOCS.VolumeKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOCS.VolumeKnob.name)
                        {
                            if (selectionSelection == 1)
                                SatsumaOCS.VolumeKnob.GetComponent<Renderer>().material = selMaterial;

                            if (selectionSelection == 2)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            float wheel = Input.GetAxis("Mouse ScrollWheel");
                            if (wheel >= 0.01f) SatsumaOCS.radioCoreInstance.IncreaseVolume();

                            if (wheel <= -0.01f) SatsumaOCS.radioCoreInstance.DecreaseVolume();

                            break;
                        }

                        SatsumaOCS.VolumeKnob.GetComponent<Renderer>().material = noSel;
                    }

                    // Switch knob
                    if (!(SatsumaOCS.SwitchKnob is null))
                    {
                        if (hit.collider.gameObject.name == SatsumaOCS.SwitchKnob.name)
                        {
                            if (selectionSelection == 1)
                                SatsumaOCS.SwitchKnob.GetComponent<Renderer>().material = selMaterial;

                            if (selectionSelection == 2)
                            {
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                            }

                            break;
                        }

                        SatsumaOCS.SwitchKnob.GetComponent<Renderer>().material = noSel;
                    }

                    if (!(SatsumaOCS.knobChoke is null) && !(SatsumaOCS.triggerChoke is null))
                    {
                        // Check other knobs
                        if (hit.collider.gameObject == SatsumaOCS.triggerChoke)
                        {
                            if (selectionSelection == 1)
                            {
                                // Color is now green
                                SatsumaOCS.knobChoke.GetComponent<Renderer>().material = selMaterial;
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
                        SatsumaOCS.knobChoke.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOCS.knobHazards is null) && !(SatsumaOCS.triggerHazard is null))
                    {
                        if (hit.collider.gameObject == SatsumaOCS.triggerHazard)
                        {
                            if (selectionSelection == 1)
                            {
                                SatsumaOCS.knobHazards.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOCS.knobHazards.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOCS.knobLights is null) && !(SatsumaOCS.triggerLightModes is null))
                    {
                        if (hit.collider.gameObject == SatsumaOCS.triggerLightModes)
                        {
                            if (selectionSelection == 1)
                            {
                                SatsumaOCS.knobLights.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOCS.knobLights.GetComponent<Renderer>().material = material1;
                    }

                    if (!(SatsumaOCS.knobWipers is null) && !(SatsumaOCS.triggerButtonWiper is null))
                    {
                        if (hit.collider.gameObject == SatsumaOCS.triggerButtonWiper)
                        {
                            if (selectionSelection == 1)
                            {
                                SatsumaOCS.knobWipers.GetComponent<Renderer>().material = selMaterial;
                                FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                                FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                            }

                            break;
                        }

                        SatsumaOCS.knobWipers.GetComponent<Renderer>().material = material1;
                    }

                    if (changeableWrenchSize)
                    {
                        bool flag1 = Input.GetKeyDown(KeyCode.U);
                        bool flag2 = Input.GetKeyDown(KeyCode.K);

                        if (flag1)
                        {
                            FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat("ToolWrenchSize");
                            fsmFloat.Value += 0.1f;
                            if (fsmFloat.Value > 1.5f) fsmFloat.Value = 1.5f;
                            if (fsmFloat.Value < 0.5f) fsmFloat.Value = 0.5f;
                        }

                        if (flag2)
                        {
                            FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat("ToolWrenchSize");
                            fsmFloat.Value -= 0.1f;
                            if (fsmFloat.Value > 1.5f) fsmFloat.Value = 1.5f;
                            if (fsmFloat.Value < 0.5f) fsmFloat.Value = 0.5f;
                        }
                    }
                }


                // If we click
                if (Input.GetMouseButtonDown(0))
                    // and we're aiming at the buttons
                    foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),
                        3f))
                        if (!(SatsumaOCS.radioCoreInstance is null))
                        {
                            // Aiming at the power knob
                            if (hit.collider.gameObject.name == "trigger_ocs_power1")
                            {
                                // Toggle radio
                                if (SatsumaOCS.radioCoreInstance.@on) SatsumaOCS.radioCoreInstance.DisableRadio();
                                else SatsumaOCS.radioCoreInstance.EnableRadio();
                            }

                            // Aiming at the switch song knob
                            if (hit.collider.gameObject.name == "trigger_ocs_switch1")
                                // Change song
                                SatsumaOCS.radioCoreInstance.NextClip();
                        }
            }

            if (noDeath)
                foreach (PlayMakerFSM playMakerFsm in fsmsAntiDeath)
                    playMakerFsm.enabled = false;
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
        public static void PrintF(
            string text, string module = "SYSTEM",
            bool console = false)
        {
            try
            {
                string modConfigFolder =
                    ModLoader.GetModConfigFolder(ModLoader.LoadedMods.First(x => x.ID == nameof(OldCarSounds_Old)));
                StreamWriter writer = new StreamWriter(Path.Combine(modConfigFolder, "log.log"), true);
                StringBuilder builder = new StringBuilder()
                    .Append(DateTime.Now)
                    .Append(" [")
                    .Append(module.ToUpper())
                    .Append("]: ")
                    .Append(text);
                writer.WriteLine(builder.ToString());
                writer.Close();

                if (console)
                {
                    if (module.ToUpper() == "ERROR" || module.ToUpper() == "ERR")
                        ModConsole.Error(builder.ToString());
                    else if (module.ToUpper() == "WARN" || module.ToUpper() == "WARNING")
                        ModConsole.Warning(builder.ToString());
                    else
                        ModConsole.Print(builder.ToString());
                }
#if DEBUG
                else
                {
                    if (module.ToUpper() == "ERROR" || module.ToUpper() == "ERR")
                    {
                        ModConsole.Error(builder.ToString());
                    }
                    else if (module.ToUpper() == "WARN" || module.ToUpper() == "WARNING")
                    {
                        ModConsole.Warning(builder.ToString());
                    }
                    else
                    {
                        ModConsole.Print(builder.ToString());
                    }
                }
#endif
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string GameObjectPath(GameObject go)
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
    }
}