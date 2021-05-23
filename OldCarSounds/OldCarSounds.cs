using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public override string Version => "1.4.0";

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
            noDeathSetting = modSettings.AddToggle("nodeath", "No death", noDeath);
            assembleSounds = modSettings.AddToggle("assemble", "Assemble sounds", loadAssembleSound);
            noEngineOverrev = modSettings.AddToggle("overrev", "No engine overrev", noEngineOverRev);
            oldRadioSongsSettings = modSettings.AddToggle("radio", "Old radio songs", oldRadioSongs);
            oldDashTexturesSettings = modSettings.AddToggle("dash", "Old dash textures", oldDashTextures);
            infoTextSettings = modSettings.AddToggle("info", "Info text", infoText);
            disableKnobSoundsSettings = modSettings.AddToggle("sounds1", "Disable Knob sounds", disableKnobSounds);
            disableFootSoundsSettings = modSettings.AddToggle("sounds2", "Disable Foot sounds", disableFootSounds);
            disableDoorSoundsSettings = modSettings.AddToggle("sounds3", "Disable Door sounds", disableDoorSounds);
            oldRPMGaugeSettings = modSettings.AddToggle("rpm", "Old RPM gauge", oldRPMGauge);
            changeableWrenchSizeSettings = modSettings.AddToggle("wrenchsize", "Changeable Wrench Size", changeableWrenchSize);

            shiftDelaySelectionSettings = modSettings.AddRadioButtons("shiftDelay", "Shift delay", shiftDelaySelection, "Default", "Old", "None");
            keySoundSelectionSettings = modSettings.AddRadioButtons("keysound", "Key sounds", keySoundSelection, "Default", "2016", "None");
            selectionSelectionSettings = modSettings.AddRadioButtons("selection", "Selection visual", selectionSelection, "Default", "Green boxes");
            engineSoundsTypeSettings = modSettings.AddRadioButtons("engineSounds", "Engine sounds", engineSoundsType, "Unchanged", "Old (2016)", "Old (2014)");

            settingNumber1 = modSettings.AddBoolean("viewed", false);

            modSettings.AddButton("reload", "Reload settings", "Reload if in game", () =>
            {
                loadGameOnMenu = true;
                Application.LoadLevel(1);
            });

            noDeath = noDeathSetting.Value;
            loadAssembleSound = assembleSounds.Value;
            noEngineOverRev = noEngineOverrev.Value;
            oldRadioSongs = oldRadioSongsSettings.Value;
            oldDashTextures = oldDashTexturesSettings.Value;
            infoText = infoTextSettings.Value;
            disableDoorSounds = disableDoorSoundsSettings.Value;
            disableFootSounds = disableFootSoundsSettings.Value;
            disableKnobSounds = disableKnobSoundsSettings.Value;
            oldRPMGauge = oldRPMGaugeSettings.Value;

            shiftDelaySelection = shiftDelaySelectionSettings.Value;
            keySoundSelection = keySoundSelectionSettings.Value;
            selectionSelection = selectionSelectionSettings.Value;
            engineSoundsType = engineSoundsTypeSettings.Value;

            if (!settingNumber1)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("If you want to add custom songs, ");
                sb.AppendLine("create a folder called \"radiosongs\"");
                sb.AppendLine("in your assets folder.");

                ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                prompt.AddButton("OK", () => { PrintF($"Path to Assets: {ModLoader.GetModAssetsFolder(this)}"); });
                prompt.Title = "OldCarSounds";
                prompt.Text = sb.ToString();
            }
        }

        public override void ModSettingsClose()
        {
            noDeath = noDeathSetting.Value;
            loadAssembleSound = assembleSounds.Value;
            noEngineOverRev = noEngineOverrev.Value;
            oldRadioSongs = oldRadioSongsSettings.Value;
            oldDashTextures = oldDashTexturesSettings.Value;
            infoText = infoTextSettings.Value;
            disableDoorSounds = disableDoorSoundsSettings.Value;
            disableFootSounds = disableFootSoundsSettings.Value;
            disableKnobSounds = disableKnobSoundsSettings.Value;
            oldRPMGauge = oldRPMGaugeSettings.Value;
            changeableWrenchSize = changeableWrenchSizeSettings.Value;

            shiftDelaySelection = shiftDelaySelectionSettings.Value;
            keySoundSelection = keySoundSelectionSettings.Value;
            selectionSelection = selectionSelectionSettings.Value;
            engineSoundsType = engineSoundsTypeSettings.Value;
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
                    ModLoader.GetModSettingsFolder(ModLoader.LoadedMods.First(x => x.ID == nameof(OldCarSounds)));
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

        #region Variables

        public static AudioClip clip1;

        public static AudioClip clip2;

        public static AudioClip clip3;

        public static SettingToggle noDeathSetting;
        public static SettingToggle assembleSounds;
        public static SettingToggle noEngineOverrev;
        public static SettingToggle oldRadioSongsSettings;
        public static SettingToggle oldDashTexturesSettings;
        public static SettingToggle infoTextSettings;
        public static SettingToggle disableKnobSoundsSettings;
        public static SettingToggle disableDoorSoundsSettings;
        public static SettingToggle disableFootSoundsSettings;
        public static SettingToggle oldRPMGaugeSettings;
        public static SettingToggle changeableWrenchSizeSettings;

        public static SettingRadioButtons shiftDelaySelectionSettings;
        public static SettingRadioButtons keySoundSelectionSettings;
        public static SettingRadioButtons selectionSelectionSettings;
        public static SettingRadioButtons engineSoundsTypeSettings;

        public static SettingBoolean settingNumber1;

        public static bool loadAssembleSound,
            noEngineOverRev,
            oldRadioSongs,
            oldDashTextures,
            infoText,
            disableKnobSounds,
            disableDoorSounds,
            disableFootSounds,
            noDeath,
            oldRPMGauge,
            changeableWrenchSize;

        public static int shiftDelaySelection, keySoundSelection, selectionSelection, engineSoundsType;

        public static GameObject radio1, satsuma;

        public static Material material1, selMaterial;
        public static bool oldDelay;

        public static bool loadGameOnMenu;

        public List<PlayMakerFSM> fsmsAntiDeath;

        private Stopwatch stopwatch;
        private Material noSel;

        #endregion
    }
}