using PE.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace PE.Framework.UWP.Helpers
{
    public class ResourceHelperImpl : IResourceHelper
    {
        public string[] GetResourceList(string dir)
        {
            IReadOnlyList<StorageFile> files = GetAssets(dir)?.Result;
            if (files == null)
            {
                return new string[0];
            }
            List<StorageFile> s = new List<StorageFile>(files);
            string[] res = new string[s.Count];
            for (int i = 0; i < s.Count; i++)
            {
                res[i] = s[i].Name;
            }
            return res;
        }

        private async Task<IReadOnlyList<StorageFile>> GetAssets(string dir)
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = null;
            IReadOnlyList<StorageFile> files = null;
            try
            {
                string name = "Assets\\Sdp\\StoneColor\\"+dir;
                assets = await appInstalledFolder.GetFolderAsync(name).AsTask().ConfigureAwait(false);
                files = await assets.GetFilesAsync().AsTask().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // NOP
            }
            return files;
        }
    }
}
