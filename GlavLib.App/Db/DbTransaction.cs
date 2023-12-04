namespace GlavLib.App.Db;

public sealed class DbTransaction : IAsyncDisposable
{
    private readonly DbSession _session;

    public DbTransaction()
    {
        _session = DbSession.Current;
        _session.BeginTransaction();
    }

    public DbTransaction(DbSession session)
    {
        _session = session;
        _session.BeginTransaction();
    }

    public async Task CommitAsync()
    {
        await _session.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _session.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _session.RollbackIfActiveAsync();
        }
        catch
        {
            //ignore
        }
    }
}