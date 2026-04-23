using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand
    {
        int Build(BuildParameters parameters);
        int ComputeHash(HashParameters parameters);
        int UploadFtp(FtpParameters parameters);
    }
}
