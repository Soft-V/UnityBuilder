using HashComputer.Backend;
using HashComputer.Backend.Services;
using Renci.SshNet;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public static class CommandHelper
    {
        async public static Task<int> ComputeHash(HashParameters parameters, CancellationToken cancellationToken)
        {
            ComputerService computerService = new ComputerService();
            var result = await computerService.ComputeHash(
                new ComputeParameters()
                {
                    Path = parameters.TargetPath,
                    Version = parameters.BuildVersion,
                    TaskNumber = 4,
                    HashFileName = "computed_hash",
                    StableFilesPath = "computed_stables",
                },
                null,
                cancellationToken
            );
            return result.Item1 ? 0 : -1;
        }

        async public static Task<int> UploadFiles(FtpParameters parameters, CancellationToken cancellationToken)
        {
            return 0;
            if (parameters.DeleteOnUpload)
            {
                using var clientSsh = new SshClient(parameters.Server, parameters.Username, parameters.Password);
                await clientSsh.ConnectAsync(cancellationToken);
                using SshCommand cmd = clientSsh.CreateCommand($"sudo rm -rf {parameters.TargetPath}");
                await cmd.ExecuteAsync(cancellationToken);
                using SshCommand cmd2 = clientSsh.CreateCommand($"mkdir -p {parameters.TargetPath}");
                await cmd2.ExecuteAsync(cancellationToken);
            }

            using var clientFtp = new SftpClient(parameters.Server, parameters.Username, parameters.Password);
            await clientFtp.ConnectAsync(cancellationToken);
        }
    }
}
