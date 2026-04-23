using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public class WindowsCommand : IPlatformCommand
    {
        public int Build(BuildParameters parameters)
        {
            throw new System.NotImplementedException();
        }

        public int ComputeHash(HashParameters parameters)
        {
            throw new System.NotImplementedException();
        }

        public int UploadFtp(FtpParameters parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
