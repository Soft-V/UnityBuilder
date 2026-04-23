using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand
    {
        Task<int> Build(BuildParameters parameters);
        Task<int> ComputeHash(HashParameters parameters);
        Task<int> UploadFtp(FtpParameters parameters);
    }
}
