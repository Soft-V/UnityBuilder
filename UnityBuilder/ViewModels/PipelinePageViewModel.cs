using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading;
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
        [ObservableProperty]
        private Node _selectedNode;

        [ObservableProperty]
        private string _selectedNodeId;
        [ObservableProperty]
        private string _selectedNodeOutput;

        private CancellationTokenSource _cancellationToken;

        public HashSet<Node> Nodes { get; set; }

        private PagesViewModel _pagesViewModel;
        public PipelinePageViewModel()
        {
            _cancellationToken = new CancellationTokenSource();
            _pagesViewModel = App.Current.Container.Resolve<PagesViewModel>();
        }

        async public Task GenerateNodes()
        {
            Nodes = await PiplineStartCommand.CreateNodes(_pagesViewModel);
        }

        async public void Start()
        {
            // await PiplineStartCommand.Execute(Nodes, _cancellationToken.Token);
        }
    }
}
