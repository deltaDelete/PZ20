using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ20.ViewModels;

namespace PZ20.Views; 

public partial class MainView : UserControl {
    
    public MainView() {
        InitializeComponent();
    }

    public static readonly DirectProperty<MainView, Window?> WindowProperty =
        AvaloniaProperty.RegisterDirect<MainView, Window?>(
            nameof(Window), 
            view => view.Window, 
            (view, window) => view.Window = window, 
            null,
            BindingMode.Default);

    private Window? _window;

    public Window? Window {
        get => _window;
        set => SetAndRaise(WindowProperty, ref _window, value);
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        if (Window is null) return;
        DataContext = new MainViewModel(Window);
    }
}