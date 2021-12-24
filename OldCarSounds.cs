using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HealthMod;
using HutongGames.PlayMaker;
using MSCLoader;
using OldCarSounds.Stuff;
using UnityEngine;


namespace OldCarSounds {
    public class OldCarSounds : Mod {
        public override string ID => nameof(OldCarSounds); //Your mod ID (unique)
        public override string Name => "Old Car Sounds"; //You mod name
        public override string Author => "MLDKYT"; //Your Username
        public override string Version => "1.5.2"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;
        
        public SatsumaOcs satsumaOcs;

        public override void OnNewGame() {
            if(!LoadGameOnMenu) return;
            LoadGameOnMenu = false;
            Application.LoadLevel(3);
        }

        public override void OnLoad() {
            if(File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(this),"log.log"))) {
                File.Delete(Path.Combine(ModLoader.GetModSettingsFolder(this),"log.log"));
            }

            // Called once, when mod is loading after game is fully loaded
            PrintF("Starting Loading of OldCarSounds...","load");

            // Load asset bundle
            PrintF("Loading AssetBundle","load");

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream("OldCarSounds.Resources.oldsound.unity3d");
            if(stream != null) {
                byte[] shit = new byte[stream.Length];
                stream.Read(shit,0,shit.Length);

                AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(shit);

                if(EngineSoundsType == 2) {
                    PrintF("Loading audio files from old builds...","load");
                    Clip2 = assetBundle.LoadAsset<AudioClip>("idle_sisa");
                    Clip1 = assetBundle.LoadAsset<AudioClip>("idle");
                }

                // Assemble sounds
                if(LoadAssembleSound) {
                    PrintF("Loading audio files for assembly sounds...","load");
                    Clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
                }

                _noSel = assetBundle.LoadAsset<Material>("nosel");

                // Music
                if(OldRadioSongs) {
                    PrintF("Loading radio songs...","load");
                    Radio1 = assetBundle.LoadAsset("oldradio") as GameObject;

                    if(ModLoader.CheckSteam()) {
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("mustamies"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("oldradiosong"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song2"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song3"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song4"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song5"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song6"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song7"));
                    }
                    else {
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("pirate"));
                    }

                    // Import custom songs
                    string path = Path.Combine(ModLoader.GetModAssetsFolder(this),"radiosongs");
                    if(File.Exists(path))
                        foreach(string name in Directory.GetFiles(path)) {
                            PrintF("Loading: " + name);
                            WWW www = new WWW("file:///" + name);
                            RadioCore.Clips.Add(www.GetAudioClip(true,false));
                        }
                }

                // Dashboard texture
                if(OldDashTextures) {
                    PrintF("Loading black material for dashboard");
                    Material1 = assetBundle.LoadAsset<Material>("black");
                }

                // Selection textures if chosen to
                if(SelectionSelection) {
                    PrintF("Loading selection material");
                    SelMaterial = assetBundle.LoadAsset<Material>("selection");
                }


