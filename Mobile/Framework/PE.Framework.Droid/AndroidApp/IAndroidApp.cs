using Android.App;
using Java.Lang;

namespace PE.Framework.Droid.AndroidApp.AppVersion
{
    public interface IAndroidApp
    {
		Activity TopActivity { get; set; }
        Class TopActivityClass { get; set; }
    }
}
