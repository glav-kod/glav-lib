namespace GlavLib.Basics.DomainEvents;

public class DomainEventsSession : IDisposable
{
    private static readonly AsyncLocal<DomainEventsSession> CurrentSession = new();

    public readonly List<DomainEvent> Events = new();

    private DomainEventsSession()
    {
    }

    public static DomainEventsSession Bind()
    {
        var session = new DomainEventsSession();
        CurrentSession.Value = session;

        return session;
    }

    public static void Raise(DomainEvent domainEvent)
    {
        var currentSession = CurrentSession.Value;
        if (currentSession is null)
            throw new InvalidOperationException("No DomainEvents session");

        currentSession.Events.Add(domainEvent);
    }

    public void Dispose()
    {
        CurrentSession.Value = null!;
    }
}