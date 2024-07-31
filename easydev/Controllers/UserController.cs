using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

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
            
            string recipientEmail = user.Mail;
            string subject = "Verify Your Account";
            string body = "activate your account";

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SENDER_MAIL"), Environment.GetEnvironmentVariable("SENDER_PASSWORD")),
                EnableSsl= true,
                Timeout=10000
            };
            MailMessage mailMessage = new MailMessage(Environment.GetEnvironmentVariable("SENDER_MAIL"),recipientEmail,subject,body);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _postgresContext.Users.Add(user);
                await _postgresContext.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                return BadRequest("Failed to create user" + ex);
            }
            

            return Ok("User created successfully");
        }
    }
}
