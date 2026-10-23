using System.Data.Common;
using System.Globalization;

namespace Tools365.Technology.DbConnectionStringGenerator;

internal enum DatabaseProvider
{
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite
}

internal sealed record ConnectionStringResult(string ConnectionString, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class ConnectionStringFactory
{
    public static ConnectionStringResult Build(
        DatabaseProvider provider,
        string server,
        double port,
        string database,
        string username,
        string password,
        string sqliteFilePath,
        bool integratedAuthentication,
        bool pooling)
    {
        var builder = new System.Data.Common.DbConnectionStringBuilder();

        if (provider == DatabaseProvider.Sqlite)
        {
            var filePath = sqliteFilePath.Trim();
            if (filePath.Length == 0)
            {
                return Invalid("Enter a database file path.");
            }

            builder["Data Source"] = filePath;
            builder["Mode"] = "ReadWriteCreate";
            builder["Cache"] = "Shared";
            builder["Pooling"] = pooling;
            return Valid(builder.ConnectionString);
        }

        var serverName = server.Trim();
        var databaseName = database.Trim();
        if (serverName.Length == 0)
        {
            return Invalid("Enter a server or host name.");
        }

        if (databaseName.Length == 0)
        {
            return Invalid("Enter a database name.");
        }

        if (!double.IsFinite(port) || port < 1 || port > 65535 || port != Math.Truncate(port))
        {
            return Invalid("Port must be a whole number from 1 to 65535.");
        }

        switch (provider)
        {
            case DatabaseProvider.SqlServer:
                builder["Server"] = $"{serverName},{port.ToString("0", CultureInfo.InvariantCulture)}";
                builder["Database"] = databaseName;
                if (integratedAuthentication)
                {
                    builder["Integrated Security"] = true;
                }
                else
                {
                    if (username.Trim().Length == 0)
                    {
                        return Invalid("Enter a user name or choose Windows authentication.");
                    }

                    builder["User ID"] = username.Trim();
                    builder["Password"] = password;
                }

                builder["Encrypt"] = true;
                builder["TrustServerCertificate"] = false;
                builder["Pooling"] = pooling;
                break;

            case DatabaseProvider.PostgreSql:
                if (username.Trim().Length == 0)
                {
                    return Invalid("Enter a user name.");
                }

                builder["Host"] = serverName;
                builder["Port"] = port;
                builder["Database"] = databaseName;
                builder["Username"] = username.Trim();
                builder["Password"] = password;
                builder["SSL Mode"] = "Prefer";
                builder["Pooling"] = pooling;
                break;

            case DatabaseProvider.MySql:
                if (username.Trim().Length == 0)
                {
                    return Invalid("Enter a user name.");
                }

                builder["Server"] = serverName;
                builder["Port"] = port;
                builder["Database"] = databaseName;
                builder["User ID"] = username.Trim();
                builder["Password"] = password;
                builder["SslMode"] = "Preferred";
                builder["Pooling"] = pooling;
                break;

            default:
                return Invalid("Choose a supported database provider.");
        }

        return Valid(builder.ConnectionString);
    }

    private static ConnectionStringResult Valid(string connectionString) =>
        new(connectionString);

    private static ConnectionStringResult Invalid(string message) =>
        new(string.Empty, message);
}
