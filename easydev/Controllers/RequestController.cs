using easydev.Interfaces;
using easydev.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
            Project proj = _context.Projects.Where(x => x.Id == request.idProject).First();
            request.database = _context.Databases.Where(x => x.Id == proj.Iddatabase).First();
            DatabaseFactory dbFactory = new DatabaseFactory();
            IDatabaseFactory db = dbFactory.CreateFactory(request.database);
            if (db.CheckDBConnection(request.database))
            {
                var result = db.Get(request.database, request.GetQuery());
                return Ok(result);
            }
            
            return BadRequest();
            
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

            if (request != null)
            {
                Project proj = _context.Projects.Where(x => x.Id == request.idProject).First();
                request.database = _context.Databases.Where(x => x.Id == proj.Iddatabase).First();
                
                DatabaseFactory dbFactory = new DatabaseFactory();
                IDatabaseFactory db = dbFactory.CreateFactory(request.database);
                string formatedQuery = request.GetQuery();
                if (db.CheckDBConnection(request.database))
                {
                    if (formatedQuery.StartsWith("SELECT"))
                    {
                        var resultado = db.Get(request.database, formatedQuery);
                        return Ok(resultado.Result);
                    }
                    else
                    {
                        var resultado = db.Post(request.database, formatedQuery);
                        return Ok(new { resultado });
                    }
                }
            
                return BadRequest();
                
                
            }
            return BadRequest();
        }
        
    }
}
