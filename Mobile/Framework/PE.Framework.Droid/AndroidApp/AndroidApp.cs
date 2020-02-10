using Android.App;
using Android.Content;
using Java.Lang;

namespace PE.Framework.Droid.AndroidApp.AppVersion
{
	public class AndroidApp : IAndroidApp
	{
		#region Fields

		private Context _context;
        private Activity _activity;
        private Class _activityClass;

		#endregion Fields

		#region Properties

		public Context ApplicationContext
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
            }
        }

		public Activity TopActivity
        {
            get
            {
				return _activity;
            }
            
            set
            {
				_activity = value;
            }
        }
      
        public Class TopActivityClass
        {
            get
            {
                return _activityClass;
            }

            set
            {
                _activityClass = value;
            }
        }
		#endregion Properties

	}
}
