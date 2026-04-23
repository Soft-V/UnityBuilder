using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public class WindowsCommand : IPlatformCommand
    {
        async public Task<int> Build(BuildParameters parameters)
        {
            
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
