using System;

namespace PE.Plugins.SecureStorage
{
    public interface ISessionService
    {
        void DeleteAllData(string name);

        bool AccountExists(string name);

        void SaveData(string name, string key, string data);

        string GetData(string name, string key);

        void Put<TEntity>(string accountName, TEntity entity);

        void Put<TEntity>(string accountName, string key, TEntity entity);

        TEntity Get<TEntity>(string accountName);

        TEntity Get<TEntity>(string accountName, string key);
    }
}
