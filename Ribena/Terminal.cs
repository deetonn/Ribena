using Ribena.Config;
using Ribena.Modules;
using Ribena.Guts;
using Ribena.Commands;

using CommandLine;

using Sharprompt;

namespace Ribena;

/// <summary>
/// The main Ribena terminal. This is monolith of all the working parts of this project
/// all working together.
/// </summary>
public class Terminal
{
    /// <summary>
    /// This terminal contexts environment.
    /// </summary>
    public readonly Environment Env;

    /// <summary>
    /// The module context.
    /// </summary>
    public readonly ModuleContext ModuleContext;

    /// <summary>
    /// The very root settings instance. This points to the root
    /// of our configuration directory.
    /// </summary>
    public readonly Settings RootSettings;

    /// <summary>
    /// The command repository. All commands from the current loaded
    /// module are here.
    /// </summary>
    public CommandRepository Commands { get; internal set; }

    /// <summary>
    /// The module repository. Caches modules and can perform refreshes.
    /// </summary>
    public ModuleRepository ModuleRepository { get; internal set; }

    /// <summary>
    /// Internal flag to see if we should still be executing.
    /// </summary>
    internal bool Running { get; set; } = true;

    /// <summary>
    /// The settings folder where modules are located.
    /// </summary>
    internal Settings ModuleDirectory { get; }

    /// <summary>
    /// The command line arguments passed to Ribena.
    /// </summary>
    internal TerminalArguments? Arguments { get; set; }

    /// <summary>
    /// Initialize the terminal correctly.
    /// </summary>
    public Terminal()
    {
        Env = new Environment();
        ModuleContext = new ModuleContext();
        RootSettings = new Settings();
        Commands = new CommandRepository();
        ModuleDirectory = RootSettings.CreateInnerDirectory("modules");
        ModuleRepository = new ModuleRepository(ModuleDirectory);
    }

    public int Execute(ReadOnlySpan<string> arguments)
    {
        // Parse out any commandline arguments.
        Parser.Default.ParseArguments<TerminalArguments>(arguments.ToArray())
            .WithParsed(a => Arguments = a);

        while (Running)
        {
            while (ModuleContext.Module is null)
            {
                var (ok, msg) = InitializeModuleContext(Arguments?.Module);
                // TODO: REMOVEME: This is for development.
                VConsole.WriteLine(msg);
            }
            // Pollute the command repository with the commands related to this module.
            Commands = CommandRepository.Pollute(ModuleContext.Module.GetCommands());
        }
        
        return 0;
    }

    public (bool Ok, string Message) InitializeModuleContext(string? AutoloadModule)
    {
        if (AutoloadModule is not null)
        {
            return ActuallyLoadModule(AutoloadModule);
        }

        var moduleFolder = ModuleDirectory;
        // We collect all modules inside the modules folder
        // And filter them by name.
        var availableModules = ModuleRepository.GetCache()
            .Select(x => new { Key = $"{x.Name} - {x.BriefDescription}", Value = x })
            .ToDictionary(t => t.Key, t => t.Value);

        if (availableModules.Count == 0)
        {
            VConsole.WriteLine("You have not installed any modules yet.");
            VConsole.WriteLine("We do install with a default module, however it's been deleted.");
            VConsole.WriteLine($"Please install a module here: {moduleFolder.GetPath()}");
            System.Environment.Exit(-1);
        }

        var selectedModuleName = Prompt.Select("Select a module to load", availableModules.Keys);
        var selectedModule = availableModules[selectedModuleName];

        ModuleContext.Module = selectedModule;
        if (!ModuleContext.Module.Init(this))
        {
            return (false, $"The module `{ModuleContext.Module.Name}` didn't initialize correctly.");
        }
        // Clean up any output from the modules Init function.
        VConsole.Clear();
        return (true, "Successfully loaded cached module.");
    }

    private (bool Ok, string Message) ActuallyLoadModule(string moduleName)
    {
        var moduleFolder = ModuleDirectory;
        var assembly = moduleFolder.TryGetAssembly(moduleName);

        if (assembly is null)
        {
            if (Arguments is not null)
                Arguments.Module = null;
            return (false, "Failed to autoload assembly, check the logfile for more information.");
        }

        var (status, message) = ModuleContext.SwitchContext(assembly, out var module);
        if (status != ModuleLoadStatus.Loaded)
        {
            return (false, $"{status}: {message}");
        }

        // SwitchContext(...) will have changed the module to the valid module!
        return (true, "Successfully switched module");
    }
}
