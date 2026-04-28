using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // completed — только успешно завершённые (разблокируют зависимости)
            var completed = new HashSet<string>();
            var finished  = new HashSet<string>();
            var running   = new Dictionary<string, Task>();

            while (finished.Count < nodes.Count)
            {
                // Глобальная отмена — останавливаем все незавершённые ноды
                if (token.IsCancellationRequested)
                {
                    foreach (var n in nodes.Where(n => !finished.Contains(n.Id)))
                        if (!n.CancellationTokenSource.IsCancellationRequested)
                            n.CancellationTokenSource.Cancel();
                }

                // Ноды, у которых хотя бы одна зависимость завершилась не успешно → Cancelled
                var skipped = nodes
                    .Where(n => !finished.Contains(n.Id)
                             && !running.ContainsKey(n.Id)
                             && n.DependsOn.Any(d => finished.Contains(d) && !completed.Contains(d)))
                    .ToList();

                foreach (var n in skipped)
                {
                    n.IsInfinityProgress = false;
                    n.State = NodeState.Cancelled;
                    finished.Add(n.Id);
                }

                // Ноды, у которых все зависимости успешно завершены → запускаем
                var ready = nodes
                    .Where(n => !finished.Contains(n.Id)
                             && !running.ContainsKey(n.Id)
                             && n.DependsOn.All(d => completed.Contains(d)))
                    .ToList();

                foreach (var n in ready)
                {
                    Console.WriteLine($"START {n.Id}");
                    running[n.Id] = RunNode(n);
                }

                if (running.Count == 0)
                    break; // дедлок или всё завершено

                var doneTask = await Task.WhenAny(running.Values);
                var pair = running.First(p => p.Value == doneTask);
                running.Remove(pair.Key);

                Console.WriteLine($"DONE {pair.Key}");

                var nodeRef = nodes.First(n => n.Id == pair.Key);
                nodeRef.IsInfinityProgress = false;

                if (doneTask.IsCompletedSuccessfully)
                {
                    completed.Add(pair.Key);
                    finished.Add(pair.Key);
                    nodeRef.Progress = 100;
                    nodeRef.State = NodeState.Done;
                }
                else
                {
                    finished.Add(pair.Key);

                    bool wasCancelled = nodeRef.CancellationTokenSource.IsCancellationRequested
                        || doneTask.Exception?.InnerExceptions.Any(e => e is OperationCanceledException) == true;

                    nodeRef.State = wasCancelled ? NodeState.Cancelled : NodeState.Error;
                }
            }

            // Оставшиеся Pending ноды (нераскрытые зависимости) → Cancelled
            foreach (var n in nodes.Where(n => n.State == NodeState.Pending))
            {
                n.IsInfinityProgress = false;
                n.State = NodeState.Cancelled;
            }

            // Итоговый статус пайплайна
            if (nodes.Any(n => n.State == NodeState.Error))
                return NodeState.Error;
            if (nodes.Any(n => n.State == NodeState.Cancelled))
                return NodeState.Cancelled;
            return NodeState.Done;
        }

        public async static Task RunNode(Node node)
        {
            var semaphore = limits[node.Type];
            bool semaphoreAcquired = false;
            int nodeResult = -1;

            try
            {
                // WaitAsync внутри try — если токен отменён, семафор не утечёт
                await semaphore.WaitAsync(node.CancellationTokenSource.Token);
                semaphoreAcquired = true;

                node.State = NodeState.Running;

                nodeResult = await node.Action(
                    node.Parameters,
                    node.CancellationTokenSource.Token,
                    (progress) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            node.Progress = progress.Progress;
                            node.IsInfinityProgress = progress.Progress == -1;
                        });
                    },
                    (data) =>
                    {
                        node.ProcessOutput += data;
                        Dispatcher.UIThread.Post(() => node.CallProcessOutputChanged(data));
                    });

                // Ненулевой код выхода — пробрасываем исключение, чтобы Run() увидел IsFaulted
                if (nodeResult != 0)
                    throw new Exception($"Node {node.Id} exited with code {nodeResult}");
            }
            finally
            {
                // Освобождаем семафор только если успели его захватить
                if (semaphoreAcquired)
                    semaphore.Release();
            }
        }
    }
}
