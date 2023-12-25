﻿using System.Data;
using System.Data.Common;
using NHibernate;

namespace GlavLib.Db;

public sealed class DbSession : IDisposable, IAsyncDisposable
{
    private static readonly AsyncLocal<DbSession?> CurrentSession = new(null);

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

    public static IDbConnection CurrentConnection => Current.Connection;

    public static ISession CurrentNhSession => Current.NhSession;

    public ISession NhSession { get; }

    public IDbConnection Connection => NhSession.Connection;

    private ITransaction? _nhTransaction;
    private IDbTransaction? _transaction;

    public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("No open transaction");

    private readonly bool _isSessionBound;

    private DbSession(ISession nhSession, bool isSessionBound)
    {
        NhSession       = nhSession;
        _isSessionBound = isSessionBound;
    }

    public DbSession(
            ISessionFactory sessionFactory,
            DbConnection dbConnection
        )
    {
        NhSession = sessionFactory.WithOptions()
                                  .Connection(dbConnection)
                                  .OpenSession();
        _isSessionBound = false;
    }

    public DbSession(ISession nhSession)
        : this(nhSession, isSessionBound: false)
    {
    }

    public DbSession Bind()
    {
        SetCurrentSession(this);

        return this;
    }

    internal void BeginTransaction()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction is already begun");

        using var command = NhSession.Connection.CreateCommand();

        _nhTransaction = NhSession.BeginTransaction();
        _nhTransaction.Enlist(command);

        _transaction = command.Transaction;
    }

    internal async Task CommitAsync()
    {
        if (_nhTransaction is null)
            throw new InvalidOperationException("Transaction is not active");

        await NhSession.FlushAsync();
        await _nhTransaction.CommitAsync();

        _nhTransaction = null;
        _transaction   = null;
    }

    internal async Task RollbackAsync()
    {
        if (_nhTransaction is null)
            throw new InvalidOperationException("Transaction is not active");

        await _nhTransaction.RollbackAsync();

        _nhTransaction = null;
        _transaction   = null;
    }

    internal async Task RollbackIfActiveAsync()
    {
        if (_nhTransaction is null)
            return;

        await _nhTransaction.RollbackAsync();

        _nhTransaction = null;
        _transaction   = null;
    }

    public void Dispose()
    {
        try
        {
            _nhTransaction?.Rollback();
        }
        catch
        {
            //ignore
        }

        try
        {
            NhSession.Dispose();
        }
        catch
        {
            //ignore
        }

        if (_isSessionBound)
            ClearCurrentSession();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_nhTransaction is not null)
                await _nhTransaction.RollbackAsync();
        }
        catch
        {
            //ignore
        }

        try
        {
            NhSession.Dispose();
        }
        catch
        {
            //ignore
        }

        if (_isSessionBound)
            ClearCurrentSession();
    }

    public static DbSession Bind(
            ISessionFactory sessionFactory,
            DbConnection dbConnection
        )
    {
        var session = sessionFactory.WithOptions()
                                    .Connection(dbConnection)
                                    .OpenSession();

        var dbSession = new DbSession(session, isSessionBound: true);

        SetCurrentSession(dbSession);

        return dbSession;
    }

    private static void SetCurrentSession(DbSession dbSession)
    {
        if (CurrentSession.Value is not null)
            throw new InvalidOperationException("Session is already bound. Probably you forgot close prior session");

        CurrentSession.Value = dbSession;
    }

    private static void ClearCurrentSession()
    {
        if (CurrentSession.Value is null)
            throw new InvalidOperationException("Session was not bound. Probably you have already closed it");

        CurrentSession.Value = null;
    }
}