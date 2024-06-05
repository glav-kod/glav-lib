using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using NHibernate;

namespace GlavLib.Db;

[PublicAPI]
public sealed class StatelessDbSession(IStatelessSession nhSession) : DbSession
{
    public new static StatelessDbSession Current
    {
        get
        {
            if (CurrentSession.Value is not StatelessDbSession session)
                throw new InvalidOperationException("No current StatelessDbSession");

            return session;
        }
    }

    public IStatelessSession NhSession { get; } = nhSession;

    public override DbConnection Connection => NhSession.Connection;

    public override IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("No opened transaction");

    private ITransaction? _nhTransaction;
    private IDbTransaction? _transaction;
    private bool _isSessionBound;

    public StatelessDbSession Bind()
    {
        if (CurrentSession.Value is not null)
            throw new InvalidOperationException("Session is already bound. Probably you forgot close prior session");

        _isSessionBound      = true;
        CurrentSession.Value = this;

        return this;
    }

    public override StatelessDbSession BeginTransaction()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Cannot begin transaction, there is already opened transaction");

        using var command = NhSession.Connection.CreateCommand();

        _nhTransaction = NhSession.BeginTransaction();
        _nhTransaction.Enlist(command);

        _transaction = command.Transaction;

        return this;
    }

    public override void Commit()
    {
        if (_nhTransaction is null)
            throw new InvalidOperationException("Cannot commit, no opened transaction");

        _nhTransaction.Commit();

        _nhTransaction = null;
        _transaction   = null;
    }

    internal override void RollbackIfActive()
    {
        if (_nhTransaction is null)
            return;

        _nhTransaction.Rollback();

        _nhTransaction = null;
        _transaction   = null;
    }

    public override void Dispose()
    {
        _nhTransaction?.Rollback();
        NhSession.Connection.Dispose();
        NhSession.Dispose();

        if (!_isSessionBound)
            return;

        if (CurrentSession.Value is null)
            throw new InvalidOperationException("Session was not bound. Probably you have already closed it");

        CurrentSession.Value = null;
    }
}