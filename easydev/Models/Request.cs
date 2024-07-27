using MySql.Data.MySqlClient;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace easydev.Models
{
    public partial class Request
    {
        public Database database { get; set; }
        public string Query { get; set; }

        public string? Params { get; set; }

        public string GetQuery()
        {
            int num = 0;
            if(this.Params != null)
            {

                string[] paramsArr = this.Params.Split(',');
                string[] queryArr = this.Query.Split();
                for (int i = 0; queryArr.Length > i; i++)
                {

                    if (queryArr[i] == "?")
                    {
                        queryArr[i] = paramsArr[num];
                    }
                }

                var query = string.Join(" ", queryArr);
                return query;
            }
            return this.Query;

        }

        public int PostgrePostRequest(NpgsqlConnection connection)
        {
            int num = 0;
            var paramsArr = this.Params.Split(',');
            var queryArr = this.Query.Split(" ");

            for (int i = 0; i < queryArr.Length; i++)
            {
                if (queryArr[i] == "?")
                {
                    queryArr[i] = paramsArr[num];
                    num++;
                }
            }

            this.Query = string.Join(" ", queryArr);
            using (var command = new NpgsqlCommand(this.Query, connection))
            {
                command.CommandTimeout = 120;
                // Agrega los parámetros de manera segura
                try
                {
                    var affectedRows = command.ExecuteNonQuery();
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    throw;
                }

                
            }
        }

        public int MysqlPostRequest(MySqlConnection connection)
        {
            int num = 0;
            var paramsArr = this.Params.Split(',');
            var queryArr = this.Query.Split(" ");

            for (int i = 0; i < queryArr.Length; i++)
            {
                if (queryArr[i] == "?")
                {
                    queryArr[i] = paramsArr[num];
                    num++;
                }
            }

            this.Query = string.Join(" ", queryArr);
            using (var command = new MySqlCommand(this.Query, connection))
            {
                command.CommandTimeout = 120;
                // Agrega los parámetros de manera segura
                try
                {
                    var affectedRows = command.ExecuteNonQuery();
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    throw;
                }


            }
        }

        public async Task<List<Dictionary<string, object>>> PostgreGetRequest(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand(this.GetQuery(), connection))
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

        public async Task<List<Dictionary<string, object>>> MysqlGetRequest(MySqlConnection connection)
        {
            using (var command = new MySqlCommand(this.GetQuery(),connection))
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

        public MySqlConnection mySqlConnection()
        {

            MySqlConnection conn = new MySqlConnection(this.database.GetConnectionString(this.database));
            return conn;

        }

        public NpgsqlConnection postgreSqlConnection()
        {
            string connString = this.database.GetConnectionString(this.database);
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            return conn;
        }

    }
}
