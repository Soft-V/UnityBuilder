using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using UnityBuilder.Commands;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Views;

public partial class SecondPage : UserControl, IPageView
{

    public event EventHandler OnNextPage;
    public event EventHandler OnPreviousPage;

    public SecondPage()
    {
        DataContext = App.Current.Container.Resolve<PagesViewModel>();
        InitializeComponent();
    }

    private async void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = (PagesViewModel)DataContext!;
        var error = vm.ValidatePage2();
        if (error != null)
        {
            await CommandHelper.ShowMessageBox("Fill in required fields", error);
            return;
        }
        OnNextPage?.Invoke(this, e);
    }

    private void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnPreviousPage?.Invoke(this, e);
    }
}