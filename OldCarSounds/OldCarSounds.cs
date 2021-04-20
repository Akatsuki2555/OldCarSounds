using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldCarSounds
{
   public class OldCarSounds : Mod
   {
      private static AudioClip _clip1,
         _clip2,
         _clip3;

      private static bool _isInMenu,
         _loadAssembleSound,
         _noEngineOverRev,
         _oldRadioSongs,
         _oldDashTextures,
         _infoText,
         _disableKnobSounds;

      private static int _shiftDelaySelection, _keySoundSelection, _selectionSelection;

      private static GameObject _satsuma,
         _radio1,
         _radio1Instantiated,
         _triggerHazard,
         _triggerLightModes,
         _triggerButtonWiper,
         _triggerChoke,
         _knobChoke,
         _knobLights,
         _knobHazards,
         _knobWipers,
         _carKeysIn,
         _carKeysOut,
         _carKeysSwitch;

      private static SoundController _satsumaSoundController;
      private static Drivetrain _satsumaDrivetrain;
      private static AudioSource _radio1AudioSource1, _dashButtonAudio;
      private static RadioCore _radioCoreInstance;

      public static GameObject PowerKnob, VolumeKnob;

      private Stopwatch _stopwatch;
      public override string ID => nameof(OldCarSounds);
      public override string Name => "Old Car Sounds";
      public override string Author => "MLDKYT";
      public override string Version => "1.2.0";
      public override bool LoadInMenu => true;

      // Set this to true if you will be load custom assets from Assets folder.
      // This will create subfolder in Assets folder for your mod.
      public override bool UseAssetsFolder => true;

      public override void OnNewGame()
      {
         // Called once, when starting a New Game, you can reset your saves here
      }

      public override void OnMenuLoad()
      {
         //Called once, when you load to the main menu
         //Enable the main menu GUI.
         _isInMenu = true;

         //Load the config file if it exists.
         if (File.Exists(ModLoader.GetModConfigFolder(this) + "/config.xml"))
         {
            FileStream fs = File.Open(ModLoader.GetModConfigFolder(this) + "/config.xml", FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveClass));
            SaveClass save = (SaveClass) serializer.Deserialize(fs);
            fs.Close();
            _loadAssembleSound = save.loadAssembleSound;
            _noEngineOverRev = save.noEngineOverRev;
            _oldRadioSongs = save.oldRadioSongs;
            _oldDashTextures = save.oldDashTextures;
            _infoText = save.infoText;
            _disableKnobSounds = save.disableKnobSounds;
            _keySoundSelection = save.keySound;
            _shiftDelaySelection = save.shiftDelay;
            _selectionSelection = save.selectionType;
         }
      }

      public override void OnLoad()
      {
         // Called once, when mod is loading after game is fully loaded
         PrintF("Starting Loading of OldCarSounds...", "load");
         // Because we're in game, disable the GUI.
         _isInMenu = false;


         // Check for the old version of this mod.
         if (ModLoader.IsModPresent("AudioLoader"))
         {
            // If it exists disable the new mod.
            // I am doing this because I don't know how to crash other mods in MSCLoader.
            PrintF("An old mod version was detected. Please remove old mod version and restart the game.", "error",
               true);
            throw new Exception("First remove the old mod and relaunch the game.");
         }

         if (!File.Exists(ModLoader.GetModAssetsFolder(this) + "/oldsound.unity3d"))
            throw new MissingComponentException("Missing assets");
         AssetBundle assetBundle = AssetBundle.CreateFromFile(ModLoader.GetModAssetsFolder(this) + "/oldsound.unity3d");

         _clip1 = assetBundle.LoadAsset("idleulkopako") as AudioClip;
         _clip2 = assetBundle.LoadAsset("idlesound") as AudioClip;
         // Assemble sounds
         if (_loadAssembleSound)
            _clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
         // Music
         if (_oldRadioSongs)
         {
            _radio1 = assetBundle.LoadAsset("oldradio") as GameObject;
            RadioCore.Clips.Add(assetBundle.LoadAsset("mustamies") as AudioClip);
            RadioCore.Clips.Add(assetBundle.LoadAsset("oldradiosong") as AudioClip);
            RadioCore.Clips.Add(assetBundle.LoadAsset("song2") as AudioClip);
            RadioCore.Clips.Add(assetBundle.LoadAsset("song3") as AudioClip);
            RadioCore.Clips.Add(assetBundle.LoadAsset("song4") as AudioClip);
         }

         // Unload the asset bundle to reduce memory usage since we have loaded the audio clips.
         assetBundle.Unload(false);

         PrintF("Loaded Asset Bundle (holding the audio files.)", "load");

         // Get Satsuma.
         _satsuma = GameObject.Find("SATSUMA(557kg, 248)");
         // Get sound controller of satsuma.
         _satsumaSoundController = _satsuma.GetComponent<SoundController>();
         // Change sounds.
         _satsumaSoundController.engineNoThrottle = _clip2;
         _satsumaSoundController.engineThrottle = _clip2;
         // Change the pitches.
         _satsumaSoundController.engineThrottlePitchFactor = 1.47f;
         _satsumaSoundController.engineNoThrottlePitchFactor = 0.6f;
         PrintF("Applied sound effects for engine.", "load");
         // Change more audio clips.
         _satsuma.transform.GetChild(40).GetComponent<AudioSource>().clip = _clip2;
         _satsuma.transform.GetChild(41).GetComponent<AudioSource>().clip = _clip2;
         PrintF("Applied extra sound effects.", "load");
         // Play these audio clips otherwise the sound will be quiet.
         // In Unity, when you change the clip, the sound will not play automatically.
         _satsuma.transform.GetChild(40).GetComponent<AudioSource>().Play();
         _satsuma.transform.GetChild(41).GetComponent<AudioSource>().Play();
         PrintF("Loaded in extra sound effects.", "load");
         // Set exhaust sounds.
         _satsuma.transform.Find("CarSimulation/Exhaust/FromMuffler").GetComponent<AudioSource>().clip = _clip1;
         _satsuma.transform.Find("CarSimulation/Exhaust/FromHeaders").GetComponent<AudioSource>().clip = _clip1;
         _satsuma.transform.Find("CarSimulation/Exhaust/FromPipe").GetComponent<AudioSource>().clip = _clip1;
         _satsuma.transform.Find("CarSimulation/Exhaust/FromEngine").GetComponent<AudioSource>().clip = _clip1;
         PrintF("Applied exhaust sound to the exhaust.", "load");
         PrintF("Loading audio pitch change...", "load");
         //Get drivetrain.
         _satsumaDrivetrain = _satsuma.GetComponent<Drivetrain>();
         PrintF("Loading old sounds for removing/attaching parts", "load");
         //Only apply assemble sound if the user enabled it.
         if (_loadAssembleSound)
         {
            //Find the container of assemble and disassemble sounds.
            GameObject go1 = GameObject.Find("MasterAudio/CarBuilding");
            //Set the disassemble clip.
            go1.transform.Find("disassemble").GetComponent<AudioSource>().clip = _clip3;
            //Set the assemble clip.
            go1.transform.Find("assemble").GetComponent<AudioSource>().clip = _clip3;
         }


         // Load old dash texture  if the user has chosen to
         if (_oldDashTextures)
         {
            PrintF("Loading old car textures...", "LOAD");
            // Create the old reflective material.
            Material theMaterial = new Material(Shader.Find("Diffuse")) {color = new Color(0.15f, 0.15f, 0.15f)};
            // Apply to dashboard.
            GameObject dashboardClone = GameObject.Find("dashboard(Clone)");
            dashboardClone.GetComponent<MeshRenderer>().material = theMaterial;
            // Apply to stock steering wheel.
            GameObject steeringWheelClone = GameObject.Find("stock steering wheel(Clone)");
            steeringWheelClone.GetComponent<MeshRenderer>().material = theMaterial;
            // Apply to dashboard meters.
            GameObject dashboardMetersClone = GameObject.Find("dashboard meters(Clone)");
            dashboardMetersClone.GetComponent<MeshRenderer>().material = theMaterial;
            // Define game object variables for knobs.
            _triggerHazard = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Hazard").gameObject;
            _triggerButtonWiper = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/ButtonWipers").gameObject;
            _triggerChoke = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Choke").gameObject;
            _triggerLightModes = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/LightModes").gameObject;
            theMaterial = new Material(Shader.Find("Diffuse"))
            {
               color = Color.grey
            };
            _knobChoke = dashboardMetersClone.transform.Find("Knobs/KnobChoke/knob").gameObject;
            _knobChoke.GetComponent<Renderer>().material = theMaterial;
            _knobHazards = dashboardMetersClone.transform.Find("Knobs/KnobHazards/knob").gameObject;
            _knobHazards.GetComponent<Renderer>().material = theMaterial;
            _knobWipers = dashboardMetersClone.transform.Find("Knobs/KnobWasher/knob").gameObject;
            _knobWipers.GetComponent<Renderer>().material = theMaterial;
            _knobLights = dashboardMetersClone.transform.Find("Knobs/KnobLights/knob").gameObject;
            _knobLights.GetComponent<Renderer>().material = theMaterial;
         }

         PrintF("Adjusting drivetrain sounds...", "load");
         //Make drivetrain quieter.
         _satsumaSoundController.transmissionVolume = 0.08f;
         _satsumaSoundController.transmissionVolumeReverse = 0.08f;
         PrintF("Adjusting drivetrain done.", "load");

         PrintF("Adjusting shift delay...", "load");
         // Shift delay selection load
         if (_shiftDelaySelection != 0)
         {
            if (_shiftDelaySelection == 1)
               // Old shift delay
               _satsumaDrivetrain.shiftTime = 0;

            if (_shiftDelaySelection == 2)
               // No shift delay
               _satsumaDrivetrain.shiftTime = 0.0000001f;
         }

         PrintF("Adjusting shift delay done.", "load");


         //If the user enabled the old radio
         if (_oldRadioSongs)
         {
            PrintF("Loading old radio...", "LOAD");
            // Define the current radio for definition later.
            PrintF("Defining radio...", "load");
            _radio1AudioSource1 = GameObject.Find("RadioChannels/Channel1").GetComponent<AudioSource>();
            // Stop the radio.
            PrintF("Stopping the radio from playing...", "load");
            _radio1AudioSource1.Stop();
            // Spawn old car radio
            PrintF("Spawning cube radio...", "load");
            _radio1Instantiated = Object.Instantiate(_radio1);
            // Add a script to the radio
            PrintF("Adding script to cube radio...", "load");
            _radioCoreInstance = _radio1Instantiated.AddComponent<RadioCore>();
         }

         PrintF("Finished old radio.", "load");

         // Disable dashboard knob sounds when the user enables it.
         if (_disableKnobSounds)
         {
            PrintF("Disabling knob sounds: Definition...", "load");
            // Define the audio source
            _dashButtonAudio = GameObject.Find("CarFoley/dash_button").GetComponent<AudioSource>();
            PrintF("Disabling knob sounds: Definition done.", "load");
         }

         PrintF("Defining key sounds...", "load");
         // Define the key sounds
         _carKeysIn = GameObject.Find("CarFoley/carkeys_in");
         _carKeysOut = GameObject.Find("CarFoley/carkeys_out");
         _carKeysSwitch = GameObject.Find("CarFoley/ignition_keys");

         // Create a new instance of stopwatch
         _stopwatch = new Stopwatch();
         // Start a stopwatch for the lake time info thing
         _stopwatch.Start();
         PrintF("Fully loaded!", "load", true);
      }

      public override void ModSettings()
      {
      }

      public override void OnSave()
      {
         PrintF("Game is saving...", "save");
      }


      public override void OnGUI()
      {
         //Draw GUI only when in menu.
         if (_isInMenu)
         {
            // The box itself
            GUI.Box(new Rect(Screen.width - 310, 10, 300, 330), new GUIContent("OldCarSounds 1.3"));
            // Old assemble sounds switch
            _loadAssembleSound = GUI.Toggle(new Rect(Screen.width - 300, 30, 290, 20), _loadAssembleSound,
               "Old assemble sounds");
            // No engine overreving switch
            _noEngineOverRev = GUI.Toggle(new Rect(Screen.width - 300, 60, 290, 20), _noEngineOverRev,
               "No engine overreving");
            // Shift delay selection
            if (GUI.Button(new Rect(Screen.width - 300, 95, 20, 20), "<")) _shiftDelaySelection--;
            if (GUI.Button(new Rect(Screen.width - 40, 95, 20, 20), ">")) _shiftDelaySelection++;
            if (_shiftDelaySelection == 0) GUI.Label(new Rect(Screen.width - 270, 90, 230, 20), "Default shift delay");
            if (_shiftDelaySelection == 1) GUI.Label(new Rect(Screen.width - 270, 90, 230, 20), "Old shift delay");
            if (_shiftDelaySelection == 2) GUI.Label(new Rect(Screen.width - 270, 90, 230, 20), "No shift delay");
            if (_shiftDelaySelection > 2) _shiftDelaySelection = 2;
            if (_shiftDelaySelection < 0) _shiftDelaySelection = 0;
            // Old radio songs switch
            _oldRadioSongs = GUI.Toggle(new Rect(Screen.width - 300, 120, 290, 20), _oldRadioSongs, "Old radio");
            // Old dash textures switch
            _oldDashTextures = GUI.Toggle(new Rect(Screen.width - 300, 150, 290, 20), _oldDashTextures, "Old textures");
            // Information text switch
            _infoText = GUI.Toggle(new Rect(Screen.width - 300, 180, 290, 20), _infoText, "Information text");
            // Disable knob sounds switch
            _disableKnobSounds = GUI.Toggle(new Rect(Screen.width - 300, 210, 290, 20), _disableKnobSounds,
               "Disable knob sounds");
            // Key sound selection
            if (GUI.Button(new Rect(Screen.width - 300, 245, 20, 20), "<")) _keySoundSelection--;
            if (GUI.Button(new Rect(Screen.width - 40, 245, 20, 20), ">")) _keySoundSelection++;
            if (_keySoundSelection > 2) _keySoundSelection = 2;
            if (_keySoundSelection < 0) _keySoundSelection = 0;
            if (_keySoundSelection == 0) GUI.Label(new Rect(Screen.width - 270, 240, 230, 20), "Default key sounds");
            if (_keySoundSelection == 1) GUI.Label(new Rect(Screen.width - 270, 240, 230, 20), "Old key sounds (2016)");
            if (_keySoundSelection == 2) GUI.Label(new Rect(Screen.width - 270, 240, 230, 20), "No key sounds");
            // Selection style selection
            if (GUI.Button(new Rect(Screen.width - 300, 275, 20, 20), "<")) _selectionSelection--;
            if (GUI.Button(new Rect(Screen.width - 40, 275, 20, 20), ">")) _selectionSelection++;
            if (_selectionSelection > 2) _selectionSelection = 2;
            if (_selectionSelection < 0) _selectionSelection = 0;
            if (_selectionSelection == 0) GUI.Label(new Rect(Screen.width - 270, 270, 230, 20), "No selection effects");
            if (_selectionSelection == 1)
               GUI.Label(new Rect(Screen.width - 270, 270, 230, 20), "Green box sel. effects");
            if (_selectionSelection == 2)
               GUI.Label(new Rect(Screen.width - 270, 270, 230, 20), "Hand and text sel. effects");

            // Save settings button
            if (GUI.Button(new Rect(Screen.width - 300, 300, 290, 25), "Save settings"))
            {
               GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 10, 100, 20), "Saving..");
               SaveClass save = new SaveClass
               {
                  loadAssembleSound = _loadAssembleSound,
                  noEngineOverRev = _noEngineOverRev,
                  oldRadioSongs = _oldRadioSongs,
                  oldDashTextures = _oldDashTextures,
                  infoText = _infoText,
                  disableKnobSounds = _disableKnobSounds,
                  shiftDelay = _shiftDelaySelection,
                  keySound = _keySoundSelection,
                  selectionType = _selectionSelection
               };
               XmlSerializer serializer = new XmlSerializer(typeof(SaveClass));
               if (File.Exists(ModLoader.GetModConfigFolder(this) + "/config.xml"))
                  File.Delete(ModLoader.GetModConfigFolder(this) + "/config.xml");
               FileStream fs = File.Create(ModLoader.GetModConfigFolder(this) + "/config.xml");
               serializer.Serialize(fs, save);
               fs.Close();
            }
         }
         else
         {
            if (_infoText)
            {
               float fps = 1 / Time.deltaTime;
               GUI.Label(new Rect(0, 0, 250, 20), $"FPS: {fps}");
               GUI.Label(new Rect(0, 20, 250, 20),
                  $"Wrench size: {FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value}");
               GUI.Label(new Rect(0, 40, 250, 20),
                  $"Lake run current time: {_stopwatch.Elapsed.Minutes}m:{_stopwatch.Elapsed.Seconds}s:{_stopwatch.Elapsed.Minutes}ms");
               GUI.Label(new Rect(0, 60, 250, 20), "Lake run last time:");
            }
         }
      }


      public override void Update()
      {
         // If No engine overrev is enabled
         if (_noEngineOverRev)
            // Check if the RPM is high
            if (_satsumaDrivetrain.rpm > 8000)
               // Set it slightly under the point.
               _satsumaDrivetrain.rpm = 7500;

         // Car key sounds
         if (_keySoundSelection != 0)
         {
            // Old key sounds (2016)
            if (_keySoundSelection == 1 || _keySoundSelection == 2)
            {
               // Disable the carkeys_in and carkeys_out sounds
               if (_carKeysIn.GetComponent<AudioSource>().isPlaying)
               {
                  PrintF("Stopped key_in sound", "stop");
                  _carKeysIn.GetComponent<AudioSource>().Stop();
               }

               if (_carKeysOut.GetComponent<AudioSource>().isPlaying)
               {
                  PrintF("Stopped key_out sound", "stop");
                  _carKeysOut.GetComponent<AudioSource>().Stop();
               }
            }

            if (_keySoundSelection == 2)
               if (_carKeysSwitch.GetComponent<AudioSource>().isPlaying)
               {
                  PrintF("Stopped key_switch sound", "stop");
                  _carKeysSwitch.GetComponent<AudioSource>().Stop();
               }
         }


         // Color buttons green if looking at them
         foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f))
         {
            if (hit.collider.gameObject == PowerKnob)
            {
               // Change color like in old versions
               if (_selectionSelection == 1) PowerKnob.GetComponent<Renderer>().material.color = Color.green;

               if (_selectionSelection == 2)
               {
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
               }

               break;
            }

            // Change color back if we are not aiming at the thing
            // No else statement because it exits the foreach statement
            // if we aim at the power knob
            PowerKnob.GetComponent<Renderer>().material.color = Color.white;

            if (hit.collider.gameObject == VolumeKnob)
            {
               // Change color
               if (_selectionSelection == 1) VolumeKnob.GetComponent<Renderer>().material.color = Color.green;

               if (_selectionSelection == 2)
               {
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
               }

               // Get the scrolling value
               float scroll = Input.GetAxis("Mouse ScrollWheel");
               // If we're scrolling up
               if (scroll > 0.01f)
               {
                  // Raise volume
                  _radioCoreInstance.IncreaseVolume();
                  // Debugging
                  PrintF("Volume increased", "radio");
               }

               // If we're scrolling down
               if (scroll < -0.01f)
               {
                  // Lower the volume
                  _radioCoreInstance.DecreaseVolume();
                  // Debugging
                  PrintF("Volume decreased", "radio");
               }

               break;
            }

            // Reset color if we are not looking at the volume knob
            VolumeKnob.GetComponent<Renderer>().material.color = Color.white;

            // Check other knobs
            if (hit.collider.gameObject == _triggerChoke)
            {
               if (_selectionSelection == 1)
               {
                  // Color is now green
                  _knobChoke.GetComponent<Renderer>().material.color = Color.green;
                  // Disable the little subtitle and stuff in center
                  // of the screen.
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                  // This "break" will exit forEach statement.
                  // Not triggering any code under the } bracket
               }

               break;
            }

            // If we don't exit in the last if statement,
            // this will run setting the color back to normal
            _knobChoke.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

            if (hit.collider.gameObject == _triggerHazard)
            {
               if (_selectionSelection == 1)
               {
                  _knobHazards.GetComponent<Renderer>().material.color = Color.green;
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
               }

               break;
            }

            _knobHazards.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

            if (hit.collider.gameObject == _triggerLightModes)
            {
               if (_selectionSelection == 1)
               {
                  _knobLights.GetComponent<Renderer>().material.color = Color.green;
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
               }

               break;
            }

            _knobLights.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

            if (hit.collider.gameObject == _triggerButtonWiper)
            {
               if (_selectionSelection == 1)
               {
                  _knobWipers.GetComponent<Renderer>().material.color = Color.green;
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
               }

               break;
            }

            _knobWipers.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

            if (hit.collider.gameObject.name == "trigger_ocs_switch1")
            {
               if (_selectionSelection == 1)
               {
                  _radio1Instantiated.transform.Find("trigger_ocs_switch1").gameObject.GetComponent<Renderer>().material
                     .color = Color.green;
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
               }

               if (_selectionSelection == 2)
               {
                  FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next track";
                  FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
               }

               break;
            }

            _radio1Instantiated.transform.Find("trigger_ocs_switch1").gameObject.GetComponent<Renderer>().material
               .color = Color.white;
         }


         // If we click
         if (Input.GetMouseButtonDown(0))
            // and we're aiming at the buttons
            foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f))
            {
               // Aiming at the power knob
               if (hit.collider.gameObject.name == "trigger_ocs_power1")
               {
                  // Toggle radio
                  if (_radioCoreInstance.on) _radioCoreInstance.DisableRadio();
                  else _radioCoreInstance.EnableRadio();
               }

               // Aiming at the switch song knob
               if (hit.collider.gameObject.name == "trigger_ocs_switch1")
                  // Change song
                  _radioCoreInstance.NextClip();
            }

         // If the user has chosen to
         if (_disableKnobSounds)
            // Disable the dashboard sound constantly.
            if (_dashButtonAudio.isPlaying)
               _dashButtonAudio.Stop();
      }

      /// <summary>
      ///    Write to logs.
      /// </summary>
      /// <param name="text">Text.</param>
      /// <param name="module">Where the message is coming from. By default it's SYSTEM.</param>
      /// <param name="console">
      ///    If it should be displayed in the console even if it's not
      ///    debugging mode.
      /// </param>
      /// <exception cref="IOException">Cannot write to logs.</exception>
      private void PrintF(
         string text, string module = "SYSTEM",
         bool console = false)
      {
         try
         {
            string modConfigFolder = ModLoader.GetModConfigFolder(this);
            StreamWriter writer =
               new StreamWriter(Path.Combine(modConfigFolder, "log.log"), true);
            StringBuilder builder = new StringBuilder()
               .Append(DateTime.Now)
               .Append(" [")
               .Append(module.ToUpper())
               .Append("]: ")
               .Append(text);
            writer.WriteLine(builder.ToString());
            writer.Close();

            if (console) ModConsole.Print(builder.ToString());
#if DEBUG
            ModConsole.Print(builder.ToString());
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

      [Serializable]
      public struct SaveClass
      {
         public int shiftDelay;
         public int keySound;
         public bool loadAssembleSound;
         public bool noEngineOverRev;
         public bool oldRadioSongs;
         public bool oldDashTextures;
         public bool infoText;
         public bool disableKnobSounds;
         public int selectionType;
      }
   }
}