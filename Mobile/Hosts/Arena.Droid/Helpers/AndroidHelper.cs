using Android.OS;

namespace Arena.Droid.Helpers
{
    public static class AndroidHelper
    {
		private static bool? _IsLollypopOrNewer;

        public static bool IsLollypopOrNewer
		{
            get
            {
                if (!_IsLollypopOrNewer.HasValue)
                {
                    _IsLollypopOrNewer = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
                }

                return _IsLollypopOrNewer.Value;
            }
		}
	}
}

