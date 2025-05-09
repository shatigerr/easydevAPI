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
                return BadRequest(new { msg = text,type = "error"});
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
                        return BadRequest(new { msg = "Account not activated",type = "error" });
                    }
                }
                else
                {
                    return BadRequest(new { msg = "Incorrect password",type = "error" });
                }

                
                
            }
  
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            if(user.Password != user.Password2) return BadRequest(new { msg = "The passwords do not match. Please check again.", type = "error" });
            if (string.IsNullOrEmpty(user.Mail))
            {
                return BadRequest(new { msg = "You haven't entered any email.", type = "error" }); 
            }
            string recipientEmail = user.Mail;
            string subject = "Verify Your Account";
            string body = @$"
            <html>
            <body style='font-family: Inter, Arial, sans-serif; background-color: #f9fafb; padding: 40px 0;'>
                <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 12px; border: 1px solid #e5e7eb; box-shadow: 0 2px 8px rgba(0,0,0,0.04);'>
                    <h1 style='color: #111827; font-size: 28px; text-align: center; margin-bottom: 24px;'>Activate Your Account</h1>

                    <p style='font-size: 16px; color: #4b5563; margin-bottom: 20px;'>Hello, <b>{user.Mail}</b></p>

                    <p style='font-size: 16px; color: #4b5563; margin-bottom: 30px;'>
                        Thank you for joining <strong>easydev</strong>. To start using your account, please activate it by clicking the button below.
                    </p>

                    <div style='text-align: center; margin-bottom: 40px;'>
                        <a href='http://localhost:4200/account?mail={user.Mail}'
                            style='display: inline-block; padding: 12px 28px; background-color: #111827; color: #ffffff; font-size: 16px; text-decoration: none; border-radius: 6px;'>
                            Activate Account
                        </a>
                    </div>

                    <p style='font-size: 14px; color: #9ca3af; text-align: center;'>
                        If you didn't create this account, please ignore this email.
                    </p>

                    <p style='font-size: 14px; color: #9ca3af; text-align: center; margin-top: 24px;'>
                        — The easydev Team
                    </p>
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
                return BadRequest(new { msg = "Failed creation of user. try Again!", type = "error" });
            }
            

            return Ok(new { msg="User created succesfully!", type="succesfull"});
        }

        [HttpPut]
        public async Task<IActionResult> ActivateAccount([FromQuery] string mail)
        {
            if (string.IsNullOrEmpty(mail))
            {
                
                return BadRequest(new { msg = "Email cannot be null or empty", type = "error" });
            }

           User user = await _postgresContext.Users.FirstOrDefaultAsync(u => u.Mail.Equals(mail));
           if (user == null)
            {
                
                return NotFound(new { msg = "User not found", type = "error" });
            }
            if (user.Activated == 1)
            {
                
                return BadRequest(new { msg = "User account is already activated", type = "error" });
            }

            try
            {

                user.Activated = 1;
                await _postgresContext.SaveChangesAsync();
                return Ok(new { msg = "OK", type = "success" });
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
