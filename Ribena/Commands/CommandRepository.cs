
namespace Ribena.Commands;

public class CommandRepository
{
    /// <summary>
    /// The actual commands.
    /// </summary>
    private List<ICommand> LoadedCommands;

    /// <summary>
    /// Initialize to it's defaults, with an empty state.
    /// </summary>
    public CommandRepository()
    {
        LoadedCommands = [];
    }

    /// <summary>
    /// Initialize with already loaded commands.
    /// </summary>
    /// <param name="loadedCommands"></param>
    public CommandRepository(List<ICommand> loadedCommands)
    {
        LoadedCommands = loadedCommands;
    }

    /// <summary>
    /// Static way of calling <see cref="CommandRepository(List{ICommand})"/>
    /// </summary>
    /// <param name="pollution">The preloaded commands.</param>
    /// <returns></returns>
    public static CommandRepository Pollute(List<ICommand> pollution)
    {
        return new CommandRepository(pollution);
    }
}
