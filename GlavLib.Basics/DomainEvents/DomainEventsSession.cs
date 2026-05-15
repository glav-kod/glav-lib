namespace GlavLib.Basics.DomainEvents;

public class DomainEventsSession : IDisposable
{
    private static readonly AsyncLocal<DomainEventsSession> CurrentSession = new();

    public static DomainEventsSession Current => CurrentSession.Value ?? throw new InvalidOperationException("No DomainEvents session");
    
    public readonly List<DomainEvent> Events = new();
    
    public bool IsCommited { get; private set; }

    private DomainEventsSession()
    {
    }

    public void Commit()
    {
        IsCommited = true;
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
        
        if (currentSession.IsCommited)
            throw new InvalidOperationException("DomainEvents session is already commited.");

        currentSession.Events.Add(domainEvent);
    }

    public void Dispose()
    {
        CurrentSession.Value = null!;
    }
}