                // Unload the asset bundle to reduce memory usage
                PrintF("Unloading AssetBundle","load");
                assetBundle.Unload(false);
            }
            else {
                PrintF("Error while loading mod resources. Your copy of OCS is corrupted.","ERR",true);
            }

            // Get the GameObject of Satsuma.
            Satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            // Add the component that does the load stuff
            PrintF("Adding component for Satsuma","load");
            Satsuma.AddComponent<SatsumaOcs>();

            // Old RPM Gauge
            if(OldRpmGauge) {
                GameObject object1 = UnityEngine.Object.FindObjectsOfType<GameObject>()
                    .First(sdf => sdf.name.ToLower().Contains("rpm gauge"));
                object1.SetActive(true);
                object1.AddComponent<RPMGauge>();
            }


            // Create a new instance of stopwatch
            PrintF("Starting stopwatch for info text","load");
            _stopwatch = new Stopwatch();
            // Start a stopwatch for the lake time info thing
            _stopwatch.Start();

            // Some lag optimization from the HP mod
            _hpStopwatch = new Stopwatch();
            _hpStopwatch.Start();

            PrintF("Fully loaded!","load",true);
        }

        public override bool SecondPass => true;

        public override void SecondPassOnLoad() {
            if (!ForceHealthModAlwaysFull) return;
            // Check if mod installed
            Mod mod = ModLoader.GetMod("Health");
            if(mod != null) return;
            _healthInstalled = false;
            PrintF("You have enabled forcing of Health mod to full, when there is no Health mod installed.","ERR",true);
        }

        public override void ModSettings() {
            AssembleSounds = new Settings("assembleSounds","Assemble Sounds",false,UpdateAllSettings);
            DisableDoorSoundsSettings = new Settings("doorSounds","Disable Door Sounds",false,UpdateAllSettings);
            DisableFootSoundsSettings = new Settings("footSounds","Disable Foot Sounds",false,UpdateAllSettings);
            DisableKnobSoundsSettings = new Settings("knobSounds","Disable Knob Sounds",false,UpdateAllSettings);
            OldDashTexturesSettings = new Settings("oldDash","Old Dashboard",false,UpdateAllSettings);
            InfoTextSettings = new Settings("info","Information Text",false,UpdateAllSettings);
            OldRadioSongsSettings = new Settings("radio","Old Radio",false,UpdateAllSettings);
            ShiftDelaySelectionSettings = new Settings("shiftDelay","Shift Delay Selection",0,UpdateAllSettings);
            KeySoundSelectionSettings = new Settings("keySound","Key Sound Selection",0,UpdateAllSettings);
            SelectionSelectionSettings = new Settings("selection","Green selections",false,UpdateAllSettings);
            EngineSoundsTypeSettings = new Settings("sounds","Engine sound type",0,UpdateAllSettings);
            OldRpmGaugeSettings = new Settings("rpmgauge","Old RPM Gauge",false,UpdateAllSettings);
            ChangeableWrenchSizeSettings =
                new Settings("wrenchsizechange","Changeable Wrench Size",false,UpdateAllSettings);
            ButtonNoDeath = new Settings("nodeath","No Death",
                () => { Process.Start("https://mldkyt.github.io/stuff/OldCarSounds/health-mod-setup.html"); });
            OldDelaySettings = new Settings("oldrev","Old engine revving",false,UpdateAllSettings);
            ForceHealthModAlwaysFullSettings = new Settings("healthforce","Force health mod to be always full health",
                false,UpdateAllSettings);

            UpdateAllSettings();

            Settings.AddButton(this, ButtonNoDeath);

            Settings.AddSlider(this, ShiftDelaySelectionSettings,0,2,new[] {
                "No change",
                "Build 172",
                "No delay"
            });
            Settings.AddSlider(this, KeySoundSelectionSettings,0,2,new[] {
                "No change",
                "Old key sounds (2016)",
                "No key sounds (2014)"
            });
            Settings.AddSlider(this, EngineSoundsTypeSettings,0,2,new[] {
                "No engine sound change",
                "Lower pitch (2016)",
                "Old alpha (2014)"
            });
            Settings.AddCheckBox(this, SelectionSelectionSettings);
            Settings.AddCheckBox(this, AssembleSounds);
            Settings.AddCheckBox(this, DisableDoorSoundsSettings);
            Settings.AddCheckBox(this, DisableFootSoundsSettings);
            Settings.AddCheckBox(this, DisableKnobSoundsSettings);
            Settings.AddCheckBox(this, OldDashTexturesSettings);
            Settings.AddCheckBox(this, InfoTextSettings);
            Settings.AddCheckBox(this, OldRadioSongsSettings);
            Settings.AddCheckBox(this, OldRpmGaugeSettings);
            Settings.AddCheckBox(this, ChangeableWrenchSizeSettings);
            Settings.AddText(this, "Please note that this setting requires Health to be installed.");
            Settings.AddCheckBox(this, ForceHealthModAlwaysFullSettings);
        }

        private void UpdateAllSettings() {
            LoadAssembleSound = (bool)AssembleSounds.GetValue();
            DisableDoorSounds = (bool)DisableDoorSoundsSettings.GetValue();
            DisableFootSounds = (bool)DisableFootSoundsSettings.GetValue();
            DisableKnobSounds = (bool)DisableKnobSoundsSettings.GetValue();
            OldDashTextures = (bool)OldDashTexturesSettings.GetValue();
            InfoText = (bool)InfoTextSettings.GetValue();
            OldRadioSongs = (bool)OldRadioSongsSettings.GetValue();
            ShiftDelaySelection = int.Parse(ShiftDelaySelectionSettings.GetValue().ToString());
            KeySoundSelection = int.Parse(KeySoundSelectionSettings.GetValue().ToString());
            SelectionSelection = (bool)SelectionSelectionSettings.GetValue();
            EngineSoundsType = int.Parse(EngineSoundsTypeSettings.GetValue().ToString());
            OldRpmGauge = (bool)OldRpmGaugeSettings.GetValue();
            ChangeableWrenchSize = (bool)ChangeableWrenchSizeSettings.GetValue();
            OldDelay = (bool)OldDelaySettings.Value;
            ForceHealthModAlwaysFull = (bool)ForceHealthModAlwaysFullSettings.Value;
        }

        public override void OnGUI() {
            // Use GUI statements
            // Not anything else
            // Called every render

            // Render a box with information about settings.
            if(ModLoader.GetCurrentScene() == CurrentScene.MainMenu) {
                GUI.Box(new Rect(Screen.width - 210,10,200,40),"OldCarSounds 1.4");
                GUI.Label(new Rect(Screen.width - 205,25,190,20),"Moved to Mod Settings");
            }

            if(ModLoader.GetCurrentScene() != CurrentScene.Game) return;
            if(!InfoText) return;
            float fps = (float)Math.Round(1f / Time.unscaledDeltaTime,2);
            float wrenchSize = FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value;
            GUI.Label(new Rect(0,0,1000,20),$"FPS: {fps}");
            GUI.Label(new Rect(0,20,1000,20),$"Wrench size: {wrenchSize}");
            GUI.Label(new Rect(0,40,1000,20),
                $"Lake run current time: {_stopwatch.Elapsed.Minutes}:{_stopwatch.Elapsed.Seconds}:{_stopwatch.Elapsed.Milliseconds}");
            GUI.Label(new Rect(0,60,1000,20),"Lake run last time: ");
        }

        public override void Update() {
            // Color buttons green if looking at them

            if(Camera.main == null) return;
            foreach(RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),3f)) {
                // Power knob
                if(!(SatsumaOcs.powerKnob == null)) {
                    if(hit.collider.gameObject.name == SatsumaOcs.powerKnob.name) {
                        if(!SelectionSelection) {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }
                        else SatsumaOcs.powerKnob.GetComponent<Renderer>().material = SelMaterial;

                        break;
                    }

                    SatsumaOcs.powerKnob.GetComponent<Renderer>().material = _noSel;
                }

                // Volume knob
                if(!(SatsumaOcs.volumeKnob == null)) {
                    if(hit.collider.gameObject.name == SatsumaOcs.volumeKnob.name) {
                        if(!SelectionSelection) {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }

                        else
                            SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = SelMaterial;

                        float wheel = Input.GetAxis("Mouse ScrollWheel");
                        if(wheel >= 0.01f) SatsumaOcs.radioCoreInstance.IncreaseVolume();

                        if(wheel <= -0.01f) SatsumaOcs.radioCoreInstance.DecreaseVolume();

                        break;
                    }

                    SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = _noSel;
                }

                // Switch knob
                if(!(SatsumaOcs.switchKnob == null)) {
                    if(hit.collider.gameObject.name == SatsumaOcs.switchKnob.name) {
                        if(SelectionSelection)
                            SatsumaOcs.switchKnob.GetComponent<Renderer>().material = SelMaterial;

                        else {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }

                        break;
                    }

                    SatsumaOcs.switchKnob.GetComponent<Renderer>().material = _noSel;
                }

                if(!(SatsumaOcs.knobChoke == null) && !(SatsumaOcs.triggerChoke == null)) {
                    // Check other knobs
                    if(hit.collider.gameObject == SatsumaOcs.triggerChoke) {
                        if(SelectionSelection) {
                            // Color is now green
                            SatsumaOcs.knobChoke.GetComponent<Renderer>().material = SelMaterial;
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
                    SatsumaOcs.knobChoke.GetComponent<Renderer>().material = Material1;
                }

                if(!(SatsumaOcs.knobHazards == null) && !(SatsumaOcs.triggerHazard == null)) {
                    if(hit.collider.gameObject == SatsumaOcs.triggerHazard) {
                        if(SelectionSelection) {
                            SatsumaOcs.knobHazards.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobHazards.GetComponent<Renderer>().material = Material1;
                }

                if(!(SatsumaOcs.knobLights == null) && !(SatsumaOcs.triggerLightModes == null)) {
                    if(hit.collider.gameObject == SatsumaOcs.triggerLightModes) {
                        if(SelectionSelection) {
                            SatsumaOcs.knobLights.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobLights.GetComponent<Renderer>().material = Material1;
                }

                if(!(SatsumaOcs.knobWipers == null) && !(SatsumaOcs.triggerButtonWiper == null)) {
                    if(hit.collider.gameObject == SatsumaOcs.triggerButtonWiper) {
                        if(SelectionSelection) {
                            SatsumaOcs.knobWipers.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobWipers.GetComponent<Renderer>().material = Material1;
                }

                if(!ChangeableWrenchSize) continue;
                bool flag1 = Input.GetKeyDown(KeyCode.U);
                bool flag2 = Input.GetKeyDown(KeyCode.K);

                if (flag1) {
                    FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat("ToolWrenchSize");
                    fsmFloat.Value += 0.1f;
                    if(fsmFloat.Value > 1.5f) fsmFloat.Value = 1.5f;
                    if(fsmFloat.Value < 0.5f) fsmFloat.Value = 0.5f;
                }

                if (!flag2) continue;
                {
                    FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat("ToolWrenchSize");
                    fsmFloat.Value -= 0.1f;
                    if(fsmFloat.Value > 1.5f) fsmFloat.Value = 1.5f;
                    if(fsmFloat.Value < 0.5f) fsmFloat.Value = 0.5f;
                }
            }


            // If we click
            if(Input.GetMouseButtonDown(0)) {
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),
                                 3f))
                        if(!(SatsumaOcs.radioCoreInstance == null)) {
                            switch(hit.collider.gameObject.name) {
                                // Aiming at the power knob
                                // Toggle radio
                                case "trigger_ocs_power1" when SatsumaOcs.radioCoreInstance.@on:
                                    SatsumaOcs.radioCoreInstance.DisableRadio();
                                    break;
                                case "trigger_ocs_power1":
                                    SatsumaOcs.radioCoreInstance.EnableRadio();
                                    break;
                                // Aiming at the switch song knob
                                // Change song
                                case "trigger_ocs_switch1":
                                    SatsumaOcs.radioCoreInstance.NextClip();
                                    break;
                            }
                        }
                }


            }

            if (!ForceHealthModAlwaysFull || !_healthInstalled || _hpStopwatch.ElapsedMilliseconds <= 1000) return;
            _hpStopwatch.Reset();
            _hpStopwatch.Start();
            Health.editHp(100);
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
        public static void PrintF(string text,string module = "SYSTEM",bool console = false) {
            try {
                string modConfigFolder =
                    ModLoader.GetModSettingsFolder(ModLoader.LoadedMods.First(x => x.ID == nameof(OldCarSounds)));
                StreamWriter writer = new StreamWriter(Path.Combine(modConfigFolder,"log.log"),true);
                StringBuilder builder = new StringBuilder().Append(DateTime.Now).Append(" [").Append(module.ToUpper())
                    .Append("]: ").Append(text);
                writer.WriteLine(builder.ToString());
                writer.Close();

                if(console) {
                    switch(module.ToUpper()) {
                        case "ERROR":
                        case "ERR":
                            ModConsole.Error(builder.ToString());
                            break;
                        case "WARN":
                        case "WARNING":
                            ModConsole.Warning(builder.ToString());
                            break;
                        default:
                            ModConsole.Print(builder.ToString());
                            break;
                    }
                }
#if DEBUG
                else {
                    if(module.ToUpper() == "ERROR" || module.ToUpper() == "ERR") {
                        ModConsole.Error(builder.ToString());
                    }
                    else if(module.ToUpper() == "WARN" || module.ToUpper() == "WARNING") {
                        ModConsole.Warning(builder.ToString());
                    }
                    else {
                        ModConsole.Print(builder.ToString());
                    }
                }
#endif
            }
            catch(Exception) {
                // ignored
            }
        }

        public static string GameObjectPath(GameObject go) {
            string s = "";
            GameObject temp1 = go;
            while(true) {
                s = temp1.name + "/" + s;
                if(temp1.transform.parent == null)
                    break;
                temp1 = temp1.transform.parent.gameObject;
            }

            return s;
        }

        #region VARIABLES

        public static Settings AssembleSounds;
        public static Settings OldRadioSongsSettings;
        public static Settings OldDashTexturesSettings;
        public static Settings InfoTextSettings;
        public static Settings DisableKnobSoundsSettings;
        public static Settings DisableDoorSoundsSettings;
        public static Settings DisableFootSoundsSettings;
        public static Settings OldRpmGaugeSettings;
        public static Settings ShiftDelaySelectionSettings;
        public static Settings KeySoundSelectionSettings;
        public static Settings SelectionSelectionSettings;
        public static Settings EngineSoundsTypeSettings;
        public static Settings ChangeableWrenchSizeSettings;
        public static Settings ButtonNoDeath;
        public static Settings OldDelaySettings;
        public static Settings ForceHealthModAlwaysFullSettings;
        public static bool LoadAssembleSound;
        public static bool OldRadioSongs;
        public static bool OldDashTextures;
        public static bool InfoText;
        public static bool DisableKnobSounds;
        public static bool DisableDoorSounds;
        public static bool DisableFootSounds;
        public static bool OldRpmGauge;
        public static bool ChangeableWrenchSize;
        public static int ShiftDelaySelection;
        public static int KeySoundSelection;
        public static bool SelectionSelection;
        public static int EngineSoundsType;
        public static bool OldDelay;
        public static bool ForceHealthModAlwaysFull;

        private bool _healthInstalled = true;

        public static AudioClip Clip1;

        public static AudioClip Clip2;

        public static AudioClip Clip3;

        public static GameObject Radio1, Satsuma;

        public static Material Material1, SelMaterial;

        public static bool LoadGameOnMenu;

        private Stopwatch _stopwatch;
        private Stopwatch _hpStopwatch;
        private Material _noSel;

        #endregion
    }
}
