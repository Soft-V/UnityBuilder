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

        public async static Task<NodeState> Run(HashSet<Node> nodes, CancellationToken token)
        {
            var completed = new HashSet<string>();
            var running = new Dictionary<string, Task>();

            while (completed.Count < nodes.Count)
            {
                try
                {
                    var ready = nodes.Where(n => !completed.Contains(n.Id) && !running.ContainsKey(n.Id) && n.DependsOn.All(d => completed.Contains(d)))
                    .ToList();

                    foreach (var node in ready)
                    {
                        Console.WriteLine($"START {node.Id}");

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

                    Console.WriteLine($"DONE {pair.Key}");

                    if (finished.IsCompletedSuccessfully)
                    {
                        completed.Add(pair.Key);
                        (nodes.First(x => x.Id == pair.Key)).State = NodeState.Done;
                    }
                    else
                    {
                        CancelNodeAndChildren(pair.Key, nodes);
                        (nodes.First(x => x.Id == pair.Key)).State = NodeState.Error;
                    }

                    // отменяем все ноды 
                    if (token.IsCancellationRequested)
                    {
                        foreach (var node in nodes)
                        {
                            if (!node.CancellationTokenSource.IsCancellationRequested)
                            {
                                node.State = NodeState.Cancelled;
                                node.CancellationTokenSource.Cancel();
                            }
                        }
                    }
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
                if(node.State == NodeState.Cancelled)
                {
                    return NodeState.Cancelled;
                }
            }
            return NodeState.Done;
        }

        private static void CancelNodeAndChildren(string key, HashSet<Node> nodes)
        {
            var cancelNode = nodes.FirstOrDefault(x => x.Id == key);
            if (!cancelNode.CancellationTokenSource.IsCancellationRequested)
                cancelNode.CancellationTokenSource.Cancel();
            cancelNode.State = NodeState.Cancelled;
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
                    }
                });

                semaphore.Release();
            }
        }
    }
}
