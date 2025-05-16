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

        [HttpPost("auth/{idProject}")]
        public async Task<IActionResult> AuthorizeProjectAccess(long idProject, [FromBody] long idUser)
        {
            int projectsByUser = _context.Projects.Where(x => x.IdUser == idUser && idProject == x.Id).Count();
            bool auth = projectsByUser == 1;
            return Ok(new {res=auth});
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewProject([FromBody] Project project)
        {
            if (project.IdUser != null)
            {
                Database db = project.db;
                db.Dbengine = db.Dbengine.ToUpper();
                _context.Databases.Add(db);
                await _context.SaveChangesAsync();

                project.Iddatabase = db.Id;
                if (project.Iddatabase != null)
                {
                    

                    project.db = db;
                    _context.Projects.Add(project);

                    await _context.SaveChangesAsync();

                    return Ok(project);
                }
                
                
            }

            return BadRequest(new {msg="Error during the creation of the project"});

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteProject(long id)
        {
            try
            {
                Project p = await _context.Projects.Where(p => p.Id == id).FirstAsync();
                Database db = _context.Databases.Where(x => x.Id == p.Iddatabase).First();
                List<TableDB> tables = _context.TableDB.Where(x => x.iddatabase == db.Id).ToList();
                List<ColumnDB> columns = new List<ColumnDB>();
                foreach (TableDB tb in tables)
                {
                    columns.AddRange(_context.ColumnDB.Where(x => x.tableid == tb.id).ToList());
                }
                
                _context.Remove(p);
                _context.RemoveRange(columns);
                _context.RemoveRange(tables);
                _context.Remove(db);
                
                await _context.SaveChangesAsync();
                return Ok(new {msg = "Project deleted succesfully"});
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
                .Include(p => p.db)
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
                
                if (project == null ||  p == null)
                {
                    return BadRequest(new {msg= "Project not found." });
                }
                
                p.Id = id;
                p.Title = project.Title;
                p.Description = project.Description;
                p.Key = project.Key;
                

                if(project.db != null)
                {
                    Database db = await _context.Databases.Where(x => x.Id == p.Iddatabase).FirstAsync();
                    db.Dbengine = project.db.Dbengine;
                    db.Host = project.db.Host;
                    db.Port = project.db.Port;
                    db.User = project.db.User;
                    db.Password = project.db.Password;
                    db.Database1 = project.db.Database1;
                } 
                await _context.SaveChangesAsync();
                return Ok(p);
            }
            catch (Exception ex)
            {
                return BadRequest(new {msg = "Unable to update the project settings." });
            }
        }
    }
}
