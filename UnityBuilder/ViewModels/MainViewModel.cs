using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftHub.Services;
using System;
using UnityBuilder.Models.Enums;

namespace UnityBuilder.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        ThemeService _themeService;

        #region Commands

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
        #endregion
        public MainViewModel(ThemeService themeService)
        {
            _themeService = themeService;
            themeService.SwitchTheme(themeService.CurrentTheme);
        }
    }
}
