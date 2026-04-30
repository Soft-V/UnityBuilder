using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityBuilder.Commands;
using UnityBuilder.Models;
using UnityBuilder.Models.Enums;
using UnityBuilder.Services;
using UnityBuilder.ViewModels;

namespace UnityBuilder.Views;

public partial class PipelinePage : UserControl, IPageView
{
    public event EventHandler OnNextPage;
    public event EventHandler OnPreviousPage;

    private NodeControl? _selectedControl;
    private Node _selectedNode;

    public PipelinePage()
    {
        DataContext = App.Current.Container.Resolve<PipelinePageViewModel>();
        InitializeComponent();

        pipelineCanvas.RenderTransformOrigin = RelativePoint.TopLeft;
        pipelineCanvas.RenderTransform = new TransformGroup();
        (pipelineCanvas.RenderTransform as TransformGroup).Children.AddRange([scaleTransform, transform]);

        var pagesViewModel = App.Current.Container.Resolve<PagesViewModel>();
        if (!pagesViewModel.IsCustomExecuteMethod)
            CopyDefaultBuildScripts(pagesViewModel.ProjectPath);

        CreateNodes();

        var vm = DataContext as PipelinePageViewModel;
        vm.Start();
        this.Unloaded += PipelinePage_Unloaded;
    }

    private void PipelinePage_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewMessageBox = false;
        CancelButton_Click(null, null);
    }

    private void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnPreviousPage?.Invoke(this, e);
    }

    private bool viewMessageBox = true;

    private async void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (viewMessageBox)
        {
            var confirmed = await CommandHelper.ShowMessageBox("Warning", "Are you sure you want to finish? All pipelines will be cancelled.", true);

            if (!confirmed)
                return;
        }

        viewMessageBox = true;

        (DataContext as PipelinePageViewModel)._cancellationToken.Cancel();
    }

    private void CopyDefaultBuildScripts(string projectPath)
    {
        CommonHelper.CopyFilesRecursively(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default-build-script"), 
            Path.Combine(projectPath, "Assets", "Editor"));
    }

    public async void CreateNodes()
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
        double canvasMaxRight = 0;
        double canvasMaxBottom = 0;
        const int nodeWidth = 250;
        const int nodeHeight = 100;
        const int padding = 30;

        for (int i = 0; i < linedUp.Count; ++i)
        {
            for (int j = 0; j < linedUp[i].Count; ++j)
            {
                int x = padding + (j * 270);
                int y = padding + (i * 120);

                NodeControl control = new NodeControl(linedUp[i][j]);
                pipelineCanvas.Children.Add(control);
                Canvas.SetLeft(control, x);
                Canvas.SetTop(control, y);

                control.NodeClicked += OnNodeControlPressed;

                canvasMaxRight = Math.Max(canvasMaxRight, x + nodeWidth + padding);
                canvasMaxBottom = Math.Max(canvasMaxBottom, y + nodeHeight + padding);

                // make the first node to be selected by default
                if (_selectedNode == null)
                    OnNodeControlPressed(control, control.DataContext as Node);
            }
        }

        pipelineCanvas.Width = canvasMaxRight;
        pipelineCanvas.Height = canvasMaxBottom;
        UpdateCanvasTranslate(0, 0);
    }

    private void OnNodeControlPressed(object sender, Node node)
    {
        if (sender is not NodeControl control)
        {
            _selectedControl.IsSelected = false;
            return;
        }

        if (_selectedNode != null)
        {
            _selectedNode.ProcessOutputChanged -= Node_ProcessOutputChanged;
        }

        if (_selectedControl != null)
            _selectedControl.IsSelected = false;

        _selectedNode = node;
        _selectedControl = control;
        _selectedControl.IsSelected = true;
        _selectedNode.ProcessOutputChanged += Node_ProcessOutputChanged;

        // set current output id name
        var vm = DataContext as PipelinePageViewModel;
        vm.SelectedNodeId = _selectedNode.Id;

        avaloniaTextEditor.Text = _selectedNode.ProcessOutput;
        avaloniaTextEditor.ScrollToEnd();
    }

    private void Node_ProcessOutputChanged(object sender, string data)
    {
        // handle only process output
        if (sender is not Node node)
            return;

        var vm = DataContext as PipelinePageViewModel;
        avaloniaTextEditor.AppendText(data);
    }

    private TranslateTransform transform = new TranslateTransform();
    private ScaleTransform scaleTransform = new ScaleTransform();
    Point scrollMousePoint = new Point();
    Vector scrollOffset = new Vector();
    bool isCaptured = false;
    private void canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        scrollMousePoint = e.GetPosition(null);
        scrollOffset = new Vector(transform.X, transform.Y);
        isCaptured = true;
    }

    private void canvas_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        isCaptured = false;
    }

    private void canvas_PointerMoved(object sender, PointerEventArgs e)
    {
        if (isCaptured)
        {
            UpdateCanvasTranslate((scrollMousePoint.X - e.GetPosition(null).X), (scrollMousePoint.Y - e.GetPosition(null).Y));
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        UpdateCanvasTranslate(0, 0);
    }

    private void UpdateCanvasTranslate(double deltaX, double deltaY)
    {
        var scaledWidth = pipelineCanvas.Bounds.Width * scaleTransform.ScaleX;
        var scaledHeight = pipelineCanvas.Bounds.Height * scaleTransform.ScaleY;

        var maxX = Math.Max(0, scaledWidth - ContentMaskBorder.Bounds.Width);
        var maxY = Math.Max(0, scaledHeight - ContentMaskBorder.Bounds.Height);

        var tmpX = scrollOffset.X - deltaX;
        transform.X = -Math.Clamp(-tmpX, 0, maxX);
        var tmpY = scrollOffset.Y - deltaY;
        transform.Y = -Math.Clamp(-tmpY, 0, maxY);
    }
}
