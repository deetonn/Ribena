
using Ribena.Guts;

namespace Ribena.Commands;

public interface ICommand
{
    /// <summary>
    /// The name of the command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A simple description for your command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Get the documentation for this command. This is optional for development, but for release
    /// modules it's expected to be a thorough description on every peice of logic this command
    /// offers.
    /// </summary>
    /// <returns>The string containing documentation, this supports Spectre.Console markup</returns>
    public string? GetDocumentation();

    /// <summary>
    /// The actual "entry point" for the command. This is where code execution begins
    /// when the command is ran from the terminal.
    /// </summary>
    /// <param name="args">The arguments passed to the command</param>
    /// <param name="context">The context</param>
    /// <returns></returns>
    public CommandResult Execute(ReadOnlySpan<string> args, Terminal context);
}
