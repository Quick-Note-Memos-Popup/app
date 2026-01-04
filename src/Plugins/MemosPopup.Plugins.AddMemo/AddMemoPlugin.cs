using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MemosPopup.Core.Plugins;

namespace MemosPopup.Plugins.AddMemo;

public class AddMemoPlugin : IPlugin
{
    public string Name => "Add";
    public string Description => "Add a new memo";
    public ImageSource? Icon => new BitmapImage(
        new Uri("pack://application:,,,/MemosPopup.Plugins.AddMemo;component/Assets/add-icon.png"));
    public int Order => 1;

    public void Execute(Window ownerWindow)
    {
        var dialog = new AddMemoWindow
        {
            Owner = ownerWindow
        };
        dialog.ShowDialog();
    }

    public FrameworkElement CreateMenuButton()
    {
        return new System.Windows.Controls.Button
        {
            Content = "+"
        };
    }
}
