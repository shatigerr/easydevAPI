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
    public class ProjectController : ControllerBase
    {
        private PostgresContext _context;
        public ProjectController(PostgresContext context)
        {
            this._context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectByUser(long id)
        {
            try
            {
                List<Project> projects = await _context.Projects.AsNoTracking().Where(p => p.IdUser == id).ToListAsync();
                return Ok(projects);

            }
            catch (Exception ex) {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewProject([FromBody] Project project)
        {
            if (project.IdUser != null && project.Iddatabase != null)
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return Ok(project);
            }

            return BadRequest("ERROR A PROJECT NEEDS A DATABASE");

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteProject(long id)
        {
            try
            {
                Project p = await _context.Projects.Where(p => p.Id == id).FirstAsync();
                _context.Remove(p);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest();
            }

        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult>GetProjectById(long id)
        {
            try
            {

                Project project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Endpoints) // Incluir los endpoints relacionados
                .Include(p => p.IddatabaseNavigation)
                .Include(p => p.Logs)
                .FirstAsync(p => p.Id == id);
                if (project == null)
                {
                    return BadRequest("NO PROJECT FOUND");
                }
                else
                {
                    return Ok(project);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(long id, [FromBody] Project project)
        {
            try
            {

                Project p = await _context.Projects.Where(p => p.Id == id).FirstAsync();
                
                if (project == null || p == null)
                {
                    return BadRequest();
                }
                
                p.Id = id;
                p.Title = project.Title;
                p.Description = project.Description;
                p.Key = project.Key;
                
                await _context.SaveChangesAsync();
                return Ok(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }
    }
}
