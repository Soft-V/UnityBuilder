using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftHub.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;
using UnityBuilder.Services;

namespace UnityBuilder.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        ThemeService _themeService;

        private GitHubRelease? _latestRelease;

        [ObservableProperty]
        private bool _buildIsRunning = false;

        [ObservableProperty]
        private string _currentLanguage = LanguageService.Instance.CurrentLang;

        [ObservableProperty]
        private string _currentVersion = string.Empty;
        [ObservableProperty]
        private bool _showUpdateButton = false;
        [ObservableProperty]
        private bool _isDownloading = false;
        [ObservableProperty]
        private double _downloadProgress = 0;

        #region Commands

        [RelayCommand]
        private void SwitchLanguage()
        {
            LanguageService.Instance.Toggle();
            CurrentLanguage = LanguageService.Instance.CurrentLang;
        }

        [RelayCommand]
        private void SwitchTheme()
        {
            var currentTheme = _themeService.CurrentTheme;
            Models.Enums.ThemeType targetTheme;

            if (currentTheme == Models.Enums.ThemeType.Default)
            {
                var actualVariant = Application.Current?.ActualThemeVariant;
                targetTheme = actualVariant == ThemeVariant.Dark
                    ? Models.Enums.ThemeType.Light
                    : Models.Enums.ThemeType.Dark;
            }
            else
            {
                targetTheme = currentTheme == Models.Enums.ThemeType.Dark
                    ? Models.Enums.ThemeType.Light
                    : Models.Enums.ThemeType.Dark;
            }

            _themeService.SwitchTheme(targetTheme);
        }

        [RelayCommand]
        private async Task DownloadAndStartUpdate()
        {
            var confirmed = await CommandHelper.ShowMessageBox(
                "New version",
                "Do you want to download the new version?");
            if (!confirmed)
            {
                return;
            }

            try
            {
                if (_latestRelease?.Assets == null)
                {
                    await CommandHelper.ShowMessageBox("Error", $"No release information available");
                    return;
                }

                var asset = GetPlatformSpecificAsset(_latestRelease.Assets);

                if (asset == null)
                {
                    await CommandHelper.ShowMessageBox("Error", $"No installer found for your platform ({RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture})");
                    return;
                }

                var result = await CommandHelper.ShowProgressDialogAsync("Updating UnityBuilder", async (progress, cancellationToken) =>
                {
                    var sanitizedName = Path.GetFileName(asset.Name);
                    if (string.IsNullOrEmpty(sanitizedName))
                        throw new InvalidOperationException("Invalid asset filename.");

                    string downloadPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), sanitizedName));
                    if (!downloadPath.StartsWith(Path.GetFullPath(Path.GetTempPath())))
                        throw new InvalidOperationException("Asset filename contains invalid path characters.");

                    progress.Report(new UpdateProgress
                    {
                        Status = $"Downloading {asset.Name}...",
                        Message = "Downloading update...",
                        IsIndeterminate = true
                    });

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "CraftHub-Updater");
                        client.Timeout = TimeSpan.FromMinutes(5);

                        var uri = new Uri(asset.BrowserDownloadUrl);
                        if (!uri.Host.EndsWith(".github.com") && uri.Host != "github.com")
                            throw new Exception("The link has been replaced, and the github version cannot be installed.");

                        using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                        {
                            response.EnsureSuccessStatusCode();

                            var totalBytes = response.Content.Headers.ContentLength ?? -1;
                            using (var fs = new FileStream(downloadPath, FileMode.Create,
                                   FileAccess.Write, FileShare.None, 8192, true))
                            using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                            {
                                var buffer = new byte[8192];
                                long totalRead = 0;
                                int bytesRead;

                                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                                {
                                    await fs.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                    totalRead += bytesRead;

                                    if (totalBytes > 0)
                                    {
                                        var percent = (int)((totalRead * 100) / totalBytes);
                                        progress.Report(new UpdateProgress
                                        {
                                            PercentComplete = percent,
                                            Status = $"Downloading... {percent}%",
                                            BytesReceived = totalRead,
                                            TotalBytes = totalBytes
                                        });
                                    }
                                    else
                                    {
                                        var updateProgress = new UpdateProgress();
                                        updateProgress.BytesReceived = totalRead;
                                        updateProgress.Status = $"Downloading... {FileSizeHelper.FormatFileSize(updateProgress.BytesReceived)}";
                                        progress.Report(updateProgress);
                                    }
                                }
                            }
                        }
                    }

                    progress.Report(new UpdateProgress
                    {
                        Status = "Verifying checksum...",
                        Message = "Checking file integrity",
                        IsIndeterminate = true
                    });

                    if (!await VerifyChecksum(downloadPath, asset.Sha256))
                    {
                        throw new Exception("Checksum verification failed. The file may be corrupted.");
                    }

                    progress.Report(new UpdateProgress
                    {
                        Status = "Starting installer...",
                        Message = "Launching installer",
                        PercentComplete = 100
                    });

                    StartInstaller(downloadPath);

                    CloseApplication();
                });

                if (result.IsCanceled)
                {
                    await CommandHelper.ShowMessageBox("Cancelled", "Update was cancelled by user");

                }
                else if (!result.IsSuccess && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    await CommandHelper.ShowMessageBox("Update Failed", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await CommandHelper.ShowMessageBox("Error", $"Update Failed {ex.Message}");
            }
        }

        #endregion
        public MainViewModel(ThemeService themeService)
        {
            _themeService = themeService;
            themeService.SwitchTheme(themeService.CurrentTheme);
        }


        private void CheckUpdate()
        {
            Task.Run(async () =>
            {
                try
                {
                    CurrentVersion = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "version.txt")).Trim();
                    var response = await NetManager.Get("https://api.github.com/repos/Soft-V/UnityBuilder/releases/latest"); 
                    if (response.IsSuccessStatusCode)
                    {
                        var release = await NetManager.ParseHttpResponseMessage<GitHubRelease>(response);
                        string latestVersion = release?.TagName?.TrimStart('v') ?? string.Empty;

                        if (!string.IsNullOrEmpty(latestVersion) && latestVersion != CurrentVersion)
                        {
                            _latestRelease = release;
                            Dispatcher.UIThread.Post(() =>
                            {
                                ShowUpdateButton = true;
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CheckUpdate failed: {ex.Message}");
                }
            });
        }

        private GitHubAsset GetPlatformSpecificAsset(List<GitHubAsset> assets)
        {
            if (assets != null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                        return assets.FirstOrDefault(a => a.Name.Contains("arm64") && a.Name.EndsWith(".exe"));
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                        return assets.FirstOrDefault(a => a.Name.Contains("x86") && a.Name.EndsWith(".exe"));
                    else
                        return assets.FirstOrDefault(a => a.Name.Contains("x64") && a.Name.EndsWith(".exe"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                        return assets.FirstOrDefault(a => a.Name.Contains("arm64") && a.Name.EndsWith(".deb"));
                    else
                        return assets.FirstOrDefault(a => (a.Name.Contains("amd64") || a.Name.Contains("x64")) && a.Name.EndsWith(".deb"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                        return assets.FirstOrDefault(a => a.Name.Contains("arm64") && a.Name.EndsWith(".dmg"));
                    else
                        return assets.FirstOrDefault(a => (a.Name.Contains("x64") || a.Name.Contains("amd64")) && a.Name.EndsWith(".dmg"));
                }
            }

            return default;
        }

        private async Task<bool> VerifyChecksum(string filePath, string expectedSha256)
        {
            if (string.IsNullOrEmpty(expectedSha256))
            {
                Debug.WriteLine("Warning: No SHA256 provided for verification");
                return true;
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = await sha256.ComputeHashAsync(stream);
                    var actualHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return actualHash == expectedSha256.ToLower();
                }
            }
        }

        private void StartInstaller(string filePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = $"/S /D={AppContext.BaseDirectory}",
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "sudo",
                    ArgumentList = { "dpkg", "-i", filePath },
                    UseShellExecute = false
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", filePath);
            }
        }

        private void CloseApplication()
        {
            Environment.Exit(0);
        }

        public MainViewModel()
        {
            _themeService = App.Current.Container.Resolve<ThemeService>();  
            CheckUpdate();
        }
    }
}
