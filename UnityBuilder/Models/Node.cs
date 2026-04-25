using CommunityToolkit.Mvvm.ComponentModel;
using HashComputer.Backend.Entities;
using System;
using System.Collections.Generic;
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

        [ObservableProperty]
        private string _processOutput;
        [ObservableProperty]
        private int _progress;
        [ObservableProperty]
        private bool _isInfinityProgress;
        [ObservableProperty]
        private NodeState _state = NodeState.Pending;
    }
}
