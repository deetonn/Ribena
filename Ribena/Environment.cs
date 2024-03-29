
namespace Ribena;

using Ribena.Events;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
using SysEnv = System.Environment;

public record class EnvVariableChangedEventData(
    string OldValue,
    string NewValue,
    string Key,
    Environment Env
);

public record class EnvVariableAddedEventData(
    string Value,
    string Key,
    Environment Env
);

public class Environment : EventComponent
{
    /// <summary>
    /// The name of the variable changed event.
    /// </summary>
    public const string VariableChanged = "EnvVariableChanged";

    /// <summary>
    /// The name of the variable added event.
    /// </summary>
    public const string VariableAdded = "EnvVariableAdded";

    /// <summary>
    /// The name of the path variable.
    /// </summary>
    public const string PathVariableName = "PATH";

    /// <summary>
    /// The actual internal dictionary that we store the environment in.
    /// </summary>
    private Dictionary<string, string> Env;

    /// <summary>
    /// The character used to seperate paths inside the system's PATH variable.
    /// </summary>
    public static char PathSplitChar
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ';';
            }
            return ':';
        }
    }

    public string CurrentDir
    {
        get => SysEnv.CurrentDirectory;
        set => SysEnv.CurrentDirectory = value;
    }
    public string UserName => SysEnv.UserName;

    /// <summary>
    /// Construct the environment using the current systems values.
    /// </summary>
    public Environment()
    {
        Env = [];

        try
        {
            Info("Refreshing environment variables.");
            Refresh();
        }
        catch (OutOfMemoryException e)
        {
            VConsole.WriteLine($"[[preload]] Out of memory, cannot get environment variables.");
            Console.WriteLine($"[preload] Message: {e}");
            SysEnv.Exit(-1);
        }
        catch (SecurityException e)
        {
            VConsole.WriteLine($"[[preload]] SecurityException occured while loading the environment.");
            VConsole.WriteLine($"[[preload]] This means the system environment values are unavailable right now.");
            Console.WriteLine($"[preload] Reason: {e}");
            Env = [];
        }

        RegisterEvent(VariableChanged);
        RegisterEvent(VariableAdded);
    }

    /// <summary>
    /// Refresh this instance of the environment. Reading in new values from the system
    /// and replacing existing values with new ones.
    /// </summary>
    public void Refresh()
    {
        // To refresh, we keep the current items and replace/insert any
        // new entrys.
        var newEnv = new Dictionary<string, string>(Env);
        try
        {
            foreach (DictionaryEntry de in SysEnv.GetEnvironmentVariables())
            {
                if (!newEnv.TryAdd((string)de.Key, (string?)de.Value ?? "<empty>"))
                    newEnv[(string)de.Key] = (string?)de.Value!;
            }
        }
        catch (OutOfMemoryException e)
        {
            VConsole.WriteLine("Ran out of memory while refreshing the environment.");
            Info($"OutOfMemoryException: {e}");
        }
        catch (SecurityException e)
        {
            VConsole.WriteLine("There was a security exception when refreshing the environment.");
            Info($"SecurityException: {e}");
        }
        finally
        {
            var totalLoadedAmount = Env.Count - newEnv.Count;
            if (0 > totalLoadedAmount)
                totalLoadedAmount = ~totalLoadedAmount;
            Info($"RefreshEnv(): Loaded {totalLoadedAmount} new environment variables.");
            Env = newEnv;
        }
    }

    /// <summary>
    /// Get the system's path variable. This is a list of paths
    /// that contain executables/data relavent to what is installed on this machine.
    /// </summary>
    /// <returns>The most up-to-date possible version of the PATH environment variable, as a list.</returns>
    public List<string> GetPath()
    {
        Refresh();
        var path = Env[PathVariableName];
        return [.. path.Split(PathSplitChar)];
    }

    /// <summary>
    /// Attempt to get the environment variable that is identified by <paramref name="key"/>.
    /// This function does not manually refresh the state of the environment variables,
    /// so if an up-to-date value is needed, please use <see cref="Refresh"/>.
    /// 
    /// This is to save CPU-cycles on a common function. You don't want a refresh each time
    /// while getting a static environment variable.
    /// </summary>
    /// <param name="key">The name of the environment variable (not case sensitive)</param>
    /// <returns>The value of that variable. null if it does not exist.</returns>
    public string? Get(string key)
    {
        if (!Env.TryGetValue(key, out var value))
        {
            return null;
        }
        return value;
    }

    /// <summary>
    /// Read the description of <see cref="Get"/> to understand what is function is.
    /// 
    /// This variant of <see cref="Get"/> exists to avoid situations like:
    /// <c>
    /// SomeCall(Env.Get("Something")) // &lt;-- Warn!
    /// // Instead!
    /// SomeCall(Env.GetOrThrow("Something", "Failed to get something"))
    /// </c>
    /// </summary>
    /// <param name="key">The name of the environment variable</param>
    /// <param name="errMsg">The message to have in the exception</param>
    /// <returns>The value, or throws <see cref="KeyNotFoundException"/> with user-specified <paramref name="errMsg"/></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public string GetOrThrow(string key, string errMsg)
    {
        return Get(key) ?? throw new KeyNotFoundException(errMsg);
    }

    /// <summary>
    /// Create an environment variable.
    /// Please note, if the value exists, it will be overwritten.
    /// </summary>
    /// <param name="key">The name of the variable.</param>
    /// <param name="value">The value of this variable.</param>
    public void Create(string key, string value)
    {
        if (!Env.TryAdd(key, value))
        {
            Set(key, value);
            return;
        }

        // Call events watching for variables added.
        Call(VariableAdded,
            new EnvVariableAddedEventData(value, key, this)
            );
    }

    /// <summary>
    /// Set the value at <paramref name="key"/> to <paramref name="value"/>
    /// 
    /// If the key does not exist, this call is simply ignored.
    /// </summary>
    /// <param name="key">The key of the value to change</param>
    /// <param name="value">The new value</param>
    public void Set(string key, string value)
    {
        if (!Env.ContainsKey(key))
            return;
        var oldValue = Env[key];
        Env[key] = value;

        // Call people that are watching for variables to change.
        Call(VariableChanged,
            new EnvVariableChangedEventData(
                oldValue, value, key, this
                )
            );
    }

    /// <summary>
    /// The same as <see cref="Set"/>, except it will throw an exception
    /// if the value is not found.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <param name="errMsg">The error message, defaulted if null</param>
    public void SetOrThrow(string key, string value, string? errMsg = null)
    {
        if (!Env.ContainsKey(key))
        {
            errMsg ??= $"The key `{key}` was not found.";
            throw new KeyNotFoundException(errMsg);
        }
        Set(key, value);
    }

    /// <summary>
    /// Get the application data folder for this applications config.
    /// </summary>
    /// <returns>The full path to our AppData folder</returns>
    public static string GetAppDataFolder()
        => Path.Combine(SysEnv.GetFolderPath(SysEnv.SpecialFolder.ApplicationData), "Ribena");

    /// <summary>
    /// Attempt to load a .env file. 
    /// 
    /// This can only be one pair per line.
    /// </summary>
    /// <param name="pathToDotEnv">The full valid path to the file. (does not need to be called anything specific)</param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public void TryLoadDotEnv(string pathToDotEnv)
    {
        if (!File.Exists(pathToDotEnv))
        {
            throw new FileNotFoundException($"The specified path `{pathToDotEnv}` does not exist. (while loading a .env file.)");
        }
        var currentLine = 0;
        var lines = File.ReadAllLines(pathToDotEnv);
        foreach (var line in lines.Select(x => x.Split("=")))
        {
            currentLine++;
            if (line.Length != 2)
                throw new InvalidDataException($"The .env contained an invalid line. Expected (key=value) but got something different. (on line {currentLine})");
            var key = line[0];
            var value = line[1];
            Create(line[0], line[1]);
        }
    }

    public const string LAST_OUTPUT_IDENTIFIER = "LastCommandOutput";
    /// <summary>
    /// Directly access the "LastCommandOutput" special environment variable.
    /// </summary>
    public string LastOutput
    {
        get
        {
            if (Get(LAST_OUTPUT_IDENTIFIER) is string s)
                return s;
            Create(LAST_OUTPUT_IDENTIFIER, string.Empty);
            return string.Empty;
        }

        set
        {
            try
            {
                SetOrThrow(LAST_OUTPUT_IDENTIFIER, value);
            }
            catch
            {
                Create(LAST_OUTPUT_IDENTIFIER, value);
            }
        }
    }

    public const string LAST_EXIT_CODE_IDENTIFIER = "LastExitCode";

    /// <summary>
    /// Directly access the "LastExitCode" special environment variable. Please note that this
    /// getter automatically converts it to an integer. Zero is returned if the value does not exist
    /// yet or the number is invalid. (Extreme edge cases that are very unlikely)
    /// </summary>
    public int LastCode
    {
        get
        {
            if (Get(LAST_EXIT_CODE_IDENTIFIER) is string data)
                if (int.TryParse(data, out var code))
                    return code;
            return 0;
        }

        set
        {
            try
            {
                SetOrThrow(LAST_EXIT_CODE_IDENTIFIER, value.ToString());
            }
            catch
            {
                Create(LAST_EXIT_CODE_IDENTIFIER, value.ToString());
            }
        }
    }
}
