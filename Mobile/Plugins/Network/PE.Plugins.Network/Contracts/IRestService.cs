using PE.Framework.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PE.Plugins.Network.Contracts
{
    public interface IRestService
    {
        #region Events

        event EventHandler AccessDenied;

        #endregion Events

        #region Properties

        bool Reachable { get; }

        Func<Task<ServiceResult<string>>> SessionRefresh { get; set; }

        int TokenExpireTime { get; set; }

        #endregion Properties

        #region Ping

        Task<bool> PingAsync();

        #endregion Ping

        #region Router and startup

        void SetBaseUrl(string baseUrl);

        void Reset(string authToken = null);

        #endregion Router and startup

        #region Delete

        Task DeleteAsync(string address, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> DeleteAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> DeleteAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0);

        #endregion Delete

        #region Get

        //Task<TReturn> GetAsync<TReturn>(string address, int timeoutOverride = 0);

        /// <summary>
        /// GET data from the API
        /// </summary>
        /// <typeparam name="TReturn">The Type to deserialize the returned data into</typeparam>
        /// <param name="maxRetries">Number of times to retry</param>
        /// <param name="address">The address fragment to call</param>
        /// <returns>An object of type TReturn on success or an exception</returns>
        /// <remarks>This method is wrapped in a try/catch block and is thus less optimal</remarks>
        Task<TReturn> GetAsync<TReturn>(int maxRetries, string address);

        Task<TReturn> GetAsync<TReturn>(string address, bool useToken, int maxRetries = 1, int retryCount = 0);

        Task<object> GetAsync(Type returnType, string address, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> GetAsync<TReturn>(string address, Dictionary<string, string> values, int maxRetries = 1, int retryCount = 0);

        Task<string> GetRawAsync(string address, int maxRetries = 1, int retryCount = 0);

        Task<byte[]> GetBytesAsync(string address, int maxRetries, int retryCount, bool publicUrl = false);

        Task<System.IO.Stream> GetStreamAsync(string address, int maxRetries = 1, int retryCount = 0);

        #endregion Get

        #region Post

        Task PostAsync(string address, int maxRetries = 1, int retryCount = 0);

        Task PostAsync<TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostAsync<TReturn, TEntity>(string address, TEntity entity, bool useToken, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostFormMultipartAsync<TReturn>(string address, Dictionary<string, string> formData, string boundary = "", int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostFormMultipartDataAsync<TReturn>(string address, Dictionary<string, byte[]> formData, string boundary = "", int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostFormMultipartFileAsync<TReturn>(string address, string key, string fileName, byte[] data, string boundary = "", int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostUrlEncodedAsync<TReturn>(string address, Dictionary<string, string> data, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostUrlEncodedNoTokenAsync<TReturn>(string address, Dictionary<string, string> data, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostFormUrlEncodedAsync<TReturn>(string address, Dictionary<string, string> formData, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PostFormMultipartFileAndFormAsync<TReturn>(string address, string key, string fileName, Dictionary<string, string> formData, byte[] data, object boundary = null, bool useBoundaryLikeGuid = true, int maxRetries = 1, int retryCount = 0);

        #endregion Post

        #region Put

        Task PutAsync(string address, int maxRetries = 1, int retryCount = 0);

        Task PutAsync<TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PutAsync<TReturn>(string address, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PutAsync<TReturn, TEntity>(string address, TEntity entity, int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PutFormMultipartAsync<TReturn>(string address, Dictionary<string, string> formData, string boundary = "", int maxRetries = 1, int retryCount = 0);

        Task<TReturn> PutFormMultipartFileAndFormAsync<TReturn>(string address, string key, string fileName, Dictionary<string, string> formData, byte[] data, string boundary = "", int maxRetries = 1, int retryCount = 0);

        #endregion Put
    }
}
