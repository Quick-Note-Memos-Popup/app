using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MemosPopup.Core.Plugins;

namespace MemosPopup.Plugins.ListMemos;

public class ListMemosPlugin : IPlugin
{
    public string Name => "List All";
    public string Description => "View all saved memos";
    public ImageSource? Icon => new BitmapImage(
        new Uri("pack://application:,,,/MemosPopup.Plugins.ListMemos;component/Assets/list-icon.png"));
    public int Order => 2;

    public void Execute(Window ownerWindow)
    {
        var window = new ListMemosWindow();
        window.Show();
    }

    public FrameworkElement CreateMenuButton()
    {
        return new System.Windows.Controls.Button
        {
            Content = "≡"
        };
    }
}
