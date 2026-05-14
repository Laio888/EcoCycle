using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;

namespace app.asp_servicios.infraestructura 
    public class DatabaseConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly string _provider;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _provider = _configuration["DatabaseProvider"];
        }

        public IDbConnection GetConnection()
        {
            if (_provider == "SQLServer")
            {
                var connectionString = _configuration.GetConnectionString("SQLServerConnection");
                return new SqlConnection(connectionString);
            }
            else if (_provider == "MySQL")
            {
                var connectionString = _configuration.GetConnectionString("MySQLConnection");
                return new MySqlConnection(connectionString);
            }
            throw new Exception("No database provider has been configured.");
        }
    }
}