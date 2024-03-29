namespace Ribena.Events;

public static class EventSystem
{
    public static readonly List<EventComponent> Components = [];

    /// <summary>
    /// This will push a component (a class that inherits from <see cref="EventComponent"/>)
    /// (This is for internal use only)
    /// </summary>
    /// <param name="component"></param>
    internal static void PushComponent(EventComponent component)
    {
        if (Components.Contains(component))
            return;
        Info($"The event component \"{component.Name}\" has been registered.");
        Components.Add(component);
    }
}
