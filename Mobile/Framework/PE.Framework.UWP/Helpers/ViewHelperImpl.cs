using PE.Framework.Helpers;using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace PE.Framework.UWP.Helpers
{
    public class ViewHelperImpl : IViewHelper
    {
        public double GetTextViewWidth(string text)
        {
            var textBlock = new TextBlock() { FontSize = 12 };
            textBlock.Text = text;
            var parentBorder = new Border { Child = textBlock };
            textBlock.MaxHeight = 50;
            textBlock.MaxWidth = double.PositiveInfinity;
            parentBorder.Measure(new Size(textBlock.MaxWidth, textBlock.MaxHeight));
            parentBorder.Child = null;
            return parentBorder.DesiredSize.Width;
        }
    }
}
