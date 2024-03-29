
using Spectre.Console;
using System.Text;

namespace Ribena;

/// <summary>
/// A tag, that is used to identify parts of output.
/// Its style will be <paramref name="TextStyle"/> and <paramref name="ExtraText"/>
/// will be output just after it.
/// </summary>
/// <param name="TextStyle">Optionally set the style, using Spectre.Console markup. (Default: white bold)</param>
/// <param name="ExtraText">Extra text attached, if this isn't null, it is output just after the <paramref name="TaggedText"/> This is meant to be used for context, and is inserted inside brackets.</param>
public record class VConsoleTag(
    string? TextStyle,
    string? ExtraText
)
{
    /// <summary>
    /// Shorthand for <c>new VConsoleTag(..., null)</c>
    /// </summary>
    /// <param name="style"></param>
    /// <returns></returns>
    public static VConsoleTag WithoutText(string style)
        => new(style, null);

    /// <summary>
    /// Shorthand for <c>new VConsoleTag(null, ...)</c>
    /// </summary>
    /// <param name="extraText"></param>
    /// <returns></returns>
    public static VConsoleTag WithoutStyle(string extraText)
        => new("white bold", extraText);
}

/// <summary>
/// The virtual console used by Ribena. This handles automatic
/// text tagging and colorization. Along with supporting hooked
/// output.
/// </summary>
public static class VConsole
{
    // All tags that have been added.
    internal static Dictionary<string, VConsoleTag> Tags { get; }
    internal static List<Func<string, string?>> Applicants { get; }

    static VConsole()
    {
        Tags = [];
        Applicants = [];
        SetupDefaultTags();
    }

    /// <summary>
    /// Create a tag for a particular peice of text. This modifes the style of that text globally.
    /// Please see <see cref="VConsoleTag"/> for documentation on what you can do with this.
    /// </summary>
    /// <param name="forIdentifier">The identifier, that if it should occur in output, will be modified to meet <paramref name="tag"/>'s specification</param>
    /// <param name="tag">The actual tag, the style information and extra information</param>
    public static void PushTag(string forIdentifier, VConsoleTag tag)
    {
        Tags.Add(forIdentifier, tag);
    }

    /// <summary>
    /// Standard output function, adhering to all VConsole features including tags.
    /// This function appends <see cref="Environment.NewLine"/> to the end of the output.
    /// </summary>
    /// <param name="data">The output</param>
    public static void WriteLine(string data)
    {
        AnsiConsole.MarkupLine(BuildOutput(data));
    }

    /// <summary>
    /// Standard output function, adhering to all VConsole features including tags.
    /// </summary>
    /// <param name="data">The output</param>
    public static void Write(string data)
    {
        Console.Write(BuildOutput(data));
    }

    /// <summary>
    /// Clear the active console buffer.
    /// </summary>
    public static void Clear()
    {
        AnsiConsole.Clear();
    }

    private static void SetupDefaultTags()
    {
        // Setup common keywords to add context and make them stand-out.
        PushTag("Ribena", 
            new VConsoleTag("#8E5983 bold", 
            "The application you're using!"));

        PushTag("github",
            new VConsoleTag("underline bold white",
            "https://github.com/deetonn/Ribena"));

        PushTag("modules",
            new VConsoleTag("#80ffe5 italic",
            "Modules are a core part of Ribena, confused? please check our github page!"));
    }

    /// <summary>
    /// This is the actual VConsole output builder. Any performance optimizations
    /// here would be very good for performance improvements.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string BuildOutput(string input)
    {
        // NOTE: .Split() automatically splits by spaces, and due to the nature of this
        //       application, we only care about actual text. So this is fine.
        var parts = input.Split();
        // The string builder instance
        var sb = new StringBuilder();

        // Iterate each part and see if we are watching for that.
        foreach (var part in parts)
        {
            if (!Tags.TryGetValue(part, out var tag))
            {
                // There is no tag for that, just re-append the data and move on.
                sb.Append($"[white bold]{part}[/] "); // Note the appended space.
                continue;
            }
            // Append TextStyle if available.
            if (tag.TextStyle is not null)
                sb.Append($"[{tag.TextStyle}]{part}[/]");
            // Append ExtraText if available.
            if (tag.ExtraText is not null)
            {
                sb.Append($" ([white bold]{tag.ExtraText}[/]) ");
            }
            else
            {
                // We need to append a space at the end to make up for 
                // the way we split the string.
                sb.Append(' ');
            }
        }
        // Done.
        return sb.ToString();
    }
}
