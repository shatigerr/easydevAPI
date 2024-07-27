using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly PostgresContext _context;
        public RequestController(PostgresContext context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRequest([FromBody] Request request)
        {

            if (request != null)
            {
                var connectionString = $"User Id={request.database.User};Password={request.database.Password};Server={request.database.Host};Port={request.database.Port};Database={request.database.Database1};";

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseNpgsql(connectionString);
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = $"{request.Query}";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        //command.Parameters.AddWithValue("@EntityId", request.EntityId);

                        using (var reader = await command.ExecuteReaderAsync())
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

                            return Ok(result);

                        }
                    }
                }
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> PostRequest([FromBody] Request request)
        {
            //User Id=postgres.otkuubonfftnnpjfpiny;Password=1ltWFTGC3rbkB5WS;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;

            if (request != null)
            {

                List<Dictionary<string, object>> result;
                dynamic conn;
                //var connectionString = $"User Id={request.database.User};Password={request.database.Password};Server={request.database.Host};Port={request.database.Port};Database={request.database.Database1};Timeout=300;CommandTimeout=300;Pooling=false;";
                if (request.database.Dbengine == "POSTGRESQL")
                {
                    conn = request.postgreSqlConnection();
                    conn.Open();
                    
                    if (request.Query.ToUpper().StartsWith("SELECT"))
                    {
                        result = await request.PostgreGetRequest(conn);
                        return Ok(result);
                    }
                    else
                    {
                        int affectedRows = request.PostgrePostRequest(conn);
                        return Ok(affectedRows);
                    }

                }
                else if(request.database.Dbengine == "MYSQL")
                {
                    conn = request.mySqlConnection();
                    conn.Open();
                    
                    if (request.Query.ToUpper().StartsWith("SELECT"))
                    {
                        result = await request.MysqlGetRequest(conn);
                        return Ok(result);
                    }
                    else
                    {
                        int affectedRows = request.MysqlPostRequest(conn);
                        return Ok(affectedRows);
                    }
                }
            }
            return BadRequest();
        }
        
    }
}
