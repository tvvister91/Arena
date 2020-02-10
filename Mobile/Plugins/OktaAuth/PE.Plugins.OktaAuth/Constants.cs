namespace PE.Plugins.OktaAuth
{
    public class Constants
    {
        public const string AuthStateKey = "authState";
        public const string AuthServiceDiscoveryKey = "authServiceDiscovery";

        public const string ClientId = "0oafpilve6IMND4r50h7";
        public const string RedirectUri = "com.oktapreview.dev-506286:/callback";
        public const string OrgUrl = "https://dev-506286.oktapreview.com";
        public const string AuthorizationServerId = "default";

        public static readonly string DiscoveryEndpoint =
            $"{OrgUrl}/oauth2/{AuthorizationServerId}/.well-known/openid-configuration";


        public static readonly string[] Scopes = new string[] {
        "openid", "profile", "email", "offline_access" };

        public const string GrantType = "password";
        public const string Scope = "openid";
    }
}
