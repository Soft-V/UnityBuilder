using Avalonia;
using Avalonia.Styling;
using UnityBuilder.Models.Enums;

namespace CraftHub.Services
{
    public class ThemeService
    {
        public ThemeType CurrentTheme { get; private set; }
        public ThemeType GetSystemTheme() => CurrentTheme;

        public ThemeService()
        {
            InitializeTheme();
        }

        private void InitializeTheme()
        {
            var savedThemeSetting = UnityBuilder.Properties.Settings.Default.CurrentTheme;

            CurrentTheme = savedThemeSetting switch
            {
                "Dark" => ThemeType.Dark,
                "Light" => ThemeType.Light,
                _ => ThemeType.Default
            };

            ApplyTheme(CurrentTheme);
        }

        public void SwitchTheme(ThemeType theme)
        {
            if (CurrentTheme == theme) return;

            CurrentTheme = theme;
            ApplyTheme(theme);
            UnityBuilder.Properties.Settings.Default.CurrentTheme = theme.ToString();
            UnityBuilder.Properties.Settings.Default.Save();
        }

        private void ApplyTheme(ThemeType theme)
        {
            var app = Application.Current;
            switch (theme)
            {
                case ThemeType.Dark:
                    app.RequestedThemeVariant = ThemeVariant.Dark;
                    break;

                case ThemeType.Light:
                    app.RequestedThemeVariant = ThemeVariant.Light;
                    break;

                case ThemeType.Default:
                default:
                    app.RequestedThemeVariant = ThemeVariant.Default;
                    break;
            }
        }
    }
}
