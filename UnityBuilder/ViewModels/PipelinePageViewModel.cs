using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;
using UnityBuilder.Views;

namespace UnityBuilder.ViewModels
{
    public partial class PipelinePageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private NodeState _stateResult = NodeState.Running;
        [ObservableProperty]
        private Node _selectedNode;

        [ObservableProperty]
        private string _selectedNodeId;
        [ObservableProperty]
        private string _selectedNodeOutput;
        [ObservableProperty]
        private bool _isPipelineRun;

        public readonly CancellationTokenSource _cancellationToken;

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

            // make double CTS
            foreach (var node in Nodes)
            {
                node.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(node.CancellationTokenSource.Token, _cancellationToken.Token);
            }
        }

        async public void Start()
        {
            var mainVM = App.Current.Container.Resolve<MainViewModel>();
            mainVM.BuildIsRunning = true;
            IsPipelineRun = true;
            StateResult = await PiplineStartCommand.Execute(Nodes, _cancellationToken.Token);
            mainVM.BuildIsRunning = false;
            IsPipelineRun = false;
        }

    }
}
