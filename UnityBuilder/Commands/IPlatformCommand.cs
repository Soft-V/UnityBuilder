using HashComputer.Backend.Entities;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand : IDisposable
    {
        Task<int> Build(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
        Task<int> ComputeHash(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
        Task<int> UploadFtp(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
    }

    public static class PlatformSpecificHelper
    {
        public static IPlatformCommand GetPlatformCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsCommand();
            }
            throw new NotImplementedException();
        }

        public static string GetPlatformExtension(string targetPlatform = null)
        {
            if (string.IsNullOrWhiteSpace(targetPlatform))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ".exe";
                }
                return "";
            }
            else
            {
                switch (targetPlatform)
                {
                    case TargetPlatforms.Windows64:
                    case TargetPlatforms.Windows86:
                        return ".exe";
                    default:
                        return "";
                }
            }
        }
    }
}
