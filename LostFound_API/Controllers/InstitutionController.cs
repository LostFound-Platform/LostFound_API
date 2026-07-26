using Azure;
using LostFound_API.DTOs.Institution;
using LostFound_API.DTOs.Users;
using LostFound_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ObjectBusiness;
using Repository;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LostFound_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitutionController : ControllerBase
    {
        #region Variables
        private readonly IInstitutionRepository institutionRepository;
        private readonly IUsersRepository usersRepository;
        private readonly StateService stateService;
        private readonly IMemoryCache memoryCache;
        private readonly IConfiguration configuration;
        private readonly EmailSender emailSender;
        private readonly IHttpClientFactory httpClientFactory;
        #endregion

        #region Constructor
        public InstitutionController(IInstitutionRepository institutionRepository,
                               StateService stateService,
                               IConfiguration configuration,
                               IHttpClientFactory httpClientFactory,
                               IUsersRepository usersRepository,
                               EmailSender emailSender,
                               IMemoryCache memoryCache)
        {
            this.institutionRepository = institutionRepository;
            this.stateService = stateService;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.usersRepository = usersRepository;
            this.emailSender = emailSender;
            this.memoryCache = memoryCache;
        }
        #endregion

        [HttpGet("full-state-name")]
        public IActionResult Test()
        {
            var state = stateService.ByCode["GA"];

            return Ok(state.Name);
        }

        // GET: api/<InstitutionController>
        [HttpGet]
        public ActionResult<Institution> Get()
        {
            var institutions = institutionRepository.AllInstitutions();
            return Ok(institutions);
        }

        #region Sign Up
        // POST api/<InstitutionController>
        [HttpPost("sign-up")]
        public async Task<ActionResult> CreateInstitution([FromBody] UserDTO userDTO)
        {
            if (userDTO == null || string.IsNullOrWhiteSpace(userDTO.InstitutionName) || string.IsNullOrWhiteSpace(userDTO.InstitutionAddress))
            {
                return BadRequest("Invalid institution data");
            }

            var institution = new Institution
            {
                InstitutionId = new Random().Next(),
                InstitutionAddress = userDTO.InstitutionAddress,
                InstitutionCity = userDTO.InstitutionCity,
                InstitutionState = userDTO.InstitutionState,
                InstitutionName = userDTO.InstitutionName,
                InstitutionWebsite = userDTO.InstitutionWebsite,
                InstitutionPhone = userDTO.InstitutionPhone,
            };

            var isAddedInstitution = await institutionRepository.CreateInstitution(institution);

            if (isAddedInstitution)
            {
                var userSignUp = new Users
                {
                    UserId = new Random().Next(),
                    Email = userDTO.Email,
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    InstitutionId = institution.InstitutionId,
                    Password = userDTO.Password,
                    PickImage1 = userDTO.PickImage1,
                    PickImage2 = userDTO.PickImage2,
                    Role = Role.Student,
                };

                var isAddedUser = await usersRepository.SignUp(userSignUp);
                if (isAddedUser)
                {
                    // Send verification code via email
                    var token = Guid.NewGuid().ToString();
                    memoryCache.Set($"EMAIL_VERIFY_{token}", userDTO.Email, TimeSpan.FromMinutes(15));

                    string senderName = "Back2Me";
                    string senderEmail = "mycampuslostfound@gmail.com";
                    string toName = institution.InstitutionName;
                    string toEmail = userDTO.Email;
                    string subject = "🏫 Verify Your Institution Registration";

                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
                            line-height: 1.6;
                            color: #072138;
                            background-color: #f9f9fb;
                            padding: 20px;
                        }}

                        a {{
                            text-decoration: none !important;
                        }}

                        .container {{
                            max-width: 600px;
                            margin: auto;
                            background: #ffffff;
                            border-radius: 12px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            overflow: hidden;
                        }}

                        .header {{
                            background-color: #ec7207;
                            color: #fff;
                            padding: 20px;
                            text-align: center;
                        }}

                        .content {{
                            padding: 20px;
                        }}

                        .highlight {{
                            background: #fffae6;
                            border-left: 4px solid #ff9900;
                            padding: 12px 16px;
                            margin: 18px 0;
                            border-radius: 6px;
                        }}

                        .btn {{
                            display: inline-block;
                            background-color: #ec7207;
                            color: white !important;
                            padding: 12px 24px;
                            border-radius: 20px;
                            font-weight: 600;
                            text-decoration: none;
                        }}

                        .footer {{
                            background: #f4f6f9;
                            padding: 15px;
                            text-align: center;
                            font-size: 0.9em;
                            color: #666;
                        }}
                      </style>
                    </head>

                    <body>

                    <div class='container'>

                        <div class='header'>
                            <h2>🏫 Verify Your Institution Registration</h2>
                        </div>

                        <div class='content'>

                            <p>Hello,</p>

                            <p>
                                Thank you for registering
                                <strong>{institution.InstitutionName} at {institution.InstitutionAddress} Campus</strong>
                                with <strong>Back2Me</strong>.
                            </p>

                            <p>
                                To activate your institution and complete the registration process,
                                please verify your administrator email by clicking the button below.
                            </p>

                            <div class='highlight'>
                                ⚡ This verification link will expire in <strong>15 minutes</strong>.
                            </div>

                            <p style='text-align:center;'>
                                <a href='https://lfcampus.vercel.app/verify-institution?token={token}' class='btn'>
                                    Verify Institution
                                </a>
                            </p>

                            <p>
                                If you did not submit this registration request,
                                you can safely ignore this email.
                            </p>

                        </div>

                        <div class='footer'>
                            Best regards,<br/>
                            <strong>Back2Me Team</strong>
                        </div>

                    </div>

                    </body>
                    </html>";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                    // JWT config
                    var issuer = configuration["JwtConfig:Issuer"];
                    var audience = configuration["JwtConfig:Audience"];
                    var key = configuration["JwtConfig:Key"];
                    var tokenValidityMins = configuration.GetValue<int>("JwtConfig:TokenValidityMins");
                    var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins); // Token expiration time

                    // Create JWT access token and assign token
                    var tokenDescriptor = new SecurityTokenDescriptor // Describe token
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                                new Claim(ClaimTypes.NameIdentifier, userDTO.Email),
                                new Claim(ClaimTypes.Role, userDTO.Role.ToString()),
                            }),
                        Issuer = issuer,
                        Audience = audience,
                        Expires = tokenExpiryTimeStamp,
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                            SecurityAlgorithms.HmacSha256Signature
                        )
                    };

                    // Process token was described
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var securityToken = tokenHandler.CreateToken(tokenDescriptor); // Create object JWT Token
                    var accessToken = tokenHandler.WriteToken(securityToken); // Serialize token to string  for client

                    // Assign token for client
                    userDTO.AccessToken = accessToken;
                    userDTO.ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds;

                    return Ok(new
                    {
                        Message = "Created institution successfully"
                    });
                }
            }

            return BadRequest("Create institution failed");
        }
        #endregion

        #region Verify Institution
        [HttpGet("verify-institution")]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token, [FromQuery] bool isForgotPassword)
        {
            try
            {
                if (!memoryCache.TryGetValue($"EMAIL_VERIFY_{token}", out string userEmail))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid or expired verification link"
                    });
                }

                var user = await usersRepository.GetUserByEmail(userEmail);

                // Seperate send email is verified and don't send for forgot password
                if (!isForgotPassword)
                {
                    user.IsVerifiedEmail = true;
                    var isUpdated = await usersRepository.UpdateUser();

                    if (isUpdated)
                    {
                        var institution = await institutionRepository.GetInstitutionByID(user.InstitutionId);

                        // Send email: Institution email verified
                        string senderName = "Back2Me";
                        string senderEmail = "mycampuslostfound@gmail.com";
                        string toName = institution.InstitutionName;
                        string toEmail = user.Email;
                        string subject = "🏫 Your Institution Has Been Verified!";
                        string content = $@"
                        <html>
                        <head>
                          <style>
                            body {{
                                font-family: 'Segoe UI', Arial, sans-serif;
                                line-height: 1.6;
                                color: #072138;
                                background-color: #f9f9fb;
                                padding: 20px;
                            }}

                            a {{
                                text-decoration: none !important;
                            }}

                            .container {{
                                max-width: 600px;
                                margin: auto;
                                background: #ffffff;
                                border-radius: 12px;
                                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                overflow: hidden;
                            }}

                            .header {{
                                background-color: #28a745;
                                color: #fff;
                                padding: 20px;
                                text-align: center;
                            }}

                            .header h2 {{
                                margin: 0;
                                font-size: 22px;
                            }}

                            .content {{
                                padding: 20px;
                            }}

                            .content p {{
                                margin: 10px 0;
                            }}

                            .highlight {{
                                background: #e6ffed;
                                border-left: 4px solid #28a745;
                                padding: 12px 16px;
                                margin: 18px 0;
                                border-radius: 6px;
                                font-weight: 600;
                                color: #155724;
                            }}

                            .btn {{
                                display: inline-block;
                                background-color: #28a745;
                                color: #fff !important;
                                font-weight: 600;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                            }}

                            .footer {{
                                background: #f4f6f9;
                                padding: 15px;
                                text-align: center;
                                font-size: 0.9em;
                                color: #666;
                            }}
                          </style>
                        </head>

                        <body>

                        <div class='container'>

                            <div class='header'>
                                <h2>🏫 Institution Verified Successfully!</h2>
                            </div>

                            <div class='content'>

                                <p>Hello,</p>

                                <p>
                                    Congratulations! Your institution
                                    <strong>{institution.InstitutionName}</strong>
                                    has been successfully verified.
                                </p>

                                <div class='highlight'>
                                    🎉 Your Institution Administrator account is now active and your campus is ready to use Back2Me.
                                </div>

                                <p>
                                    You can now sign in to manage your institution, oversee lost and found activities, and support students on your campus.
                                </p>

                                <p style='text-align:center;'>
                                    <a href='https://lfcampus.vercel.app/login' class='btn'>
                                        Sign In
                                    </a>
                                </p>

                                <p>
                                    If you did not perform this verification, please contact the Back2Me support team immediately.
                                </p>

                            </div>

                            <div class='footer'>
                                <p>
                                    Best regards,<br/>
                                    <strong>Back2Me Team</strong>
                                </p>
                            </div>

                        </div>

                        </body>
                        </html>
                        ";

                        await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                    }
                }

                // Remove cache
                memoryCache.Remove($"EMAIL_VERIFY_{token}");

                return Ok(new
                {
                    Message = "Verified email successfully",
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        // PUT api/<InstitutionController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<InstitutionController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
