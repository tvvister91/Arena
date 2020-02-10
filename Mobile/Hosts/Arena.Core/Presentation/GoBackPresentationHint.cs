using System;

using MvvmCross.ViewModels;

namespace Arena.Core.Presentation
{
    public class GoBackPresentationHint : MvxPresentationHint
    {
        public readonly Type ViewModelType;

        public readonly int? BackPageCount;

        public GoBackPresentationHint(Type viewModelType)
        {
            ViewModelType = viewModelType;
        }

        public GoBackPresentationHint(int backPageCount)
        {
            BackPageCount = backPageCount;
        }
    }
}
