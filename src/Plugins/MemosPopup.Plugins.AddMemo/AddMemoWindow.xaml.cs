using System.Windows;
using System.Windows.Input;
using MemosPopup.Core.Data;
using MemosPopup.Core.Models;

namespace MemosPopup.Plugins.AddMemo;

public partial class AddMemoWindow : Window
{
    private readonly MemoRepository _repository;

    public AddMemoWindow()
    {
        InitializeComponent();
        _repository = new MemoRepository();
        
        Loaded += (s, e) => TitleTextBox.Focus();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        var content = ContentTextBox.Text.Trim();

        if (string.IsNullOrEmpty(title))
        {
            MessageBox.Show("Please enter a title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleTextBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(content))
        {
            MessageBox.Show("Please enter content.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ContentTextBox.Focus();
            return;
        }

        var memo = new Memo
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.Now
        };

        try
        {
            await _repository.AddMemoAsync(memo);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save memo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
