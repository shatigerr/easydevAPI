using System.Data;
using easydev.Interfaces;
using Microsoft.Data.SqlClient;

namespace easydev.Models;

public class SQLServerDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"Server={db.Host};Database={db.Database1};User Id={db.User};Password={db.Password};TrustServerCertificate=True;Connection Timeout=30;";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new SqlConnection(GetConnectionString(db)))
            {
                conn.Open();
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    connected = true;
                }
            }
        }
        catch (Exception ex)
        {
            connected = false;
        }


        return connected;
    }

    public async Task<List<Dictionary<string, object>>> Get(Database db, string query)
    {
        SqlConnection connection = new SqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new SqlCommand(query, connection))
        {
            using (var reader = command.ExecuteReader())
            {

                var result = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    result.Add(row);
                }

                return result;
            }
        }
    }

    public List<int> Post(Database db, string query)
    {
        SqlConnection connection = new SqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new SqlCommand(query, connection))
        {
            command.CommandTimeout = 120;
            // Agrega los parámetros de manera segura
            try
            {
                var affectedRows = command.ExecuteNonQuery();
                List<int> res = new List<int>();
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }

                
        }
    }

    public DataTable GetDBTables(Database db)
    {
        throw new NotImplementedException();
    }

    public DataTable GetDBColumns(Database db, string table)
    {
        throw new NotImplementedException();
    }
}