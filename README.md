# Quick Note

A lightweight Windows application for quick note-taking with a floating action button (FAB) interface, similar to Android's floating menu.

## Features

- **Always-on-top floating button** - Draggable FAB that stays visible over all windows
- **Animated floating menu** - Android-style expanding menu with smooth animations
- **Plugin architecture** - Each feature is a separate plugin for easy extensibility
- **SQLite storage** - Memos are stored locally in a SQLite database

## Plugins

### Add Memo
- Quick memo creation with title and content
- Material Design-inspired UI

### List All Memos
- View all saved memos
- Delete memos with confirmation
- Empty state when no memos exist

## Requirements

- Windows 10/11
- .NET 8.0 SDK

## Building

```bash
cd c:\Projects\memos-popup
dotnet build
```

## Running

```bash
dotnet run --project src\MemosPopup\MemosPopup.csproj
```

## Project Structure

```
MemosPopup/
├── src/
│   ├── MemosPopup/              # Main WPF application
│   │   ├── MainWindow.xaml      # Floating FAB and menu
│   │   └── Styles/              # UI styles and colors
│   ├── MemosPopup.Core/         # Core library
│   │   ├── Data/                # SQLite database context and repository
│   │   ├── Models/              # Data models (Memo)
│   │   └── Plugins/             # Plugin interfaces and manager
│   └── Plugins/
│       ├── MemosPopup.Plugins.AddMemo/     # Add memo plugin
│       └── MemosPopup.Plugins.ListMemos/   # List memos plugin
└── MemosPopup.sln
```

## Database Location

The SQLite database is stored at:
```
%LOCALAPPDATA%\MemosPopup\memos.db
```

## Usage

1. Run the application - a floating orange button appears at the bottom-right
2. Drag the button anywhere on screen
3. Click the button to expand the menu
4. Select "Add" to create a new memo
5. Select "List All" to view/delete existing memos
6. Click outside the menu to close it

## Adding New Plugins

1. Create a new class library project
2. Reference `MemosPopup.Core`
3. Implement the `IPlugin` interface
4. Register your plugin in `App.xaml.cs`

```csharp
public class MyPlugin : IPlugin
{
    public string Name => "My Plugin";
    public string Description => "Description";
    public ImageSource? Icon => null;
    public int Order => 3;

    public void Execute(Window ownerWindow)
    {
        // Your plugin logic
    }

    public FrameworkElement CreateMenuButton()
    {
        return new Button { Content = "M" };
    }
}
```
