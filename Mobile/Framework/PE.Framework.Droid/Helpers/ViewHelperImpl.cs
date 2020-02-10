using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using PE.Framework.Helpers;

namespace PE.Framework.Droid.Helpers
{
	public class ViewHelperImpl : IViewHelper
    {
        double IViewHelper.GetTextViewWidth(string text)
        {
            Rect bounds = new Rect();
            TextView textView = new TextView(Application.Context);
            textView.Paint.GetTextBounds(text, 0, text.Length, bounds);
            var length = bounds.Width();
            return length / Resources.System.DisplayMetrics.ScaledDensity;
        }
    }
}
