using CommunityToolkit.Mvvm.ComponentModel;

namespace UnityBuilder.ViewModels
{
    public partial class PipelinePageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isRunning;
        [ObservableProperty]
        private bool _isDone;

        private PagesViewModel _pagesViewModel;
        public PipelinePageViewModel()
        {
            _pagesViewModel = App.Current.Container.Resolve<PagesViewModel>();
        }
    }
}
