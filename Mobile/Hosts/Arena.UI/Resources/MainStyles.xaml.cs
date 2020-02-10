using Xamarin.Forms;

namespace Arena.UI.Resources
{
	public partial class MainStyles : ResourceDictionary
    {
        private static MainStyles _Instance;

        public static Style BaseLabel => _Instance["BaseLabel"] as Style;

        public static Style BodyLabel => _Instance["BodyLabel"] as Style;

        public static Style ClaimDetailsInfoLabel => _Instance["ClaimDetailsInfoLabel"] as Style;

        public static Style OverallTimeLabel => _Instance["OverallTimeLabel"] as Style;

        public MainStyles()
		{
			InitializeComponent();

            _Instance = this;
        }

        ~MainStyles()
        {
            _Instance = null;
        }
	}
}