using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand
    {
        Task<int> Build(BuildParameters parameters, CancellationToken cancellationToken);
        Task<int> ComputeHash(HashParameters parameters, CancellationToken cancellationToken);
        Task<int> UploadFtp(FtpParameters parameters, CancellationToken cancellationToken);
    }
}
