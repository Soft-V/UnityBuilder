using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;
using UnityBuilder.Services;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Commands
{
    public static class PiplineStartCommand
    {
        public static async Task Execute(PagesViewModel viewModel, CancellationToken token)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            Node previousBuildNode = null;

            IPlatformCommand platformCommand = new WindowsCommand();

            foreach(var platform in viewModel.GetBuildPlatforms)
            {
                if (!platform.NeedBuild)
                    continue;
                var buildNode = new Node() 
                {
                    Id = $"build-{platform.PlatformName}",
                    Parameters = new BuildParameters
                    {
                        BuildVersion = viewModel.BuildVersion,
                        OutputPath = Path.Combine(viewModel.OutputDirectory, platform.PlatformName),
                        ProjectPath = viewModel.ProjectPath,
                        TargetPlatform = platform.PlatformName,
                        UnityPath = viewModel.UnityPath,
                    },
                    Type = Models.Enums.NodeType.Build,
                    DependsOn = previousBuildNode == null ? [] : [previousBuildNode.Id],
                    Action = platformCommand.Build
                };
                previousBuildNode = buildNode;

                nodes.Add(buildNode);

                Node computeHashNode = null;

                if (viewModel.ComputeHashes)
                {
                    computeHashNode = new Node()
                    {
                        Id = $"hash-{platform.PlatformName}",
                        Parameters = new HashParameters
                        {
                            BuildVersion = viewModel.BuildVersion,
                            TargetPath = Path.Combine(viewModel.OutputDirectory, platform.PlatformName),
                        },
                        Type = Models.Enums.NodeType.Hash,
                        DependsOn = [buildNode.Id],
                        Action = platformCommand.ComputeHash
                    };

                    nodes.Add(computeHashNode);
                }

                Node ftpNode = null;

                if (viewModel.UploadOnFtp)
                {
                    ftpNode = new Node()
                    {
                        Id = $"ftp-{platform.PlatformName}",
                        Parameters = new FtpParameters
                        {
                           Server = viewModel.FtpServer,
                           Port = viewModel.FtpPort,
                           DeleteOnUpload = viewModel.FtpDeleteOnUpload,
                           Username = viewModel.FtpUsername, 
                           Password = viewModel.FtpPassword,
                           LocalPath = Path.Combine(viewModel.OutputDirectory, platform.PlatformName),
                           TargetPath = platform.Path
                        },
                        Type = Models.Enums.NodeType.Ftp,
                        DependsOn = computeHashNode == null ? [buildNode.Id] : [computeHashNode.Id],
                        Action = platformCommand.UploadFtp
                    };

                    nodes.Add(ftpNode);
                }

            }

            await PipelineRunner.Run(nodes, token);
        }
    }
}
