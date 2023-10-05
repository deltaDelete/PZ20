using Avalonia.Controls;
using PZ20.Views;

namespace PZ20;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        Panel.Children.Add(new MainView(this));
    }
}