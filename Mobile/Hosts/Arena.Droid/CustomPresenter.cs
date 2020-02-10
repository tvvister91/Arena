using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using MvvmCross.Forms.Platforms.Android.Presenters;
using MvvmCross.ViewModels;
using Xamarin.Forms;

using Arena.Core.Presentation;
using Arena.UI.Helpers;

namespace Arena.Droid
{
    public class CustomPresenter : MvxFormsAndroidViewPresenter
	{
        public CustomPresenter(IEnumerable<Assembly> androidViewAssemblies, Application formsApplication)
            : base(androidViewAssemblies, formsApplication)
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
