using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Views;

public partial class FirstPage : UserControl, IPageView
{
    public event EventHandler OnNextPage;
    public event EventHandler OnPreviousPage;
    public FirstPage()
    {
        DataContext = App.Current.Container.Resolve<PagesViewModel>();
        InitializeComponent();
    }

    private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnNextPage?.Invoke(this, e);
    }
}