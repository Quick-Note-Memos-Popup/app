using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MemosPopup.Core.Plugins;
using WpfButton = System.Windows.Controls.Button;
using WpfImage = System.Windows.Controls.Image;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace MemosPopup;

public partial class MainWindow : Window
{
    private bool _isDragging;
    private System.Windows.Point _dragStartPoint;
    private System.Windows.Point _windowStartPosition;
    private bool _isMenuOpen;
    private DateTime _mouseDownTime;
    private const int DragThresholdMs = 150;
    private const double DragThresholdPixels = 5;
    private Window? _menuWindow;
    private StackPanel? _menuPanel;

    public MainWindow()
    {
        InitializeComponent();
        
        // Position window at bottom-right of primary screen
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 20;
        Top = workArea.Bottom - Height - 20;
    }

    private FrameworkElement CreateMenuItem(IPlugin plugin)
    {
        var container = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 0, 12),
            Opacity = 0,
            RenderTransform = new TranslateTransform(0, 20)
        };

        // Label
        var label = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12, 6, 12, 6),
            Margin = new Thickness(0, 0, 8, 0),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 4,
                ShadowDepth = 1,
                Opacity = 0.2
            },
            Child = new TextBlock
            {
                Text = plugin.Name,
                FontSize = 13,
                Foreground = new SolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString("#212121"))
            }
        };

        // Button
        var button = new WpfButton
        {
            Style = (Style)FindResource("MiniFabStyle"),
            Tag = plugin
        };

        // Set icon content
        if (plugin.Icon != null)
        {
            button.Content = new WpfImage { Source = plugin.Icon, Width = 20, Height = 20 };
        }
        else
        {
            button.Content = new TextBlock
            {
                Text = plugin.Name.Substring(0, 1),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
        }

        button.Click += (s, e) =>
        {
            CloseMenu();
            plugin.Execute(this);
        };

        container.Children.Add(label);
        container.Children.Add(button);

        return container;
    }

    private void MainFabButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _mouseDownTime = DateTime.Now;
        _dragStartPoint = PointToScreen(e.GetPosition(this));
        _windowStartPosition = new System.Windows.Point(Left, Top);
        _isDragging = false;
        MainFabButton.CaptureMouse();
    }

    private void MainFabButton_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (MainFabButton.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentScreenPoint = PointToScreen(e.GetPosition(this));
            var deltaX = currentScreenPoint.X - _dragStartPoint.X;
            var deltaY = currentScreenPoint.Y - _dragStartPoint.Y;

            if (!_isDragging && (Math.Abs(deltaX) > DragThresholdPixels || Math.Abs(deltaY) > DragThresholdPixels))
            {
                _isDragging = true;
                if (_isMenuOpen)
                {
                    CloseMenu();
                }
            }

            if (_isDragging)
            {
                var newLeft = _windowStartPosition.X + deltaX;
                var newTop = _windowStartPosition.Y + deltaY;

                // Keep within screen bounds (use ActualWidth/Height for dynamic sizing)
                var workArea = SystemParameters.WorkArea;
                Left = Math.Max(workArea.Left, Math.Min(newLeft, workArea.Right - ActualWidth));
                Top = Math.Max(workArea.Top, Math.Min(newTop, workArea.Bottom - ActualHeight));
            }
        }
    }

    private void MainFabButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        MainFabButton.ReleaseMouseCapture();

        if (_isDragging)
        {
            _isDragging = false;
            e.Handled = true;
        }
        else
        {
            // It was a click, not a drag
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (_isMenuOpen || _menuWindow != null)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        _isMenuOpen = true;

        // Animate FAB icon rotation
        var rotateAnimation = new DoubleAnimation(0, 45, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        FabIconRotation.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

        // Create and show menu popup window
        CreateMenuWindow();
    }

    private void CreateMenuWindow()
    {
        // Determine if menu should appear above or below FAB
        var workArea = SystemParameters.WorkArea;
        var fabScreenPos = PointToScreen(new System.Windows.Point(0, 0));
        var spaceAbove = fabScreenPos.Y - workArea.Top;
        var spaceBelow = workArea.Bottom - (fabScreenPos.Y + Height);
        // Show below if there's more space below, otherwise show above
        var showBelow = spaceBelow >= spaceAbove;

        // Create menu panel
        _menuPanel = new StackPanel
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };

        // Add menu items
        foreach (var plugin in PluginManager.Instance.Plugins)
        {
            var menuItem = CreateMenuItem(plugin);
            _menuPanel.Children.Add(menuItem);
        }

        // Create popup window
        _menuWindow = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = this
        };

        _menuWindow.Content = _menuPanel;

        // Position the menu window
        _menuWindow.Show();

        // Calculate position after window is shown (so we have actual size)
        double menuLeft;
        double menuTop;

        // Horizontal positioning - check if menu would go off left edge
        var preferredLeft = fabScreenPos.X + Width - _menuWindow.ActualWidth;
        if (preferredLeft < workArea.Left)
        {
            // Align menu to left edge of FAB instead
            menuLeft = fabScreenPos.X;
        }
        else if (preferredLeft + _menuWindow.ActualWidth > workArea.Right)
        {
            // Menu would go off right edge
            menuLeft = workArea.Right - _menuWindow.ActualWidth;
        }
        else
        {
            menuLeft = preferredLeft;
        }

        // Vertical positioning
        if (showBelow)
        {
            menuTop = fabScreenPos.Y + Height + 8;
        }
        else
        {
            menuTop = fabScreenPos.Y - _menuWindow.ActualHeight - 8;
        }

        _menuWindow.Left = menuLeft;
        _menuWindow.Top = menuTop;

        // Animate menu items with stagger
        var delay = 0;
        foreach (FrameworkElement item in _menuPanel.Children)
        {
            AnimateMenuItemIn(item, delay, showBelow);
            delay += 50;
        }
    }

    private void CloseMenu()
    {
        _isMenuOpen = false;

        // Animate FAB icon rotation back
        var rotateAnimation = new DoubleAnimation(45, 0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        FabIconRotation.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

        if (_menuPanel == null || _menuWindow == null) return;

        // Animate menu items out
        var delay = 0;
        var items = _menuPanel.Children.Cast<FrameworkElement>().Reverse().ToList();
        foreach (var item in items)
        {
            AnimateMenuItemOut(item, delay);
            delay += 30;
        }

        // Close window after animation
        var menuWindowToClose = _menuWindow;
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200 + delay)
        };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            menuWindowToClose?.Close();
        };
        timer.Start();

        _menuWindow = null;
        _menuPanel = null;
    }

    private void AnimateMenuItemIn(FrameworkElement item, int delayMs, bool fromAbove = false)
    {
        var transform = item.RenderTransform as TranslateTransform ?? new TranslateTransform();
        item.RenderTransform = transform;

        var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // Animate from above or below based on menu position
        var startY = fromAbove ? -20 : 20;
        var translateAnimation = new DoubleAnimation(startY, 0, TimeSpan.FromMilliseconds(200))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        item.BeginAnimation(OpacityProperty, opacityAnimation);
        transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
    }

    private void AnimateMenuItemOut(FrameworkElement item, int delayMs)
    {
        var transform = item.RenderTransform as TranslateTransform ?? new TranslateTransform();
        item.RenderTransform = transform;

        var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var translateAnimation = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(150))
        {
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        item.BeginAnimation(OpacityProperty, opacityAnimation);
        transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
    }

    private void OverlayBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isMenuOpen)
        {
            CloseMenu();
        }
    }
}
