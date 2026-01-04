using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MemosPopup.Core.Data;
using MemosPopup.Core.Models;

namespace MemosPopup.Plugins.ListMemos;

public partial class ListMemosWindow : Window
{
    private readonly MemoRepository _repository;

    public ListMemosWindow()
    {
        InitializeComponent();
        _repository = new MemoRepository();
        
        Loaded += async (s, e) => await LoadMemosAsync();
        MemoRepository.MemosChanged += OnMemosChanged;
        Closed += (s, e) => MemoRepository.MemosChanged -= OnMemosChanged;
    }

    private async void OnMemosChanged(object? sender, EventArgs e)
    {
        await Dispatcher.InvokeAsync(async () => await LoadMemosAsync());
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private async Task LoadMemosAsync()
    {
        try
        {
            var memos = await _repository.GetAllMemosAsync();
            MemosItemsControl.ItemsSource = memos;
            
            EmptyState.Visibility = memos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load memos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int memoId)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this memo?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _repository.DeleteMemoAsync(memoId);
                    await LoadMemosAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete memo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
