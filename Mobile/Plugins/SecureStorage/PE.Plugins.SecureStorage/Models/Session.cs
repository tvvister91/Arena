namespace PE.Plugins.SecureStorage.Models
{
	//
    // This class is designed to bind User data in DB with Username saved in secured keychain
    //
	public class Session
	{
		#region Constructors

		public Session()
		{ }

		public Session(string username, string password)
		{
			Username = username;
			Password = password;
		}

		#endregion Constructors
      
		#region Properties
		public string Username
		{
			set
			{
				HashedUsername = HashUsername(value);
			}
		}

		public string Password
        {
            set
            {
                HashedPassword = HashPassword(value);
            }
        }

		public string Token { get; set; }

		public string HashedUsername { get; set; }

		public byte[] HashedPassword { get; set; }

		public byte[] passwordSalt { get; private set; }

		#endregion Properties

		#region Methods

		public static string HashUsername(string username)
		{
			return CryptoService.HexHashString(username);
		}

		public bool ValidatePassword(string password)
		{
			var validPassword = CryptoService.VerifyString(password, HashedPassword);
			return validPassword;
		}

		byte[] HashPassword(string password)
        {
            return CryptoService.HashString(password);
        }
		#endregion Methods
	}
}
