using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using ProduktVerwaltung.Controllers;

namespace ProduktVerwaltung.Services
{
    public class TableService
    {
        private readonly IConfiguration _configuration;

        string connectionString;
        TableServiceClient tableServiceClient;

        public TableService(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration["StorageConnectionString"];
            tableServiceClient = new TableServiceClient(connectionString);
        }

        public void CreateTable(string tableName)
        {
            tableServiceClient.CreateTableIfNotExists(tableName);
        }

        

        internal void AddTableEntry(string v, TableEntity entry)
        {
            try
            {
                TableClient tableClient = new TableClient(connectionString, v);
                tableClient.Create();
                tableClient.AddEntity(entry);
            }
            catch (Exception e)
            {
            }
        }
    }
}