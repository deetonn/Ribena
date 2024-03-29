
using Newtonsoft.Json;
using Ribena.Events;
using System.Reflection;

namespace Ribena.Config;

/// <summary>
/// Event data for the <see cref="Settings.SettingFileCreated"/> event.
/// </summary>
/// <param name="FileName">The filename of the setting file</param>
/// <param name="FileContents">The contents it was created with</param>
public record class SettingFileCreatedEventData(
    string FileName,
    string FileContents
);

/// <summary>
/// Event data for the <see cref="Settings.SettingFileModified"/> event.
/// </summary>
/// <param name="FileName">The filename of the setting file</param>
/// <param name="NewFileContents">The new contents of the setting file</param>
public record class SettingFileModifiedEventData(
    string FileName,
    string NewFileContents
);

public class Settings : EventComponent
{
    /// <summary>
    /// The name of the setting file created event.
    /// </summary>
    public const string SettingFileCreated = "SettingFileCreated";

    /// <summary>
    /// The name of the setting file modified event.
    /// </summary>
    public const string SettingFileModified = "SettingFileModified";

    /// <summary>
    /// The directory this settings object refers to.
    /// </summary>
    public readonly DirectoryInfo ConfigDir;
    
    /// <summary>
    /// Initialize using the default configuration directory. Typically
    /// %AppData%\Ribena
    /// </summary>
    public Settings()
    {
        ConfigDir = Directory.CreateDirectory(Environment.GetAppDataFolder());

        RegisterEvent(SettingFileCreated);
        RegisterEvent(SettingFileModified);
    }

    /// <summary>
    /// Create an inner configuration directory (living within the parent folder)
    /// </summary>
    /// <param name="parent">The parent settings folder</param>
    /// <param name="innerDirName">The name of this new directory</param>
    public Settings(Settings parent, string innerDirName)
    {
        ConfigDir = Directory.CreateDirectory(
            Path.Combine(parent.ConfigDir.FullName, innerDirName));
    }

    /// <summary>
    /// Create an inner directory. See <see cref="Settings(Settings, string)"/> for more details.
    /// If this directory already exists, that is used.
    /// </summary>
    /// <param name="name">The name of this new directory</param>
    /// <returns>A newly constructed <see cref="Settings"/> instance that points towards that directory</returns>
    public Settings CreateInnerDirectory(string name)
        => new(this, name);

    /// <summary>
    /// Attempt to read a configuration file, if it does not exist, create it.
    /// If we have to create the file, serialize <paramref name="default"/> and write it to it. Then <paramref name="default"/> is returned.
    /// Otherwise, we read the file contents, and call <see cref="JsonConvert.DeserializeObject(string)"/>.
    /// We attempt to deserialize it to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to attempt to deserialize the file contents to</typeparam>
    /// <param name="fileName">The name of the file</param>
    /// <param name="default">The default value, if the file does not exist and has to be created</param>
    /// <returns></returns>
    public T? ReadOrCreateConfigFile<T>(string fileName, T @default)
    {
        var pathToFile = Path.Combine(ConfigDir.FullName, fileName);
        if (!File.Exists(pathToFile))
        {
            using var _ = File.Create(pathToFile);
            var serialized = JsonConvert.SerializeObject(@default);
            File.WriteAllText(pathToFile, serialized);
            // Maybe add a check here to add checked files.
            FireSettingFileCreatedEvent(fileName, serialized);
            return @default;
        }
        var fileContents = File.ReadAllText(pathToFile);
        return JsonConvert.DeserializeObject<T>(fileContents);
    }

    /// <summary>
    /// Attempt to write an object to a configuration file. If the file does not exist yet, it is created.
    /// The contents are then serialized into a string and written to it.
    /// </summary>
    /// <param name="fileName">The name of the file. (case sensitive)</param>
    /// <param name="obj">The object to write to file (using json serialization)</param>
    public void WriteOrCreateConfigFile(string fileName, object? obj)
    {
        var pathToFile = Path.Combine(ConfigDir.FullName, fileName);
        if (!File.Exists(pathToFile))
        {
            using var _ = File.Create(pathToFile);
        }
        var serialized = JsonConvert.SerializeObject(obj);
        FireSettingFileModifiedEvent(fileName, serialized);
        File.WriteAllText(pathToFile, serialized);
    }

    /// <summary>
    /// Attempt to load an assembly from this settings folder.
    /// </summary>
    /// <param name="name">The name of the assembly, .dll is appended if not present.</param>
    /// <returns></returns>
    public Assembly? TryGetAssembly(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        if (!name.EndsWith(".dll"))
            name += ".dll";

        var fullPathToAssembly = Path.Combine(ConfigDir.FullName, name);
        try
        {
            return Assembly.LoadFrom(fullPathToAssembly);
        }
        catch (Exception e)
        {
            Warn($"Failed to load assembly \"{name}\"");
            Warn($"  Reason: {e}");
            return null;
        }
    }

    /// <summary>
    /// Get the actual path to this configuration folder.
    /// </summary>
    /// <returns></returns>
    public string GetPath() => ConfigDir.FullName;

    private bool FireSettingFileCreatedEvent(string fileName, string fileContents)
    {
        return Call(SettingFileCreated,
            new SettingFileCreatedEventData(fileName, fileContents));
    }

    private bool FireSettingFileModifiedEvent(string fileName, string newFileContents)
    {
        return Call(SettingFileModified,
            new SettingFileModifiedEventData(fileName, newFileContents));
    }
}
