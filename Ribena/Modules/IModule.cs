
using Ribena.Commands;
using Ribena.Guts;

namespace Ribena.Modules;

/// <summary>
/// The entire application is made of modules, which are their own little console applications that use
/// our api to create a "cmd loader" type application.
/// </summary>
public interface IModule
{
    /// <summary>
    /// The name of the module. This is used to visually display the module name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The brief description of this module. This is displayed next to the name.
    /// </summary>
    public string BriefDescription { get; }

    /// <summary>
    /// The version of this module.
    /// </summary>
    public VersionInfo Version { get; }

    /// <summary>
    /// The author of this module.
    /// </summary>
    public AuthorInfo Author { get; }

    /// <summary>
    /// Get the executor for this instance. This can be customized, but is defaulted to
    /// the builtin executor.
    /// </summary>
    /// <returns></returns>
    public ICommandExecutor GetExecutor()
    {
        return new DefaultCommandExecutor();
    }

    /// <summary>
    /// Called when the module is first loaded by the user.
    /// </summary>
    /// <returns>If this function returns false, it will be automatically unloaded.</returns>
    public bool Init(Terminal context);

    /// <summary>
    /// Get all of the commands associated with this module. This 
    /// is only called once per module load. This is typically implemented by your module
    /// inheriting from <see cref="CommandAdapter"/>, then this function will return
    /// <see cref="CommandAdapter.Adapt"/>. However, it can be done manually too.
    /// </summary>
    /// <returns></returns>
    public List<ICommand> GetCommands();
}
