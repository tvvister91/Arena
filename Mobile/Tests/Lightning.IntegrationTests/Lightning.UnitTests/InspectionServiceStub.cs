using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Lightning.Core.Enums;
using Lightning.Core.Model;
using Lightning.Core.Services;
using Lightning.Core.ViewModels.Reports;
using MvvmCross.ViewModels;
using PE.Modules.Sedgwick.Models.Inspection;

namespace Lightning.UnitTests
{
    internal class InspectionServiceStub : IInspectionService
    {
        public MvxObservableCollection<FormItemViewModel> FormsAndCompletness => new MvxObservableCollection<FormItemViewModel>();

        public MvxObservableCollection<OptionItem> ReportItems => new MvxObservableCollection<OptionItem>();

        public MvxObservableCollection<AddableFormItem> AddableFormItems => new MvxObservableCollection<AddableFormItem>();

        public Guid? CurrentFormInputId => default(Guid);

        public Guid? LastInitializedClaimId => default(Guid);

        public string CurrentPageResult => null;

        public string MandatorySelectAddItemTitle => null;

        public Guid? CurrentPageFormOptionIdResult => default(Guid);

        public Guid? CurrentClaimAssetCategoryId => default(Guid);

        public Guid? CurrentClaimInputResultId => default(Guid);

        public ViewItem ViewItem => new ViewItem();

        public OptionItem ParentOptionItem => null;

        public ReportPageType CurrentPageType => default;

        public event EventHandler<(Guid? nodeIdForViewUpdate, ViewItem viewItem)> ViewItemUpdatedForNode;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public Task AddItemToForm(string customName)
        {
            throw new NotImplementedException();
        }

        public Task AddItemToForm(AddableFormItem item)
        {
            throw new NotImplementedException();
        }

        public Task<InspectionView?> CompleteContinueButtonPressed()
        {
            throw new NotImplementedException();
        }

        public Task DeleteNodeWithId(Guid id, bool shouldReinitialize = true)
        {
            throw new NotImplementedException();
        }

        public void GetFormsAndCompletness()
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }

        public Task InitializeForFormInputId(Guid formInputId)
        {
            throw new NotImplementedException();
        }

        public int NumberOfPagesToGoBack()
        {
            throw new NotImplementedException();
        }

        public Task RenameNodeWithId(Guid id, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<InspectionView> SelectForm(Form form)
        {
            throw new NotImplementedException();
        }

        public Task<InspectionView> SelectItem(OptionItem item)
        {
            throw new NotImplementedException();
        }

        public Task SetResultForCurrentPage(string result, Guid claimInputResultId, bool complete = false)
        {
            throw new NotImplementedException();
        }

        public Task ToggleMultiSelectOptionWithId(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}