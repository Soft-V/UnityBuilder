namespace UnityBuilder.Models
{
    public class FtpParameters : IParameters
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool DeleteOnUpload { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LocalPath { get; set; }
        public string TargetPath { get; set; }
    }
}
