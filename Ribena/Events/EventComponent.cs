
using System.Numerics;

namespace Ribena.Events;

/// <summary>
/// This class is the base class of all classes that want to partake in 
/// the <see cref="EventSystem"/>
/// </summary>
public class EventComponent
{
    /// <summary>
    /// The name of this component.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// All events registered on this component.
    /// </summary>
    public readonly List<Event> Events;

    /// <summary>
    /// The base constructor, sets <see cref="Name"/> to 
    /// <c>GetType().Name</c> and <see cref="Events"/> to an empty list.
    /// </summary>
    public EventComponent()
    {
        Name = GetType().Name;
        Events = [];

        EventSystem.PushComponent(this);
    }

    /// <summary>
    /// Register an event to this component. Adding it to 
    /// <see cref="Events"/>
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    protected Event RegisterEvent(string identifier)
    {
        var ev = new Event(identifier);
        Events.Add(ev);
        return ev;
    }

    /// <summary>
    /// Call a specific event. This does not throw on failure.
    /// </summary>
    /// <param name="identifier">The identifier of the event</param>
    /// <param name="eventData">The event data.</param>
    /// <returns>True if the call was a success, otherwise false</returns>
    protected bool Call(string identifier, object eventData)
    {
        var selectedEvent = Events.Where(x => x.Name == identifier)
            .FirstOrDefault();

        if (selectedEvent is null)
            return false;

        Info($"The event \"{identifier}\" is being called on component \"{Name}\"");

        try
        {
            selectedEvent.Call(eventData);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static bool operator ==(EventComponent a, EventComponent b)
        => a.Name == b.Name;

    public static bool operator !=(EventComponent a, EventComponent b)
        => !(a == b);
}
