using CommandLine;

namespace Ribena.Guts;

public class TerminalArguments
{
    /// <summary>
    /// A module to load automatically.
    /// </summary>
    [Option('m', "module")]
    public string? Module { get; set; }

    /// <summary>
    /// A script to execute within <see cref="Module"/>. This can only be present
    /// when <see cref="Module"/> is too.
    /// </summary>
    [Option('s', "script")]
    public string? Script { get; set; }
}
