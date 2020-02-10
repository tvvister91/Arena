using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace Lightning.UnitTests
{
    public class NavigationServiceStub : IMvxNavigationService
    {
        public event BeforeNavigateEventHandler BeforeNavigate;
        public event AfterNavigateEventHandler AfterNavigate;
        public event BeforeCloseEventHandler BeforeClose;
        public event AfterCloseEventHandler AfterClose;
        public event BeforeChangePresentationEventHandler BeforeChangePresentation;
        public event AfterChangePresentationEventHandler AfterChangePresentation;

        public MvxPresentationHint LastPresentationHint { get; private set; }
        public Type LastNavigatedViewModel { get; private set; }

        public Task<bool> CanNavigate(string path)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CanNavigate<TViewModel>() where TViewModel : IMvxViewModel
        {
            return Task.FromResult(true);
        }

        public Task<bool> CanNavigate(Type viewModelType)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangePresentation(MvxPresentationHint hint, CancellationToken cancellationToken = default)
        {
            LastPresentationHint = hint;
            return Task.FromResult(true);
        }

        public Task<bool> Close(IMvxViewModel viewModel, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Close<TResult>(IMvxViewModelResult<TResult> viewModel, TResult result, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Navigate(IMvxViewModel viewModel, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModel.GetType();
            return Task.FromResult(true);
        }

        public Task<bool> Navigate<TParameter>(IMvxViewModel<TParameter> viewModel, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModel.GetType();
            return Task.FromResult(true);
        }

        public Task<TResult> Navigate<TResult>(IMvxViewModelResult<TResult> viewModel, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModel.GetType();
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> Navigate<TParameter, TResult>(IMvxViewModel<TParameter, TResult> viewModel, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModel.GetType();
            return Task.FromResult(default(TResult));
        }

        public Task<bool> Navigate(Type viewModelType, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModelType;
            return Task.FromResult(true);
        }

        public Task<bool> Navigate<TParameter>(Type viewModelType, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModelType;
            return Task.FromResult(true);
        }

        public Task<TResult> Navigate<TResult>(Type viewModelType, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModelType;
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> Navigate<TParameter, TResult>(Type viewModelType, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            LastNavigatedViewModel = viewModelType;
            return Task.FromResult(default(TResult));
        }

        public Task<bool> Navigate(string path, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(bool));
        }

        public Task<bool> Navigate<TParameter>(string path, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(bool));
        }

        public Task<TResult> Navigate<TResult>(string path, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> Navigate<TParameter, TResult>(string path, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TResult));
        }

        public Task<bool> Navigate<TViewModel>(IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default) where TViewModel : IMvxViewModel
        {
            return Task.FromResult(true);
        }

        public Task<bool> Navigate<TViewModel, TParameter>(TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default) where TViewModel : IMvxViewModel<TParameter>
        {
            return Task.FromResult(true);
        }

        public Task<TResult> Navigate<TViewModel, TResult>(IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default) where TViewModel : IMvxViewModelResult<TResult>
        {
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> Navigate<TViewModel, TParameter, TResult>(TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default) where TViewModel : IMvxViewModel<TParameter, TResult>
        {
            return Task.FromResult(default(TResult));
        }
    }
}
