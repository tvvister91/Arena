using System.Threading.Tasks;

using MvvmCross.Forms.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using UIKit;

using Arena.Core.Presentation;
using Arena.UI.Helpers;

namespace Arena.iOS
{
    public class CustomPresenter : MvxFormsIosViewPresenter
    {
        public CustomPresenter(IUIApplicationDelegate applicationDelegate, UIWindow window, Xamarin.Forms.Application formsApplication)
            : base(applicationDelegate, window, formsApplication)
        {
        }

        public override async Task<bool> ChangePresentation(MvxPresentationHint hint)
        {
            if (hint is GoBackPresentationHint goBackPresentation)
            {
                if (goBackPresentation.BackPageCount.HasValue)
                {
                    await NavigationHelper.GoBack(goBackPresentation.BackPageCount.Value);
                }
                else
                {
                    await NavigationHelper.GoBack(goBackPresentation.ViewModelType);
                }

                return true;
            }

            return await base.ChangePresentation(hint);
        }
    }
}
