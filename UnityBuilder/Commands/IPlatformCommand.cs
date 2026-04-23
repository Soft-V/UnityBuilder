using HashComputer.Backend.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IPlatformCommand
    {
        Task<int> Build(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged);
        Task<int> ComputeHash(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged);
        Task<int> UploadFtp(IParameters parameters, CancellationToken cancellationToken, Action<ProgressChangedArgs> progressChanged);
    }
}
