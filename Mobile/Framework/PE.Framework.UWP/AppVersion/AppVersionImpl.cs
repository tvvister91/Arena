using PE.Framework.AppVersion;
using Windows.ApplicationModel;
using Windows.Devices.Input;

namespace PE.Framework.UWP.AppVersion
{
    public class AppVersionImpl : IVersion
    {
        public string Version
        {
            get
            {
                string result;
                try
                {
                    PackageVersion pv = Package.Current.Id.Version;
                    result = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";
                }
                catch
                {
                    result = string.Empty;
                }
                return result;
            }
        }

        public string Platform
        {
            get
            {
                TouchCapabilities touchCapabilities = new Windows.Devices.Input.TouchCapabilities();
                string platform = touchCapabilities.TouchPresent != 0 ? "UWP_Tablet" : "UWP_Desktop";
                return platform;
            }
        }
    }
}
