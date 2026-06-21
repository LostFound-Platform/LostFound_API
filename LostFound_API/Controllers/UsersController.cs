using FBLA_API.DTOs.Auth;
using FBLA_API.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ObjectBusiness;
using Repository;
using Services;
using SignalRLayer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Variables
        private readonly IUsersRepository usersRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IWebHostEnvironment webHost;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public UsersController(IUsersRepository usersRepository,
                               IWebHostEnvironment webHost,
                               IConfiguration configuration,
                               IStudentRepository studentRepository,
                               IMemoryCache memoryCache,
                               EmailSender emailSender,
                               IHubContext<SystemHub> hubContext)
        {
            this.usersRepository = usersRepository;
            this.memoryCache = memoryCache;
            this.hubContext = hubContext;
            this.webHost = webHost;
            this.configuration = configuration;
            this.studentRepository = studentRepository;
            this.emailSender = emailSender;
        }
        #endregion

        // GET: api/<UsersController>
        #region Get All Users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<Users>>> Get()
        {
            var users = await usersRepository.AllUsers().ToListAsync();
            var results = users.Select(user => new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.Avatar,
                UrlAvatar = user.UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{user.Avatar}",
                user.IsActive,
                user.IsAgreedToTerms,
                user.IsVerifiedEmail,
                user.CreatedAt,
                user.StudentId
            });

            return Ok(results);
        }
        #endregion

        #region Search Email
        [Authorize(Roles = "Admin")]
        [HttpGet("search-email")]
        public async Task<ActionResult<List<Users>>> SearchEmail([FromQuery] string query)
        {
            var users = await usersRepository.SearchUserByEmail(query).ToListAsync();

            var results = users.Select(user => new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.Avatar,
                UrlAvatar = user.UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{user.Avatar}",
                user.IsActive,
                user.IsAgreedToTerms,
                user.IsVerifiedEmail,
                user.CreatedAt,
                user.StudentId
            });

            return Ok(users);
        }
        #endregion

        // GET My Profile: api/<UsersController>
        #region Get My Profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult> GetMyProfile()
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Get User email from user signed in in JWT (AccessToken)

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    message = "User not authenticated"
                });
            }

            var user = await usersRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound(new
                {
                    Message = "User not found"
                });
            }

            return Ok(new
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                UrlAvatar = user.UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{user.Avatar}",
                Email = user.Email,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth,
                IsVerifiedEmail = user.IsVerifiedEmail,
                Student = user.Student == null
                     ? null
                     : new
                     {
                         user.Student.StudentId,
                         user.Student.CreatedAt
                     }
            });
        }
        #endregion

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UsersController>
        #region Sign Up
        [HttpPost("sign-up")]
        public async Task<ActionResult> SignUp([FromBody] Users user)
        {
            // Check duplicate student ID
            var existingStudentByStudentId = studentRepository.AllStudents()
                                                                      .Any(s => s.StudentId == user.StudentId);

            if (existingStudentByStudentId)
            {
                return Conflict(new
                {
                    message = "Your student ID is already in use"
                });
            }

            // Check duplicate email
            var existingUserByEmail = usersRepository.AllUsers()
                                                     .Any(u => u.Email.Equals(user.Email));

            if (existingUserByEmail)
            {
                return Conflict(new
                {
                    message = "Your email is already in use"
                });
            }

            try
            {
                var isAddedUser = await usersRepository.SignUp(user);
                if (isAddedUser)
                {
                    var student = new Student
                    {
                        StudentId = user.StudentId,
                        UserId = user.UserId,
                    };

                    var isAddedStudent = await studentRepository.SignUpStudent(student);

                    if (isAddedStudent)
                    {
                        // Send verification code via email
                        var token = Guid.NewGuid().ToString();
                        memoryCache.Set($"EMAIL_VERIFY_{token}", user.Email, TimeSpan.FromMinutes(15));

                        // Send verification email
                        // Send email verify
                        string senderName = "Back2Me";
                        string senderEmail = "baoandng07@gmail.com";
                        string toName = user.FirstName + " " + user.LastName;
                        string toEmail = user.Email;
                        string subject = "✅ Please Verify Your Email";
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
                                background: #fffae6;
                                border-left: 4px solid #ff9900;
                                padding: 10px 15px;
                                margin: 15px 0;
                                border-radius: 6px;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #ec7207;
                                border: none;
                                width: max-content;
                                color: #fff !important;
                                font-weight: 600;
                                cursor: pointer;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                                margin-top: 10px;
                                margin-bottom: 10px;
                                transition: all 0.3s ease-in-out;
                            }}
                            .btn:hover {{
                                transform: scale(1.05);
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
                              <h2>✅ Verify Your Email</h2>
                            </div>
                            <div class='content'>
                              <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>
                              <p>Thank you for registering! Please verify your email address by clicking the button below:</p>

                              <p style='text-align: center;' class='highlight'>
                                ⚡ This verification link will expire in 15 minutes.
                              </p>

                              <p style='text-align: center;'>
                                 <a href='https://back2me.vercel.app/verify-email?token={token}' class='btn'>✅ Verify Email</a>
                              </p>

                              <p>If you did not perform this action, you can safely ignore this email.</p>
                            </div>
                            <div class='footer'>
                              <p>Best regards,<br/><strong>Back2me Team</strong></p>
                            </div>
                          </div>
                        </body>
                        </html>
                        ";

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
                                new Claim(ClaimTypes.NameIdentifier, user.Email),
                                new Claim(ClaimTypes.Role, user.Role.ToString()),
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
                        user.AccessToken = accessToken;
                        user.ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds;

                        //Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                        //{
                        //    Secure = true, // Set to true in production
                        //    HttpOnly = true,
                        //    SameSite = SameSiteMode.None, // If SameSite is none Secure must be true
                        //    Expires = tokenExpiryTimeStamp // Set cookie expiration time
                        //});

                        //Response.Cookies.Append("Username", user.FirstName + user.LastName, new CookieOptions
                        //{
                        //    Secure = true,
                        //    SameSite = SameSiteMode.None,
                        //    Expires = tokenExpiryTimeStamp
                        //});

                        return Ok(new
                        {
                            Message = "Signed up successfully",
                            AccessToken = accessToken,
                        });
                    }
                }

                return BadRequest("Failed to sign up. Please try again");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        #region Sign In
        [HttpPost("sign-in")]
        public async Task<ActionResult> SignIn([FromBody] SignInRequestDTO signInRequestDTO)
        {
            try
            {
                var user = await usersRepository.SignIn(signInRequestDTO.StudentId, signInRequestDTO.Password, signInRequestDTO.Email);

                if (user != null && !user.IsActive)
                {
                    return Forbid();
                }

                if (user != null)
                {
                    return Ok(); // Ok to change to select image
                }

                return Unauthorized(signInRequestDTO.StudentId == 0 ?
                                    "Email or password is invalid" :
                                    "Student ID or password is invalid");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        #region Sign In with select images
        [HttpPost("select-images")]
        public async Task<IActionResult> SelectImages([FromBody] SignInRequestDTO signInRequestDTO)
        {
            try
            {
                var user = await usersRepository.SignIn(signInRequestDTO.StudentId, signInRequestDTO.Password, signInRequestDTO.Email);
                if (user != null)
                {
                    // Check Pick image
                    var correctImages = new List<int>
                    {
                        Convert.ToInt32(user.PickImage1),
                        Convert.ToInt32(user.PickImage2)
                    };

                    var isCorrect = signInRequestDTO.PickedIndexes.Count == correctImages.Count &&
                                    signInRequestDTO.PickedIndexes.All(i => correctImages.Contains(i));

                    if (isCorrect)
                    {
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
                                new Claim(ClaimTypes.NameIdentifier, user.Email),
                                new Claim(ClaimTypes.Role, user.Role.ToString()),
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
                        user.AccessToken = accessToken;
                        user.ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds;

                        //Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                        //{
                        //    Secure = true, // Set to true in production
                        //    HttpOnly = true,
                        //    SameSite = SameSiteMode.None, // If SameSite is none Secure must be true
                        //    Expires = tokenExpiryTimeStamp // Set cookie expiration time
                        //});

                        //Response.Cookies.Append("Username", user.FirstName + user.LastName, new CookieOptions
                        //{
                        //    Secure = true,
                        //    SameSite = SameSiteMode.None,
                        //    Expires = tokenExpiryTimeStamp
                        //});

                        return Ok(new
                        {
                            Message = "Signed in successfully",
                            AccessToken = accessToken,
                        });
                    }
                    else
                    {
                        return Unauthorized("Incorrect selection");
                    }
                }

                return Unauthorized("Student ID or password is invalid");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        #region Verify Email
        [HttpGet("verify-email")]
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
                        // Send email email has been verified
                        string senderName = "Back2Me";
                        string senderEmail = "baoandng07@gmail.com";
                        string toName = user.FirstName + " " + user.LastName;
                        string toEmail = user.Email;
                        string subject = "✅ Your Email Has Been Verified!";
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
                                background-color: #28a745; /* xanh lá thành công */
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
                                padding: 10px 15px;
                                margin: 15px 0;
                                border-radius: 6px;
                                font-weight: 600;
                                color: #155724;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #28a745;
                                border: none;
                                width: max-content;
                                color: #fff !important;
                                font-weight: 600;
                                cursor: pointer;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                                margin-top: 10px;
                                margin-bottom: 10px;
                                transition: all 0.3s ease-in-out;
                            }}
                            .btn:hover {{
                                transform: scale(1.05);
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
                              <h2>✅ Email Verified Successfully!</h2>
                            </div>
                            <div class='content'>
                              <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>
                              <p>Congratulations! Your email has been successfully verified.</p>

                              <p class='highlight'>
                                🎉 You can now access all features of Back2me and enjoy the full experience.
                              </p>

                              <p style='text-align: center;'>
                                <a href='https://back2me.vercel.app/me' class='btn'>Go to Profile</a>
                              </p>

                              <p>If you did not perform this action, please contact our support team immediately.</p>
                            </div>
                            <div class='footer'>
                              <p>Best regards,<br/><strong>Back2me Team</strong></p>
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

        #region Resend Verify Email
        [HttpPost("resend-verify-email")]
        public async Task<ActionResult> ResendVerifyEmail()
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Get User email from user signed in in JWT (AccessToken)

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    message = "User not authenticated"
                });
            }

            var user = await usersRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound(new
                {
                    Message = "User not found"
                });
            }

            if (user.IsVerifiedEmail)
            {
                return Ok(new
                {
                    Message = "Your email has already been verified"
                });
            }

            var token = Guid.NewGuid().ToString();
            var cacheKey = $"VERIFY_EMAIL_RESEND_{user.Email}";

            // Check if user send resend verification
            if (memoryCache.TryGetValue(cacheKey, out _))
            {
                return BadRequest(new
                {
                    Message = "You can resend the verification email in 2 minutes"
                });
            }

            // Set cache: key exists in 2 mins and block resend in 2 mins
            memoryCache.Set($"VERIFY_EMAIL_RESEND_{user.Email}", true, TimeSpan.FromMinutes(2));

            // Send verification code via email
            memoryCache.Set($"EMAIL_VERIFY_{token}", user.Email, TimeSpan.FromMinutes(15));

            // Send verification email
            string senderName = "Back2Me";
            string senderEmail = "baoandng07@gmail.com";
            string toName = user.FirstName + " " + user.LastName;
            string toEmail = user.Email;
            string subject = "✅ Please Verify Your Email";
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
                                background: #fffae6;
                                border-left: 4px solid #ff9900;
                                padding: 10px 15px;
                                margin: 15px 0;
                                border-radius: 6px;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #ec7207;
                                border: none;
                                width: max-content;
                                color: #fff !important;
                                font-weight: 600;
                                cursor: pointer;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                                margin-top: 10px;
                                margin-bottom: 10px;
                                transition: all 0.3s ease-in-out;
                            }}
                            .btn:hover {{
                                transform: scale(1.05);
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
                              <h2>✅ Verify Your Email</h2>
                            </div>
                            <div class='content'>
                              <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>
                              <p>Please verify your email address by clicking the button below:</p>

                              <p style='text-align: center;' class='highlight'>
                                ⚡ This verification link will expire in 15 minutes.
                              </p>

                              <p style='text-align: center;'>
                                 <a href='https://back2me.vercel.app/verify-email?token={token}' class='btn'>✅ Verify Email</a>
                              </p>

                              <p>If you did not perform this action, you can safely ignore this email.</p>
                            </div>
                            <div class='footer'>
                              <p>Best regards,<br/><strong>Back2me Team</strong></p>
                            </div>
                          </div>
                        </body>
                        </html>
                        ";

            await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

            return Ok("Verification email sent");
        }
        #endregion

        #region Sign out
        [HttpPost("sign-out")]
        public ActionResult SignOut()
        {
            Response.Cookies.Delete("AccessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/" // Ensure the cookie is deleted from the root path
            });

            Response.Cookies.Delete("Username", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            return Ok("Signed out successfully");
        }
        #endregion

        // PUT api/<UsersController>/5
        #region Update My Profile
        [HttpPut("update-user")]
        public async Task<ActionResult> Update([FromForm] UserDTO user)
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    message = "User not authenticated"
                });
            }

            var userExisted = await usersRepository.GetUserByEmail(userEmail);

            if (userExisted == null)
            {
                return NotFound(new
                {
                    message = "User does not found"
                });
            }

            // Update information
            userExisted.FirstName = user.FirstName;
            userExisted.LastName = user.LastName;
            userExisted.DateOfBirth = user.DateOfBirth;
            userExisted.Avatar = user.AvatarUpload != null ?
                                 await UploadFiles(user.AvatarUpload) : userExisted.Avatar;
            userExisted.UpdatedAt = DateTime.Now;

            var isUpdated = await usersRepository.UpdateUser();
            if (!isUpdated)
            {
                return BadRequest("Update failed");
            }

            return Ok("Updated successfully");
        }
        #endregion

        #region Check email reset password
        [HttpGet("check-email-reset-password/{email}")]
        public async Task<ActionResult> CheckEmailForResetPassword(string email)
        {
            // Check user if it exists
            var user = await usersRepository.GetUserByEmail(email);

            if (user != null && user.IsActive)
            {
                var token = Guid.NewGuid().ToString();
                var cacheKey = $"VERIFY_EMAIL_RESEND_{user.Email}";

                // Check if user send resend verification
                if (memoryCache.TryGetValue(cacheKey, out _))
                {
                    return BadRequest(new
                    {
                        Message = "You can resend the verification email in 2 minutes"
                    });
                }

                // Set cache: key exists in 2 mins and block resend in 2 mins
                memoryCache.Set($"VERIFY_EMAIL_RESEND_{user.Email}", true, TimeSpan.FromMinutes(2));

                // Send verification code via email
                memoryCache.Set($"EMAIL_VERIFY_{token}", user.Email, TimeSpan.FromMinutes(15));

                // Send email verify
                string senderName = "Back2Me";
                string senderEmail = "baoandng07@gmail.com";
                string toName = user.FirstName + " " + user.LastName;
                string toEmail = user.Email;
                string subject = "🔐 Reset Your Password";
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
                                background: #fffae6;
                                border-left: 4px solid #ff9900;
                                padding: 10px 15px;
                                margin: 15px 0;
                                border-radius: 6px;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #ec7207;
                                border: none;
                                width: max-content;
                                color: #fff !important;
                                font-weight: 600;
                                cursor: pointer;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                                margin-top: 10px;
                                margin-bottom: 10px;
                                transition: all 0.3s ease-in-out;
                            }}
                            .btn:hover {{
                                transform: scale(1.05);
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
                              <h2>🔐 Reset Your Password</h2>
                            </div>
                            <div class='content'>
                              <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>
                              <p>We received a request to reset the password for your account</p>

                              <p style='text-align: center;' class='highlight'>
                                ⚡ This password reset link will expire in <strong>15 minutes</strong>.
                              </p>

                              <p style='text-align: center;'>
                                 <a href='https://back2me.vercel.app/authentication/reset-password?token={token}' class='btn'> Reset Password </a>
                              </p>

                              <p>If you did not request a password reset, please ignore this email. Your account will remain secure.</p>
                            </div>
                            <div class='footer'>
                              <p>Best regards,<br/><strong>Back2me Team</strong></p>
                            </div>
                          </div>
                        </body>
                        </html>
                        ";

                await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
            }

            return Ok(new
            {
                Message = "If this email exists, we’ve sent a verification code"
            });
        }
        #endregion

        #region Confirm reset password
        [HttpPut("confirm-reset-password/{email}/{newPassword}")]
        public async Task<ActionResult> ConfirmResetPassword(string email, string newPassword)
        {
            // Check user if it exists
            var user = await usersRepository.GetUserByEmail(email);

            if (user != null && user.IsActive)
            {
                // Update information
                user.FirstName = user.FirstName;
                user.LastName = user.LastName;
                user.DateOfBirth = user.DateOfBirth;
                user.Avatar = user.Avatar;
                user.UpdatedAt = DateTime.Now;


                // Check new password if equals old password
                var isNotValidPassword = BCrypt.Net.BCrypt.EnhancedVerify(newPassword, user.Password);

                if (isNotValidPassword)
                {
                    return Conflict(new
                    {
                        Message = "Your new password must be different from your current password",
                        Email = user.Email
                    });
                }

                // Hash password
                var hashNewPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword);

                user.Password = hashNewPassword;

                var isUpdated = await usersRepository.UpdateUser();
                if (!isUpdated)
                {
                    return BadRequest("Change password failed");
                }

                // Send email confirm email is changed successfully
                string senderName = "Back2Me";
                string senderEmail = "baoandng07@gmail.com";
                string toName = user.FirstName + " " + user.LastName;
                string toEmail = user.Email;
                string subject = "✅ Your Password Has Been Changed Successfully";
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
                        background-color: #22c55e;
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
                        background: #fffae6;
                        border-left: 4px solid #ff9900;
                        padding: 10px 15px;
                        margin: 15px 0;
                        border-radius: 6px;
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
                      <h2>✅ Password Changed</h2>
                    </div>

                    <div class='content'>
                      <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>

                      <p>
                        This is a confirmation that your account password has been
                        <strong>successfully changed</strong>.
                      </p>

                      <div class='highlight'>
                        🔐 If you made this change, no further action is required.
                      </div>

                      <p>
                        If you did <strong>not</strong> change your password, please contact our
                        support team immediately to secure your account.
                      </p>
                    </div>

                    <div class='footer'>
                      <p>
                        Best regards,<br/>
                        <strong>Back2me Team</strong>
                      </p>
                    </div>
                  </div>
                </body>
                </html>
                ";

                await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
            }

            return Ok(new
            {
                Message = "Changed password successfully"
            });
        }
        #endregion

        #region Confirm reset images
        [HttpPut("confirm-reset-images/{email}/{image1}/{image2}")]
        public async Task<ActionResult> ConfirmResetImages(string email, string image1, string image2)
        {
            // Check user if it exists
            var user = await usersRepository.GetUserByEmail(email);

            if (user != null && user.IsActive)
            {
                // Update information
                user.FirstName = user.FirstName;
                user.LastName = user.LastName;
                user.DateOfBirth = user.DateOfBirth;
                user.Avatar = user.Avatar;
                user.UpdatedAt = DateTime.Now;
                user.Password = user.Password;
                user.PickImage1 = image1;
                user.PickImage2 = image2;

                var isUpdated = await usersRepository.UpdateUser();
                if (!isUpdated)
                {
                    return BadRequest("Change images failed");
                }

                // Send email confirm email is changed successfully
                string senderName = "Back2Me";
                string senderEmail = "baoandng07@gmail.com";
                string toName = user.FirstName + " " + user.LastName;
                string toEmail = user.Email;
                string subject = "✅ Your Second-Step Authentication Images Have Been Updated";
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
                        background-color: #22c55e;
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
                        background: #fffae6;
                        border-left: 4px solid #ff9900;
                        padding: 10px 15px;
                        margin: 15px 0;
                        border-radius: 6px;
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
                      <h2>✅ Authentication Images Updated</h2>
                    </div>

                    <div class='content'>
                      <p>Hi <strong>{user.FirstName} {user.LastName}</strong>,</p>

                      <p>
                        This is a confirmation that your <strong>two authentication images</strong> used for the second-step login have been <strong>successfully updated</strong>.
                      </p>

                      <div class='highlight'>
                        🔐 If you made this change, no further action is required.
                      </div>

                      <p>
                        If you did <strong>not</strong> change your password, please contact our
                        support team immediately to secure your account.
                      </p>
                    </div>

                    <div class='footer'>
                      <p>
                        Best regards,<br/>
                        <strong>Back2me Team</strong>
                      </p>
                    </div>
                  </div>
                </body>
                </html>
                ";

                await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
            }

            return Ok(new
            {
                Message = "Changed images successfully"
            });
        }
        #endregion

        #region Suspend User
        [Authorize(Roles = "Admin")]
        [HttpPut("suspend-user/{userId}")]
        public async Task<ActionResult> SuspendUser(int userId)
        {
            var userExisted = await usersRepository.GetUserByID(userId);

            if (userExisted == null)
            {
                return NotFound(new
                {
                    message = "User does not found"
                });
            }

            // Update information
            userExisted.FirstName = userExisted.FirstName;
            userExisted.LastName = userExisted.LastName;
            userExisted.DateOfBirth = userExisted.DateOfBirth;
            userExisted.Avatar = userExisted.Avatar;
            userExisted.UpdatedAt = DateTime.Now;
            userExisted.IsActive = !userExisted.IsActive; // Toggle suspend status

            var isSuspended = await usersRepository.SuspendUser();
            if (!isSuspended)
            {
                return BadRequest("Suspend failed");
            }

            // Send to user who has been suspended
            await hubContext.Clients.User(userExisted.Email)
                                    .SendAsync("ReceiveForceSignOut", new
                                    {
                                        Message = "Your account has been suspended by the administrator"
                                    });

            // Send to all admins
            await hubContext.Clients.Group("Admin")
                                    .SendAsync("ReceiveUserSuspended", new
                                    {
                                        UserId = userExisted.UserId,
                                    });

            return Ok("Suspended successfully");
        }
        #endregion

        #region Unsuspend User
        [Authorize(Roles = "Admin")]
        [HttpPut("unsuspend-user/{userId}")]
        public async Task<ActionResult> UnsuspendUser(int userId)
        {
            var userExisted = await usersRepository.GetUserByID(userId);

            if (userExisted == null)
            {
                return NotFound(new
                {
                    message = "User does not found"
                });
            }

            // Update information
            userExisted.FirstName = userExisted.FirstName;
            userExisted.LastName = userExisted.LastName;
            userExisted.DateOfBirth = userExisted.DateOfBirth;
            userExisted.Avatar = userExisted.Avatar;
            userExisted.UpdatedAt = DateTime.Now;
            userExisted.IsActive = !userExisted.IsActive; // Toggle suspend status

            var isSuspended = await usersRepository.UnsuspendUser();
            if (!isSuspended)
            {
                return BadRequest("Unsuspend failed");
            }

            // Send to all admins
            await hubContext.Clients.Group("Admin")
                                    .SendAsync("ReceiveUserUnsuspended", new
                                    {
                                        UserId = userExisted.UserId,
                                    });

            return Ok("Unsuspended successfully");
        }
        #endregion

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // Functions are not related to API endpoints
        #region Upload Files
        private async Task<string> UploadFiles(IFormFile file)
        {
            if (file == null) return "";

            string uploadsFolder = Path.Combine(webHost.WebRootPath, "Uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid() + Path.GetFileName(file.FileName);
            string fileSavePath = Path.Combine(uploadsFolder, fileName);

            using FileStream stream = new FileStream(fileSavePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
        #endregion
    }
}
