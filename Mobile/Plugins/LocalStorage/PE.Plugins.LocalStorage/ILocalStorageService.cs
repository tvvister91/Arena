namespace PE.Plugins.LocalStorage
{
    public interface ILocalStorageService
    {
        bool Put<TEntity>(TEntity entity);

        bool Put<TEntity>(TEntity entity, bool secure);

        bool Put<TEntity>(string name, TEntity entity);

        bool Put<TEntity>(string name, TEntity entity, bool secure);

        void PutData(string name, byte[] data);

        void PutData(string name, byte[] data, bool secure);

        TEntity Get<TEntity>();

        TEntity Get<TEntity>(bool secure);

        TEntity Get<TEntity>(string name);

        TEntity Get<TEntity>(string name, bool secure);

        byte[] GetData(string name);

        byte[] GetData(string name, bool secure);

        void Delete(string name);

        void Delete(string name, bool secure);

        void Write(string key, string value);

        bool Exists(string key);

        string Read(string key);

        string GetPath(string fileName);
    }
}
