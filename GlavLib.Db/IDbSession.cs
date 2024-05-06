using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace GlavLib.Db;

public interface IDbSession : IDisposable
{
    [PublicAPI]
    public DbConnection Connection { get; }

    [PublicAPI]
    public IDbTransaction Transaction { get; }
}