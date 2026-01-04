using System.Drawing;
using System.Windows;
using MemosPopup.Core.Plugins;
using MemosPopup.Plugins.AddMemo;
using MemosPopup.Plugins.ListMemos;
using Forms = System.Windows.Forms;

namespace MemosPopup;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _notifyIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Register built-in plugins
        PluginManager.Instance.RegisterPlugin(new AddMemoPlugin());
        PluginManager.Instance.RegisterPlugin(new ListMemosPlugin());
        
        // Setup system tray icon
        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Visible = true,
            Text = "Quick Note"
        };

        var contextMenu = new Forms.ContextMenuStrip();
        
        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApplication();
        
        var showItem = new Forms.ToolStripMenuItem("Show");
        showItem.Click += (s, e) => MainWindow?.Activate();
        
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => MainWindow?.Activate();
    }

    private Icon LoadTrayIcon()
    {
        var uri = new Uri("pack://application:,,,/app-icon.ico");
        using var stream = GetResourceStream(uri).Stream;
        return new Icon(stream);
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
