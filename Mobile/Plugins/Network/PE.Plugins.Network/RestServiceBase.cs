
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MvvmCross;
using PE.Framework.Models;
using PE.Framework.Serialization;
using PE.Plugins.Analytics;
using PE.Plugins.Network.Exceptions;

namespace PE.Plugins.Network
{
    [Preserve(AllMembers = true)]
    public abstract class RestServiceBase
    {
        #region Events

        public event EventHandler AccessDenied;

        #endregion Events

        #region Constants

        public const string MULTIPART_SEPARATOR = "~Separator~";

        public const string ANA_CAT_REST = "REST_CALLS";
        public const string ANA_REST_DENIED = "REST_DENIED";
        public const string ANA_REST_FAIL = "REST_FAILED";

        private const int REST_CLIENT_COUNT = 5;
        private const int TOKEN_EXPIRE_VARIANCE = 100;
#if ECHO_ADDRESS && DEBUG
        private const bool ECHO_ADDRESS = true;
#else
        private const bool ECHO_ADDRESS = false;
#endif


        #endregion Constants

        #region Fields

        protected IAnalyticsService _AnalyticsService;
        protected readonly NetworkConfiguration _Configuration;

        protected readonly string _RegisterPath = "register";
        protected readonly int _Timeout = 5;

        protected static SemaphoreSlim _Mutex = new SemaphoreSlim(1, 1);
        private List<LockedHttpClient> _Clients = new List<LockedHttpClient>();

        protected string _AuthToken = string.Empty;

        #endregion Fields

        #region Constructors

        protected RestServiceBase(NetworkConfiguration configuration)
        {
            _Configuration = configuration;
            _Timeout = (configuration.Timeout == 0) ? 5 : configuration.Timeout;

            if (Mvx.IoCProvider.CanResolve<IAnalyticsService>()) _AnalyticsService = Mvx.IoCProvider.Resolve<IAnalyticsService>();
        }

        #endregion Constructors

        #region Properties

        public bool Reachable { get; private set; }

        public Func<Task<ServiceResult<string>>> SessionRefresh { get; set; }

        public int TokenExpireTime { get; set; }

        #endregion Properties

        #region Operations

        #region Ping

        public virtual async Task<bool> PingAsync()
        {
            try
            {
                var path = (string.IsNullOrEmpty(_Configuration.PingPath)) ? "ping" : _Configuration.PingPath;
                //  ping the API
                var result = await GetAsync<string>(path, false, 0);
                Reachable = true;
                return Reachable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{GetType().Name}.{nameof(PingAsync)}: Ping failed - {ex}");
                return false;
            }
        }

        #endregion Ping

        #region Router and startup

        public void SetBaseUrl(string baseUrl)
        {
            if (baseUrl.EndsWith("/", StringComparison.InvariantCultureIgnoreCase)) baseUrl = baseUrl.TrimEnd(new char[] { '/' });
            //  we don't take this lightly
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(SetBaseUrl)} - Setting base URL to {baseUrl}");
            _Configuration.BaseUrl = baseUrl;
        }

