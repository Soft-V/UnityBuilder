using Renci.SshNet;

namespace UnityBuilder.ConsoleTest
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            var ct = new CancellationTokenSource();
            using var clientSsh = new SshClient("softv.su", "ftpuser", "Softwaresoftv28032022");
            await clientSsh.ConnectAsync(ct.Token);
            Console.WriteLine("Hello, World!");
        }
    }
}
