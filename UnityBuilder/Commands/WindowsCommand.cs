using HashComputer.Backend.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;
using UnityBuilder.Services;

namespace UnityBuilder.Commands
{
    public class WindowsCommand : IPlatformCommand
    {
        async public Task<int> Build(IParameters pars, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged)
        {
            if (pars is not BuildParameters parameters)
                throw new ArgumentException(nameof(parameters));

            string logFile = $"{parameters.TargetPlatform}_log.txt";
            File.WriteAllText(logFile, "");

            // build path
            string buildPath = Path.Combine(parameters.OutputPath, $"{parameters.BuildVersion}{CommandHelper.GetPlatformExtension()}");

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
                    "-buildName", "robocadV",
                    "-productName", "robocadV",
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
            var proc = Process.Start(startInfo);

            using DelayedActionCaller outputDelayer = new DelayedActionCaller(outputDataChanged, 1000);
            proc.OutputDataReceived += (s, a) => outputDelayer.Handle(a.Data);
            proc.ErrorDataReceived += (s, a) => outputDelayer.Handle(a.Data);
            outputDelayer.Handle($"Building {parameters.ProjectPath}");

            proc.BeginOutputReadLine(); 
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();
            outputDelayer.Handle($"Done");
            return proc.ExitCode;
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
    }
}
