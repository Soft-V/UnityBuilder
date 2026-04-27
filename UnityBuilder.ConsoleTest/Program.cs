using Renci.SshNet;
using System.Threading;
using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;
using UnityBuilder.Services;

namespace UnityBuilder.ConsoleTest
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            using var clientSsh = new SshClient("softv.su", "ftpuser", "SoftVCreator28032022");
            await clientSsh.ConnectAsync(cancellationToken);
            Console.WriteLine("Hello, World!");
        }
    }
}
