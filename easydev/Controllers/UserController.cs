using easydev.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                string text = string.IsNullOrEmpty(loginRequest.Email) ? "User NOT FOUND" : $"User with {loginRequest.Email} NOT FOUND";
                return BadRequest(new {message = text,type = "error"});
            }
            else
            {
                if(loginRequest.Password.Equals(user.Password))
                {
                    if(user.Activated == 1)
                    {
                        return Ok(user);
                    }
                    else
                    {
                        return BadRequest(new { message = "Account not activated",type = "error" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Invalid password",type = "error" });
                }

                
                
            }
  
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Mail))
            {
                return BadRequest();
            }
            string recipientEmail = user.Mail;
            string subject = "Verify Your Account";
            string body = @$"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #dddddd; border-radius: 5px;'>
                    <h1 style='color: #4CAF50; text-align: center;'>Activate Your Account</h1>
                    <p style='font-size: 18px;'>Hello, {user.Mail}</p>
                    <p style='font-size: 16px;'>Thank you for registering with us. Please click the button below to activate your account!</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <a href='http://localhost:4200/account?mail={user.Mail}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #ffffff; background-color: #4338ca; text-decoration: none; border-radius: 5px;'>Activate Account</a>
                    </div>
                    <p style='font-size: 16px;'>Best regards,<br>easydev</p>
                </div>
            </body>
            </html>";

            SmtpClient smtpClient = new SmtpClient("smtp.ionos.es")
            {
                Port = 587,
                Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SENDER_MAIL"), Environment.GetEnvironmentVariable("SENDER_PASSWORD")),
                EnableSsl= true,
                Timeout=10000
            };
            MailMessage mailMessage = new MailMessage(Environment.GetEnvironmentVariable("SENDER_MAIL"), recipientEmail, subject, body)
            {
                IsBodyHtml=true
            };
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _postgresContext.Users.Add(user);
                await _postgresContext.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                return BadRequest(new {message = "Failed creation of user. try Again!", noti = "2"});
            }
            

            return Ok(new { message = "User created successfully!!", noti = "1" });
        }

        [HttpPut]
        public async Task<IActionResult> ActivateAccount([FromQuery] string mail)
        {
            if (string.IsNullOrEmpty(mail))
            {
                return BadRequest("Email cannot be null or empty");
            }

           User user = await _postgresContext.Users.FirstOrDefaultAsync(u => u.Mail.Equals(mail));
           if (user == null)
            {
                return NotFound("User not found");
            }
            if (user.Activated == 1)
            {
                return BadRequest("User account is already activated");
            }

            try
            {

                user.Activated = 1;
                //_postgresContext.Users.Update(user);
                await _postgresContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception message
                Console.WriteLine($"An error occurred while activating the account: {ex.Message}");

                // Return the exception message for debugging purposes (optional)
                return BadRequest($"An error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
