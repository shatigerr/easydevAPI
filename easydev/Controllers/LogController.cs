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
    public class LogController : ControllerBase
    {
        private PostgresContext _context;
        public LogController(PostgresContext context)
        {
            this._context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogsByProject(long id)
        {
            List<Log> Logs = await _context.Logs.Where(l => l.IdProject == id).ToListAsync();
            if (Logs.Count > 0) {
                return Ok(Logs);
            }
            return BadRequest("No logs Found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] Log log)
        {
            if (log != null) {
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
                return Ok(log);
            }

            return BadRequest("Invalid Log");
        }
    }
}
