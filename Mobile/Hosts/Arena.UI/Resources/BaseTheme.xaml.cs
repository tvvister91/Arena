using Xamarin.Forms;

namespace Arena.UI.Resources
{
	public partial class BaseTheme : ResourceDictionary
    {
        private static BaseTheme _Instance;
        
        public static Color GraySeparator => (Color) _Instance["GraySeparator"];

        public static Color ActiveText => (Color) _Instance["activetext"];

        public static Color SedgwicBlueNav => (Color) _Instance["sedgwicbluenav"];

        public static Color SedgwickGrey4 => (Color) _Instance["sedgwickgrey4"];
         
        public static Color SedgwickGreen => (Color) _Instance["sedgwickgreen"];

        public static Color SedgwickGreenalt => (Color) _Instance["sedgwickgreenalt"];

        public static Color InfoGrayTextColor => (Color) _Instance["InfoGrayTextColor"];

        public static Color InProgressDotColor => (Color) _Instance["InProgressDotColor"];

        public static Color Tomato => (Color)_Instance["tomato"];

        public static Color LightBlueGrey => (Color)_Instance["lightBlueGrey"];

        public BaseTheme()
		{
			InitializeComponent();

            _Instance = this;
        }

        ~BaseTheme()
        {
            _Instance = null;
        }
	}
}