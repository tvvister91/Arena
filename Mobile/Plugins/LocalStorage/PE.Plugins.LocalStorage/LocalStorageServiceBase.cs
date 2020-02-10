using PE.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace PE.Plugins.LocalStorage
{
    public abstract class LocalStorageServiceBase
    {
        #region Constants

        protected const string SETTINGS = "Settings";
        protected const string SECURE_PATH = "Secure";

        #endregion Constants

        #region Fields

        protected Dictionary<string, string> _Settings;

        #endregion Fields

        #region Constructors

        public LocalStorageServiceBase()
        {
        }

        #endregion Constructors

        #region Put

        public virtual bool Put<TEntity>(TEntity entity)
        {
            return Put(typeof(TEntity).Name, entity, false);
        }

        public virtual bool Put<TEntity>(TEntity entity, bool secure)
        {
            return Put(typeof(TEntity).Name, entity, secure);
        }

        public virtual bool Put<TEntity>(string name, TEntity entity)
        {
            return Put(name, entity, false);
        }

        public virtual bool Put<TEntity>(string name, TEntity entity, bool secure)
        {
            var data = Serializer.Serialize(entity);
            if (secure)
            {
                Task.Run(async () => await SecureStorage.SetAsync(name, data)).Wait();
            }
            else
            {
                if (_Settings == null)
                {
                    Initialize();
                }

                var b = System.Text.Encoding.UTF8.GetBytes(data);
                name = GetPath(name);
                if (File.Exists(name))
                {
                    File.Delete(name);
                }

                using (var stream = File.Open(name, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.Write(b, 0, b.Length);
                }
            }

            return true;
        }

        public void PutData(string name, byte[] data)
        {
            PutData(name, data, false);
        }

        public void PutData(string name, byte[] data, bool secure)
        {
            try
            {
                if (secure)
                {
                    using (var sr = new StringWriter())
                    {
                        sr.Write(data);
                        sr.Flush();
                        //  TODO: Test - this needs testing
                        Task.Run(async () => await SecureStorage.SetAsync(name, sr.GetStringBuilder().ToString())).Wait();
                    }
                }
                else
                {
                    name = GetPath(name);

                    using (var stream = File.Open(name, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.PutData - Exception: {ex}");
            }
        }

        #endregion Put

        #region Get

        public virtual TEntity Get<TEntity>()
        {
            return Get<TEntity>(typeof(TEntity).Name, false);
        }
        public virtual TEntity Get<TEntity>(bool secure)
        {
            return Get<TEntity>(typeof(TEntity).Name, secure);
        }

        public virtual TEntity Get<TEntity>(string name)
        {
            return Get<TEntity>(name, false);
        }

        public virtual TEntity Get<TEntity>(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    //  get the data
                    var data = Task.Run(async () => await SecureStorage.GetAsync(name)).Result;
                    return (string.IsNullOrEmpty(data)) ? default(TEntity) : Serializer.Deserialize<TEntity>(data);
                }
                else
                {
                    //  open the file
                    name = GetPath(name);
                    if (!File.Exists(name)) return default(TEntity);

                    using (Stream stream = File.Open(name, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            while (true)
                            {
                                byte[] d = new byte[1024];
                                int result = stream.Read(d, 0, 1024);
                                ms.Write(d, 0, result);
                                if (result < 1024) break;
                            }
                            //  deserialize
                            ms.Seek(0, SeekOrigin.Begin);
                            byte[] data = ms.ToArray();
                            return Serializer.Deserialize<TEntity>(System.Text.Encoding.UTF8.GetString(data, 0, data.Length));
                        }
                    }
                }
            }
            catch (FileNotFoundException fex)
            {
                return default(TEntity);
            }
        }

        public virtual byte[] GetData(string name)
        {
            return GetData(name, false);
        }

        public virtual byte[] GetData(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    //  get the data
                    var data = Task.Run(async () => await SecureStorage.GetAsync(name)).Result;
                    return (string.IsNullOrEmpty(data)) ? null : Encoding.ASCII.GetBytes(data);
                }
                else
                {
                    //  open the file
                    name = GetPath(name);
                    if (!File.Exists(name)) return null;

                    using (Stream stream = File.Open(name, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            while (true)
                            {
                                byte[] d = new byte[1024];
                                int result = stream.Read(d, 0, 1024);
                                ms.Write(d, 0, result);
                                if (result < 1024) break;
                            }
                            //  deserialize
                            ms.Seek(0, SeekOrigin.Begin);
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** LocalStorageServiceBase.GetData - Exception: {0}", ex));
                return null;
            }
        }

        #endregion Get

        #region Delete

        public virtual void Delete(string name)
        {
            Delete(name, false);
        }

        public virtual void Delete(string name, bool secure)
        {
            try
            {
                if (secure)
                {
                    SecureStorage.Remove(name);
                }
                else
                {
                    if (_Settings == null)
                    {
                        Initialize();
                    }

                    name = GetPath(name);
                    if (File.Exists(name)) File.Delete(name);
                }
            }
            catch { /* do nothing */ }
        }

        #endregion Delete

        #region Exists

        public virtual bool Exists(string key)
        {
            if (_Settings == null)
            {
                Initialize();
            }

            try
            {
                //  open the file
                key = GetPath(key);
                return (!File.Exists(key)) ? false : true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        #endregion Exists

        #region Other Operations

        public virtual void Write(string key, string value)
        {
            if (_Settings == null)
            {
                Initialize();
            }

            if (_Settings.ContainsKey(key))
                _Settings[key] = value;
            else
                _Settings.Add(key, value);
        }

        public virtual string Read(string key)
        {
            if (_Settings == null)
            {
                Initialize();
            }

            //  check if the setting exists
            return (_Settings.ContainsKey(key)) ? _Settings[key] : string.Empty;
        }

        public abstract string GetPath(string file);

        #endregion Other Operations

        #region Private Methods

        protected virtual void Initialize()
        {
            _Settings = Get<Dictionary<string, string>>(SETTINGS);
            if (_Settings == null) _Settings = new Dictionary<string, string>();
        }
        #endregion Private Methods
    }
}
