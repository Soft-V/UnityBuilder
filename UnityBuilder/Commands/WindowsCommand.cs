using HashComputer.Backend.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;
using UnityBuilder.Services;

namespace UnityBuilder.Commands
{
    public class WindowsCommand : IPlatformCommand
    {
        private Process _unityBuildProcess;

        async public Task<int> Build(IParameters pars, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (pars is not BuildParameters parameters)
                throw new ArgumentException(nameof(parameters));

            // build path
            string buildPath = Path.Combine(parameters.OutputPath, $"{parameters.BuildName}{PlatformSpecificHelper.GetPlatformExtension(parameters.TargetPlatform)}");

            progressChanged?.Invoke(new ProgressChangedArgs() { Progress = -1 });
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = parameters.UnityPath,
                Arguments = string.Join(' ', 
                [
                    "-quit",
                    "-batchmode",
                    "-silent-crashes",
                    "-projectPath", $"\"{parameters.ProjectPath}\"",
                    "-executeMethod", "UnityBuilderAction.Builder.BuildProject", // TODO: allow custom
                    "-buildTarget", $"\"{parameters.TargetPlatform}\"",
                    "-customBuildPath", $"\"{buildPath}\"",
                    "-buildVersion", $"\"{parameters.BuildVersion}\"",
                    /* TODO: android
                    "-androidVersionCode", "`"$Env: ANDROID_VERSION_CODE`"",
                    "-androidKeystorePass", "`"$Env: ANDROID_KEYSTORE_PASS`"",
                    "-androidKeyaliasName", "`"$Env: ANDROID_KEYALIAS_NAME`"",
                    "-androidKeyaliasPass", "`"$Env: ANDROID_KEYALIAS_PASS`"",
                    "-androidTargetSdkVersion", "`"$Env: ANDROID_TARGET_SDK_VERSION`"",
                    "-androidExportType", "`"$Env: ANDROID_EXPORT_TYPE`"",
                    "-androidSymbolType", "`"$Env: ANDROID_SYMBOL_TYPE`"",
                    */
                    "-logfile", "-"
                ]),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            _unityBuildProcess = Process.Start(startInfo);

            using DelayedActionCaller outputDelayer = new DelayedActionCaller(outputDataChanged, 1000);
            _unityBuildProcess.OutputDataReceived += (s, a) => outputDelayer.Handle(a.Data);
            _unityBuildProcess.ErrorDataReceived += (s, a) => outputDelayer.Handle(a.Data);
            outputDelayer.Handle($"Building {parameters.ProjectPath}");

            _unityBuildProcess.BeginOutputReadLine();
            _unityBuildProcess.BeginErrorReadLine();

            try
            {
                await _unityBuildProcess.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Убиваем процесс при отмене, иначе он продолжит держать stdout/stderr открытыми
                try
                {
                    if (!_unityBuildProcess.HasExited)
                        _unityBuildProcess.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) { /* процесс уже завершился */ }

                throw new TaskCanceledException();
            }
            finally
            {
                // CancelOutputRead гарантирует завершение потоков чтения
                try { _unityBuildProcess.CancelOutputRead(); } catch { }
                try { _unityBuildProcess.CancelErrorRead(); } catch { }
            }
            outputDelayer.Handle($"Done");
            return _unityBuildProcess?.ExitCode ?? -1;
        }

        async public Task<int> ComputeHash(IParameters pars, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (pars is not HashParameters parameters)
                throw new ArgumentException(nameof(parameters));
            return await CommandHelper.ComputeHash(parameters, cancellationToken, progressChanged, outputDataChanged);
        }

        async public Task<int> UploadFtp(IParameters pars, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (pars is not FtpParameters parameters)
                throw new ArgumentException(nameof(parameters));
            return await CommandHelper.UploadFiles(parameters, cancellationToken, progressChanged, outputDataChanged);
        }

        private bool isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            _unityBuildProcess?.Kill();
            _unityBuildProcess?.Dispose();

            isDisposed = true;
        }

        ~WindowsCommand()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
