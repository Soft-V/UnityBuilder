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

        public async static Task Run(List<Node> nodes, CancellationToken token)
        {
            var completed = new HashSet<string>();
            var running = new Dictionary<string, Task>();

            while (completed.Count < nodes.Count)
            {
                var ready = nodes.Where(n =>!completed.Contains(n.Id) && !running.ContainsKey(n.Id) && n.DependsOn.All(d => completed.Contains(d)))
                    .ToList();

                foreach (var node in ready)
                {
                    Console.WriteLine($"START {node.Id}");

                    var task = RunNode(node, token); 
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
                    completed.Add(pair.Key);
                else
                    throw finished.Exception!;

                // отменяем все ноды 
                if (token.IsCancellationRequested)
                {
                    foreach (var node in nodes)
                    {
                        node.CancellationTokenSource.Cancel();
                    }
                }
            }
        }

        public async static Task RunNode(Node node)
        {
            var semaphore = limits[node.Type];

            await semaphore.WaitAsync(node.CancellationTokenSource.Token);

            try
            {
                var _ = await node.Action(node.Parameters, node.CancellationTokenSource.Token, (progress) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        node.Progress = progress.Progress;
                    });
                }); 
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
