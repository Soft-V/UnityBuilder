using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
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

        [ObservableProperty]
        private string _workingTime = "-";
        private Timer _timer;
        private Stopwatch _workingStopwatch;

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
            _workingStopwatch = new Stopwatch();
            _workingStopwatch.Start();
            _timer = new Timer(WorkingTimeCallback, null, 0, 100);

            var mainVM = App.Current.Container.Resolve<MainViewModel>();
            mainVM.BuildIsRunning = true;
            IsPipelineRun = true;
            StateResult = await PiplineStartCommand.Execute(Nodes, _cancellationToken.Token);
            mainVM.BuildIsRunning = false;
            IsPipelineRun = false;

            _workingStopwatch.Stop();
            _timer?.Dispose();
        }

        private void WorkingTimeCallback(object state)
        {
            if (_workingStopwatch == null)
                return;
            WorkingTime = _workingStopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.f");
        }
    }
}
