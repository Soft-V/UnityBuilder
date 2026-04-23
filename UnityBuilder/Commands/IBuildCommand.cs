using UnityBuilder.Models;

namespace UnityBuilder.Commands
{
    public interface IBuildCommand
    {
        int Build(BuildParameters parameters);
    }
}
