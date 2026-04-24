using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using CraftHub.Services;
using System;

namespace UnityBuilder.Views
{
    public partial class MainWindow : Window
    {
        private IPageView _currentPage;
        private int _currentPageIndex;

        private ThemeService _themeService;
        public MainWindow()
        {
            InitializeComponent();

            _themeService = App.Current.Container.Resolve<ThemeService>();
            ContentControl.PageTransition = new PageSlide()
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Orientation = PageSlide.SlideAxis.Horizontal,
            };
            ContentControl.IsTransitionReversed = false;

            _currentPage = App.Current.Container.Resolve<FirstPage>();
            ContentControl.Content = _currentPage;
            _currentPage.OnNextPage += OnNextPage;
            _currentPage.OnPreviousPage += OnPreviousPage;
        }

        private void OnNextPage(object sender, EventArgs args)
        {
            _currentPage.OnNextPage -= OnNextPage;
            _currentPage.OnPreviousPage -= OnPreviousPage;

            _currentPageIndex++;
            _currentPage = GetCurrentPage();
            ContentControl.IsTransitionReversed = false;
            ContentControl.Content = _currentPage;

            _currentPage.OnNextPage += OnNextPage;
            _currentPage.OnPreviousPage += OnPreviousPage;
        }

        private void OnPreviousPage(object sender, EventArgs args)
        {
            _currentPage.OnNextPage -= OnNextPage;
            _currentPage.OnPreviousPage -= OnPreviousPage;

            _currentPageIndex--;
            _currentPage = GetCurrentPage();
            ContentControl.IsTransitionReversed = true;
            ContentControl.Content = _currentPage;

            _currentPage.OnNextPage += OnNextPage;
            _currentPage.OnPreviousPage += OnPreviousPage;
        }

        private IPageView GetCurrentPage()
        {
            switch (_currentPageIndex)
            {
                case 0: return App.Current.Container.Resolve<FirstPage>();
                case 1: return App.Current.Container.Resolve<SecondPage>();
                default: throw new NotImplementedException();
            }
        }

        #region Theme
        private void SwitchTheme()
        {
            var currentTheme = _themeService.CurrentTheme;
            Models.Enums.ThemeType targetTheme;

            if (currentTheme == Models.Enums.ThemeType.Default)
            {
                var actualVariant = App.Current?.ActualThemeVariant;
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

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SwitchTheme();
        }
        #endregion
    }
}