using System.IO;
using System.Reflection;

namespace MemosPopup.Core.Plugins;

public class PluginManager
{
    private static PluginManager? _instance;
    private readonly List<IPlugin> _plugins = new();

    public static PluginManager Instance => _instance ??= new PluginManager();

    public IReadOnlyList<IPlugin> Plugins => _plugins.OrderBy(p => p.Order).ToList();

    private PluginManager() { }

    public void RegisterPlugin(IPlugin plugin)
    {
        if (!_plugins.Any(p => p.Name == plugin.Name))
        {
            _plugins.Add(plugin);
        }
    }

    public void LoadPluginsFromAssembly(Assembly assembly)
    {
        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in pluginTypes)
        {
            if (Activator.CreateInstance(type) is IPlugin plugin)
            {
                RegisterPlugin(plugin);
            }
        }
    }

    public void DiscoverPlugins(string pluginsPath)
    {
        if (!Directory.Exists(pluginsPath))
            return;

        var dllFiles = Directory.GetFiles(pluginsPath, "*.dll");
        foreach (var dll in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                LoadPluginsFromAssembly(assembly);
            }
            catch
            {
                // Skip invalid assemblies
            }
        }
    }
}
