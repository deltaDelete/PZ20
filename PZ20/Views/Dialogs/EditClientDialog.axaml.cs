using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PZ20.Models;

namespace PZ20.Views.Dialogs; 

public partial class EditClientDialog : Window {
    private readonly Action<Client> _confirmAction;

    public EditClientDialog(Client client, Action<Client> confirmAction, string title = "") {
        _confirmAction = confirmAction;
        InitializeComponent();
        DataContext = client;
        Title = title;
        InitializeGenderBox();
    }

    private async void InitializeGenderBox() {
        await using var db = new MyDatabase();
        GenderBox.ItemsSource = await db.GetAsync<Gender>().ToListAsync();
        GenderBox.SelectedIndex = (DataContext as Client)!.GenderId - 1;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        _confirmAction.Invoke((DataContext as Client)!);
        Close();
    }
}