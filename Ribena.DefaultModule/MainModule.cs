using Ribena.Commands;
using Ribena.Guts;
using Ribena.Modules;

namespace Ribena.DefaultModule;

public class DefaultTestingCommand : ICommand
{
    public string Name => "test";

    public string Description => "This is a test description";

    public CommandResult Execute(ReadOnlySpan<string> args, Terminal context)
    {
        VConsole.WriteLine("This is a module inside Ribena.");
        return 0;
    }

    public string? GetDocumentation()
    {
        return null;
    }
}

public class MainModule : CommandAdapter, IModule
{
    public string Name => "Ribena";

    public string BriefDescription => "The default Ribena module. Used for installing modules and debugging.";

    public VersionInfo Version => new(0, 1, "dev");

    public AuthorInfo Author => new("Deeton Rushton", null);

    public List<ICommand> GetCommands()
    {
        return Adapt();
    }

    public bool Init(Terminal context)
    {
        VConsole.WriteLine("Default module has loaded!");
        return true;
    }
}
