using System;

namespace PE.Plugins.Dialogs.Enums
{
    [Flags]
    public enum PlatformCode
    {
        None = 0,
        Ios = 1,
        Droid = 2,
        UWP = 3,
        All = Ios | Droid | UWP
    }
}
