
namespace Ribena.Events;

/// <summary>
/// A function that handles an event.
/// </summary>
/// <param name="Data"></param>
public delegate void EventHandler(object Data);

public class Event
{
    /// <summary>
    /// The visual name of this event.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// This event ID.
    /// </summary>
    public readonly Guid Guid = Guid.NewGuid();

    /// <summary>
    /// The list of all handlers handling this event.
    /// </summary>
    public readonly List<EventHandler> Handlers = [];

    /// <summary>
    /// Create the event, with <paramref name="name"/>
    /// </summary>
    /// <param name="name"></param>
    public Event(string name)
    {
        Name = name;
        VConsole.PushTag(
            Guid.ToString(), 
            VConsoleTag.WithoutStyle($"Event \"{Name}\""));

        Info($"The event \"{name}\" ({Guid}) has been created.");
    }

    /// <summary>
    /// Push a callback into this event state. This will cause the provided callback to 
    /// be fired whenever <see cref="Call(object)"/> is called.
    /// </summary>
    /// <param name="handler"></param>
    public void PushCallback(EventHandler handler)
        => Handlers.Add(handler);

    /// <summary>
    /// Call all of the <see cref="Handlers"/> with the argument <paramref name="data"/>.
    /// </summary>
    /// <param name="data">
    /// The data, using the default prefix for event names. This is typically a class that is named as
    /// the event name followed by "EventData". So for example, the event is called "OnInput",
    /// the data class for that event data would be "OnInputEventData". This allows subscribers to figure out
    /// the name of this quickly.
    /// </param>
    public void Call(object data)
    {
        foreach (var handler in Handlers)
            handler(data);
    }
}
