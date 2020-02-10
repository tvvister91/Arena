using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PE.Plugins.Network.ClientManager
{
    public class LockedHttpClientManager
    {
        #region Fields

        protected static SemaphoreSlim _Mutex = new SemaphoreSlim(1, 1);
        private List<LockedHttpClient> _Clients = new List<LockedHttpClient>();

        #endregion Fields

        #region Constructors

        public LockedHttpClientManager()
        {
        }

        public LockedHttpClientManager(int timeout, int poolSize)
        {
            Timeout = timeout;
            PoolSize = poolSize;
        }

        #endregion Constructors

        #region Properties

        public int Timeout { get; set; } = 5;

        public int PoolSize { get; set; } = 5;

        #endregion Properties

        #region Operations

        public async Task<LockedHttpClient> CheckOut()
        {
            try
            {
                await _Mutex.WaitAsync();

                LockedHttpClient client = null;
                if (_Clients.Count < PoolSize)
                {
                    client = CreateClient();
                }
                else
                {
                    client = await GetUsedClientAsync();
                    if (client == null) throw new Exception("Could not get a HttpClient.");
                    client.Locked = true;
                    client.UseCount++;
                    //  client is stale - retire
                    if (client.Refresh)
                    {
                        System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckOut)} - Client is state, refreshing: {client.Id}, used: {client.UseCount}");
                        _Clients.Remove(client);
                        client.Dispose();
                        client = CreateClient();
                    }
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckOut)} - Checkout HttpClient: {client.Id}, used: {client.UseCount}");
                }
                return client;
            }
            finally
            {
                _Mutex.Release();
            }
        }

        public void Checkin(LockedHttpClient client)
        {
            client.Locked = false;
        }

        private LockedHttpClient CreateClient()
        {
            var client = new LockedHttpClient();
            client.Timeout = TimeSpan.FromSeconds(Timeout);
            client.Locked = true;
            _Clients.Add(client);
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckOut)} - Created new HttpClient: {client.Id}");
            return client;
        }

        private async Task<LockedHttpClient> GetUsedClientAsync()
        {
            LockedHttpClient client = null;
            while (client == null)
            {
                //  get the first available client
                client = _Clients.FirstOrDefault(c => !c.Locked);
                if (client == null)
                {
                    await Task.Delay(10);
                    continue;
                }
            }
            return client;
        }

        /// <summary>
        /// Clears all RestClients.
        /// </summary>
        /// <remarks>Never call this methods without a lock</remarks>
        public void Reset()
        {
            try
            {
                _Mutex.Wait();
                //  wait for all clients to become available
                var stamp = DateTime.Now;
                while (true)
                {
                    var notReady = _Clients.FirstOrDefault(c => c.Locked);
                    if (notReady == null) break;
                    //  we need a timeout here in case someone uses a client without a finally
                    if (DateTime.Now.Subtract(stamp).TotalSeconds > Timeout) break;
                }

                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(Reset)} - Purging HttpClients: {_Clients?.Count}");
                foreach (var client in _Clients)
                {
                    client.Dispose();
                }
                _Clients.Clear();
                _Clients = new List<LockedHttpClient>();
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(Reset)} - The Purge is complete");
            }
            finally
            {
                _Mutex.Release();
            }
        }

        #endregion Operations
    }
}
