using easydev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DatabaseController : ControllerBase
    {
        private readonly PostgresContext _context;

        public DatabaseController(PostgresContext context) 
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDbById(long id)
        {
            Database db = await _context.Databases.Where(d => d.Id == id).FirstAsync();
            if (db == null) return NotFound();
            return Ok(db);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewDatabase([FromBody] Database db)
        {
            string conn = db.GetConnectionString(db);
            if(db.CheckDBConnection(db.Dbengine, conn))
            {
                try
                {
                    _context.Databases.Add(db);
                    await _context.SaveChangesAsync();
                    return Ok(db);
                }
                catch (Exception ex) {
                    return BadRequest("Error");
                }
            }
            else
            {
                return BadRequest("Unable to connect to the database");
            }
            
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDatabase(long id)
        {
            var db = await _context.Databases.FirstOrDefaultAsync(e => e.Id == id);
            if (db != null) _context.Databases.Remove(db);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDatabase(long id, [FromBody] Database db)
        {
            Database database = await _context.Databases.Where(d => d.Id == id).FirstAsync();
            if (db != null && database != null)
            {
                database.Id = id;
                database.Host = db.Host;
                database.Port = db.Port;
                database.Database1 = db.Database1;
                database.Dbengine = db.Dbengine;
                database.Password = db.Password;
                database.User = db.User;

                await _context.SaveChangesAsync();
            }
            else return BadRequest();
            return Ok();
        }

    }
}
