using PE.Framework.Serialization;

using System.Linq;

using Xamarin.Auth;

namespace PE.Plugins.SecureStorage
{
    // TODO implement working with Sessions (with hashed Username and additional session details)
    public abstract class SessionServiceBase : ISessionService
    {
        #region Fields

        protected readonly SecureStorageConfiguration _Configuration;

        #endregion Fields

        #region Constructors

        public SessionServiceBase(SecureStorageConfiguration configuration)
        {
            _Configuration = configuration;

            var store = GetStore();
            var account = store.FindAccountsForService(_Configuration.AppName).FirstOrDefault();
            if (account == null) account = new Account();
            store.Save(account, _Configuration.AppName);
        }

        #endregion Constructors

        #region Methods

        public void DeleteAllData(string name)
        {
            var store = GetStore();
            var account = GetAccount(store, name);
            if ((account == null) || !account.Properties.ContainsKey("Password")) return;
            account.Properties.Remove("Password");
            store.Save(account, _Configuration.AppName);
        }

        public bool AccountExists(string name)
        {
            var store = GetStore();
            //  get the account
            var account = GetAccount(store, name);
            return (account != null);
        }

        public void SaveData(string name, string key, string data)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(key)) return;
            var store = GetStore();
            var account = GetAccount(store, name);
            if (account == null) account = new Account(name);
            if (account.Properties.ContainsKey(key))
            {
                if (string.IsNullOrEmpty(data))
                {
                    account.Properties.Remove(key);
                }
                else
                {
                    account.Properties[key] = data;
                }
            }
            else
            {
                account.Properties.Add(key, data);
            }
            store.Save(account, _Configuration.AppName);
        }

        public string GetData(string name, string key)
        {
            var store = GetStore();
            //  get the account
            var account = GetAccount(store, name);
            return ((account == null) || !account.Properties.ContainsKey(key)) ? null : account.Properties[key];
        }

        private Account GetAccount(AccountStore store, string account)
        {
            return store.FindAccountsForService(_Configuration.AppName).Where(a => a.Username.Equals(account)).FirstOrDefault();
        }

        public void Put<TEntity>(string accountName, TEntity entity)
        {
            Put(accountName, typeof(TEntity).Name, entity);
        }

        public void Put<TEntity>(string accountName, string key, TEntity entity)
        {
            //  serialize the entity 
            var data = (entity == null) ? string.Empty : Serializer.Serialize(entity);
            SaveData(accountName, key, data);
        }

        public TEntity Get<TEntity>(string accountName)
        {
            return Get<TEntity>(accountName, typeof(TEntity).Name);
        }

        public TEntity Get<TEntity>(string accountName, string key)
        {
            //  get the data
            var data = GetData(accountName, key);
            return (string.IsNullOrEmpty(data)) ? default(TEntity) : Serializer.Deserialize<TEntity>(data);
        }

        protected abstract AccountStore GetStore();

        #endregion Methods
    }
}
