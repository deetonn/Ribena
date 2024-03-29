
using Ribena.Guts;

namespace Ribena.Commands;

public interface ICommandExecutor
{
    /// <summary>
    /// Execute a command right now. This function MUST block and return 
    /// the result without switching threads.
    /// </summary>
    /// <param name="command">The actual command to execute</param>
    /// <param name="args">The arguments passed to this command</param>
    /// <param name="context">The terminal context</param>
    CommandResult Execute(ICommand command, ReadOnlySpan<string> args, Terminal context);
}

public class DefaultCommandExecutor : ICommandExecutor
{
    public CommandResult Execute(ICommand command, ReadOnlySpan<string> args, Terminal context)
    {
        return command.Execute(args, context);
    }
}
