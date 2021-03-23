using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using MSCLoader;
using UnityEngine;

namespace OldCarSounds
{
   public class OldCarSounds : Mod
   {
      public override string ID => "OldCarSounds"; //Your mod ID (unique)
      public override string Name => "Old Car Sounds"; //You mod name
      public override string Author => "MLDKYT"; //Your Username
      public override string Version => "1.2.0"; //Version
      public override bool LoadInMenu => true;

      public static AudioClip Clip1,
         Clip2,
         Clip3,
         OClip1,
         OClip2,
         OClip31,
         OClip32;

      public static float DefaultNoThrottlePitch, DefaultThrottlePitch;

      public static bool DefaultEngine, DefaultAssemble;

      public static Settings SettingsButtonUnloadEngine,
         SettingsButtonLoadEngine,
         SettingsButtonUnloadAssembleSounds,
         SettingsButtonLoadAssembleSounds;

      public static bool IsInMenu, LoadAssembleSound, OldShiftDelay, NoEngineOverRev, NoShiftDelay;

      public static GameObject Satsuma;
      public static SoundController SatsumaSoundController;
      public static Drivetrain SatsumaDrivetrain;

      // Set this to true if you will be load custom assets from Assets folder.
      // This will create subfolder in Assets folder for your mod.
      public override bool UseAssetsFolder => true;

      public override void OnNewGame()
      {
         // Called once, when starting a New Game, you can reset your saves here
      }
      
      [Serializable]
      public struct SaveClass
      {
         public bool LoadAssembleSound;
         public bool NoEngineOverRev;
         public bool NoShiftDelay;
         public bool OldShiftDelay;
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
            LoadAssembleSound = save.LoadAssembleSound;
            NoEngineOverRev = save.NoEngineOverRev;
            OldShiftDelay = save.OldShiftDelay;
            NoShiftDelay = save.NoShiftDelay;
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
            PrintF("An old mod version was detected. Please remove old mod version and restart the game.", "error");
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
            PrintF("Missing assets", "error");
            throw new MissingComponentException("Completely missing assets file.");
         }

         //Load files from asset bundle
         PrintF("Loaded Asset Bundle (holding the audio files.)", "load");
         Clip1 = assetBundle.LoadAsset("idle_ulko_pako.wav") as AudioClip;
         PrintF("Loaded exhaust sound.", "load");
         Clip2 = assetBundle.LoadAsset("idle_sound.wav") as AudioClip;
         PrintF("Loaded main engine sound.", "load");
         try
         {
            //Test if the user has the latest version of the assets file.
            Clip3 = assetBundle.LoadAsset("assemble.wav") as AudioClip;
         }
         catch (Exception)
         {
            //Throw an error if they have the assets file from OldCarSounds 1.1.
            ModConsole.Error("You have an older version of the assets file!");
            ModConsole.Error("Version v1 detected, latest version is v2!");
            PrintF("Outdated version of oldsound.unity3d (assemble.wav) not found", "error");
            throw new MissingComponentException("An older version of the assets file was detected.");
         }
         
         PrintF("Loaded assemble/disassemble sound.", "load");
         //Get Satsuma.
         Satsuma = GameObject.Find("SATSUMA(557kg, 248)");
         //Get sound controller of satsuma.
         SatsumaSoundController = Satsuma.GetComponent<SoundController>();
         //Back up the throttle sound for no reason.
         OClip2 = SatsumaSoundController.engineNoThrottle;
         //Change sounds.
         SatsumaSoundController.engineNoThrottle = Clip2;
         SatsumaSoundController.engineThrottle = Clip2;
         //Back up pitches for no reason.
         DefaultThrottlePitch = SatsumaSoundController.engineThrottlePitchFactor;
         DefaultNoThrottlePitch = SatsumaSoundController.engineNoThrottlePitchFactor;
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
         //Back up exhaust sound.
         OClip1 = Satsuma.transform.Find("CarSimulation/Exhaust/FromMuffler").GetComponent<AudioSource>().clip;
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
            //Back up disassemble clip.
            OClip31 = go1.transform.Find("disassemble").GetComponent<AudioSource>().clip;
            //Set the disassemble clip.
            go1.transform.Find("disassemble").GetComponent<AudioSource>().clip = Clip3;
            //Back up assemble clip.
            OClip32 = go1.transform.Find("assemble").GetComponent<AudioSource>().clip;
            //Set the assemble clip.
            go1.transform.Find("assemble").GetComponent<AudioSource>().clip = Clip3;
         }
         //Make shift delay non existant.
         if (OldShiftDelay)
         {
            //Set the shift delay to the old one (0). 
            SatsumaDrivetrain.shiftTime = 0f;
         }

