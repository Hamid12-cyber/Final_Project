using System.Data;
using Microsoft.Data.SqlClient;

namespace MotoHM.Api.Data;

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("Default");
        return new SqlConnection(connectionString);
    }
}