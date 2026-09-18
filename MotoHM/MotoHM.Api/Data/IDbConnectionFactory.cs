using System.Data;

namespace MotoHM.Api.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}