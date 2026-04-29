using CommunityToolkit.Mvvm.ComponentModel;
using HashComputer.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models.Enums;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Models
{
    public partial class Node : ObservableObject
    {
        public string Id { get; set; }
        public IParameters Parameters { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public NodeType Type { get; set; }
        public List<string> DependsOn { get; set; } = new();
        public Func<IParameters, CancellationToken, Action<ProgressChangedArgs>, Action<string>, Task<int>> Action { get; set; }

        public string ProcessOutput { get; set; }

        [ObservableProperty]
        private int _progress;
        [ObservableProperty]
        private bool _isInfinityProgress;

        public event EventHandler<string> ProcessOutputChanged;
        public void CallProcessOutputChanged(string data)
        {
            ProcessOutputChanged?.Invoke(this, data);
        }
        [ObservableProperty]
        private NodeState _state = NodeState.Pending;

        [ObservableProperty]
        private string _workingTime = "-";

        private Timer _timer;
        private Stopwatch _workingStopwatch;
        partial void OnStateChanged(NodeState value)
        {
            if (value == NodeState.Running)
            {
                _workingStopwatch = new Stopwatch();
                _workingStopwatch.Start();
                _timer = new Timer(WorkingTimeCallback, null, 0, 100);
            }
            else if (_workingStopwatch != null)
            {
                _workingStopwatch.Stop();
                _timer?.Dispose();
            }
        }

        private void WorkingTimeCallback(object state)
        {
            if (_workingStopwatch == null)
                return;
            WorkingTime = _workingStopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.f");
        }
    }
}
