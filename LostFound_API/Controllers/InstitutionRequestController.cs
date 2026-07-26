using DataAccess;
using LostFound_API.DTOs.Institution;
using LostFound_API.DTOs.Users;
using LostFound_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using ObjectBusiness;
using Repository;
using Services;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LostFound_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitutionRequestController : ControllerBase
    {
        #region Variables
        private readonly IInstitutionRequestRepository institutionRequestRepository;
        private readonly IInstitutionRepository institutionRepository;
        private readonly IUsersRepository usersRepository;
        private readonly StateService stateService;
        private readonly IMemoryCache memoryCache;
        private readonly IConfiguration configuration;
        private readonly EmailSender emailSender;
        private readonly IHttpClientFactory httpClientFactory;
        #endregion

        #region Constructor
        public InstitutionRequestController(IInstitutionRequestRepository institutionRequestRepository,
                               StateService stateService,
                               IConfiguration configuration,
                               IHttpClientFactory httpClientFactory,
                               IUsersRepository usersRepository,
                               EmailSender emailSender,
                               IMemoryCache memoryCache,
                               IInstitutionRepository institutionRepository)
        {
            this.institutionRequestRepository = institutionRequestRepository;
            this.stateService = stateService;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.usersRepository = usersRepository;
            this.emailSender = emailSender;
            this.memoryCache = memoryCache;
            this.institutionRepository = institutionRepository;
        }
        #endregion

        #region Get All Institution Requests
        // GET: api/<InstitutionRequestController>
        [Authorize]
        [HttpGet]
        public ActionResult<InstitutionRequest> Get()
        {
            var requests = institutionRequestRepository.AllInstitutionRequests();
            return Ok(requests);
        }
        #endregion

        #region Get Institution Request By ID
        // GET api/<InstitutionRequestController>/5
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var request = await institutionRequestRepository.GetInstitutionRequestByID(id);
            if (request == null)
            {
                return NotFound(new
                {
                    message = "Request not found"
                });
            }

            return Ok(request);
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

                var institutionRequest = await institutionRequestRepository.GetInstitutionRequestByWorkEmail(userEmail!);

                // Seperate send email is verified and don't send for forgot password
                if (!isForgotPassword)
                {
                    institutionRequest.IsVerifiedEmail = true;
                    institutionRequest.EmailVerifiedAt = DateTime.Now;
                    var isUpdated = await institutionRequestRepository.UpdateInstitutionRequest();

                    if (isUpdated)
                    {
                        // Send email email has been verified
                        string senderName = "Back2Me";
                        string senderEmail = "mycampuslostfound@gmail.com";
                        string toName = institutionRequest.ApplicantName;
                        string toEmail = institutionRequest.WorkEmail;
                        string subject = "Your Email Has Been Verified!";
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
                            <h2>✅ Work Email Verified</h2>
                          </div>

                          <div class='content'>

                            <p>Hello <strong>{institutionRequest.ApplicantName}</strong>,</p>

                            <p>
                              Thank you! Your work email has been successfully verified.
                            </p>

                            <div class='highlight'>
                              📧 <strong>Email Verified:</strong> {institutionRequest.WorkEmail}
                            </div>

                            <p>
                              Your institution registration request for
                              <strong>{institutionRequest.InstitutionName}</strong>
                              has now been submitted for review.
                            </p>

                            <p>
                              Our team will verify the information you provided before approving your institution.
                              This process may take some time depending on the volume of requests.
                            </p>

                            <div class='highlight'>
                              ⏳ <strong>Current Status:</strong> Pending Institution Review
                            </div>

                            <p>
                              Once your institution has been approved, we'll send you another email with instructions
                              to create your administrator password and activate your Back2Me administrator account.
                            </p>

                            <p>
                              No further action is required at this time.
                            </p>

                          </div>

                          <div class='footer'>
                            Best regards,<br/>
                            <strong>Back2Me Team</strong>
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
                    Email = institutionRequest.WorkEmail
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        #region Verify Website
        [HttpGet("verify-website/{id}")]
        public async Task<ActionResult> VerifyWebsite(int id)
        {
            try
            {
                var institutionRequest = await institutionRequestRepository.GetInstitutionRequestByID(id);
                if (institutionRequest != null)
                {
                    institutionRequest.IsVerifiedWebsite = true;
                    institutionRequest.WebsiteVerifiedAt = DateTime.Now;
                    var isUpdated = await institutionRequestRepository.UpdateInstitutionRequest();
                    if (isUpdated)
                    {
                        return Ok(new
                        {
                            Message = "Verified website successfully",
                            Website = institutionRequest.InstitutionWebsite
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            Message = "Failed to update website verification status"
                        });
                    }
                }
                else
                {
                    return NotFound(new
                    {
                        Message = "Institution request not found"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong!");
            }
        }
        #endregion

        #region Search Institution
        // GET api/<InstitutionController>/5
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Query is required"
                });
            }

            var apiKey = configuration["CollegeScorecard:ApiKey"];
            var client = httpClientFactory.CreateClient();

            var url =
                   $"https://api.data.gov/ed/collegescorecard/v1/schools" +
                   $"?api_key={apiKey}" +
                   $"&school.name={Uri.EscapeDataString(query)}" +
                   $"&fields=id,school.name,school.address,school.city,school.state,school.zip,school.school_url,location.lat,location.lon" +
                   $"&per_page=10";

            // Send GET request to College Scorecard API
            var res = await client.GetAsync(url);

            // If API returns errors
            if (!res.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)res.StatusCode,
                    "Unable to retrieve institutions.");
            }

            var json = await res.Content.ReadAsStringAsync();

            // Parse JSON to JsonDocument in order to read each property
            using var document = JsonDocument.Parse(json);

            // Get property "results"
            var results = document.RootElement.GetProperty("results");

            // Create DTO list to return to frontend.
            var institutions = new List<InstitutionSearchDTO>();

            foreach (var item in results.EnumerateArray())
            {
                institutions.Add(new InstitutionSearchDTO
                {
                    // GET "id" field
                    //
                    // JSON:
                    //
                    // "id":166027
                    //
                    // =>
                    // int
                    Id = item.TryGetProperty("id", out var id)
                       && id.ValueKind == JsonValueKind.Number
                       ? id.GetInt32() : 0,

                    // GET school name.
                    //
                    // JSON:
                    //
                    // "school.name":"Georgia State University"
                    //
                    // if null then return ""
                    Name = item.GetProperty("school.name").GetString() ?? "",

                    // TryGetProperty to skip throw exception
                    // if property does not exist.
                    //
                    // If exist:
                    // get Address.
                    //
                    // If no:
                    // return null.
                    Address = item.TryGetProperty("school.address", out var address)
                            ? address.GetString() : null,

                    // City
                    City = item.TryGetProperty("school.city", out var city)
                         ? city.GetString() : null,

                    // State
                    State = item.TryGetProperty("school.state", out var state)
                          ? state.GetString() : null,

                    // Zip Code
                    ZipCode = item.TryGetProperty("school.zip", out var zip)
                            ? zip.GetString() : null,

                    // Website
                    Website = item.TryGetProperty("school.school_url", out var website)
                            ? website.GetString() : null,

                    // Latitude
                    Latitude = item.TryGetProperty("location.lat", out var lat)
                             && lat.ValueKind == JsonValueKind.Number
                             ? lat.GetDouble() : null,

                    // Longitude
                    Longitude = item.TryGetProperty("location.lon", out var lon)
                              && lon.ValueKind == JsonValueKind.Number
                              ? lon.GetDouble() : null,

                });
            }

            // return List<InstitutionSearchDTO> to frontend.
            //
            // Frontend will receive:
            //
            // [
            //   {
            //      id:166027,
            //      name:"Georgia State University",
            //      address:"33 Gilmer St SE",
            //      city:"Atlanta",
            //      state:"GA",
            //      zipCode:"30303",
            //      website:"www.gsu.edu",
            //      latitude:33.75,
            //      longitude:-84.38
            //   }
            // ]

            return Ok(institutions);
        }
        #endregion

        #region Create Institution Request
        // POST api/<InstitutionRequestController>
        [HttpPost("sign-up")]
        public async Task<ActionResult> CreateRequest([FromBody] InstitutionRequest request)
        {
            var requestExists = await institutionRequestRepository
                                .GetInstitutionRequestByNameAndAddress(request.InstitutionName, request.InstitutionAddress);
            if (requestExists != null)
            {
                return Conflict(new
                {
                    message = "This campus already registered"
                });
            }

            var isAdded = await institutionRequestRepository.CreateInstitutionRequest(request);
            if (isAdded)
            {
                // Send email notification to the user who made the request
                var token = Guid.NewGuid().ToString();
                memoryCache.Set($"EMAIL_VERIFY_{token}", request.WorkEmail, TimeSpan.FromMinutes(15));

                string senderName = "Back2Me";
                string senderEmail = "mycampuslostfound@gmail.com";
                string toName = request.ApplicantName;
                string toEmail = request.WorkEmail;
                string subject = "Institution Registration Received";

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

                    .content {{
                        padding: 20px;
                    }}

                    .highlight {{
                        background: #eafaf1;
                        border-left: 4px solid #28a745;
                        padding: 12px 16px;
                        margin: 18px 0;
                        border-radius: 6px;
                    }}

                    .btn {{
                        display: inline-block;
                        background-color: #28a745;
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
                        <h2>🏫 Registration Request Received</h2>
                    </div>

                    <div class='content'>

                        <p>Hello,</p>

                        <p>
                            Thank you for registering
                            <strong>{request.InstitutionName}'s {request.InstitutionAddress} campus</strong>
                            with <strong>Back2Me</strong>.
                        </p>

                        <p>
                            We have successfully received your institution registration request.
                            Before we review your institution registration, please verify your work email address by clicking the button below.
                        </p>

                        <div style='text-align:center; margin:30px 0;'>
                            <a href='http://localhost:5173/verify-email?token={token}' class='btn'>
                                Verify Work Email
                            </a>
                        </div>

                        <p>
                        This verification link will expire in <strong>15 minutes</strong>.
                        </p>

                        <div class='highlight'>
                            ⏳ <strong>Status:</strong> Pending Approval
                        </div>

                        <p>
                            Once your institution has been approved, we'll send another email with instructions
                            to create your administrator password and access your Back2Me account.
                        </p>

                        <p>
                            No further action is required at this time.
                        </p>

                        <p>
                            If you did not submit this registration request, please ignore this email.
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

                return Ok(new
                {
                    message = "Create request successfully"
                });
            }

            return BadRequest(new
            {
                message = "Create request failed"
            });
        }
        #endregion

        #region Approve Request
        // PUT api/<InstitutionRequestController>/5
        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("approve/{id}")]
        public async Task<ActionResult> ApproveRequest(int id)
        {
            // Get the authenticated user's email from the claims
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
            var institutionRequest = await institutionRequestRepository.GetInstitutionRequestByID(id);
            institutionRequest.Status = StatusRequestInstitution.Approved;
            institutionRequest.UpdatedDate = DateTime.Now;

            var isUpdated = await institutionRequestRepository.UpdateInstitutionRequest();
            if (!isUpdated)
            {
                return BadRequest(new
                {
                    message = "Update request failed"
                });
            }

            var institution = new Institution
            {
                InstitutionName = institutionRequest.InstitutionName,
                InstitutionAddress = institutionRequest.InstitutionAddress,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var isAdded = await institutionRepository.CreateInstitution(institution);
            if (!isAdded)
            {
                return BadRequest(new
                {
                    message = "Create institution failed"
                });
            }

            // Send email notification to the user who made the request
            var token = Guid.NewGuid().ToString();
            memoryCache.Set($"EMAIL_VERIFY_{token}", institutionRequest.WorkEmail, TimeSpan.FromMinutes(15));

            string senderName = "Back2Me";
            string senderEmail = "mycampuslostfound@gmail.com";
            string toName = institutionRequest.ApplicantName;
            string toEmail = institutionRequest.WorkEmail;
            string subject = "Your Institution Has Been Approved";

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

                .content {{
                    padding: 20px;
                }}

                .highlight {{
                    background: #eafaf1;
                    border-left: 4px solid #28a745;
                    padding: 12px 16px;
                    margin: 18px 0;
                    border-radius: 6px;
                }}

                .btn {{
                    display: inline-block;
                    background-color: #28a745;
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
                    <h2>🎉 Institution Approved</h2>
                </div>

                <div class='content'>

                    <p>Hello,</p>

                    <p>
                        Congratulations! Your institution
                        <strong>{institutionRequest.InstitutionName}</strong>
                        has been successfully approved by the <strong>Back2Me Team</strong>.
                    </p>

                    <div class='highlight'>
                        ✅ Your institution is now active and ready to use on the Back2Me platform.
                    </div>

                    <p>
                        Before you can sign in, you'll need to create a password for your administrator account.
                    </p>

                    <p>
                        Click the button below to set your password and complete your account setup.
                    </p>

                    <p style='text-align:center;'>
                        <a href='https://lfcampus.vercel.app/institution/set-password?token={token}' class='btn'>
                            Set Your Password
                        </a>
                    </p>

                    <p>
                        For your security, this link will expire in <strong>15 minutes</strong>.
                    </p>

                    <p>
                        Once your password has been created, you can sign in and begin managing your campus Lost & Found system.
                    </p>

                    <p>
                        If you were not expecting this email, you can safely ignore it.
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

            return Ok(new
            {
                message = "Update request successfully"
            });
        }
        #endregion

        #region Reject Request
        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("reject/{id}")]
        public async Task<ActionResult> RejectRequest(int id, [FromBody] string rejectReason)
        {
            // Get the authenticated user's email from the claims
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
            var institutionRequest = await institutionRequestRepository.GetInstitutionRequestByID(id);
            institutionRequest.Status = StatusRequestInstitution.Rejected;
            institutionRequest.RejectReason = rejectReason;
            institutionRequest.ReviewedBy = userExisted.FirstName + " " + userExisted.LastName;
            institutionRequest.ReviewedDate = DateTime.Now;
            institutionRequest.UpdatedDate = DateTime.Now;

            var isUpdated = await institutionRequestRepository.UpdateInstitutionRequest();
            if (!isUpdated)
            {
                return BadRequest(new
                {
                    message = "Update request failed"
                });
            }

            var institution = new Institution
            {
                InstitutionName = institutionRequest.InstitutionName,
                InstitutionAddress = institutionRequest.InstitutionAddress,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var isAdded = await institutionRepository.CreateInstitution(institution);
            if (!isAdded)
            {
                return BadRequest(new
                {
                    message = "Create institution failed"
                });
            }

            // Send email notification to the user who made the request
            var token = Guid.NewGuid().ToString();
            memoryCache.Set($"EMAIL_VERIFY_{token}", institutionRequest.WorkEmail, TimeSpan.FromMinutes(15));

            string senderName = "Back2Me";
            string senderEmail = "mycampuslostfound@gmail.com";
            string toName = institutionRequest.ApplicantName;
            string toEmail = institutionRequest.WorkEmail;
            string subject = "Institution Registration Request Update";

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

                .content {{
                    padding: 20px;
                }}

                .highlight {{
                    background: #eafaf1;
                    border-left: 4px solid #28a745;
                    padding: 12px 16px;
                    margin: 18px 0;
                    border-radius: 6px;
                }}

                .btn {{
                    display: inline-block;
                    background-color: #28a745;
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
                    <h2>❌ Registration Request Rejected</h2>
                </div>

                <div class='content'>

                    <p>Hello,</p>

                    <p>
                        Thank you for your interest in registering
                        <strong>{institution.InstitutionName}</strong>
                        with <strong>Back2Me</strong>.
                    </p>

                    <p>
                        After reviewing your submission, we regret to inform you that your institution registration request has not been approved at this time.
                    </p>

                    <div class='highlight'>
                        <strong>Reason:</strong><br/>
                        {institutionRequest.RejectReason}
                    </div>

                    <p>
                        If you believe this decision was made in error or you would like to provide additional information, please contact the Back2Me Team or submit a new registration request.
                    </p>

                    <p>
                        Thank you for your understanding.
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

            return Ok(new
            {
                message = "Update request successfully"
            });
        }
        #endregion

        // DELETE api/<InstitutionRequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
