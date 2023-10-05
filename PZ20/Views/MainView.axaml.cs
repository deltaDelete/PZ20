using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PZ20.ViewModels;

namespace PZ20.Views; 

public partial class MainView : UserControl {
    public MainView(Window window) {
        InitializeComponent();
        DataContext = new MainViewModel(window);
    }
}