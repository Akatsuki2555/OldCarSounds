using MSCLoader;
using UnityEngine;
using static OldCarSounds.OldCarSounds;

namespace OldCarSounds
{
   public class OcsCommand : ConsoleCommand
   {
      public override string Name => "ocs";

      public override string Help => "ocs <load|unload|l|u> <engine|assemble|e|a>";


      public override void Run(string[] args)
      {
         if (args.Length < 2)
         {
            ModConsole.Print("You have to add more arguments. See help ocs.");
         }
         else
         {
            if (args[0] == "load" || args[0] == "l")
            {
               if (args[1] == "assemble" || args[1] == "a")
               {
                  foreach (var var1 in GameObject.Find("MasterAudio").GetComponentsInChildren<AudioSource>())
                  {
                     if (var1 == null) continue;
                     if (var1.clip == null) continue;
                     if (var1.clip.name == "disassemble")
                     {
                        var1.clip = Clip3;
                     }

                     if (var1.clip.name == "assemble")
                     {
                        var1.clip = Clip3;
                     }
                  }
                  
                  DefaultAssemble = false;
               }
            }

            else if (args[0] == "unload" || args[0] == "u")
            {
               if (args[1] == "assemble" || args[1] == "a")
               {
                  foreach (var var1 in GameObject.Find("MasterAudio").GetComponentsInChildren<AudioSource>())
                  {
                     if (var1 == null) continue;
                     if (var1.clip == null) continue;
                     if (var1.clip.name == "disassemble")
                     {
                        var1.clip = OClip31;
                     }

                     if (var1.clip.name == "assemble")
                     {
                        var1.clip = OClip32;
                     }
                  }
                  
                  DefaultAssemble = true;
               }
            }
         }
      }
   }
}