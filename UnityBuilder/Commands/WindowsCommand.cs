using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public class WindowsCommand : IPlatformCommand
    {
        async public Task<int> Build(BuildParameters parameters)
        {
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
                    "-customBuildTarget", $"\"{parameters.TargetPlatform}\"",
                    "-buildTarget", $"\"{parameters.TargetPlatform}\"",
                    "-customBuildPath", $"\"{parameters.OutputPath}\"",
                    "-customBuildProfile", "",
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
            };
            var proc = Process.Start(startInfo);
            await proc.WaitForExitAsync();
            return proc.ExitCode;
        }

        async public Task<int> ComputeHash(HashParameters parameters)
        {
            throw new System.NotImplementedException();
        }

        async public Task<int> UploadFtp(FtpParameters parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
