using Ribena.Config;
using System.Reflection;

namespace Ribena.Modules;

/// <summary>
/// A loaded module that has been cached to avoid reloading.
/// </summary>
/// <param name="Path">The full path to this cached module</param>
/// <param name="Module">The actual module itself</param>
public record class CachedRepositoryItem(
    string Path,
    IModule Module
);

/// <summary>
/// What kind of refresh should take place when calling <see cref="ModuleRepository.Refresh"/>
/// </summary>
public enum ModuleRefreshKind
{
    /// <summary>
    /// Perform a full refresh of the module repository.
    /// </summary>
    FullRefresh,

    /// <summary>
    /// Only perform a partial refresh of the module repository, this will only load
    /// new modules that are not already loaded.
    /// </summary>
    LoadNew,
}

/// <summary>
/// A place to cache loaded modules. This is to avoid loading them all multiple times.
/// </summary>
public class ModuleRepository
{
    /// <summary>
    /// This class is the master of loading modules. We use a dummy version of this
    /// to load them. This does not actually affect the primary module context. 
    /// </summary>
    private readonly ModuleContext DummyContext;

    /// <summary>
    /// The actual cache. This contains non-duplicate instances of modules.
    /// </summary>
    private List<CachedRepositoryItem> CachedModules { get; } = [];

    /// <summary>
    /// The directory we are looking for modules.
    /// </summary>
    private readonly Settings Modules;

    public ModuleRepository(Settings modules)
    {
        DummyContext = new ModuleContext();
        Modules = modules;

        Refresh();
    }

    /// <summary>
    /// Perform a full or partial refresh, depending on <paramref name="kind"/>. The default behaviour is a full refresh.
    /// If a full refresh occurs, the cache is flushed and all modules are reloaded.
    /// If we are in <see cref="ModuleRefreshKind.LoadNew"/>, only modules that are not yet loaded are.
    /// </summary>
    public void Refresh(ModuleRefreshKind kind = ModuleRefreshKind.FullRefresh)
    {
        if (kind == ModuleRefreshKind.FullRefresh)
            CachedModules.Clear();

        var modulePath = Modules.GetPath();
        // We find all modules that end with "Module.dll", so for example, "Ribena.MainModule.dll" would match.
        var allModules = Directory.GetFiles(modulePath, "*Module.dll", SearchOption.AllDirectories);

        Info($"All files prepared for cache refresh: [{string.Join(", ", allModules)}]");

        foreach (var module in allModules)
        {
            if (kind == ModuleRefreshKind.LoadNew)
            {
                if (CachedModules.Any(x => x.Path == module))
                    continue;
            }

            Assembly? sharpLibrary;

            try
            {
                sharpLibrary = Assembly.LoadFrom(module);
            }
            catch (Exception e)
            {
                Warn($"Failed to load CSharp assembly: `{module}`");
                Warn($"  Reason: {e}");
                continue;
            }

            var (status, message)
                = DummyContext.SwitchContext(sharpLibrary, out IModule? loadedModule);

            if (status != ModuleLoadStatus.Loaded)
            {
                Warn($"ModuleRepository failed to load module: {message}");
                Warn($"  ^ Please note, this is related to the module itself. We loaded the assembly fine.");
                continue;
            }

            CachedModules.Add(new CachedRepositoryItem(module, loadedModule!));
        }
    }

    /// <summary>
    /// Get all modules that are currently cached.
    /// </summary>
    /// <returns></returns>
    public List<IModule> GetCache()
        => CachedModules.Select(x => x.Module).ToList();
}
