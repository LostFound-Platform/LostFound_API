using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObjectBusiness;
using Repository;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        #region Variables
        private readonly IPostRepository postRepository;
        private readonly INotificationRepository notificationRepository;
        private readonly IUsersRepository userRepository;
        private readonly IStudentRepository studentRepository;
        private readonly ITransferRequestRepository transferRequestRepository;
        private readonly IWebHostEnvironment webHost;
        #endregion

        #region Constructor
        public NotificationController(IPostRepository postRepository,
                               IUsersRepository userRepository,
                               INotificationRepository notificationRepository,
                               IWebHostEnvironment webHost,
                               ITransferRequestRepository transferRequestRepository,
                               IStudentRepository studentRepository)
        {
            this.postRepository = postRepository;
            this.userRepository = userRepository;
            this.webHost = webHost;
            this.transferRequestRepository = transferRequestRepository;
            this.studentRepository = studentRepository;
            this.notificationRepository = notificationRepository;
        }
        #endregion

        // GET: api/<NotificationController>
        #region My Notifications match image
        [Authorize]
        [HttpGet("my-notifications-match-image")]
        public async Task<ActionResult<List<Notifications>>> GetAllNotificationsMatchImageByUserId()
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail == null)
            {
                return Unauthorized("User not authenticated");
            }

            var user = await userRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var notifications = await notificationRepository.AllNotificationsMatchImageByUserId(user.UserId);

            foreach (var post in notifications)
            {
                if (!string.IsNullOrEmpty(post.ImagePostMatched))
                {
                    post.UrlImagePostMatched = $"{Request.Scheme}://{Request.Host}/Uploads/{post.ImagePostMatched}";
                }

                if (!string.IsNullOrEmpty(post.AvatarUserMatched))
                {
                    post.UrlAvatarUserMatched = $"{Request.Scheme}://{Request.Host}/Uploads/{post.AvatarUserMatched}";
                }
            }

            return Ok(notifications);
        }
        #endregion

        #region My Notifications match image
        [Authorize]
        [HttpGet("my-notifications-match-description")]
        public async Task<ActionResult<List<Notifications>>> GetAllNotificationsMatchDescriptionByUserId()
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail == null)
            {
                return Unauthorized("User not authenticated");
            }

            var user = await userRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var notifications = await notificationRepository.AllNotificationsMatchDescriptionByUserId(user.UserId);

            foreach (var notification in notifications)
            {
                if (!string.IsNullOrEmpty(notification.ImagePostMatched))
                {
                    notification.UrlImagePostMatched = $"{Request.Scheme}://{Request.Host}/Uploads/{notification.ImagePostMatched}";
                }

                if (!string.IsNullOrEmpty(notification.AvatarUserMatched))
                {
                    notification.UrlAvatarUserMatched = $"{Request.Scheme}://{Request.Host}/Uploads/{notification.AvatarUserMatched}";
                }
            }

            return Ok(notifications);
        }
        #endregion

        // GET api/<NotificationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<NotificationController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<NotificationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<NotificationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
