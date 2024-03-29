
namespace Ribena.Guts;

/// <summary>
/// A result type, that can be either an <see cref="int"/> or <see cref="CommandError"/>.
/// There are implicit operators defined, so they can be returned naturally.
/// </summary>
public class CommandResult
{
    private int? _code;
    private CommandError? _error;

    public static implicit operator CommandResult(int code)
    {
        return new CommandResult { _code = code };
    }

    public static implicit operator CommandResult(CommandError error)
    {
        return new CommandResult { _error = error };
    }

    /// <summary>
    /// Is this an ok result, I.E is the code not null.
    /// </summary>
    /// <returns></returns>
    public bool IsOk() => _code.HasValue;

    /// <summary>
    /// The code. It is not guaranteed that this will not throw without first calling
    /// <see cref="IsOk"/>.
    /// </summary>
    public int Code => _code!.Value;

    /// <summary>
    /// The error. It is not guaranteed that this will not throw without first calling
    /// <see cref="IsOk"/>. Please note, this will not throw when <see cref="IsOk"/> returns false.
    /// </summary>
    public CommandError Error => _error!;
}
