using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using UnityBuilder.Models;

namespace UnityBuilder.Views
{
    public partial class NodeControl : UserControl
    {
        private readonly Node _node;

        public event EventHandler<Node> NodeClicked;

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                this.FindControl<Button>("CardButton")?.Classes.Set("selected", value);
            }
        }

        public NodeControl(Node node)
        {
            _node = node;
            DataContext = node;
            InitializeComponent();
            this.FindControl<Button>("CardButton").Click += OnCardClick;
        }

        private void OnCardClick(object? sender, RoutedEventArgs e)
        {
            NodeClicked?.Invoke(this, _node);
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            var currentNode = DataContext as Node;
            if (!currentNode.CancellationTokenSource.IsCancellationRequested)
            {
                currentNode.State = Models.Enums.NodeState.Cancelled;
                currentNode.CancellationTokenSource.Cancel();
            }
        }
    }
}