using System.Windows;
using System.Windows.Media;

namespace MemosPopup.Core.Plugins;

public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    ImageSource? Icon { get; }
    int Order { get; }
    
    void Execute(Window ownerWindow);
    FrameworkElement CreateMenuButton();
}
