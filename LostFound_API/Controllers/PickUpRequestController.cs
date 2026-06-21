using DataAccess;
using FBLA_API.DTOs.Posts;
using FBLA_API.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using Repository;
using Services;
using SignalRLayer;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PickUpRequestController : ControllerBase
    {
        #region Variables
        private readonly IPickUpRequestRepository pickUpRequestRepository;
        private readonly IUsersRepository userRepository;
        private readonly IPostRepository postRepository;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public PickUpRequestController(IPickUpRequestRepository pickUpRequestRepository,
                                       IUsersRepository userRepository,
                                       IPostRepository postRepository,
                                       IHubContext<SystemHub> hubContext,
                                       EmailSender emailSender)
        {
            this.pickUpRequestRepository = pickUpRequestRepository;
            this.userRepository = userRepository;
            this.postRepository = postRepository;
            this.hubContext = hubContext;
            this.emailSender = emailSender;
        }
        #endregion

        // GET: api/<PickUpRequestController>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<PickUpRequest>>> Get()
        {
            var request = await pickUpRequestRepository.AllRequests().ToListAsync();
            return request;
        }

        #region Search Pick-Up Request for Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("search-request")]
        public async Task<ActionResult<List<PickUpRequest>>> SearchRequest([FromQuery] string query)
        {
            var posts = await pickUpRequestRepository.SearchRequest(query).ToListAsync();
            return Ok(posts);
        }
        #endregion

        [Authorize]
        [HttpGet("check-status-post-pick-up/{postId}")]
        public async Task<ActionResult<PickUpRequest>> CheckStatusPostPickUp(int postId)
        {
            var request = await pickUpRequestRepository.CheckContainsPost(postId);

            if (request != null)
            {
                return Ok(request);
            }

            return NotFound();
        }

        #region Accept Time
        [Authorize(Roles = "Admin")]
        [HttpPost("accept-time/{requestId}")]
        public async Task<ActionResult> AcceptTime(int requestId)
        {
            var requestAccepted = await pickUpRequestRepository.AcceptTime(requestId);
            if (requestAccepted == null)
            {
                return BadRequest("Accept failed or post not found");
            }

            return Ok(new
            {
                message = "Accepted successfully"
            });
        }
        #endregion

        #region Accept Time Rescheduled
        [Authorize]
        [HttpPost("accept-time-rescheduled/{requestId}")]
        public async Task<ActionResult> AcceptTimeRescheduled(int requestId)
        {
            var requestAccepted = await pickUpRequestRepository.AcceptTimeRescheduled(requestId);
            if (requestAccepted == null)
            {
                return BadRequest("Accept failed or post not found");
            }

            // Send to a specific student
            await hubContext.Clients.User(requestAccepted.Post.User?.Email).SendAsync("ReceiveStatusPickUpPost", new
            {
                Status = requestAccepted.Status,
                PostId = requestAccepted.PostId,
            });

            return Ok(new
            {
                message = "Accepted successfully"
            });
        }
        #endregion

        #region Change Time
        [Authorize(Roles = "Admin")]
        [HttpPost("change-time/{requestId}")]
        public async Task<ActionResult> ChangeTime(int requestId, [FromBody] DateTime newDate)
        {
            var requestAccepted = await pickUpRequestRepository.ChangeTime(requestId, newDate);
            if (requestAccepted == null)
            {
                return BadRequest("Change failed or post not found");
            }

            return Ok(new
            {
                message = "Changed successfully"
            });
        }
        #endregion

        // GET api/<PickUpRequestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PickUpRequestController>
        #region Create Request
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateRequet([FromBody] PickUpRequest request)
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail == null)
            {
                return Unauthorized(new { messag = "User not authenticated" });
            }

            // Get user
            var user = await userRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound(new { message = "User does not found" });
            }

            // Get post
            var post = await postRepository.GetPostById(request.PostId);

            if (post == null)
            {
                return NotFound(new { message = "This post does not found" });
            }

            // Check existed post
            var checkExistedPost = await pickUpRequestRepository.CheckContainsPost(post.PostId);

            if (checkExistedPost != null)
            {
                return Conflict(new
                {
                    Message = "You have already requested pick-up for this item"
                });
            }

            request.Description = $"{user.FirstName} {user.LastName} will pick up <a href='/detail-post/{post.PostId}' class='description-link-pick-up'>{post.Title}</a> at {request.PickUpDate}";
            var isAddedRequest = await pickUpRequestRepository.CreateRequest(request);

            if (isAddedRequest)
            {
                try
                {
                    // Send to a specific student
                    await hubContext.Clients.User(user.Email).SendAsync("ReceiveStatusPickUpPost", new
                    {
                        Status = request.Status,
                        PostId = request.PostId,
                    });

                    // Send to admin
                    await hubContext.Clients.Group("Admin").SendAsync("ReceiveNewPickUpRequest", new
                    {
                        Request = new
                        {
                            RequestId = request.RequestId,
                            Description = request.Description,
                            CreatedDate = request.CreatedDate,
                            Status = request.Status,
                            PickUpDate = request.PickUpDate,
                        },
                        Message = "A pick-up request is awaiting your confirmation",
                    });
                }
                catch (Exception ex)
                {

                }

                return Ok(new
                {
                    Message = "Created request successfully"
                });
            }

            return BadRequest("Create request failed or post not found");
        }
        #endregion

        // PUT api/<PickUpRequestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PickUpRequestController>/5
        [Authorize]
        [HttpDelete("pick-later/{postId}")]
        public async Task<ActionResult> PickLater(int postId)
        {
            var post = await postRepository.GetPostById(postId);

            if (post == null)
            {
                return NotFound(new
                {
                    Message = "Post does not found to delete pick-up request"
                });
            }

            var isDeleted = await pickUpRequestRepository.DeletePickUpRequest(post.PostId);

            if (isDeleted)
            {
                var admin = userRepository.GetAdmin();

                if (post != null && admin != null)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = admin.FirstName + " " + admin.LastName;
                    string toEmail = admin.Email;
                    string subject = "User Chose to Pick Up Later";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                          font-family: 'Segoe UI', Arial, sans-serif;
                          background-color: #f5f7fb;
                          padding: 20px;
                          color: #1f2a44;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #ffffff;
                          border-radius: 14px;
                          box-shadow: 0 6px 18px rgba(0,0,0,0.08);
                          overflow: hidden;
                        }}
                        .header {{
                          background: linear-gradient(135deg, #f59e0b, #d97706);
                          color: #fff;
                          padding: 24px;
                          text-align: center;
                        }}
                        .header h2 {{
                          margin: 0;
                          font-size: 22px;
                        }}
                        .content {{
                          padding: 24px;
                        }}
                        .content p {{
                          margin: 12px 0;
                          font-size: 15px;
                        }}
                        .status-box {{
                          background: #fffbeb;
                          border-left: 5px solid #f59e0b;
                          padding: 16px 18px;
                          border-radius: 10px;
                          margin: 20px 0;
                        }}
                        .info-card {{
                          background: #f9fafc;
                          border-radius: 10px;
                          padding: 16px;
                          margin: 18px 0;
                        }}
                        .info-row {{
                          display: flex;
                          justify-content: space-between;
                          padding: 6px 0;
                          font-size: 14px;
                        }}
                        .info-row span:first-child {{
                          color: #6b7280;
                        }}
                        .btn {{
                          display: inline-block;
                          background-color: #f59e0b;
                          color: #fff !important;
                          font-weight: 600;
                          font-size: 15px;
                          padding: 12px 26px;
                          border-radius: 22px;
                          margin-top: 16px;
                        }}
                        .footer {{
                          background: #f1f3f7;
                          padding: 16px;
                          text-align: center;
                          font-size: 13px;
                          color: #6b7280;
                        }}
                      </style>
                    </head>

                    <body>
                      <div class='container'>
                        <div class='header'>
                          <h2>⏸ {post.User?.FirstName} {post.User?.LastName} Will Pick Up Later</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{admin.FirstName} {admin.LastName}</strong>,</p>

                          <p>
                            The user has chosen to <strong>pick up the item at a later time</strong>. No pickup schedule has been confirmed yet.
                          </p>

                          <div class='status-box'>
                            <strong>📍 Pickup Schedule Confirmed</strong>
                            <p>The item is still reserved for the user. You will be notified once the user submits a new pickup request.</p>
                          </div>

                          <div class='info-card'>
                            <div class='info-row'>
                              <span>📦 Item <strong>{post.Title}</strong></span>
                            </div>
                          </div>

                          <p style='text-align:center;'>
                            <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>
                              📄 View Pickup Details
                            </a>
                          </p>

                          <p>
                            The user will arrive according to the confirmed schedule.
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong><br/>
                            This is an automated notification — please do not reply.
                          </p>
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
                    Message = "Pickup request postponed. You can schedule a pickup time later"
                });
            }

            return BadRequest("Deletn e pick-up request failed or request not found");
        }
    }
}
