using HashComputer.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models.Enums;

namespace UnityBuilder.Models
{
    public class Node
    {
        public string Id { get; set; }
        public IParameters Parameters { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public NodeType Type { get; set; }
        public List<string> DependsOn { get; set; } = new();
        public Func<IParameters, CancellationToken, Action<ProgressChangedArgs>, Action<string>, Task<int>> Action { get; set; }

        public string ProcessOutput { get; set; }
        public int Progress { get; set; }
        public bool IsInfinityProgress { get; set; }
    }
}
