using JetBrains.Annotations;

namespace GlavLib.Db;

[PublicAPI]
public sealed class DbTransaction :  IDisposable
{
    private readonly DbSession _dbSession;

    public DbTransaction()
    {
        _dbSession = DbSession.Current;
        _dbSession.BeginTransaction();
    }
    
    public DbTransaction(DbSession dbSession)
    {
        _dbSession = dbSession;
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