using PE.Framework.Serialization;
using System;
using System.Text;

namespace PE.Framework.Models
{
    public class Jwt
    {
        #region Constructors

        public Jwt(string token)
        {
            //  token consists of 3 parts
            var parts = token.Split(new char[] { '.' });
            //  header 
            Header = Serializer.Deserialize<JwtHeader>(Encoding.ASCII.GetString(Convert.FromBase64String(ToMod4(parts[0]))));
            //  payload
            Payload = Serializer.Deserialize<JwtPayload>(Encoding.ASCII.GetString(Convert.FromBase64String(ToMod4(parts[1]))));
            //  we don't care much for the other part although it may be useful
            //  TODO: JWT - implement security check
        }

        #endregion Constructors

        #region Properties

        public JwtHeader Header { get; set; }

        public JwtPayload Payload { get; set; }

        #endregion Properties

        #region Methods

        public bool IsValid()
        {
            return (Payload == null) ? false : (DateTimeOffset.Now.ToUniversalTime() >= Payload.NotBefore) && (DateTimeOffset.Now.ToUniversalTime() <= Payload.Expiration);
        }

        #endregion Methods

        #region Private Methods

        private string ToMod4(string base64)
        {
            while (!(base64.Length % 4 == 0))
            {
                base64 += "=";
            }
            return base64;
        }

        #endregion Private Methods
    }
}
