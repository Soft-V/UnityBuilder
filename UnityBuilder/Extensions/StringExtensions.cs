using System.IO;
using System.Linq;

namespace UnityBuilder.Extensions
{
    public static class StringExtensions
    {
        public static string ExcludePathPart(this string path, string exclude)
        {
            path = path.Replace("\\", "/").TrimEnd('/');
            exclude = exclude.Replace("\\", "/").TrimEnd('/');

            var pathParts = path.Split('/');
            var excludeParts = exclude.Split('/');

            for (int i = 0; i < excludeParts.Length; i++)
            {
                if (excludeParts[i] != pathParts[i])
                    throw new InvalidDataException("Path parts have to be the same");
            }

            return string.Join("/", pathParts.Skip(excludeParts.Length));
        }
    }
}
