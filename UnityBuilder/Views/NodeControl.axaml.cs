using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UnityBuilder.Models;

namespace UnityBuilder.Views;

public partial class NodeControl : UserControl
{
    public NodeControl(Node node)
    {
        DataContext = node;
        InitializeComponent();
    }
}