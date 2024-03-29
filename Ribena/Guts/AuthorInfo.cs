
namespace Ribena.Guts;

/// <summary>
/// Author information. This includes their name, and their repository link (if applicable)
/// </summary>
/// <param name="AuthorName">The name of the author</param>
/// <param name="RepositoryLink">The link to the repository</param>
public record class AuthorInfo(
    string AuthorName,
    string? RepositoryLink
);
