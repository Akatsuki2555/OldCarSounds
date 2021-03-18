using System;
using System.IO;
using System.Text;
using MSCLoader;
using UnityEngine;

namespace OldCarSounds
{
   public class OldCarSounds : Mod
   {
      public override string ID => "OldCarSounds"; //Your mod ID (unique)
      public override string Name => "Old Car Sounds"; //You mod name
      public override string Author => "MLDKYT"; //Your Username
      public override string Version => "0.0.1"; //Version

      private AudioClip _clip1, _clip2, _clip3;

      private GameObject _satsuma;
      private SoundController _satsumaSoundController;
      private Drivetrain _satsumaDrivetrain;

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
         try
         {
            PrintF("Starting Loading of OldCarSounds...", "load");
            AssetBundle assetBundle;
            try
            {
               assetBundle = LoadAssets.LoadBundle(this, "oldsound.unity3d");
            }
            catch (Exception)
            {
               ModConsole.Error("You probably did not drag the assets folder to the mods! Disabling mod now!");
               PrintF("Missing assets", "error");
               throw new MissingComponentException("Completely missing assets file.");
            }

            PrintF("Loaded Asset Bundle (holding the audio files.)", "load");
            _clip1 = assetBundle.LoadAsset("idle_ulko_pako.wav") as AudioClip;
            PrintF("Loaded exhaust sound.", "load");
            _clip2 = assetBundle.LoadAsset("idle_sound.wav") as AudioClip;
            PrintF("Loaded main engine sound.", "load");
            try
            {
               _clip3 = assetBundle.LoadAsset("assemble.wav") as AudioClip;
            }
            catch (Exception)
            {
               ModConsole.Error("You have an older version of the assets file!");
               ModConsole.Error("Version 1 detected, latest version is 2!");
               PrintF("Outdated version of oldsound.unity3d (assemble.wav) not found", "error");
               throw new MissingComponentException("An older version of the assets file was detected.");
            }

            PrintF("Loaded assemble/disassemble sound.", "load");
            _satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            _satsumaSoundController = _satsuma.GetComponent<SoundController>();
            _satsumaSoundController.engineNoThrottle = _clip2;
            _satsumaSoundController.engineThrottle = _clip2;
            PrintF("Applied sound effects for engine.", "load");
            _satsuma.transform.GetChild(40).GetComponent<AudioSource>().clip = _clip2;
            _satsuma.transform.GetChild(41).GetComponent<AudioSource>().clip = _clip2;
            PrintF("Applied extra sound effects.", "load");
            _satsuma.transform.GetChild(40).GetComponent<AudioSource>().Play();
            _satsuma.transform.GetChild(41).GetComponent<AudioSource>().Play();
            PrintF("Loaded in extra sound effects.", "load");
            _satsuma.transform.Find("CarSimulation/Exhaust/FromMuffler").GetComponent<AudioSource>().clip = _clip1;
            _satsuma.transform.Find("CarSimulation/Exhaust/FromHeaders").GetComponent<AudioSource>().clip = _clip1;
            _satsuma.transform.Find("CarSimulation/Exhaust/FromPipe").GetComponent<AudioSource>().clip = _clip1;
            _satsuma.transform.Find("CarSimulation/Exhaust/FromEngine").GetComponent<AudioSource>().clip = _clip1;
            PrintF("Applied exhaust sound to the exhaust.", "load");
            assetBundle.Unload(false);
            PrintF("Small performance boost was applied successfully.", "load");
            PrintF("Loading audio pitch change...", "load");
            _satsumaDrivetrain = _satsuma.GetComponent<Drivetrain>();
            PrintF("Loading old sounds for removing/attaching parts", "load");
            GameObject go1 = GameObject.Find("MasterAudio");
            foreach (var var1 in go1.GetComponentsInChildren<AudioSource>())
            {
               if (var1 == null) continue;
               if (var1.clip == null) continue;
               if (var1.clip.name == "disassemble")
               {
                  var1.clip = _clip3;
                  PrintF("Found 'disassemble.wav' under " + var1.transform.parent.name);
               }

               if (var1.clip.name == "assemble")
               {
                  var1.clip = _clip3;
                  PrintF("Found 'assemble.wav' under " + var1.transform.parent.name);
               }
            }

            PrintF("Fully loaded!", "load");
         }
         catch (Exception e)
         {
            StringBuilder errorBuild = new StringBuilder()
               .AppendLine(e.ToString())
               .AppendLine(e.Message)
               .AppendLine(e.Source)
               .AppendLine(e.StackTrace)
               .AppendLine("Please report this to the mod author via the bugs section of the mod.");
            PrintF(errorBuild.ToString(), "error");
         }
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
      }


      public override void Update()
      {
         try
         {
            if (_satsumaDrivetrain.rpm > 8000)
            {
               // Above 8000 rpm
               _satsumaSoundController.engineThrottlePitchFactor = 1.5f;
               _satsumaSoundController.engineNoThrottlePitchFactor = 0.65f;
            }
            else if (_satsumaDrivetrain.rpm > 6000)
            {
               // Between 6000 and 8000 rpm
               _satsumaSoundController.engineThrottlePitchFactor = 1.45f;
               _satsumaSoundController.engineNoThrottlePitchFactor = 0.7f;
            }
            else if (_satsumaDrivetrain.rpm > 4000)
            {
               // Between 4000 and 6000 rpm
               _satsumaSoundController.engineThrottlePitchFactor = 1.4f;
               _satsumaSoundController.engineNoThrottlePitchFactor = 0.8f;
            }
            else if (_satsumaDrivetrain.rpm > 2000)
            {
               // Between 2000 and 4000 rpm
               _satsumaSoundController.engineThrottlePitchFactor = 1.5f;
               _satsumaSoundController.engineNoThrottlePitchFactor = 1;
            }
            else
            {
               // Under 2000 rpm
               _satsumaSoundController.engineThrottlePitchFactor = 1.6f;
               _satsumaSoundController.engineNoThrottlePitchFactor = 1.1f;
            }
         }
         catch (Exception)
         {
            // ignored
         }
      }

      /// <summary>
      /// Write to logs.
      /// </summary>
      /// <param name="text">Text.</param>
      /// <param name="module">Where the message is coming from. By default it's SYSTEM.</param>
      /// <exception cref="IOException">Cannot write to logs.</exception>
      private void PrintF(string text, string module = "SYSTEM")
      {
         try
         {
            string modConfigFolder = ModLoader.GetModConfigFolder(this);
            StreamWriter writer = new StreamWriter(Path.Combine(modConfigFolder, "log.log"), true);
            StringBuilder builder = new StringBuilder()
               .Append(DateTime.Now)
               .Append(" [")
               .Append(module.ToUpper())
               .Append("]: ")
               .Append(text);
            writer.WriteLine(builder.ToString());
            writer.Close();
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