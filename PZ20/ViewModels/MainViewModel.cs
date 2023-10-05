using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using PZ20.Models;
using PZ20.Views.Dialogs;

namespace PZ20.ViewModels;

public class MainViewModel : ViewModelBase {
    private readonly Window _view;
    private string _searchQuery = string.Empty;
    private ObservableCollection<Client> _items = new();
    private List<Client> _itemsFull;
    private int _selectedSearchColumn;
    private bool _isSortByDescending = false;
    private int _take = 10;
    private int _skip = 0;
    private int _currentPage = 1;
    private List<Client> _filtered = new List<Client>();
    private bool _isLoading = true;
    private Client _selectedRow;

    #region Notifying Properties

    public int SelectedSearchColumn {
        get => _selectedSearchColumn;
        set {
            RaisePropertyChanging();
            if (value == _selectedSearchColumn) return;
            _selectedSearchColumn = value;
            RaisePropertyChanged();
        }
    }

    public bool IsSortByDescending {
        get => _isSortByDescending;
        set => SetField(ref _isSortByDescending, value);
    }

    public string SearchQuery {
        get => _searchQuery;
        set {
            if (value == _searchQuery) return;
            _searchQuery = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<Client> Items {
        get => _items;
        set {
            if (Equals(value, _items)) return;
            _items = value;
            RaisePropertyChanged();
        }
    }

    public int Take {
        get => _take;
        set => SetField(ref _take, value);
    }

    public int Skip {
        get => _skip;
        set {
            if (value >= _itemsFull.Count) {
                return;
            }

            if (!SetField(ref _skip, value)) {
                return;
            }

            CurrentPage = (int)Math.Ceiling(value / (double)Take) + 1;
        }
    }

    public int CurrentPage {
        get => _currentPage;
        set {
            if (!SetField(ref _currentPage, value)) {
                return;
            }

            ReevaluateCommands();
        }
    }

    public int TotalPages => (int)Math.Ceiling(Filtered.Count / (double)Take);

    public List<Client> Filtered {
        get => _filtered;
        set {
            if (SetField(ref _filtered, value)) {
                TakeFirst();
                RaisePropertyChanged(nameof(TotalPages));
            }
        }
    }

    public bool IsLoading {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public Client SelectedRow {
        get => _selectedRow;
        set {
            if (!SetField(ref _selectedRow, value)) {
                return;
            }
            RemoveItemCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
            EditItemCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
        }
    }

    #endregion

    public Command<Client> EditItemCommand { get; }
    public Command<Client> RemoveItemCommand { get; }
    public ICommand NewItemCommand { get; }
    public Command TakeNextCommand { get; }
    public Command TakePrevCommand { get; }
    public Command TakeFirstCommand { get; }
    public Command TakeLastCommand { get; }

    public MainViewModel(Window view) {
        _view = view;
        EditItemCommand = new Command<Client>(EditItem, client => client is not null);
        RemoveItemCommand = new Command<Client>(RemoveItem, client => client is not null);
        NewItemCommand = new AsyncCommand(NewItem);
        GetDataFromDb();
        TakeNextCommand = new Command(TakeNext, () => CurrentPage < TotalPages);
        TakePrevCommand = new Command(TakePrev, () => CurrentPage > 1);
        TakeFirstCommand = new Command(TakeFirst, () => CurrentPage > 1);
        TakeLastCommand = new Command(TakeLast, () => CurrentPage < TotalPages);
        PropertyChanged += OnSearchChanged;
    }

    private void OnSearchChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(SearchQuery)
            && e.PropertyName != nameof(SelectedSearchColumn)
            && e.PropertyName != nameof(IsSortByDescending)) {
            return;
        }

        var filtered = SearchQuery == ""
            ? new ObservableCollection<Client>(_itemsFull)
            : SelectedSearchColumn switch {
                1 => _itemsFull
                    .Where(it => it.ClientId.ToString().Contains(SearchQuery)),
                2 => _itemsFull
                    .Where(it => it.LastName.ToLower().Contains(SearchQuery.ToLower())),
                3 => _itemsFull
                    .Where(it => it.FirstName.ToLower().Contains(SearchQuery.ToLower())),
                4 => _itemsFull
                    .Where(it => it.Gender!.Name.ToLower().Contains(SearchQuery.ToLower())),
                _ => _itemsFull
                    .Where(it =>
                        it.LastName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.FirstName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.ClientId.ToString().Contains(SearchQuery) ||
                        it.Gender!.Name.ToLower().Contains(SearchQuery.ToLower())
                    )
            };

        Filtered = SelectedSearchColumn switch {
            2 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.LastName).ToList()
                : filtered.OrderBy(it => it.LastName).ToList(),
            3 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.FirstName).ToList()
                : filtered.OrderBy(it => it.FirstName).ToList(),
            4 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.Gender!.Name).ToList()
                : filtered.OrderBy(it => it.Gender!.Name).ToList(),
            _ => IsSortByDescending
                ? filtered.OrderByDescending(it => it.ClientId).ToList()
                : filtered.OrderBy(it => it.ClientId).ToList()
        };
    }

    private async void GetDataFromDb() {
        await Task.Run(async () => {
            IsLoading = true;
            await using var db = new MyDatabase();
            var users = db.GetAsync<Client>();
            var list = await users.ToListAsync();
            list = list.Select(it => {
                it.Gender = it.Gender = db.GetById<Gender>(it.GenderId);
                return it;
            }).ToList();
            _itemsFull = list;
            Filtered = _itemsFull;
            IsLoading = false;
            Dispatcher.UIThread.Invoke(ReevaluateCommands);
            SearchQuery = string.Empty;
            return Task.CompletedTask;
        });
    }

    private async void RemoveItem(Client? arg) {
        if (arg is null) return;
        await new ConfirmationDialog(
            "Вы собираетесь удалить строку",
            $"Пользователь: {arg.LastName} {arg.FirstName}",
            async dialog => {
                await using var db = new MyDatabase();
                await db.RemoveAsync(arg);
                // GetDataFromDb();
                RemoveLocal(arg);
            },
            dialog => { }
        ).ShowDialog(_view);
    }

    private void RemoveLocal(Client arg) {
        Items.Remove(arg);
        _itemsFull.Remove(arg);
        Filtered.Remove(arg);
    }

    private async void EditItem(Client? arg) {
        if (arg is null) return;
        await new EditClientDialog(
            arg,
            async client => {
                await using var db = new MyDatabase();
                client.GenderId = client.Gender!.GenderId;
                await db.UpdateAsync(client.ClientId, client);
                // GetDataFromDb();
                _itemsFull.Remove(arg);
                _itemsFull.Add(client);
            }
        ).ShowDialog(_view);
    }

    private async Task NewItem() {
        await new EditClientDialog(
            new Client(),
            async client => {
                await using var db = new MyDatabase();
                client.GenderId = client.Gender!.GenderId;
                int newItemId = await db.InsertAsync(client);
                // GetDataFromDb();
                client.ClientId = newItemId;
                _itemsFull.Add(client);
            }
        ).ShowDialog(_view);
    }

    private void TakeNext() {
        Skip += Take;
        Items = new ObservableCollection<Client>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakePrev() {
        Skip -= Take;
        Items = new ObservableCollection<Client>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakeFirst() {
        Skip = 0;
        Items = new ObservableCollection<Client>(
            Filtered.Take(Take)
        );
    }

    private void TakeLast() {
        Skip = Filtered.Count - Take;
        Items = new ObservableCollection<Client>(
            Filtered.TakeLast(Take)
        );
    }

    private void ReevaluateCommands() {
        TakeFirstCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
        TakePrevCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
        TakeNextCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
        TakeLastCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
    }
}