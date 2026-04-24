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


    }
}
