using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PE.Framework.Models
{
    public class JwtPayload
    {
        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public List<string> Roles { get; set; }

        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty("token_usage")]
        public string Usage { get; set; }

        [JsonProperty("jti")]
        public string JwtId { get; set; }

        [JsonProperty("scope")]
        public List<string> Scope { get; set; }

        [JsonProperty("aud")]
        public string Audience { get; set; }

        [JsonProperty("nbf")]
        public long NotValidBefore { get; set; }

        [JsonProperty("exp")]
        public int ExpirationTime { get; set; }

        [JsonProperty("iat")]
        public int IssuedAt { get; set; }

        [JsonProperty("iss")]
        public string Issuer { get; set; }

        [JsonProperty("upn")]
        public string Username { get; set; }

        [JsonProperty("unique_name")]
        public string UniqueName { get; set; }

        [JsonProperty("scp")]
        public string Scopes
        {
            get
            {
                if (Scope == null) return string.Empty;
                var builder = new System.Text.StringBuilder();
                foreach (var s in Scope) builder.Append(s);
                return builder.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Scope = null;
                }
                else
                {
                    Scope = new List<string>(value.Split((char)32));
                }
            }
        }

        public DateTimeOffset NotBefore
        {
            get { return (DateTimeOffset.FromUnixTimeSeconds(NotValidBefore)); }
        }

        public DateTimeOffset Expiration
        {
            get { return (DateTimeOffset.FromUnixTimeSeconds(ExpirationTime)); }
        }

        public DateTimeOffset Issued
        {
            get { return (DateTimeOffset.FromUnixTimeSeconds(IssuedAt)); }
        }
    }
}
