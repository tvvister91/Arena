using System.Collections.Generic;
using System.Net.Http;

namespace PE.Plugins.OktaAuth
{
    public class AuthHelper
    {
        public static FormUrlEncodedContent GetAuthContent(string username, string password)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", Constants.GrantType);
            dict.Add("scope", Constants.Scope);
            dict.Add("username", username);
            dict.Add("password", password);

            return new FormUrlEncodedContent(dict);
        }
    }
}
