using System.Text;

namespace Ribena.Guts;

/// <summary>
/// Any error that happened inside of a command. This is a visual representation
/// and is only really meant for commands that 
/// </summary>
public class CommandError(string message, List<string>? notes = null, int? position = null, SourceInfo? sourceInfo = null)
{
    /// <summary>
    /// The main message of the error.
    /// </summary>
    public readonly string Message = message;

    /// <summary>
    /// Any attached notes.
    /// </summary>
    public readonly List<string>? Notes = notes;

    /// <summary>
    /// The position (in the executed string) that the error occured.
    /// If this is null, it's unknown and no extra information is shown about
    /// position.
    /// </summary>
    public int? Position = position ?? sourceInfo?.Column ?? null;

    /// <summary>
    /// Any source information about the (if there is one) source file this command was executed from.
    /// </summary>
    public SourceInfo? SourceInfo = sourceInfo;

    /// <summary>
    /// Format the data into a user readable error message.
    /// </summary>
    /// <param name="inputString">The input the user did to cause this error</param>
    /// <returns></returns>
    public string Format(string inputString)
    {
        if (Position is null)
            return _DoFormatWithoutPosition(inputString);
        return _DoFormatWithPosition(inputString);
    }

    private string _DoFormatWithoutPosition(string input)
    {
        var s = new StringBuilder();
        s.AppendLine($"[bold red]ERROR[/]: [bold white]{Message}[/]");
        var srcStr = _GetLocationString();
        s.AppendLine($"  --> {srcStr}");
        var line = SourceInfo?.Line ?? 1;
        s.AppendLine($"{line} | [bold red]{input}[/]");
        if (Notes is null)
            return s.ToString();
        foreach (var note in Notes)
            s.AppendLine($"  = note: {note}");
        return s.ToString();
    }

    private string _DoFormatWithPosition(string input)
    {
        var s = new StringBuilder();
        s.AppendLine($"[bold red]ERROR[/]: [bold white]{Message}[/]");
        var srcStr = _GetLocationString();
        s.AppendLine($"  --> {srcStr}");
        var line = SourceInfo?.Line ?? 1;
        s.AppendLine($"{line} | [bold red]{input}[/]");
        var whiteSpace = new string(' ', Position!.Value - 1);
        s.AppendLine($"{new string(' ', line.ToString().Length)} | {whiteSpace}[bold red]^[/]");
        if (Notes is null)
            return s.ToString();
        foreach (var note in Notes)
            s.AppendLine($"  = note: {note}");
        return s.ToString();
    }

    private string _GetLocationString()
    {
        if (SourceInfo is null)
            return "(inside native console)";
        return $"({SourceInfo.FileName}:{SourceInfo.Line}:{SourceInfo.Column})";
    }
}
