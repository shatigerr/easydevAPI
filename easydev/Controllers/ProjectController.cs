using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            List<Project> projects = await _context.Projects.Where(p => p.IdUser == id).ToListAsync();

            return Ok(projects);
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

        [HttpGet("details/{id}")]
        public async Task<IActionResult>GetProjectById(long id)
        {
            try
            {

                Project project =  _context.Projects
                    .Include(p => p.Endpoints)// Incluir los endpoints relacionados
                    .Include(p => p.IddatabaseNavigation)
                    .First(p => p.Id == id);
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
    }
}
