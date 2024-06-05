using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace GlavLib.Db;

[PublicAPI]
public abstract class DbSession : IDisposable
{
    public static DbSession Current
    {
        get
        {
            if (CurrentSession.Value is null)
                throw new InvalidOperationException("No current DbSession");

            return CurrentSession.Value;
        }
    }

    protected internal static readonly AsyncLocal<DbSession?> CurrentSession = new(null);

    public abstract DbConnection Connection { get; }

    public abstract IDbTransaction Transaction { get; }

    public abstract DbSession BeginTransaction();

    public abstract void Commit();

    public abstract void Dispose();

    internal abstract void RollbackIfActive();
}