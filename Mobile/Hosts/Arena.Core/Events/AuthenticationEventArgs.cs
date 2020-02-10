using System;

namespace Arena.Core.Events
{   // TODO: move folder into Infrastructure one
    public class AuthenticationEventArgs : EventArgs
    {
        #region Constructors

        public AuthenticationEventArgs() { } 

        public AuthenticationEventArgs(string userAlias, bool authenticated, bool connectionRestored)
            : base()
        {
            UserAlias = userAlias;
            Authenticated = authenticated;
            ConnectionRestored = connectionRestored;
        }

        #endregion Constructors

        #region Properties

        public string UserAlias { get; set; }

        public bool Authenticated { get; set; }

        public bool ConnectionRestored { get; set; } = false;

        #endregion Properties
    }
}
