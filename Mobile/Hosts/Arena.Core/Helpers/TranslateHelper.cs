using Arena.Core.Resources;

namespace Arena.Core.Helpers
{
    // <summary>
    // Helper class to allow other projects to access translated strings.
    // </summary>
    public static class TranslateHelper
    {
        #region Properties

        public static string NewClaimsLabel => AppResources.NewClaimsLabel;
        public static string ReturnToTheMapHintiOS => AppResources.ReturnToTheMapHintiOS;
        public static string ReturnToTheMapHintDroid => AppResources.ReturnToTheMapHintDroid;
        public static string NoClaimsLabel => AppResources.NoClaimsLabel;
        public static string TapOnAPinToSelect => AppResources.TapOnAPinToSelect;

        public static string LatestClaimStatus => AppResources.LatestClaimStatus;

        #endregion Properties
    }
}
