using HashComputer.Backend;
using HashComputer.Backend.Entities;
using HashComputer.Backend.Services;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Extensions;
using UnityBuilder.Models;
using UnityBuilder.ViewModels;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UnityBuilder.Commands
{
    public static class CommandHelper
    {
        async public static Task<int> ComputeHash(HashParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outputDataChanged?.Invoke($"Cancelled\n");
                return -1;
            }
            ComputerService computerService = new ComputerService();
            outputDataChanged?.Invoke($"Computing hashes in {parameters.TargetPath}\n");
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
            string res = string.IsNullOrWhiteSpace(result.Item2) ? "Done\n" : result.Item2 + "\n";
            outputDataChanged?.Invoke(res);
            return result.Item1 ? 0 : -1;
        }

        async public static Task<int> UploadFiles(FtpParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outputDataChanged?.Invoke($"Cancelled\n");
                return -1;
            }
            try
            {
                outputDataChanged?.Invoke("Trying to create session...\n");
                using var clientSsh = new SshClient(parameters.Server, parameters.Username, parameters.Password);
                await clientSsh.ConnectAsync(cancellationToken);
                using var clientFtp = new SftpClient(parameters.Server, parameters.Username, parameters.Password);
                await clientFtp.ConnectAsync(cancellationToken);

                if (parameters.DeleteOnUpload)
                {
                    outputDataChanged?.Invoke("Removing files...\n");
                    using SshCommand cmd = clientSsh.CreateCommand($"sudo rm -rf {parameters.TargetPath}");
                    await cmd.ExecuteAsync(cancellationToken);
                    outputDataChanged?.Invoke(cmd.Result);
                    using SshCommand cmd2 = clientSsh.CreateCommand($"mkdir -p {parameters.TargetPath}");
                    await cmd2.ExecuteAsync(cancellationToken);
                    outputDataChanged?.Invoke(cmd2.Result);
                }

                // upload all files
                outputDataChanged?.Invoke("Starting upload...\n");
                var files = Directory.GetFiles(parameters.LocalPath, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; ++i)
                {
                    progressChanged?.Invoke(new ProgressChangedArgs()
                    {
                        Progress = (int)(i / (float)files.Length * 100),
                    });

                    if (cancellationToken.IsCancellationRequested)
                    {
                        outputDataChanged?.Invoke($"Cancelled\n");
                        return -1;
                    }

                    var file = files[i];
                    var relative = file.ExcludePathPart(parameters.LocalPath);
                    var target = parameters.TargetPath.TrimEnd('/');

                    // create a directory for it
                    using SshCommand cmd3 = clientSsh.CreateCommand(
                        $"mkdir -p {target}/{string.Join('/', relative.Split('/').SkipLast(1))}");
                    await cmd3.ExecuteAsync(cancellationToken);
                    outputDataChanged?.Invoke(cmd3.Result);

                    using var fs = System.IO.File.OpenRead(file);
                    await clientFtp.UploadAsync(fs, $"{target}/{relative}");
                    outputDataChanged?.Invoke($"Uploaded {target}/{relative}\n");
                }
                return 0;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                outputDataChanged?.Invoke($"Exception {e}\n");
                return -1;
            }
        }

        public static void SaveParameters(PagesViewModel pagesViewModel)
        {
            var json = JsonSerializer.Serialize(pagesViewModel);
            UnityBuilder.Properties.Settings.Default.ParametersJson = json;
            UnityBuilder.Properties.Settings.Default.Save();
        }
        public static PagesViewModel GetSavedParameters()
        {
            
            var json = UnityBuilder.Properties.Settings.Default.ParametersJson;
            if (string.IsNullOrWhiteSpace(json))
                return null;
            var content = JsonSerializer.Deserialize<PagesViewModel>(json);
            return content;
        }

        public async static Task<(string Log, string Color)> CheckFTPConnection(string ftpServer, string username, string password)
        {
            try
            {
                
                CancellationToken cancellationToken = new CancellationToken();

                using var clientSsh = new SshClient(ftpServer, username, password);
                await clientSsh.ConnectAsync(cancellationToken);
                return ("Successful connection to the ftp server", "SuccessColor");
            }
            catch (Exception ex)
            {
                string log = $"Error connecting to FTP server: {ex.Message}";
                return (log, "DangerColor");
            }
        }

    }
}
