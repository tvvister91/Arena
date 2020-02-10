using PE.Framework.Serialization;

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Windows.Security.Cryptography;
using Windows.Storage;
using Xamarin.Essentials;

namespace PE.Plugins.LocalStorage.WindowsCommon
{
    public class LocalStorageService : ILocalStorageService
    {
        #region Fields

        private readonly ApplicationDataContainer _Settings;

        #endregion Fields

        #region Constructors

        public LocalStorageService()
        {
            _Settings = ApplicationData.Current.LocalSettings.CreateContainer("Settings", ApplicationDataCreateDisposition.Always);
        }

        #endregion Constructors

        #region Put

        private async Task<bool> PutAsync<TEntity>(string name, TEntity entity, bool secure)
        {
            try
            {
                var json = Serializer.Serialize(entity);
                if (secure)
                {
                    await SecureStorage.SetAsync(name, json);
                }
                else
                {
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                    var buffer = CryptographicBuffer.ConvertStringToBinary(json, BinaryStringEncoding.Utf8);
                    await FileIO.WriteBufferAsync(file, buffer);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("*** LocalStorageManager.PutAsync - Exception: {0}", ex);
                return false;
            }
        }

        public bool Put<TEntity>(TEntity entity)
        {
            return Put(typeof(TEntity).Name, entity, false);
        }

        public bool Put<TEntity>(TEntity entity, bool secure)
        {
            return Put(typeof(TEntity).Name, entity, secure);
        }

        public bool Put<TEntity>(string name, TEntity entity)
        {
            return Put(name, entity, false);
        }

        public bool Put<TEntity>(string name, TEntity entity, bool secure)
        {
            return Task.Run(async () => await PutAsync(name, entity, secure)).Result;
        }

        public void PutData(string name, byte[] data)
        {
            Task.Run(async () => await PutDataAsync(name, data, false));
        }

        public void PutData(string name, byte[] data, bool secure)
        {
            Task.Run(async () => await PutDataAsync(name, data, secure));
        }

        private async Task PutDataAsync(string name, byte[] data, bool secure)
        {
            try
            {
                if (secure)
                {
                    var s = Encoding.ASCII.GetString(data);
                    await SecureStorage.SetAsync(name, s);
                }
                else
                {
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        await stream.WriteAsync(data, 0, data.Length);
                        await stream.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** LocalStorageService.PutDataAsync - Exception: {0}", ex));
            }
        }

        #endregion Put

        #region Get

        public TEntity Get<TEntity>()
        {
            return Task.Run(async () => await GetAsync<TEntity>(typeof(TEntity).Name, false)).Result;
        }

        public TEntity Get<TEntity>(bool secure)
        {
            return Task.Run(async () => await GetAsync<TEntity>(typeof(TEntity).Name, secure)).Result;
        }

        public TEntity Get<TEntity>(string name)
        {
            return Task.Run(async () => await GetAsync<TEntity>(name, false)).Result;
        }

        public TEntity Get<TEntity>(string name, bool secure)
        {
            return Task.Run(async () => await GetAsync<TEntity>(name, secure)).Result;
        }

        private async Task<TEntity> GetAsync<TEntity>(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    var s = await SecureStorage.GetAsync(name);
                    return (string.IsNullOrEmpty(s)) ? default(TEntity) : Serializer.Deserialize<TEntity>(s);
                }
                else
                {
                    if (!Exists(name)) return default(TEntity);

                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
                    var buffer = await FileIO.ReadBufferAsync(file);
                    var json = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
                    return Serializer.Deserialize<TEntity>(json);
                }
            }
            catch (FileNotFoundException fex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** LocalStorageManager.GetAsync - File Not Found: {0}", name));
                return default(TEntity);
            }
        }

        public byte[] GetData(string name)
        {
            return Task.Run(async () => await GetDataAsync(name, false)).Result;
        }

        public byte[] GetData(string name, bool secure)
        {
            return Task.Run(async () => await GetDataAsync(name, secure)).Result;
        }

        private async Task<byte[]> GetDataAsync(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    var s = await SecureStorage.GetAsync(name);
                    return (string.IsNullOrEmpty(s)) ? null : Encoding.ASCII.GetBytes(s);
                }
                else
                {
                    if (!Exists(name)) return null;
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
                    var buffer = await FileIO.ReadBufferAsync(file);
                    return buffer.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** LocalStorageService.GetData - Exception: {0}", ex));
                return null;
            }
        }

        #endregion Get

        #region Delete

        public void Delete(string name)
        {
            Task.Run(async () => await DeleteAsync(name, false));
        }

        public void Delete(string name, bool secure)
        {
            Task.Run(async () => await DeleteAsync(name, secure));
        }

        private async Task DeleteAsync(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    SecureStorage.Remove(name);
                }
                else
                {
                    if (!Exists(name)) return;
                    //  open the file
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
                    if (file == null) return;
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("LocalStorageService.DeleteAsync - Exception: {0}", ex));
            }
        }

        #endregion Delete

        #region Exists

        public bool Exists(string key)
        {
            try
            {
                var path = $"{ApplicationData.Current.LocalFolder.Path}\\{key}";
                return File.Exists(path);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        #endregion Exists

        #region Operations

        public void Write(string key, string value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public string Read(string key)
        {
            try
            {
                return (string)ApplicationData.Current.LocalSettings.Values[key];
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetPath(string fileName)
        {
            var path = ApplicationData.Current.LocalFolder.Path;
            return (path.EndsWith("\\")) ? string.Format("{0}{1}", path, fileName) : string.Format("{0}\\{1}", path, fileName);
        }

        #endregion Operations
    }
}
