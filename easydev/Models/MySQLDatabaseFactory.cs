using System.Data;
using easydev.Interfaces;
using MySql.Data.MySqlClient;

namespace easydev.Models;

public class MySQLDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"server={db.Host};uid={db.User};pwd={db.Password};database={db.Database1}";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new MySqlConnection(db.GetConnectionString(db)))
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
        MySqlConnection connection = new MySqlConnection(GetConnectionString(db));
        connection.Open();
        using (var command = new MySqlCommand(query,connection))
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
        MySqlConnection connection = new MySqlConnection(GetConnectionString(db));
        connection.Open();
        using (var command = new MySqlCommand(query, connection))
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

    public DataTable GetForeignKeys(Database db, string fromTable)
    {
        throw new NotImplementedException();
    }
}