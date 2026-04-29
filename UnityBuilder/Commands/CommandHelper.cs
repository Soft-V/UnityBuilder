using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using HashComputer.Backend;
using HashComputer.Backend.Entities;
using HashComputer.Backend.Services;
using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Extensions;
using UnityBuilder.Models;
using UnityBuilder.ViewModels;
using UnityBuilder.Views;

namespace UnityBuilder.Commands
{
    public static class CommandHelper
    {
        private const int UploadTaskAmount = 4;

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

            ConcurrentQueue<string> filesQueue;
            int filesAmount;
            object progressChangeLock = new object();
            int currentFileNumber = 0;

            try
            {
                outputDataChanged?.Invoke("Trying to create session...\n");
                using var clientSsh = new SshClient(parameters.Server, parameters.Username, parameters.Password);
                clientSsh.KeepAliveInterval = TimeSpan.FromSeconds(30);
                await clientSsh.ConnectAsync(cancellationToken);
                using var clientFtp = new SftpClient(parameters.Server, parameters.Username, parameters.Password);
                clientFtp.KeepAliveInterval = TimeSpan.FromSeconds(30);
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
                filesAmount = files.Length;
                filesQueue = new ConcurrentQueue<string>(files);

                List<Task<int>> list = new List<Task<int>>();
                for (int i = 0; i < UploadTaskAmount; i++)
                {
                    list.Add(UploadParallelAsync(clientSsh, clientFtp));
                }

                var results = await Task.WhenAll(list);
                return results.Any(x => x != 0) ? -1 : 0;
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

            Task<int> UploadParallelAsync(SshClient sshClient, SftpClient sftpClient)
            {
                return Task.Run(() =>
                {
                    try
                    {
                        while (filesQueue.TryDequeue(out var file) && !cancellationToken.IsCancellationRequested)
                        {
                            var relative = file.ExcludePathPart(parameters.LocalPath);
                            var target = parameters.TargetPath.TrimEnd('/');

                            // create a directory for it
                            using SshCommand cmd3 = sshClient.CreateCommand(
                                $"mkdir -p {target}/{string.Join('/', relative.Split('/').SkipLast(1))}");
                            cmd3.ExecuteAsync(cancellationToken).GetAwaiter().GetResult();

                            using var fs = System.IO.File.OpenRead(file);
                            sftpClient.UploadAsync(fs, $"{target}/{relative}").GetAwaiter().GetResult();

                            lock (progressChangeLock)
                            {
                                outputDataChanged?.Invoke(cmd3.Result);
                                outputDataChanged?.Invoke($"Uploaded {target}/{relative}\n");
                                currentFileNumber++;
                                progressChanged?.Invoke(new ProgressChangedArgs
                                {
                                    Progress = (int)(currentFileNumber / (float)filesAmount * 100f),
                                });
                            }
                        }
                        return 0;
                    }
                    catch
                    {
                        return -1;
                    }
                }, cancellationToken);
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

        public static async Task<bool> ShowMessageBox(string title, string message, bool isConfirm = false)
        {
            var window = GetMainWindow();
            if (window == null) return false;

            var msgDialog = new MessageBoxView
            {
                Title = title,
                TitleText = title,
                MessageText = message,
                IsConfirm = isConfirm
            };

            return await msgDialog.ShowDialog<bool>(window);
        }

        public static async Task<ProgressResult> ShowProgressDialogAsync(string title, Func<IProgress<UpdateProgress>, CancellationToken, Task> task)
        {
            var window = GetMainWindow();
            if (window == null) return ProgressResult.Error("No main window found");

            var dialog = new ProgressDialogView
            {
                TitleText = title,
                MessageText = "Starting...",
                IsIndeterminate = true
            };

            ProgressResult result = ProgressResult.Canceled();

            var taskRun = Task.Run(async () =>
            {
                try
                {
                    await dialog.RunWithProgress(task);
                    result = ProgressResult.Success();
                }
                catch (OperationCanceledException)
                {
                    result = ProgressResult.Canceled();
                }
                catch (Exception ex)
                {
                    result = ProgressResult.Error(ex.Message);
                }
            });

            await dialog.ShowDialog(window);

            await taskRun;

            return result;
        }

        private static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow;
            return null;
        }
    }
}
