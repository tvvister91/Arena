using MvvmCross;

using PE.Framework.Serialization;
using PE.Plugins.Network.Contracts;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PE.Plugins.OktaAuth
{
    public class OktaAuthService : IOktaAuthService
    {
        private OktaAuthConfiguration _configuration;
        private IRestService _restService;

        public OktaAuthService(OktaAuthConfiguration configuration)
        {
            _configuration = configuration;
            _restService = Mvx.Resolve<IRestService>();
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var _CredentialBase64 = "MG9hZnBpbHZlNklNTkQ0cjUwaDc6ZVFNV0xzaHAyekVrQXpQdFEyeE54Nmd6dk1CNVJmSzB1TmFJZ2NlcQ==";
                client.DefaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", _CredentialBase64));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://dev-506286.oktapreview.com/oauth2/ausfbduzw4g1FOgr60h7/v1/token");
                FormUrlEncodedContent content = AuthHelper.GetAuthContent(username, password);
                string contentString = await content.ReadAsStringAsync().ConfigureAwait(false);
                request.Content = content;

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await client.SendAsync(request);

                return await GetResponse<AuthInfo>(response);
            }
        }

        private async Task<AuthResponse> GetResponse<AuthInfo>(HttpResponseMessage response)
        {
            AuthResponse authResponse = new AuthResponse();

            if (!response.IsSuccessStatusCode && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
            {
                authResponse.Success = false;
            }
            else if (!response.IsSuccessStatusCode)
            {
                authResponse.Success = false;
            }
            else
            {
                authResponse.Success = true;
                string stream = await response.Content.ReadAsStringAsync();
                PE.Plugins.OktaAuth.AuthInfo authInfo = Serializer.Deserialize<PE.Plugins.OktaAuth.AuthInfo>(stream);
                authInfo.IsAuthorized = true;
                authResponse.AuthInfo = authInfo;
            }

            return authResponse;
        }
    }
}
