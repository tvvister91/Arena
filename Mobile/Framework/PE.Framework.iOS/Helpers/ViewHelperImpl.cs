using CoreGraphics;
using PE.Framework.Helpers;
using UIKit;

namespace PE.Framework.iOS.Helpers
{
    public class ViewHelperImpl : IViewHelper
    {
        public double GetTextViewWidth(string text)
        {
            UILabel uiLabel = new UILabel();
            uiLabel.Text = text;
            CGSize length = uiLabel.Text.StringSize(uiLabel.Font);
            return length.Width;
        }
    }
}
