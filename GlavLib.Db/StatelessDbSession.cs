using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using NHibernate;

namespace GlavLib.Db;

public sealed class StatelessDbSession(IStatelessSession nhStatelessSession) : IDbSession
{
    [PublicAPI]
    public IStatelessSession NhStatelessSession { get; } = nhStatelessSession;

    [PublicAPI]
    public DbConnection Connection => NhStatelessSession.Connection;

    [PublicAPI]
    public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("No opened transaction");

    private ITransaction? _nhTransaction;
    private IDbTransaction? _transaction;

    [PublicAPI]
    public StatelessDbSession BeginTransaction()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Cannot begin transaction, there is already opened transaction");

        using var command = NhStatelessSession.Connection.CreateCommand();

        _nhTransaction = NhStatelessSession.BeginTransaction();
        _nhTransaction.Enlist(command);

        _transaction = command.Transaction;

        return this;
    }

    [PublicAPI]
    public void Commit()
    {
        if (_nhTransaction is null)
            throw new InvalidOperationException("Cannot commit, no opened transaction");

        _nhTransaction.Commit();

        _nhTransaction = null;
        _transaction   = null;
    }

    [PublicAPI]
    public void Dispose()
    {
        _nhTransaction?.Rollback();
        NhStatelessSession.Connection.Dispose();
        NhStatelessSession.Dispose();
    }
}