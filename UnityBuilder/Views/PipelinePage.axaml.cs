using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Views;

public partial class PipelinePage : UserControl, IPageView
{
    public event EventHandler OnNextPage;
    public event EventHandler OnPreviousPage;

    public PipelinePage()
    {
        DataContext = App.Current.Container.Resolve<PipelinePageViewModel>();
        InitializeComponent();
    }

    private void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnPreviousPage?.Invoke(this, e);
    }
}