
namespace Ribena.Guts;

/// <summary>
/// Version information. This includes a major, minor and branch.
/// An example would be "1.0-main" and could be constructed as 
/// <c>new VersionInfo(1, 0, "main")</c>
/// </summary>
/// <param name="MajorVersion"></param>
/// <param name="MinorVersion"></param>
/// <param name="Branch"></param>
public record class VersionInfo(
    int MajorVersion,
    int MinorVersion,
    string Branch
)
{
    /// <summary>
    /// Is this a pre-release? I.E is the major version smaller than 1.
    /// </summary>
    /// <returns></returns>
    public bool IsPreRelease() => MajorVersion < 1;

    /// <summary>
    /// Is the branch stable? I.E is the branch name "stable"
    /// </summary>
    /// <returns></returns>
    public bool IsStable() => Branch == "stable";
}
