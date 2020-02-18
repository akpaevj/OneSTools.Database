using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using OneSTools.Config;
using System.Threading;

namespace OneSTools.DatabaseTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var connStr = new SqlConnectionStringBuilder()
            {
                DataSource = "localhost",
                InitialCatalog = "struct",
                IntegratedSecurity = true
            }.ToString();

            //var connStr = new SqlConnectionStringBuilder()
            //{
            //    DataSource = "s01",
            //    InitialCatalog = "erp_main",
            //    UserID = "sa",
            //    Password = "1qazxsw@"
            //}.ToString();

            var connection = new SqlConnection(connStr);

            await connection.OpenAsync();

            try
            {
                await Configuration.ReadFromDatabaseAsync(connection, new CancellationToken());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }

            return 0;
        }
    }
}
