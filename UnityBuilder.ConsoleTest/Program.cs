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
                    Parameters = new BuildParameters()
                    {
                        BuildVersion = "1.0.0",
                        OutputPath = "C:\\RobocadBuild",
                        ProjectPath = "C:\\Scripts\\robocadV-dev",
                        TargetPlatform = TargetPlatforms.Windows64,
                        UnityPath = "C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.45f2\\Editor\\Unity.exe",
                    },
                    Action = winBuild.Build,
                },
                new Node
                {
                    Id = "build-linux",
                    Type = NodeType.Build,
                    Parameters = new BuildParameters()
                    {
                        BuildVersion = "1.0.0",
                        OutputPath = "C:\\RobocadBuild",
                        ProjectPath = "C:\\Scripts\\robocadV-dev",
                        TargetPlatform = TargetPlatforms.Windows64,
                        UnityPath = "C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.45f2\\Editor\\Unity.exe",
                    },
                    DependsOn = new List<string> { "build-win" },
                    Action = winBuild.Build,
                }
            };

            await PipelineRunner.Run(nodes, cts.Token);
        }
    }
}
