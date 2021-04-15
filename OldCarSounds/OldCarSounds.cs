using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using MSCLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldCarSounds
{
   public class OldCarSounds : Mod
   {
      public static AudioClip Clip1,
         Clip2,
         Clip3,
         Clip4;

      public static bool IsInMenu,
         LoadAssembleSound,
         OldShiftDelay,
         NoEngineOverRev,
         NoShiftDelay,
         OldRadioSongs,
         OldDashTextures,
         InfoText;

      public static GameObject Satsuma,
         Radio1,
         Radio1Instantiated,
         TriggerHazard,
         TriggerLightModes,
         TriggerButtonWiper,
         TriggerChoke,
         KnobChoke,
         KnobLights,
         KnobHazards,
         KnobWipers;

      public static SoundController SatsumaSoundController;
      public static Drivetrain SatsumaDrivetrain;
      public static AudioSource Radio1AudioSource, Radio1AudioSource1;

      public static GameObject CameraRayCastedGameObject, PowerKnob, VolumeKnob;
      public override string ID => nameof(OldCarSounds); //Your mod ID (unique)
      public override string Name => "Old Car Sounds"; //You mod name
      public override string Author => "MLDKYT"; //Your Username
      public override string Version => "1.2.0"; //Version
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
         IsInMenu = true;

         //Load the config file if it exists.
         if (File.Exists(ModLoader.GetModConfigFolder(this) + "/config.xml"))
         {
            FileStream fs = File.Open(ModLoader.GetModConfigFolder(this) + "/config.xml", FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveClass));
            SaveClass save = (SaveClass) serializer.Deserialize(fs);
            fs.Close();
            LoadAssembleSound = save.loadAssembleSound;
            NoEngineOverRev = save.noEngineOverRev;
            OldShiftDelay = save.oldShiftDelay;
            NoShiftDelay = save.noShiftDelay;
            OldRadioSongs = save.oldRadioSongs;
            OldDashTextures = save.oldDashTextures;
            InfoText = save.infoText;
         }
      }

      public override void OnLoad()
      {
         // Called once, when mod is loading after game is fully loaded
         PrintF("Starting Loading of OldCarSounds...", "load");
         //Because we're in game, disable the GUI.
         IsInMenu = false;

         //Check for the old version of this mod.
         if (ModLoader.IsModPresent("AudioLoader"))
         {
            //If it exists disable the new mod.
            //I am doing this because I don't know how to crash other mods in MSCLoader.
            PrintF("An old mod version was detected. Please remove old mod version and restart the game.", "error",
               true);
            throw new Exception("First remove the old mod and relaunch the game.");
         }

         //Define asset bundle to load it later.
         AssetBundle assetBundle;
         try
         {
            //Try loading asset bundle.
            //If it fails, it is most likely because the user did not copy assets folder to mod folder.
            assetBundle = LoadAssets.LoadBundle(this, "oldsound.unity3d");
         }
         catch (Exception)
         {
            //Inform the user that they have missing assets.
            ModConsole.Error("You probably did not drag the assets folder to the mods! Disabling mod now!");
            PrintF("Missing assets", "error", true);
            throw new MissingComponentException("Completely missing assets file.");
         }

         //Load files from asset bundle
         PrintF("Loaded Asset Bundle (holding the audio files.)", "load");
         Clip1 = assetBundle.LoadAsset("idle_ulko_pako") as AudioClip;
         PrintF("Loaded exhaust sound.", "load");
         Clip2 = assetBundle.LoadAsset("idle_sound") as AudioClip;
         PrintF("Loaded main engine sound.", "load");
         try
         {
            //Test if the user has the 1.2 of the assets file.
            Clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
            if (Clip3 == null) throw new MissingComponentException("Missing assemble sounds. Outdated assets file.");
         }
         catch (Exception)
         {
            //Throw an error if they have the assets file from OldCarSounds 1.1.
            ModConsole.Error("You have an older version of the assets file!");
            ModConsole.Error("Version v1 detected, latest version is v3!");
            PrintF("Outdated version of oldsound.unity3d (assemble.wav) not found", "error", true);
            throw new MissingComponentException("An older version of the assets file was detected.");
         }

         //Test for radio song and object
         try
         {
            //Load
            Radio1 = assetBundle.LoadAsset("oldradio") as GameObject;
            Clip4 = assetBundle.LoadAsset("old_radio_song") as AudioClip;
            if (Radio1 == null) throw new MissingComponentException("Missing radio object. Outdated assets file.");

            if (Clip4 == null) throw new MissingComponentException("Missing radio song. Outdated assets file.");
         }
         catch (Exception)
         {
            //Exception
            ModConsole.Error("You have an older version of the assets file!");
            ModConsole.Error("Version v2 detected, latest version is v3");
            PrintF("Outdated version of oldsound.unity3d (assemble.wav not found)", "error", true);
            throw new MissingComponentException("An older version of the assets file was detected.");
         }

         PrintF("Loaded assemble/disassemble sound.", "load");
         //Get Satsuma.
         Satsuma = GameObject.Find("SATSUMA(557kg, 248)");
         //Get sound controller of satsuma.
         SatsumaSoundController = Satsuma.GetComponent<SoundController>();
         //Change sounds.
         SatsumaSoundController.engineNoThrottle = Clip2;
         SatsumaSoundController.engineThrottle = Clip2;
         //Change the pitches.
         SatsumaSoundController.engineThrottlePitchFactor = 1.47f;
         SatsumaSoundController.engineNoThrottlePitchFactor = 0.7f;
         PrintF("Applied sound effects for engine.", "load");
         //Change more audio clips.
         Satsuma.transform.GetChild(40).GetComponent<AudioSource>().clip = Clip2;
         Satsuma.transform.GetChild(41).GetComponent<AudioSource>().clip = Clip2;
         PrintF("Applied extra sound effects.", "load");
         //Play these audio clips otherwise the sound will be quiet.
         //In unity, when you change the clip, the sound will not autoplay.
         Satsuma.transform.GetChild(40).GetComponent<AudioSource>().Play();
         Satsuma.transform.GetChild(41).GetComponent<AudioSource>().Play();
         PrintF("Loaded in extra sound effects.", "load");
         //Set exhaust sounds.
         Satsuma.transform.Find("CarSimulation/Exhaust/FromMuffler").GetComponent<AudioSource>().clip = Clip1;
         Satsuma.transform.Find("CarSimulation/Exhaust/FromHeaders").GetComponent<AudioSource>().clip = Clip1;
         Satsuma.transform.Find("CarSimulation/Exhaust/FromPipe").GetComponent<AudioSource>().clip = Clip1;
         Satsuma.transform.Find("CarSimulation/Exhaust/FromEngine").GetComponent<AudioSource>().clip = Clip1;
         PrintF("Applied exhaust sound to the exhaust.", "load");
         //Unload asset bundle for memory and performance.
         assetBundle.Unload(false);
         PrintF("Loading audio pitch change...", "load");
         //Get drivetrain.
         SatsumaDrivetrain = Satsuma.GetComponent<Drivetrain>();
         PrintF("Loading old sounds for removing/attaching parts", "load");
         //Only apply assemble sound if the user enabled it.
         if (LoadAssembleSound)
         {
            //Find the container of assemble and disassemble sounds.
            GameObject go1 = GameObject.Find("MasterAudio/CarBuilding");
            //Set the disassemble clip.
            go1.transform.Find("disassemble").GetComponent<AudioSource>().clip = Clip3;
            //Set the assemble clip.
            go1.transform.Find("assemble").GetComponent<AudioSource>().clip = Clip3;
         }

         //Make shift delay old style.
         if (OldShiftDelay)
            //Set the shift delay to the old one (0). 
            SatsumaDrivetrain.shiftTime = 0f;

         //Remove shift delay (Setting to 0 will make it old style)
         if (NoShiftDelay) SatsumaDrivetrain.shiftTime = 0.000000000001f;


         // Load old dash texture  if the user has chosen to
         if (OldDashTextures)
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
            TriggerHazard = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Hazard").gameObject;
            TriggerButtonWiper = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/ButtonWipers").gameObject;
            TriggerChoke = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/Choke").gameObject;
            TriggerLightModes = dashboardMetersClone.transform.Find("Knobs/ButtonsDash/LightModes").gameObject;
            KnobChoke = dashboardMetersClone.transform.Find("Knobs/KnobChoke/knob").gameObject;
            KnobHazards = dashboardMetersClone.transform.Find("Knobs/KnobHazards/knob").gameObject;
            KnobWipers = dashboardMetersClone.transform.Find("Knobs/KnobWasher/knob").gameObject;
            KnobLights = dashboardMetersClone.transform.Find("Knobs/KnobLights/knob").gameObject;
         }

         //Make drivetrain quieter.
         SatsumaSoundController.transmissionVolume = 0.08f;
         SatsumaSoundController.transmissionVolumeReverse = 0.08f;

         //If the user enabled the old radio
         if (OldRadioSongs)
         {
            PrintF("Loading old radio...", "LOAD");
            // Define the current radio for definition later.
            Radio1AudioSource1 = GameObject.Find("RadioChannels/Channel1").GetComponent<AudioSource>();
            // Stop the radio.
            Radio1AudioSource1.Stop();
            // Spawn old car radio
            Radio1Instantiated = Object.Instantiate(Radio1);
            // Add a script to the radio
            RadioCore a = Radio1Instantiated.AddComponent<RadioCore>();
            // Set the clip (global)
            RadioCore.Clip = Clip4;
         }

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
         if (IsInMenu)
         {
            GUI.Box(new Rect(Screen.width - 310, 10, 300, 270), new GUIContent("OldCarSounds 1.3 Beta"));
            LoadAssembleSound = GUI.Toggle(new Rect(Screen.width - 300, 30, 290, 20), LoadAssembleSound,
               "Old assemble sounds");
            NoEngineOverRev = GUI.Toggle(new Rect(Screen.width - 300, 60, 290, 20), NoEngineOverRev,
               "No engine overreving");
            OldShiftDelay = GUI.Toggle(new Rect(Screen.width - 300, 90, 290, 20), OldShiftDelay, "Old shift delay");
            NoShiftDelay = GUI.Toggle(new Rect(Screen.width - 300, 120, 290, 20), NoShiftDelay, "No shift delay");
            NoShiftDelay = (!OldShiftDelay || !NoShiftDelay) && NoShiftDelay;
            OldRadioSongs = GUI.Toggle(new Rect(Screen.width - 300, 150, 290, 20), OldRadioSongs, "Old radio");
            OldDashTextures = GUI.Toggle(new Rect(Screen.width - 300, 180, 290, 20), OldDashTextures, "Old textures");
            InfoText = GUI.Toggle(new Rect(Screen.width - 300, 210, 290, 20), InfoText, "Info");
            if (GUI.Button(new Rect(Screen.width - 300, 240, 290, 25), "Save settings"))
            {
               GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 10, 100, 20), "Saving..");
               SaveClass save = new SaveClass
               {
                  loadAssembleSound = LoadAssembleSound,
                  noEngineOverRev = NoEngineOverRev,
                  noShiftDelay = NoShiftDelay,
                  oldShiftDelay = OldShiftDelay,
                  oldRadioSongs = OldRadioSongs,
                  oldDashTextures = OldDashTextures,
                  infoText = InfoText
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
            if (InfoText)
            {
               GUI.Label(new Rect(10, 10, 1000, 20), $"FPS: {Math.Round(1f / Time.deltaTime, 2)}");
               GUI.Label(new Rect(10, 30, 1000, 20), $"RPM: {Math.Round(SatsumaDrivetrain.rpm, 2)}");
               if (CameraRayCastedGameObject != null)
               {
                  GUI.Label(new Rect(10, 50, Screen.width - 20, 20),
                     $"Looking at: {GameObjectPath(CameraRayCastedGameObject)}");
                  GUI.Label(new Rect(10, 70, Screen.width - 20, 20),
                     $"Looking at tag: {CameraRayCastedGameObject.tag}");
                  GUI.Label(new Rect(10, 90, Screen.width - 20, 20),
                     $"Looking at layer: {LayerMask.LayerToName(CameraRayCastedGameObject.layer)}");
               }
            }
         }
      }


      public override void Update()
      {
         // If No engine overrev is enabled
         if (NoEngineOverRev)
            // Check if the RPM is high
            if (SatsumaDrivetrain.rpm > 8000)
               // Set it slightly under the point.
               SatsumaDrivetrain.rpm = 7500;

         // Keep stopping the normal radio.
         Radio1AudioSource1.Stop();

         // Color buttons green if looking at them
         foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f))
         {
            if (hit.collider.gameObject == PowerKnob)
            {
               // Change color like in old versions
               PowerKnob.GetComponent<Renderer>().material.color = Color.green;
               break;
            }

            // Change color back if we are not aiming at the thing
            // No else statement because it exits the foreach statement
            // if we aim at the power knob
            PowerKnob.GetComponent<Renderer>().material.color = Color.white;

            if (hit.collider.gameObject == VolumeKnob)
            {
               // Change color
               VolumeKnob.GetComponent<Renderer>().material.color = Color.green;
               // Get the scrolling value
               float scroll = Input.GetAxis("Mouse ScrollWheel");
               // If we're scrolling up
               if (scroll > 0.01f)
               {
                  // Raise volume
                  RadioCore.volume += 0.01f;
                  // Debugging
                  PrintF("Volume increased", "radio");
               }

               // If we're scrolling down
               if (scroll < -0.01f)
               {
                  // Lower the volume
                  RadioCore.volume -= 0.01f;
                  // Debugging
                  PrintF("Volume decreased", "radio");
               }

               break;
            }

            // Reset color if we are not looking at the volume knob
            VolumeKnob.GetComponent<Renderer>().material.color = Color.white;
         }


         // If we click
         if (Input.GetMouseButtonDown(0))
            // and we're aiming at the buttons
            foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 3f))
               // Aiming at the power knob
               if (hit.collider.gameObject.name == "trigger_ocs_power1")
               {
                  // Toggle radio
                  RadioCore.@on = !RadioCore.@on;
                  // Debugging
                  PrintF("Toggled radio " + (RadioCore.@on ? "on" : "off"), "radio");
               }
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
         public bool loadAssembleSound;
         public bool noEngineOverRev;
         public bool noShiftDelay;
         public bool oldShiftDelay;
         public bool oldRadioSongs;
         public bool oldDashTextures;
         public bool infoText;
      }
   }
}