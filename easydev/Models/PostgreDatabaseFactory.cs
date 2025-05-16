using System.Data;
using easydev.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace easydev.Models;

public class PostgreDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"User Id={db.User};Password={db.Password};Server={db.Host};Port={db.Port};Database={db.Database1};Timeout=300;CommandTimeout=300;Pooling=false;";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new NpgsqlConnection(GetConnectionString(db)))
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
        NpgsqlConnection connection = new NpgsqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new NpgsqlCommand(query, connection))
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
        NpgsqlConnection connection = new NpgsqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new NpgsqlCommand(query, connection))
        {
            command.CommandTimeout = 120;
            // Agrega los parámetros de manera segura
            try
            {
                var affectedRows = command.ExecuteNonQuery();
                List<int> res = new List<int>();
                res.Add(affectedRows);
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
        DataTable dataTable = new DataTable();
        using (var conn = new Npgsql.NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
            ";

            using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
            using (var adapter = new Npgsql.NpgsqlDataAdapter(cmd))
            {
                adapter.Fill(dataTable);
            }
        }

        return dataTable;
    }

    public DataTable GetDBColumns(Database db, string table)
    {
        DataTable dataTable = new DataTable();
        using (var conn = new Npgsql.NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
                SELECT 
                    column_name,
                    data_type,
                    character_maximum_length,
                    is_nullable,
                    column_default
                FROM information_schema.columns
                WHERE table_schema = 'public'
                AND table_name = @tableName;
            ";

            using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", table);

                using (var adapter = new Npgsql.NpgsqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    public DataTable GetForeignKeys(Database db, string fromTable)
    {
        DataTable dataTable = new DataTable();
        using (var conn = new NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
            SELECT 
                tc.constraint_name,
                tc.table_name AS from_table,
                ccu.table_name AS to_table,
                kcu.column_name AS from_column,
                ccu.column_name AS to_column
            FROM 
                information_schema.table_constraints tc
            JOIN 
                information_schema.key_column_usage kcu 
                ON tc.constraint_name = kcu.constraint_name 
                AND tc.constraint_schema = kcu.constraint_schema
            JOIN 
                information_schema.constraint_column_usage ccu 
                ON ccu.constraint_name = tc.constraint_name 
                AND ccu.constraint_schema = tc.constraint_schema
            WHERE 
                tc.constraint_type = 'FOREIGN KEY'
                AND tc.table_name = @fromTable;
        ";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@fromTable", fromTable);

                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }


}