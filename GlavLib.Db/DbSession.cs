using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using NHibernate;

namespace GlavLib.Db;

public sealed class DbSession(ISession nhSession) : IDbSession
{
    private static readonly AsyncLocal<DbSession?> CurrentSession = new(null);

    [PublicAPI]
    public static DbSession Current
    {
        get
        {
            var session = CurrentSession.Value;
            if (session is null)
                throw new InvalidOperationException("No current DatabaseSession");

            return session;
        }
    }

    [PublicAPI]
    public static IDbConnection CurrentConnection => Current.Connection;

    [PublicAPI]
    public static ISession CurrentNhSession => Current.NhSession;

    [PublicAPI]
    public ISession NhSession { get; } = nhSession;

    [PublicAPI]
    public DbConnection Connection => NhSession.Connection;

    [PublicAPI]
    public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("No opened transaction");

    private ITransaction? _nhTransaction;
    private IDbTransaction? _transaction;
    private bool _isSessionBound;

    [PublicAPI]
    public DbSession Bind()
    {
        BindSession(this);

        return this;
    }

    [PublicAPI]
    public DbSession BeginTransaction()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Cannot begin transaction, there is already opened transaction");

        using var command = NhSession.Connection.CreateCommand();

        _nhTransaction = NhSession.BeginTransaction();
        _nhTransaction.Enlist(command);

        _transaction = command.Transaction;

        return this;
    }

    [PublicAPI]
    public void Commit()
    {
        if (_nhTransaction is null)
            throw new InvalidOperationException("Cannot commit, no opened transaction");

        NhSession.Flush();
        _nhTransaction.Commit();

        _nhTransaction = null;
        _transaction   = null;
    }

    [PublicAPI]
    public void Dispose()
    {
        _nhTransaction?.Rollback();
        NhSession.Connection.Dispose();
        NhSession.Dispose();

        if (_isSessionBound)
            ClearCurrentSession();
    }

    internal void RollbackIfActive()
    {
        if (_nhTransaction is null)
            return;

        _nhTransaction.Rollback();

        _nhTransaction = null;
        _transaction   = null;
    }

    public void Deconstruct(out ISession nhSession, out DbConnection dbConnection)
    {
        nhSession    = NhSession;
        dbConnection = Connection;
    }
    
    public void Deconstruct(out ISession nhSession, out DbConnection dbConnection, out IDbTransaction dbTransaction)
    {
        nhSession     = NhSession;
        dbConnection  = Connection;
        dbTransaction = Transaction;
    }

    private void BindSession(DbSession dbSession)
    {
        if (CurrentSession.Value is not null)
            throw new InvalidOperationException("Session is already bound. Probably you forgot close prior session");

        _isSessionBound      = true;
        CurrentSession.Value = dbSession;
    }

    private static void ClearCurrentSession()
    {
        if (CurrentSession.Value is null)
            throw new InvalidOperationException("Session was not bound. Probably you have already closed it");

        CurrentSession.Value = null;
    }
}