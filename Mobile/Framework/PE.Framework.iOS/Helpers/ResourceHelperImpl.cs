using System;
using System.IO;
using Foundation;
using PE.Framework.Helpers;

namespace PE.Framework.iOS.Helpers
{
    public class ResourceHelperImpl : IResourceHelper
    {
        private string path = "";
        private NSError nSError;
        //private NSError * error;
        //private NSArray *  directoryContents;

        public ResourceHelperImpl()
        {
            path = Path.Combine(NSBundle.MainBundle.ResourcePath, "Sdp/StoneColor/");
        }

        public string[] GetResourceList(string dir)
        {
            string[] result;
            try
            {
                result = NSFileManager.DefaultManager.GetDirectoryContentRecursive(path+"/"+dir, out nSError);

                int i = 0;
                i++;
            }
            catch
            {
                result = new string[0];
            }
            return result;
        }
    }
}
