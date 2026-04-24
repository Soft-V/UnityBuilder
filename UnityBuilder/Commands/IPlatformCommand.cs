using HashComputer.Backend.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand
    {
        Task<int> Build(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
        Task<int> ComputeHash(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
        Task<int> UploadFtp(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged, Action<string> outputDataChanged);
    }
}