        public void Reset(string authToken = "")
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(Reset)} - Resetting, token is null: {string.IsNullOrEmpty(authToken)} ({_Clients?.Count})");
            if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(Reset)} - TOKEN: {authToken}");

            _AuthToken = authToken;
            //_LocalStorage.Delete(_Configuration.AuthToken);
            if ((_Clients == null) || (_Clients.Count == 0)) return;
            //  clear all clients and start new ones now
            try
            {
                _Mutex.Wait();
                ClearClients();
            }
            finally
            {
                _Mutex.Release();
            }
        }

        #endregion Router and startup

        #region Delete

        public virtual async Task DeleteAsync(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - ADDRESS: {url}");

                var response = await client.DeleteAsync(new Uri(url));
                CheckResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                await DeleteAsync(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount > maxRetries) throw ex;
                await DeleteAsync(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> DeleteAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - ADDRESS: {url}");

                var response = await client.DeleteAsync(new Uri(url));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await DeleteAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await DeleteAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> DeleteAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var payload = Serializer.Serialize(entity);

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - ADDRESS: {url}");

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Content = new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE),
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(url),
                };

                var response = await client.SendAsync(request);

                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await DeleteAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(DeleteAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await DeleteAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        #endregion Delete

        #region Get

        public virtual async Task<TReturn> GetAsync<TReturn>(int maxRetries, string address)
        {
            return await GetAsync<TReturn>(address, maxRetries, 0);
        }

        private async Task<TReturn> GetAsync<TReturn>(string address, int maxRetries, int retryCount)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - ADDRESS: {url}");

                var response = await client.GetAsync(url);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public async Task<TReturn> GetAsync<TReturn>(string address, bool useToken, int maxRetries = 1, int retryCount = 0)
        {
            if (useToken)
            {
                return await GetAsync<TReturn>(address, maxRetries, retryCount);
            }

            HttpClient client = null;
            try
            {
                client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(_Timeout);
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - Created new HttpClient not using token");

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - ADDRESS: {url}");

                var response = await client.GetAsync(url);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(null);
                    throw dex;
                }
                //  try again
                return await GetAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (retryCount >= maxRetries) throw ex;
                return await GetAsync<TReturn>(address, useToken, maxRetries, retryCount + 1);
            }
        }

        public virtual async Task<object> GetAsync(Type returnType, string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - ADDRESS: {url}");

                var response = await client.GetAsync(new Uri(url));
                return await GetResponse(returnType, response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetAsync(returnType, address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetAsync(returnType, address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> GetAsync<TReturn>(string address, Dictionary<string, string> values, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();
                var builder = new StringBuilder(address);
                foreach (var pair in values)
                {
                    builder.Append(string.Format("&{0}={1}", pair.Key, pair.Value));
                }

                string requestStr = builder.ToString();
                var url = $"{_Configuration.BaseUrl}/{builder.ToString()}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - ADDRESS: {url}");

                var response = await client.GetAsync(new Uri(url));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetAsync<TReturn>(address, values, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetAsync<TReturn>(address, values, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<string> GetRawAsync(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetRawAsync)} - ADDRESS: {url}");

                var response = await client.GetAsync(new Uri(url));
                if (!response.IsSuccessStatusCode && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                {
                    throw new DeniedException();
                }
                else if (!response.IsSuccessStatusCode)
                {
                    throw new RestException(response.ReasonPhrase);
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetRawAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetAsync<string>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetRawAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetAsync<string>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public async Task<byte[]> GetBytesAsync(string address, int maxRetries, int retryCount, bool publicUrl = false)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                if (publicUrl)
                {
                    client.DefaultRequestHeaders.Authorization = null;
                    client.HasToken = false;
                }

                var url = $"{(publicUrl ? string.Empty : $"{_Configuration.BaseUrl}/")}{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetBytesAsync)} - ADDRESS: {url}");
                var response = await client.GetByteArrayAsync(url);

                return response;
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetBytesAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetBytesAsync(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetBytesAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetBytesAsync(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<Stream> GetStreamAsync(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetStreamAsync)} - ADDRESS: {url}");
                var response = await client.GetAsync(new Uri(url));
                return await GetResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetStreamAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await GetStreamAsync(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetStreamAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await GetStreamAsync(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        #endregion Get

        #region Post

        public virtual async Task PostAsync(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), new StringContent(string.Empty, Encoding.UTF8, Constants.CONTENT_TYPE));
                CheckResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                await PostAsync(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                await PostAsync<string>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task PostAsync<TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var payload = Serializer.Serialize(entity);
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE));
                CheckResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                await PostAsync<TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                await PostAsync<TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public async Task<TReturn> PostAsync<TReturn, TEntity>(string address, TEntity entity, bool useToken, int maxRetries = 1, int retryCount = 0)
        {
            if (useToken)
            {
                return await PostAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount);
            }

            HttpClient client;
            try
            {
                client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(_Timeout);
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - Created new HttpClient not using token");

                var payload = Serializer.Serialize(entity);
                var url = new Uri(address);
                if (!url.IsAbsoluteUri) url = new Uri($"{_Configuration.BaseUrl}/{address}");
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - ADDRESS: {address}");

                var response = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(null);
                    throw dex;
                }
                //  try again
                return await PostAsync<TReturn, TEntity>(address, entity, useToken, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (retryCount >= maxRetries) throw ex;
                return await PostAsync<TReturn, TEntity>(address, entity, useToken, maxRetries, retryCount + 1);
            }
        }

        public virtual async Task<TReturn> PostAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), new StringContent(string.Empty, Encoding.UTF8, Constants.CONTENT_TYPE));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var payload = Serializer.Serialize(entity);
                var url = new Uri(address, UriKind.Relative);
                if (!url.IsAbsoluteUri) url = new Uri($"{_Configuration.BaseUrl}/{address}"); // will it be false forever?
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - ADDRESS: {address}");

                var response = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostFormMultipartAsync<TReturn>(string address, Dictionary<string, string> formData, string boundary = "", int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create a boundary if necessary
                if (string.IsNullOrEmpty(boundary)) boundary = Guid.NewGuid().ToString();
                //  create content
                var content = new MultipartFormDataContent(boundary);
                if ((formData != null) && (formData.Keys.Count > 0))
                {
                    foreach (var key in formData.Keys)
                    {
                        if (key.Contains(MULTIPART_SEPARATOR))
                        {
                            content.Add(new StringContent(string.Empty));
                        }
                        else
                        {
                            content.Add(new StringContent(formData[key], Encoding.UTF8), key);
                        }
                    }
                }
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), content);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostFormMultipartAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostFormMultipartAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostFormMultipartDataAsync<TReturn>(string address, Dictionary<string, byte[]> formData, string boundary = "", int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create a boundary if necessary
                if (string.IsNullOrEmpty(boundary)) boundary = Guid.NewGuid().ToString();
                //  create content
                var content = new MultipartFormDataContent(boundary);
                if ((formData != null) && (formData.Keys.Count > 0))
                {
                    foreach (var key in formData.Keys)
                    {
                        content.Add(new ByteArrayContent(formData[key]), key);
                    }
                }
                //  make the call
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartDataAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), content);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartDataAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostFormMultipartDataAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartDataAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostFormMultipartDataAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostFormMultipartFileAsync<TReturn>(string address, string key, string fileName, byte[] data, string boundary = "", int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create a boundary if necessary
                if (string.IsNullOrEmpty(boundary)) boundary = Guid.NewGuid().ToString();
                //  create content
                var content = new MultipartFormDataContent(boundary);
                content.Add(new StreamContent(new System.IO.MemoryStream(data)), key, fileName);
                //  make the call
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), content);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostFormMultipartFileAsync<TReturn>(address, key, fileName, data, boundary, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostFormMultipartFileAsync<TReturn>(address, key, fileName, data, boundary, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostFormMultipartFileAndFormAsync<TReturn>(string address, string key, string fileName, Dictionary<string, string> formData, byte[] data,
            object boundary = null, bool useBoundaryLikeGuid = true, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {   //  create a boundary if necessary
                if (boundary == null) boundary = Guid.NewGuid();
                //  create content
                var content = new MultipartFormDataContent(boundary.ToString());

                if (useBoundaryLikeGuid && Guid.TryParse(boundary.ToString(), out _) &&
                    NameValueHeaderValue.TryParse($"boundary={boundary}", out NameValueHeaderValue contentTypeParameter))
                {
                    content.Headers.ContentType.Parameters.Clear();
                    content.Headers.ContentType.Parameters.Add(contentTypeParameter);
                }

                if (data != null)
                {
                    content.Add(new StreamContent(new System.IO.MemoryStream(data)), key, fileName);
                }

                if ((formData != null) && (formData.Keys.Count > 0))
                {
                    foreach (var key2 in formData.Keys)
                    {
                        if (key2.Contains(MULTIPART_SEPARATOR))
                        {
                            content.Add(new StringContent(string.Empty));
                        }
                        else
                        {
                            if (formData[key2] == null)
                            {
                                continue;
                            }

                            content.Add(new StringContent(formData[key2], Encoding.UTF8), key2);
                        }
                    }
                }

                //  make the call
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAndFormAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), content);

                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAndFormAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostFormMultipartFileAndFormAsync<TReturn>(address, key, fileName, formData, data, boundary, useBoundaryLikeGuid, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormMultipartFileAndFormAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostFormMultipartFileAndFormAsync<TReturn>(address, key, fileName, formData, data, boundary, useBoundaryLikeGuid, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PostUrlEncodedAsync<TReturn>(string address, Dictionary<string, string> data, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create content
                var content = new FormUrlEncodedContent(data);
                //  make the call
                client = await GetClient();
                var url = new Uri(address);
                if (!url.IsAbsoluteUri) url = new Uri(string.Format("{0}/{1}", _Configuration.BaseUrl, address));
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedAsync)} - ADDRESS: {address}");

                var result = await client.PostAsync(url, content);
                return await GetResponse<TReturn>(result);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostUrlEncodedAsync<TReturn>(address, data, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostUrlEncodedAsync<TReturn>(address, data, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public async Task<TReturn> PostUrlEncodedNoTokenAsync<TReturn>(string address, Dictionary<string, string> data, int maxRetries = 1, int retryCount = 0)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //  create content
                    var content = new FormUrlEncodedContent(data);
                    //  make the call
                    var url = new Uri(address);
                    if (!url.IsAbsoluteUri) url = new Uri(string.Format("{0}/{1}", _Configuration.BaseUrl, address));
                    if (ECHO_ADDRESS)
                        System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedNoTokenAsync)} - ADDRESS: {address}");

                    var result = await client.PostAsync(url, content);
                    return await GetResponse<TReturn>(result);
                }
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedNoTokenAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(null);
                    throw dex;
                }
                //  try again
                return await PostUrlEncodedNoTokenAsync<TReturn>(address, data, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostUrlEncodedNoTokenAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (retryCount >= maxRetries) throw ex;
                return await PostUrlEncodedNoTokenAsync<TReturn>(address, data, maxRetries, retryCount + 1);
            }
        }

        public virtual async Task<TReturn> PostFormUrlEncodedAsync<TReturn>(string address, Dictionary<string, string> formData, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create content
                var content = new List<KeyValuePair<string, string>>();
                if (formData != null && formData.Keys.Count > 0)
                {
                    foreach (var key in formData.Keys)
                    {
                        if (key.Contains(MULTIPART_SEPARATOR))
                        {
                            content.Add(new KeyValuePair<string, string>(string.Empty, null));
                        }
                        else
                        {
                            content.Add(new KeyValuePair<string, string>(key, formData[key]));
                        }
                    }
                }
                var formUrlEncoded = new FormUrlEncodedContent(content);
                //  make the call
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormUrlEncodedAsync)} - ADDRESS: {url}");

                var response = await client.PostAsync(new Uri(url), formUrlEncoded);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormUrlEncodedAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PostFormUrlEncodedAsync<TReturn>(address, formData, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PostFormUrlEncodedAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PostFormUrlEncodedAsync<TReturn>(address, formData, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        #endregion Post

        #region Put

        public virtual async Task PutAsync(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - ADDRESS: {url}");

                var response = await client.PutAsync(new Uri(url), new StringContent(string.Empty, Encoding.UTF8, Constants.CONTENT_TYPE));
                CheckResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                await PutAsync(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                await PutAsync(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task PutAsync<TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var payload = Serializer.Serialize(entity);
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - ADDRESS: {url}");

                var response = await client.PutAsync(new Uri(url), new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE));
                CheckResponse(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                await PutAsync<TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                await PutAsync<TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PutAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - ADDRESS: {url}");

                var response = await client.PutAsync(new Uri(url), new StringContent(string.Empty, Encoding.UTF8, Constants.CONTENT_TYPE));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PutAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PutAsync<TReturn>(address, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PutAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                client = await GetClient();

                var payload = Serializer.Serialize(entity);
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - ADDRESS: {url}");

                var response = await client.PutAsync(new Uri(url), new StringContent(payload, Encoding.UTF8, Constants.CONTENT_TYPE));
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PutAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PutAsync<TReturn, TEntity>(address, entity, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PutFormMultipartAsync<TReturn>(string address, Dictionary<string, string> formData, string boundary = "", int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create a boundary if necessary
                if (string.IsNullOrEmpty(boundary)) boundary = Guid.NewGuid().ToString();
                //  create content
                var content = new MultipartFormDataContent(boundary);
                if ((formData != null) && (formData.Keys.Count > 0))
                {
                    foreach (var key in formData.Keys)
                    {
                        if (key.Equals(MULTIPART_SEPARATOR))
                        {
                            content.Add(new StringContent(string.Empty));
                        }
                        else
                        {
                            content.Add(new StringContent(formData[key], Encoding.UTF8), key);
                        }
                    }
                }
                //  make the call
                client = await GetClient();
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartAsync)} - ADDRESS: {url}");

                var response = await client.PutAsync(new Uri(url), content);
                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PutFormMultipartAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PutFormMultipartAsync<TReturn>(address, formData, boundary, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        public virtual async Task<TReturn> PutFormMultipartFileAndFormAsync<TReturn>(string address, string key, string fileName, Dictionary<string, string> formData, byte[] data, string boundary = "", int maxRetries = 1, int retryCount = 0)
        {
            LockedHttpClient client = null;
            try
            {
                //  create a boundary if necessary
                if (string.IsNullOrEmpty(boundary)) boundary = Guid.NewGuid().ToString();

                //  create content
                var content = new MultipartFormDataContent(boundary);

                if (data != null)
                {
                    content.Add(new StreamContent(new System.IO.MemoryStream(data)), key, fileName);
                }

                if ((formData != null) && (formData.Keys.Count > 0))
                {
                    foreach (var key2 in formData.Keys)
                    {
                        if (key2.Contains(MULTIPART_SEPARATOR))
                        {
                            content.Add(new StringContent(string.Empty));
                        }
                        else
                        {
                            if (formData[key2] == null)
                            {
                                continue;
                            }

                            content.Add(new StringContent(formData[key2], Encoding.UTF8), key2);
                        }
                    }
                }

                //  make the call
                var url = $"{_Configuration.BaseUrl}/{address}";
                if (ECHO_ADDRESS)
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartFileAndFormAsync)} - ADDRESS: {url}");
                client = await GetClient();
                var response = await client.PutAsync(new Uri(url), content);

                return await GetResponse<TReturn>(response);
            }
            catch (DeniedException dex)
            {
                if ((retryCount >= maxRetries) || !(await TokenRefresh(true)))
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartFileAndFormAsync)} - {address}, Denied Exception ({retryCount + 1}/{maxRetries}): {dex.Message}");
                    HandleDeniedException(client);
                    throw dex;
                }
                //  try again
                return await PutFormMultipartFileAndFormAsync<TReturn>(address, key, fileName, formData, data, boundary, maxRetries, retryCount + 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(PutFormMultipartFileAndFormAsync)} - {address}, Exception ({retryCount + 1}/{maxRetries}): {ex.Message}");
                if (client != null) client.Locked = false;
                if (retryCount >= maxRetries) throw ex;
                return await PutFormMultipartFileAndFormAsync<TReturn>(address, key, fileName, formData, data, boundary, maxRetries, retryCount + 1);
            }
            finally
            {
                if (client != null) client.Locked = false;
            }
        }

        #endregion Put

        #endregion Operations

        #region Private Methods

        protected virtual HttpResponseMessage CheckResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                Task.Run(async () => await TrackFailures(response));
                if ((response.StatusCode == System.Net.HttpStatusCode.Unauthorized) || (response.StatusCode == System.Net.HttpStatusCode.Forbidden))
                {
                    throw new DeniedException();
                }
                else
                {
                    throw new RestException(response.ReasonPhrase);
                }
            }

            return response;
        }

        protected virtual async Task<T> GetResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await TrackFailures(response);
                if ((response.StatusCode == System.Net.HttpStatusCode.Unauthorized) || (response.StatusCode == System.Net.HttpStatusCode.Forbidden))
                {
                    throw new DeniedException();
                }
                else
                {
                    var streamError = await response.Content.ReadAsStringAsync();
                    throw new RestException(System.Net.HttpStatusCode.Conflict, response.ReasonPhrase, streamError);
                }
            }

            //System.Diagnostics.Debug.WriteLine($"==> {response.RequestMessage.RequestUri.ToString()}");

            var stream = await response.Content.ReadAsStringAsync();
            return (string.IsNullOrEmpty(stream)) ? default(T) : Serializer.Deserialize<T>(stream);
        }

        protected virtual async Task<object> GetResponse(Type returnType, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await TrackFailures(response);
                if ((response.StatusCode == System.Net.HttpStatusCode.Unauthorized) || (response.StatusCode == System.Net.HttpStatusCode.Forbidden))
                {
                    throw new DeniedException();
                }
                else
                {
                    throw new RestException(response.ReasonPhrase);
                }
            }

            //System.Diagnostics.Debug.WriteLine($"==> {response.RequestMessage.RequestUri.ToString()}");

            var stream = await response.Content.ReadAsStringAsync();
            Console.WriteLine(stream);
            return (string.IsNullOrEmpty(stream)) ? null : Serializer.Deserialize(returnType, stream);

        }

        protected virtual async Task<Stream> GetResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await TrackFailures(response);
                if ((response.StatusCode == System.Net.HttpStatusCode.Unauthorized) || (response.StatusCode == System.Net.HttpStatusCode.Forbidden))
                {
                    AccessDenied?.Invoke(this, new EventArgs());
                    throw new DeniedException();
                }
                else
                {
                    throw new RestException(response.ReasonPhrase);
                }
            }

            return await response.Content.ReadAsStreamAsync();
        }

        protected virtual void AddToken(LockedHttpClient client)
        {
            //  add token to header for client
            //if (string.IsNullOrEmpty(_AuthToken)) _AuthToken = _LocalStorage.Get<string>(_Configuration.AuthToken);
            //System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(AddToken)} - Adding token: {client.Id}");
            if (!string.IsNullOrEmpty(_AuthToken))
            {
                //  add token to header
                if (client.DefaultRequestHeaders.Contains(_Configuration.AuthToken))
                    client.DefaultRequestHeaders.Remove(_Configuration.AuthToken);
                //  ensure that we always use the most up to date token
                //  add to the header using the same name as it is saved to in storage, eg: Authorization
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_Configuration.AuthToken, _AuthToken);
                client.HasToken = true;
            }
        }

        private async Task<LockedHttpClient> GetClient()
        {
            try
            {
                await _Mutex.WaitAsync();

                //  check if refresh is needed
                if (SessionRefresh != null)
                {
                    var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if ((TokenExpireTime - now) < 70)
                    {
                        _AuthToken = string.Empty;
                        //  token is refresh-able
                        //  if this method fails it should throw some sort of exception to be handled upstream
                        //  a denied exception should indicate that the refresh token is also stale
                        System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetClient)} - Before refresh");
                        var result = await SessionRefresh();
                        if (result.Status == ServiceResultStatus.Success)
                        {
                            _AuthToken = result.Payload;
                            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetClient)} - After refresh");
                            //  retire old clients
                            foreach (var c in _Clients)
                            {
                                c.Refresh = true;
                            }
                        }
                        else
                        {
                            throw new DeniedException();
                        }
                    }
                }

                LockedHttpClient client = null;
                if (_Clients.Count < REST_CLIENT_COUNT)
                {
                    client = CreateClient();
                }
                else
                {
                    client = await GetUsedClientAsync();
                    if (client == null) throw new Exception("Could not get a HttpClient.");
                    client.Locked = true;
                    client.UseCount++;
                    //  add the token if necessary
                    if (!client.HasToken)
                    {
                        AddToken(client);
                    }
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetClient)} - Checkout HttpClient: {client.Id}, used: {client.UseCount}");
                }
                return client;
            }
            finally
            {
                _Mutex.Release();
            }
        }

        private LockedHttpClient CreateClient()
        {
            var client = new LockedHttpClient();
            client.Timeout = TimeSpan.FromSeconds(_Timeout);
            AddToken(client);
            client.Locked = true;
            _Clients.Add(client);
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(CreateClient)} - Created new HttpClient: {client.Id}");
            return client;
        }

        private async Task<LockedHttpClient> GetUsedClientAsync()
        {
            LockedHttpClient client = null;
            while (client == null)
            {
                //  get the first available client
                client = _Clients.Where(c => !c.Locked).OrderBy(c => c.UseCount).FirstOrDefault();
                if (client == null)
                {
                    await Task.Delay(10);
                    continue;
                }
                else if (client.Refresh)
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetUsedClientAsync)} - Stale HttpClient found - removing: {client.Id}");
                    _Clients.Remove(client);
                    client.Dispose();
                    client = CreateClient();
                }
            }
            return client;
        }

        protected async Task<bool> TokenRefresh(bool force)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - Attempting to refresh the token ({force})");
            if (SessionRefresh == null)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - Not possible - this should not happen");
                return false;
            }
            //  one at a time through here please
            await _Mutex.WaitAsync();
            try
            {
                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (force || (TokenExpireTime - now) <= TOKEN_EXPIRE_VARIANCE)
                {
                    _AuthToken = string.Empty;
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - Before refresh");
                    var result = await SessionRefresh();
                    if (result.Status != ServiceResultStatus.Success) throw new Exception("Could not refresh auth token.");
                    _AuthToken = result.Payload;
                    if (ECHO_ADDRESS) System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - TOKEN: {_AuthToken}");
                    //  expire clients
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - Expire HTTP Clients");
                    foreach (var client in _Clients) client.Refresh = true;
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - After refresh");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TokenRefresh)} - Exception: {ex}");
                return false;
            }
            finally
            {
                _Mutex.Release();
            }
        }

        protected async Task TrackFailures(HttpResponseMessage response)
        {
            if (_AnalyticsService == null) return;
            try
            {
                var action = (response.StatusCode == System.Net.HttpStatusCode.Forbidden) ? ANA_REST_DENIED : ANA_REST_FAIL;
                var auth = response.RequestMessage.Headers.Contains(_Configuration.AuthToken) ? response.RequestMessage.Headers.GetValues(_Configuration.AuthToken).FirstOrDefault() : string.Empty;
                var content = null != response.RequestMessage.Content ? await response.RequestMessage.Content.ReadAsStringAsync() : null;
                var label = $"RESPONSE CODE: {response.StatusCode},\r\n URL: {response.RequestMessage.RequestUri.AbsolutePath}, \r\nAUTH: {auth}, \r\nContent: {content}";
                _AnalyticsService.TrackEvent(ANA_CAT_REST, action, label);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(TrackFailures)}: {ex}");
            }
        }

        /// <summary>
        /// Clears all RestClients.
        /// </summary>
        /// <remarks>Never call this method without a lock</remarks>
        private void ClearClients()
        {
            if ((_Clients == null) || (_Clients.Count == 0)) return;
            //  wait for all clients to stop their work
            var stamp = DateTime.Now;
            while (true)
            {
                var inUse = _Clients.FirstOrDefault(c => c.Locked);
                if (inUse == null) break;
                //  need a timeout here incase someone used a client without a unlock in a finally
                if (DateTime.Now.Subtract(stamp).TotalSeconds > _Timeout) break;
                Thread.Sleep(10);
            }

            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(ClearClients)} - Purging HttpClients: {_Clients?.Count}");
            foreach (var client in _Clients)
            {
                client.Dispose();
            }
            _Clients.Clear();
            _Clients = new List<LockedHttpClient>();
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(ClearClients)} - The Purge is complete");
        }

        #endregion Private Methods

        #region Helpers

        private void HandleDeniedException(LockedHttpClient client)
        {
            try
            {
                //  get a lock first
                _Mutex.Wait();
                if (client != null) client.Locked = false;
                //ClearClients();
                AccessDenied?.Invoke(this, new EventArgs());
            }
            finally
            {
                _Mutex.Release();
            }
        }

        #endregion Helpers
    }

    [Preserve(AllMembers = true)]
    public class LockedHttpClient : HttpClient
    {
        static int _Count = 0;

        public LockedHttpClient()
        {
            Id = _Count++;
        }

        public LockedHttpClient(HttpMessageHandler handler)
            : base(handler)
        {
            Id = _Count++;
        }

        public LockedHttpClient(HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
            Id = _Count++;
        }

        public bool Locked = false;

        public int UseCount = 0;

        public int Id = 0;

        public bool Refresh = false;

        public bool HasToken = false;
    }
}
