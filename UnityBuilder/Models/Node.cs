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
        public CancellationToken CancellationToken { get; set; }
        public NodeType Type { get; set; }
        public List<string> DependsOn { get; set; } = new();
        public Func<CancellationToken, Task> Action { get; set; }
    }
}
