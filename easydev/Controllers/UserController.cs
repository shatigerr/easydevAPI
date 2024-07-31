using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Net.Mail;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PostgresContext _postgresContext;

        public UserController(PostgresContext context)
        {
            this._postgresContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers() // Especifica el nombre del método y el tipo de retorno
        {
            var users = await _postgresContext.Users.ToListAsync(); // Asegúrate de que 'Users' es el DbSet en tu contexto
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest loginRequest)
        {
            var user = await _postgresContext.Users.FirstOrDefaultAsync(user => user.Mail.Equals(loginRequest.Email));
            if (user == null) 
            {
                return BadRequest($"User with {loginRequest.Email} NOT FOUND");
            }
            else
            {
                if(loginRequest.Password.Equals(user.Password))
                {
                    return Ok(user);
                }

                return BadRequest("Incorrect password");
            }
  
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            _postgresContext.Users.Add(user);
            await _postgresContext.SaveChangesAsync();
            
            return Ok();
        }
    }
}
