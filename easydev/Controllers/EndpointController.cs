using easydev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Endpoint = easydev.Models.Endpoint;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEndpoint([FromBody] Endpoint updatedEndpoint ,long id)
        {
            try
            {

                Endpoint endpoint = await _context.Endpoints.Where(e => e.Id == id).FirstAsync();

                if (endpoint == null)
                {
                    return NotFound("Endpoint not found");
                }

                // Actualizar las propiedades del endpoint existente con los valores del updatedEndpoint
                endpoint.Url = updatedEndpoint.Url;
                endpoint.Query = updatedEndpoint.Query;
                endpoint.HttpMethod = updatedEndpoint.HttpMethod;
                endpoint.IdProject = updatedEndpoint.IdProject;
                endpoint.Params = updatedEndpoint.Params;
                endpoint.Id = id;
                // Copiar otras propiedades necesarias...

                // Guardar los cambios en la base de datos
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
