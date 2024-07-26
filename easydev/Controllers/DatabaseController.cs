using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly PostgresContext _context;

        public DatabaseController(PostgresContext context) 
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewDatabase([FromBody] Database db)
        {
            string conn = db.GetConnectionString(db);
            if(db.CheckDBConnection(db.Dbengine, conn))
            {
                _context.Databases.Add(db);
                await _context.SaveChangesAsync();
                return Ok(db);
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

    }
}
