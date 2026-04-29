using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;

namespace UnityBuilder.Services
{
    public static class PipelineRunner
    {
        private static Dictionary<NodeType, SemaphoreSlim> limits =
            new()
            {
                [NodeType.Build] = new SemaphoreSlim(1),
                [NodeType.Hash] = new SemaphoreSlim(10),
                [NodeType.Ftp] = new SemaphoreSlim(10)
            };

        private static HashSet<Node> nodesList;

        public async static Task<NodeState> Run(HashSet<Node> nodes, CancellationToken token)
        {
            var completed = new HashSet<string>();
            var running = new Dictionary<string, Task>();
            nodesList = nodes;
            while (completed.Count < nodes.Count)
            {
                try
                {
                    var ready = nodes.Where(n => !completed.Contains(n.Id) && !running.ContainsKey(n.Id) && n.DependsOn.All(d => completed.Contains(d)))
                    .ToList();

                    foreach (var node in ready)
                    {
                        var task = RunNode(node);
                        running[node.Id] = task;
                    }

                    if (running.Count == 0)
                        throw new Exception("Deadlock");

                    // ждем пока вернётся выполненный таск
                    var finished = await Task.WhenAny(running.Values);

                    // ищем его среди выполненных
                    var pair = running.First(p => p.Value == finished);
                    running.Remove(pair.Key);

                    var nodeRef = nodes.First(x => x.Id == pair.Key);

                    completed.Add(pair.Key);

                }
                catch (Exception e)
                {
                    break;
                }
            }

            foreach (var node in nodes)
            {
                if (node.State == NodeState.Error)
                {
                    return NodeState.Error;
                }
                if (node.State == NodeState.Cancelled)
                {
                    return NodeState.Cancelled;
                }
            }
            return NodeState.Done;
        }

        private static void CancelNodeAndChildren(Node cancelNode)
        {
            var canceledNodes = nodesList.Where(x => x.DependsOn.Contains(cancelNode.Id) && x.Type != NodeType.Build).ToList();
            if (canceledNodes.Count != 0)
            {
                foreach (var child in canceledNodes)
                {
                    child.State = NodeState.Cancelled;
                    CancelNodeAndChildren(child);
                }
            }
        }

        public async static Task RunNode(Node node)
        {
            var semaphore = limits[node.Type];

            int nodeResult = -1;
            try
            {
                await semaphore.WaitAsync(node.CancellationTokenSource.Token);

                node.State = NodeState.Running;
                nodeResult = await node.Action(node.Parameters, node.CancellationTokenSource.Token,
                (progress) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        node.Progress = progress.Progress;
                        if (progress.Progress == -1)
                            node.IsInfinityProgress = true;
                    });
                },
                (data) =>
                {
                    node.ProcessOutput += data;
                    Dispatcher.UIThread.Post(() =>
                    {
                        node.CallProcessOutputChanged(data);
                    });
                });
            }
            catch (TaskCanceledException e)
            {
                node.State = NodeState.Cancelled;
                if (!node.CancellationTokenSource.IsCancellationRequested)
                    node.CancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
                node.State = NodeState.Error;
                if (!node.CancellationTokenSource.IsCancellationRequested)
                    node.CancellationTokenSource.Cancel();
                CancelNodeAndChildren(node);
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {

                    if (node.Type == NodeType.Build)
                        node.IsInfinityProgress = false;
                    node.Progress = 100;
                    if (nodeResult != 0 && !node.CancellationTokenSource.IsCancellationRequested)
                    {
                        node.State = NodeState.Error;
                        if (!node.CancellationTokenSource.IsCancellationRequested)
                            node.CancellationTokenSource.Cancel();
                        CancelNodeAndChildren(node);
                    }
                    else
                    {
                        node.State = NodeState.Done;
                    }
                });

                semaphore.Release();
            }
        }
    }
}