         if (NoShiftDelay)
         {
            SatsumaDrivetrain.shiftTime = 0.01f;
         }
         //Make drivetrain quieter.
         SatsumaSoundController.transmissionVolume = 0.08f;
         SatsumaSoundController.transmissionVolumeReverse = 0.08f;
         PrintF("Initializing commands...", "load");
         //Initialize command.
         try
         {
            //Will load the command.
            //Will throw an exception if the command is already loaded.
            ConsoleCommand.Add(new OcsCommand());
         }
         catch (Exception)
         {
            // ignored because the command is in most cases already loaded
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
            GUI.Box(new Rect(Screen.width - 310, 10, 300, 180), new GUIContent("OldCarSounds 1.2"));
            LoadAssembleSound = GUI.Toggle(new Rect(Screen.width - 300, 30, 290, 20), LoadAssembleSound, "Old assemble sounds");
            NoEngineOverRev = GUI.Toggle(new Rect(Screen.width - 300, 60, 290, 20), NoEngineOverRev, "No engine overreving");
            OldShiftDelay = GUI.Toggle(new Rect(Screen.width - 300, 90, 290, 20), OldShiftDelay, "Old shift delay");
            NoShiftDelay = GUI.Toggle(new Rect(Screen.width - 300, 120, 290, 20), NoShiftDelay, "No shift delay");
            NoShiftDelay = OldShiftDelay && NoShiftDelay ? false : NoShiftDelay;
            if (GUI.Button(new Rect(Screen.width - 300, 150, 290, 25), "Save settings"))
            {
               SaveClass save = new SaveClass
               {
                  LoadAssembleSound = LoadAssembleSound,
                  NoEngineOverRev = NoEngineOverRev,
                  NoShiftDelay = NoShiftDelay,
                  OldShiftDelay = OldShiftDelay,
               };
               XmlSerializer serializer = new XmlSerializer(typeof(SaveClass));
               if (File.Exists(ModLoader.GetModConfigFolder(this) + "/config.xml")) File.Delete(ModLoader.GetModConfigFolder(this) + "/config.xml");
               FileStream fs = File.Create(ModLoader.GetModConfigFolder(this) + "/config.xml");
               serializer.Serialize(fs, save);
               fs.Close();
            }
         }
      }


      public override void Update()
      {
         //If No engine overrev is enabled
         if (NoEngineOverRev)
         {
            //Check if the RPM is high
            if (SatsumaDrivetrain.rpm > 8000)
            {
               //Set it slightly under the point.
               SatsumaDrivetrain.rpm = 7500;
            }
         }
      }

      /// <summary>
      /// Write to logs.
      /// </summary>
      /// <param name="text">Text.</param>
      /// <param name="module">Where the message is coming from. By default it's SYSTEM.</param>
      /// <exception cref="IOException">Cannot write to logs.</exception>
      private void PrintF(string text, string module = "SYSTEM",
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

            if (console)
            {
               ModConsole.Print(builder.ToString());
            }
#if DEBUG
            ModConsole.Print(builder.ToString());
#endif
         }
         catch (Exception)
         {
            // ignored
         }
      }
   }
}