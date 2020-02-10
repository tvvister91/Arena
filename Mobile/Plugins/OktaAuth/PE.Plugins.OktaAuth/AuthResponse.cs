namespace PE.Plugins.OktaAuth
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public bool TwoFa { get; set; }
        public string ErrorMessage { get; set; }
        public AuthInfo AuthInfo { get; set; }
    }
}
