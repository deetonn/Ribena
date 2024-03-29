using System.Reflection;

namespace Ribena.Commands;

/// <summary>
/// This class serves as an adapter for modules to load commands with ease.
/// It will load all commands within the current assembly automatically.
/// 
/// To actually use them, use <see cref="Adapt()"/>
/// </summary>
public class CommandAdapter
{
    /// <summary>
    /// This function will load all classes that implement <see cref="ICommand"/> in the calling assembly
    /// into a <see cref="List{ICommand}"/>. Then returns it. This allows you to just create your
    /// command classes in seperate files and not have to initialize them manually. The only requirement is that
    /// they have a default constructor.
    /// </summary>
    /// <returns></returns>
    public List<ICommand> Adapt()
    {
        List<ICommand> commands = [];

        var currentAssembly = Assembly.GetExecutingAssembly();
        var typesInCurrentAssembly = currentAssembly.GetTypes();

        foreach (var t in typesInCurrentAssembly)
        {
            if (!t.IsAssignableTo(typeof(ICommand)))
                continue;
            if (t == typeof(ICommand))
                continue;
            try
            {
                var command = Activator.CreateInstance(t);
                commands.Add((ICommand)command!);
            }
            catch (Exception e)
            {
                Warn($"(Inside of module '{GetType().Name}') Failed to adapt command");
                Warn($" ^^ Reason: {e}");
            }
        }

        return commands;
    }
}
