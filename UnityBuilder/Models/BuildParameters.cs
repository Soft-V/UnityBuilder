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
}
