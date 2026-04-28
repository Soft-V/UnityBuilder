using HashComputer.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;
using UnityBuilder.Services;

namespace UnityBuilder.Commands
{
    public class MacOsCommand : IPlatformCommand
    {
        private Process _unityBuildProcess;
        public async Task<int> Build(IParameters pars, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (pars is not BuildParameters parameters)
                throw new ArgumentException(nameof(parameters));
            
            // build path
            string buildPath = Path.Combine(parameters.OutputPath, $"{parameters.BuildName}{PlatformSpecificHelper.GetPlatformExtension(parameters.TargetPlatform)}");

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

            progressChanged.Invoke(new ProgressChangedArgs() { Progress = -1 });

            return _unityBuildProcess?.ExitCode ?? -1;
        }

        public Task<int> ComputeHash(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<int> UploadFtp(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            throw new NotImplementedException();
        }
    }
}
