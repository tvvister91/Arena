using System;
using System.IO;

namespace PE.Plugins.LocalStorage.iOS
{
    public class LocalStorageService : LocalStorageServiceBase, ILocalStorageService
    {
        public override string GetPath(string file)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, file);
        }
    }
}