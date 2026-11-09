using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Pnv.Desktop.Infrastructure.Interceptors;

public sealed class SqlLoggingInterceptor(ILogger<SqlLoggingInterceptor> logger)
    : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        logger.LogDebug("SQL: {CommandText}", command.CommandText);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("SQL: {CommandText}", command.CommandText);
        return ValueTask.FromResult(result);
    }
}
