using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityBuilder.Commands;
using UnityBuilder.Models;

namespace UnityBuilder.ViewModels
{
    public partial class PipelinePageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isRunning;
        [ObservableProperty]
        private bool _isDone;

        public HashSet<Node> Nodes { get; set; }

        private PagesViewModel _pagesViewModel;
        public PipelinePageViewModel()
        {
            _pagesViewModel = App.Current.Container.Resolve<PagesViewModel>();
        }

        async public Task GenerateNodes()
        {
            Nodes = await PiplineStartCommand.CreateNodes(_pagesViewModel);
        }
    }
}
