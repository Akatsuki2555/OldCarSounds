using System.Globalization;
using System.Resources;

namespace OldCarSounds
{
   internal class Storage
   {
      public static ResourceManager ResourceManager
      {
         get
         {
            const string baseName = "OldCarSounds.Storage";
            return new ResourceManager(baseName, typeof(Storage).Assembly);
         }
      }

      public static byte[] AssetBundle1 => ResourceManager.GetObject("ocs1", CultureInfo.CurrentCulture) as byte[];
   }
}