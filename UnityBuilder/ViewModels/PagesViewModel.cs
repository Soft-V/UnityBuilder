using CommunityToolkit.Mvvm.ComponentModel;

namespace UnityBuilder.ViewModels
{
    public partial class PagesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _unityPath;
        [ObservableProperty]
        private string _projectPath;
        [ObservableProperty]
        private string _buildVersion;
        [ObservableProperty]
        private string _outputDirectory;

        [ObservableProperty]
        private bool _computeHashes;
        [ObservableProperty]
        private bool _uploadOnFtp;
        [ObservableProperty]
        private string _ftpUsername;
        [ObservableProperty]
        private string _ftpPassword;
        [ObservableProperty]
        private string _ftpServer;
        [ObservableProperty]
        private int _ftpPort;
        [ObservableProperty]
        private bool _ftpDeleteOnUpload;

        [ObservableProperty]
        private bool _buildWinX64;
        [ObservableProperty]
        private string _winX64FtpPath;
        [ObservableProperty]
        private bool _buildWinArm64;
        [ObservableProperty]
        private string _winArm64FtpPath;
        [ObservableProperty]
        private bool _buildWinX86;
        [ObservableProperty]
        private string _winX86FtpPath;
        [ObservableProperty]
        private bool _buildLinuxX64;
        [ObservableProperty]
        private string _linuxX64FtpPath;
        [ObservableProperty]
        private bool _buildMacX64;
        [ObservableProperty]
        private string _macX64FtpPath;
        [ObservableProperty]
        private bool _buildAndroid;
        [ObservableProperty]
        private string _androidFtpPath;
    }
}
