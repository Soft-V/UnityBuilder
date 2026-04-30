using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using CraftHub.Services;
using System;
using UnityBuilder.Commands;
using UnityBuilder.Services;
using UnityBuilder.ViewModels;

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

            var savedParameters = CommandHelper.GetSavedParameters();
            if (savedParameters != null)
                App.Current.Container.Resolve<PagesViewModel>().SetParameters(savedParameters);

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

            UpdateStepIndicators();
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
            UpdateStepIndicators();
            CommandHelper.SaveParameters(App.Current.Container.Resolve<PagesViewModel>());
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
            UpdateStepIndicators();
            CommandHelper.SaveParameters(App.Current.Container.Resolve<PagesViewModel>());
        }

        private IPageView GetCurrentPage()
        {
            switch (_currentPageIndex)
            {
                case 0: return App.Current.Container.Resolve<FirstPage>();
                case 1: return App.Current.Container.Resolve<SecondPage>();
                case 2: return App.Current.Container.Resolve<ThirdPage>();
                case 3: return App.Current.Container.Resolve<PipelinePage>();
                default: throw new NotImplementedException();
            }
        }

        private void UpdateStepIndicators()
        {
            this.FindControl<Border>("StepOne")?.Classes.Set("done", _currentPageIndex > 0);
            this.FindControl<Border>("StepTwo")?.Classes.Set("done", _currentPageIndex > 1);
            this.FindControl<Border>("StepThree")?.Classes.Set("done", _currentPageIndex > 2);
            this.FindControl<Border>("StepPipline")?.Classes.Set("done", _currentPageIndex > 3);

            this.FindControl<Border>("StepOne")?.Classes.Set("selected", _currentPageIndex == 0);
            this.FindControl<Border>("StepTwo")?.Classes.Set("selected", _currentPageIndex == 1);
            this.FindControl<Border>("StepThree")?.Classes.Set("selected", _currentPageIndex == 2);
            this.FindControl<Border>("StepPipline")?.Classes.Set("selected", _currentPageIndex == 3);

            this.FindControl<TextBlock>("TBProject")?.Classes.Set("selected", _currentPageIndex == 0);
            this.FindControl<TextBlock>("TBBuildOptions")?.Classes.Set("selected", _currentPageIndex == 1);
            this.FindControl<TextBlock>("TBPlatforms")?.Classes.Set("selected", _currentPageIndex == 2);
            this.FindControl<TextBlock>("TBPipline")?.Classes.Set("selected", _currentPageIndex == 3);

            this.FindControl<Border>("StepTwoSeparator")?.Classes.Set("selected", _currentPageIndex > 0);
            this.FindControl<Border>("StepThreeSeparator")?.Classes.Set("selected", _currentPageIndex > 1);
            this.FindControl<Border>("StepPiplineSeparator")?.Classes.Set("selected", _currentPageIndex > 2);

            this.FindControl<Border>("StepTwoSeparator")?.Classes.Set("done", _currentPageIndex > 1);
            this.FindControl<Border>("StepThreeSeparator")?.Classes.Set("done", _currentPageIndex > 2);
            this.FindControl<Border>("StepPiplineSeparator")?.Classes.Set("done", _currentPageIndex > 3);
        }

        private void GoToStep(int targetIndex)
        {
            if (targetIndex == _currentPageIndex) return;

            _currentPage.OnNextPage -= OnNextPage;
            _currentPage.OnPreviousPage -= OnPreviousPage;

            ContentControl.IsTransitionReversed = targetIndex < _currentPageIndex;
            _currentPageIndex = targetIndex;
            _currentPage = GetCurrentPage();
            ContentControl.Content = _currentPage;

            _currentPage.OnNextPage += OnNextPage;
            _currentPage.OnPreviousPage += OnPreviousPage;
            UpdateStepIndicators();
            CommandHelper.SaveParameters(App.Current.Container.Resolve<PagesViewModel>());
        }

        private void StepOne_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => GoToStep(0);
        private void StepTwo_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => GoToStep(1);
        private void StepThree_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => GoToStep(2);
        private void StepPipline_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => GoToStep(3);

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
        private bool _isConfirmedClose = false;

        private async void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (_isConfirmedClose) return;

            e.Cancel = true;
            var confirmed = await CommandHelper.ShowMessageBox(Localizer.Get("DlgCloseWarningTitle"), Localizer.Get("DlgCloseWarningMsg"), true);
            if (!confirmed)
            {
                return;
            }
            if (confirmed)
            {
                _isConfirmedClose = true;
                Close();
            }
        }
    }
}