using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Views;

namespace UnityBuilder.ViewModels
{
    public partial class PagesViewModel : ViewModelBase
    {
        [ObservableProperty] private string _unityPath;
        [ObservableProperty] private string _projectPath;
        [ObservableProperty] private string _buildVersion;
        [ObservableProperty] private string _buildName;
        [ObservableProperty] private string _outputDirectory;

        [ObservableProperty] private bool _computeHashes;
        [ObservableProperty] private bool _uploadOnFtp;
        [ObservableProperty] private string _ftpUsername;
        [ObservableProperty] private string _ftpPassword;
        [ObservableProperty] private string _ftpServer;
        [ObservableProperty] private int _ftpPort;
        [ObservableProperty] private bool _ftpDeleteOnUpload;

        [ObservableProperty] private bool _buildWinX64;
        [ObservableProperty] private string _winX64FtpPath;
        [ObservableProperty] private bool _buildWinX86;
        [ObservableProperty] private string _winX86FtpPath;
        [ObservableProperty] private bool _buildLinuxX64;
        [ObservableProperty] private string _linuxX64FtpPath;
        [ObservableProperty] private bool _buildMacX64;
        [ObservableProperty] private string _macX64FtpPath;
        [ObservableProperty] private bool _buildAndroid;
        [ObservableProperty] private string _androidFtpPath;

        private string _ftpConnectionLog;
        [JsonIgnore]
        public string FtpConnectionLog
        {
            get => _ftpConnectionLog;
            set
            {
                _ftpConnectionLog = value;
                OnPropertyChanged();
            }
        }

        private IBrush _logColor;
        [JsonIgnore]
        public IBrush LogColor
        {
            get => _logColor;
            set
            {
                _logColor = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] private bool _isChecking;

        [JsonIgnore]
        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                _isChecking = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IEnumerable<(bool? NeedBuild, string Path, string PlatformName)> GetBuildPlatforms =>
        [
            (BuildWinX64, WinX64FtpPath, TargetPlatforms.Windows64),
            (BuildWinX86, WinX86FtpPath, TargetPlatforms.Windows86),
            (BuildLinuxX64, LinuxX64FtpPath, TargetPlatforms.Linux64),
            (BuildMacX64, MacX64FtpPath, TargetPlatforms.OsX),
            (BuildAndroid, AndroidFtpPath, TargetPlatforms.Android),
        ];

        public void SetParameters(PagesViewModel savedParameters)
        {
            UnityPath = savedParameters.UnityPath;
            ProjectPath = savedParameters.ProjectPath;
            BuildVersion = savedParameters.BuildVersion;
            BuildName = savedParameters.BuildName;
            OutputDirectory = savedParameters.OutputDirectory;
            ComputeHashes = savedParameters.ComputeHashes;
            UploadOnFtp = savedParameters.UploadOnFtp;
            FtpUsername = savedParameters.FtpUsername;
            FtpPassword = savedParameters.FtpPassword;
            FtpPort = savedParameters.FtpPort;
            FtpServer = savedParameters.FtpServer;
            FtpDeleteOnUpload = savedParameters.FtpDeleteOnUpload;
            BuildWinX64 = savedParameters.BuildWinX64;
            WinX64FtpPath = savedParameters.WinX64FtpPath;
            BuildMacX64 = savedParameters.BuildMacX64;
            MacX64FtpPath = savedParameters.MacX64FtpPath;
            BuildLinuxX64 = savedParameters.BuildLinuxX64;
            LinuxX64FtpPath = savedParameters.LinuxX64FtpPath;
            BuildWinX86 = savedParameters.BuildWinX86;
            WinX86FtpPath = savedParameters.WinX86FtpPath;
        }

        public PagesViewModel()
        {
            ChooseUnityPathCommand = new RelayCommand(OnChooseUnityPathAsync);
            ChooseProjectPathCommand = new RelayCommand(OnChooseProjectPath);
            ChooseOutputPathCommand = new RelayCommand(OnChooseOutputPath);
            CheckFTPConnection = new RelayCommand(OnCheckFTPConnection);
        }



        [JsonIgnore] public ICommand ChooseUnityPathCommand { get; set; }
        [JsonIgnore] public ICommand ChooseProjectPathCommand { get; set; }
        [JsonIgnore] public ICommand ChooseOutputPathCommand { get; set; }
        [JsonIgnore] public ICommand CheckFTPConnection { get; set; }

        async private void OnChooseUnityPathAsync()
        {
            var result = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                result = await ChooseUnityAppMacOs();
            else
                result = await CommonChooseFile();
            if (!string.IsNullOrWhiteSpace(result))
            {
                UnityPath = result;
            }
        }

        async private void OnChooseProjectPath()
        {
            var result = await CommonChooseFolder();
            if (!string.IsNullOrWhiteSpace(result))
            {
                ProjectPath = result;
            }
        }

        async private void OnChooseOutputPath()
        {
            var result = await CommonChooseFolder();
            if (!string.IsNullOrWhiteSpace(result))
            {
                OutputDirectory = result;
            }
        }

        private async void OnCheckFTPConnection()
        {
            IsChecking = true;
            FtpConnectionLog = string.Empty;

            var result = await Task.Run(() => CommandHelper.CheckFTPConnection(FtpServer, FtpUsername, FtpPassword));

            IsChecking = false;
            FtpConnectionLog = result.Log;

            if (Application.Current!.Resources.TryGetResource(result.Color, Application.Current.ActualThemeVariant, out var resource))
            {
                if (resource is IBrush brush)
                    LogColor = brush;
            }
        }

        async private Task<string> CommonChooseFolder()
        {
            var topLevel = TopLevel.GetTopLevel(App.Current.Container.Resolve<FirstPage>());
            if (topLevel == null) return "";

            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
            });

            var folder = result?.FirstOrDefault();
            if (folder != null)
            {
                return folder.Path.AbsolutePath;
            }

            return "";
        }

        async private Task<string> CommonChooseFile()
        {
            var topLevel = TopLevel.GetTopLevel(App.Current.Container.Resolve<FirstPage>());
            if (topLevel == null) return "";

            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
            });

            var folder = result?.FirstOrDefault();
            if (folder != null)
            {
                return folder.Path.AbsolutePath;
            }

            return "";
        }

        async private Task<string> ChooseUnityAppMacOs()
        {
            return await Task.Run(() =>
            {
                using var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "osascript";
                process.StartInfo.ArgumentList.Add("-e");
                process.StartInfo.ArgumentList.Add(
                    "POSIX path of (choose file of type {\"com.apple.application-bundle\"} with prompt \"Select Unity Application\")");
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return process.ExitCode == 0 ? output : "";
            });
        }
    }
}
