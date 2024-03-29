
using Ribena.Events;
using System.Reflection;

namespace Ribena.Modules;

public enum ModuleLoadStatus
{
    /// <summary>
    /// The module was loaded successfully.
    /// </summary>
    Loaded,
    /// <summary>
    /// The module contains multiple classes that inherit from <see cref="IModule"/>
    /// </summary>
    MultipleDefinitions,
    /// <summary>
    /// The module contains no classes that inherit from <see cref="IModule"/>
    /// </summary>
    NoDefinitions,
    /// <summary>
    /// An exception was thrown while loading the module.
    /// </summary>
    LoadingThrewException
}

public class ModuleContext : EventComponent
{
    /// <summary>
    /// The name of the module loaded event.
    /// </summary>
    public const string ModuleLoadedEvent = "ModuleLoaded";
    /// <summary>
    /// The name of the module unloaded event.
    /// </summary>
    public const string ModuleUnloadEvent = "ModuleUnloaded";

    /// <summary>
    /// The current selected module, if there is one.
    /// </summary>
    public IModule? Module { get; internal set; }

    public ModuleContext()
    {
        RegisterEvent(ModuleLoadedEvent);
        RegisterEvent(ModuleUnloadEvent);
    }

    /// <summary>
    /// Switch context to a module inside of <paramref name="assembly"/>.
    /// This not only returns the module inside the <paramref name="module"/> argument, but it also 
    /// sets <see cref="Module"/> if it's loaded correctly.
    /// </summary>
    /// <param name="assembly">The assembly to load a module from</param>
    /// <param name="module">The module, this is null if it fails to load, otherwise it's the module</param>
    /// <returns>The status and a message related to the status.</returns>
    public (ModuleLoadStatus status, string message) SwitchContext(Assembly assembly, out IModule? module)
    {
        module = null;
        var types = assembly.GetTypes();
        var allModuleTypes = types.Where(x => x.IsAssignableTo(typeof(IModule)) && x != typeof(IModule));

        if (!allModuleTypes.Any())
            return (ModuleLoadStatus.NoDefinitions, $"The assembly `{assembly.FullName}` does not contain any classes that implement Ribena.Modules.IModule");
        else if (allModuleTypes.Count() > 1)
            return (ModuleLoadStatus.MultipleDefinitions, $"The assembly `{assembly.FullName}` contains too many classes that implement Ribena.Modules.IModule (keep them split, how can we know what to choose?)");
        var moduleType = allModuleTypes.First();

        try
        {
            module = (IModule)Activator.CreateInstance(moduleType)!;
        }
        catch (Exception e)
        {
            return (ModuleLoadStatus.LoadingThrewException, $"The module class `{moduleType.Name}` threw an exception during load. (Message: {e.Message})");
        }

        Module = module;
        return (ModuleLoadStatus.Loaded, $"The module `{module!.Name}` has loaded successfully.");
    }
}
