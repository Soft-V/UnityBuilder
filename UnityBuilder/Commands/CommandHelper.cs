using HashComputer.Backend;
using HashComputer.Backend.Entities;
using HashComputer.Backend.Services;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Extensions;
using UnityBuilder.Models;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Commands
{
    public static class CommandHelper
    {
        async public static Task<int> ComputeHash(HashParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            ComputerService computerService = new ComputerService();
            outputDataChanged?.Invoke($"Computing hashes in {parameters.TargetPath}");
            var result = await computerService.ComputeHash(
                new ComputeParameters()
                {
                    Path = parameters.TargetPath,
                    Version = parameters.BuildVersion,
                    TaskNumber = 4,
                    HashFileName = "computed_hash",
                    StableFilesPath = "computed_stables",
                },
                progressChanged,
                cancellationToken
            );
            outputDataChanged?.Invoke(result.Item2);
            return result.Item1 ? 0 : -1;
        }

        async public static Task<int> UploadFiles(FtpParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            using var clientSsh = new SshClient(parameters.Server, parameters.Username, parameters.Password);
            await clientSsh.ConnectAsync(cancellationToken);
            using var clientFtp = new SftpClient(parameters.Server, parameters.Username, parameters.Password);
            await clientFtp.ConnectAsync(cancellationToken);

            if (parameters.DeleteOnUpload)
            {
                using SshCommand cmd = clientSsh.CreateCommand($"sudo rm -rf {parameters.TargetPath}");
                await cmd.ExecuteAsync(cancellationToken);
                outputDataChanged?.Invoke(cmd.Result);
                using SshCommand cmd2 = clientSsh.CreateCommand($"mkdir -p {parameters.TargetPath}");
                await cmd2.ExecuteAsync(cancellationToken);
                outputDataChanged?.Invoke(cmd2.Result);
            }

            // upload all files
            var files = Directory.GetFiles(parameters.LocalPath, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                progressChanged?.Invoke(new ProgressChangedArgs()
                {
                    Progress = (int)(i / (float)files.Length * 100),
                });

                var file = files[i];
                var relative = file.ExcludePathPart(parameters.LocalPath);
                var target = parameters.TargetPath.TrimEnd('/');

                // create a directory for it
                using SshCommand cmd3 = clientSsh.CreateCommand(
                    $"mkdir -p {target}/{string.Join('/', relative.Split('/').SkipLast(1))}");
                await cmd3.ExecuteAsync(cancellationToken);
                outputDataChanged?.Invoke(cmd3.Result);

                using var fs = File.OpenRead(file);
                await clientFtp.UploadAsync(fs, $"{target}/{relative}");
                outputDataChanged?.Invoke($"Uploaded {target}/{relative}");
            }
            return 0;
        }

        public static void SaveParameters(PagesViewModel pagesViewModel)
        {
            var json = JsonConvert.SerializeObject(pagesViewModel);
            UnityBuilder.Properties.Settings.Default.ParametersJson = json;
            UnityBuilder.Properties.Settings.Default.Save();
        }
        public static PagesViewModel GetSavedParameters()
        {
            var json = UnityBuilder.Properties.Settings.Default.ParametersJson;
            if (string.IsNullOrWhiteSpace(json))
                return null;
            var content = JsonConvert.DeserializeObject<PagesViewModel>(json);
            return content; 
        }
    }
}
