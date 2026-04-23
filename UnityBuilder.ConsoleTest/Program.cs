using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;
using UnityBuilder.Services;

namespace UnityBuilder.ConsoleTest
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            await TestWindowsBuild();
            Console.WriteLine("Hello, World!");
        }

        async private static Task TestWindowsBuild()
        {
            var cts = new CancellationTokenSource();
            var buildParameters = new BuildParameters()
            {
                BuildVersion = "1.0.0",
                OutputPath = "C:\\RobocadBuild",
                ProjectPath = "C:\\Scripts\\robocadV-dev",
                TargetPlatform = TargetPlatforms.Windows64,
                UnityPath = "C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.45f2\\Editor\\Unity.exe",
            };

            var winBuild = new WindowsCommand();
            var nodes = new List<Node>
            {
                new Node
                {
                    Id = "build-win",
                    Type = NodeType.Build,
                    Action = token => winBuild.Build(buildParameters, token)
                },
                new Node
                {
                    Id = "build-win2",
                    Type = NodeType.Build,
                    DependsOn = new List<string> { "build-win" },
                    Action = token => winBuild.Build(buildParameters, token)
                }
            };

            await PipelineRunner.Run(nodes, cts.Token);
        }
    }
}
