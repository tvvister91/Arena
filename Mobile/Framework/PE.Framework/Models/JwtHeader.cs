using Newtonsoft.Json;

namespace PE.Framework.Models
{
    public class JwtHeader
    {
        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        [JsonProperty("kid")]
        public string KeyId { get; set; }

        [JsonProperty("typ")]
        public string Type { get; set; }
    }
}
