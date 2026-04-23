namespace UnityBuilder.Models
{
    public class BuildParameters
    {
        public string UnityPath { get; set; }
        public string ProjectPath { get; set; }
        public string OutputPath { get; set; }
        public string BuildVersion { get; set; }
        public string TargetPlatform { get; set; }
    }

    // check here https://github.com/game-ci/unity-builder/blob/main/src/model/platform.ts
    public static class TargetPlatforms
    {
        public const string OsX = "StandaloneOSX";
        public const string Windows = "StandaloneWindows";
        public const string Windows64 = "StandaloneWindows64";
        public const string Linux64 = "StandaloneLinux64";
    }
}
