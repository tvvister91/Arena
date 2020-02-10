using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PE.Plugins.OktaAuth
{
    [DataContract]
    public class AuthInfo
    {
        [JsonProperty(PropertyName = "is_authorized")]
        public bool IsAuthorized { get; set; }
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }
    }
}
