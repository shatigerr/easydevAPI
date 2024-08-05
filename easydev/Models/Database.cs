using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace easydev.Models;

public partial class Database
{
    public long Id { get; set; }

    public string? Dbengine { get; set; }

    public string? Host { get; set; }

    public string? Password { get; set; }

    public string? User { get; set; }

    public string? Database1 { get; set; }

    public string? Port { get; set; }

    [JsonIgnore]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public string GetConnectionString(Database db)
    {
        string connectionString = "";
        if (db.Dbengine.ToUpper() == "POSTGRESQL")
        {
            connectionString = $"User Id={db.User};Password={db.Password};Server={db.Host};Port={db.Port};Database={db.Database1};Timeout=300;CommandTimeout=300;Pooling=false;";
        }
        else if (db.Dbengine.ToUpper() == "MYSQL")
        {
            connectionString = $"server={db.Host};uid={db.User};pwd={db.Password};database={db.Database1}";
        }

        return connectionString;
    }

    public bool CheckDBConnection(string dbEngine, string connString)
    {
        if (dbEngine == "POSTGRESQL")
        {
            return checkPostgreSQLConnection(connString);
        }
        else
        {
            return checkMySqlConnection(connString);
        }
    }



    private bool checkMySqlConnection(string connString)
    {
        bool connected = false;
        try
        {
            using (var conn = new MySqlConnection(connString))
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

    private bool checkPostgreSQLConnection(string connString)
    {
        bool connected = false;
        try
        {
            using (var conn = new NpgsqlConnection(connString))
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
}