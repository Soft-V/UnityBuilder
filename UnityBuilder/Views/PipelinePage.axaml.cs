using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityBuilder.Commands;
using UnityBuilder.Models;
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
        CreateNodes();

        var vm = DataContext as PipelinePageViewModel;
        vm.Start();
    }

    private void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnPreviousPage?.Invoke(this, e);
    }

    async public void CreateNodes()
    {
        var vm = DataContext as PipelinePageViewModel;
        await vm.GenerateNodes();

        // sort
        List<List<Node>> linedUp = new List<List<Node>>();
        foreach (var node in vm.Nodes)
        {
            if (node.Type != Models.Enums.NodeType.Build)
                continue;
            linedUp.Add(new List<Node>() { node });
        }
        foreach (var node in vm.Nodes)
        {
            if (node.Type != Models.Enums.NodeType.Hash)
                continue;
            var nodes = linedUp.First(x => x.FirstOrDefault(x => x.Id == node.DependsOn.First()) != null);
            nodes.Add(node);
        }
        foreach (var node in vm.Nodes)
        {
            if (node.Type != Models.Enums.NodeType.Ftp)
                continue;
            var nodes = linedUp.First(x => x.FirstOrDefault(x => x.Id == node.DependsOn.First()) != null);
            nodes.Add(node);
        }

        // create
        for (int i = 0; i < linedUp.Count; ++i)
        {
            for (int j = 0; j < linedUp[i].Count; ++j)
            {
                int x = 30 + (j * 220);
                int y = 30 + (i * 100);

                NodeControl control = new NodeControl(linedUp[i][j]);
                pipelineCanvas.Children.Add(control);
                Canvas.SetLeft(control, x);
                Canvas.SetTop(control, y);
            }
        }
    }
}