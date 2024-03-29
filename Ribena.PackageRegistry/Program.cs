// We take a very minimal approach to our repository.

namespace Ribena.PackageRegistry;

public record class CachedModule(
    string Name,
    byte[] Binary
);

public static class Program
{
    public static void Main(string[] args)
    {
        var moduleDir = Path.Combine(Environment.CurrentDirectory, "modules");
        Directory.CreateDirectory(moduleDir);

        // On load, we redo the registry.
    }
}