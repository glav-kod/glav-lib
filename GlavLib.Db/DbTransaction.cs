using JetBrains.Annotations;

namespace GlavLib.Db;

[PublicAPI]
public sealed class DbTransaction : IDisposable
{
    private readonly DbSession _dbSession;

    public DbTransaction(DbSession dbSession)
    {
        _dbSession = dbSession;
        _dbSession.BeginTransaction();
    }

    public DbTransaction()
    {
        var dbSession = DbSession.CurrentSession.Value;

        _dbSession = dbSession ?? throw new InvalidOperationException("Cannot begin transaction, no current DbSession");
        _dbSession.BeginTransaction();
    }

    public void Commit()
    {
        _dbSession.Commit();
    }

    public void Dispose()
    {
        _dbSession.RollbackIfActive();
    }
}