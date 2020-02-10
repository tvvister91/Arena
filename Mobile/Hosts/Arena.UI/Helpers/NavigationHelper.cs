using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MvvmCross.ViewModels;
using Xamarin.Forms;

namespace Arena.UI.Helpers
{
    public static class NavigationHelper
    {
        public static INavigation Navigation=> Application.Current.MainPage.Navigation;

        public static IReadOnlyList<Page> NavigationStack => Navigation.NavigationStack;

        public static async Task<Page> GoBack(int pageCount)
        {
            var page = NavigationStack.LastOrDefault();

            if (pageCount < 0 || pageCount > NavigationStack.Count) return page;

            var reversedNavigationStack = NavigationStack.Reverse().ToList();

            for (int i = 1; i < pageCount; i++)
            {
                Navigation.RemovePage(reversedNavigationStack[i]);
            }

            if (pageCount > 0) page = await Navigation.PopAsync();

            if (pageCount == 0 && page.BindingContext is IMvxViewModel viewModel)
            {
                viewModel.ViewAppearing();
            }

            return page;
        }

        public static async Task<Page> GoBack(Page page)
        {
            if (page == null) return NavigationStack.LastOrDefault();

            var pageCount = 0;

            foreach (var p in NavigationStack.Reverse())
            {
                if (p == page) break;
                pageCount++;
            }

            return await GoBack(pageCount);
        }

        public static async Task<Page> GoBack(IMvxViewModel viewModel)
        {
            var page = NavigationStack.LastOrDefault(x => x.BindingContext == viewModel);

            return await GoBack(page);
        }

        /// <summary>
        /// Go back to the LATEST page/binding context with that type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task<Page> GoBack(Type type)
        {
            var page = NavigationStack.LastOrDefault(x => x.GetType() == type || x.BindingContext?.GetType() == type);

            return await GoBack(page);
        }

        /// <summary>
        /// Go back to the LATEST page/binding context with that type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<Page> GoBack<T>()
        {
            return GoBack(typeof(T));
        }
    }
}
