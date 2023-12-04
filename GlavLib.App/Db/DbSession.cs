using System.Data;
using NHibernate;

namespace GlavLib.App.Db;

/// <summary>
/// Сессия подключения к базе данных.
/// </summary>
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

    /// <summary>
    /// NHibernate сессия
    /// </summary>
    public ISession NhSession { get; }

    public IDbConnection Connection => NhSession.Connection;

    private ITransaction? _nhTransaction;
    private IDbTransaction? _transaction;

    public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("No open transaction");

    private bool _isSessionBound;

    private DbSession(ISession nhSession, bool isSessionBound)
    {
        NhSession       = nhSession;
        _isSessionBound = isSessionBound;
    }

    public DbSession(ISessionFactory sessionFactory)
    {
        NhSession       = sessionFactory.OpenSession();
        _isSessionBound = true;
    }

    public DbSession(ISession nhSession)
        : this(nhSession, isSessionBound: false)
    {
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
            CurrentSession.Value = null;
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
            CurrentSession.Value = null;
    }

    /// <summary>
    /// Создать новую сессию и сделать ее текущей.
    /// </summary>
    /// <param name="sessionFactory">Фабрика сессий</param>
    /// <returns>Сессия подключения к базе данных</returns>
    public static DbSession Bind(ISessionFactory sessionFactory)
    {
        var session = sessionFactory.OpenSession();

        var databaseSession = new DbSession(session, isSessionBound: true);

        CurrentSession.Value = databaseSession;

        return databaseSession;
    }
}