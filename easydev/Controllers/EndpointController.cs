using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Endpoint = easydev.Models.Endpoint;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EndpointController : ControllerBase
    {
        private readonly PostgresContext _context;

        public EndpointController(PostgresContext context) 
        {
            this._context = context;
        }

        [HttpGet("{idProject}")]
        public async Task<IActionResult> GetEnpointsByProject(long idProject)
        {
            List<Endpoint> endpoints = await _context.Endpoints.Where(e => e.IdProject == idProject).ToListAsync();

            return Ok(endpoints);
        }

        

        [HttpPost]
        public async Task<IActionResult> CreateNewEndpoint([FromBody] Endpoint endpoint)
        {
            try
            {
                _context.Endpoints.Add(endpoint);
                await _context.SaveChangesAsync();
                return Ok(endpoint);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndpoint(long id)
        {
            var endpoint = await _context.Endpoints.FirstOrDefaultAsync(e => e.Id == id);
            if (endpoint != null) _context.Endpoints.Remove(endpoint);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
