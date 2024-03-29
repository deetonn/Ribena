
namespace Ribena.Guts;

/// <summary>
/// Information about source location.
/// </summary>
/// <param name="fileName">The name of the file</param>
/// <param name="line">The line this instance should represent</param>
/// <param name="column">The column this instance should represent</param>
public class SourceInfo(string fileName, int line, int column)
{
    public readonly string FileName = fileName;
    public readonly int Line = line;
    public readonly int Column = column;
}
