using FBLA_API.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using Repository;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferRequestsController : ControllerBase
    {
        #region Variables
        private readonly IPostRepository postRepository;
        private readonly IUsersRepository userRepository;
        private readonly ITransferRequestRepository transferRequestRepository;
        private readonly IWebHostEnvironment webHost;
        #endregion

        #region Constructor
        public TransferRequestsController(IPostRepository postRepository,
                               IUsersRepository userRepository,
                               IWebHostEnvironment webHost,
                               ITransferRequestRepository transferRequestRepository)
        {
            this.postRepository = postRepository;
            this.userRepository = userRepository;
            this.webHost = webHost;
            this.transferRequestRepository = transferRequestRepository;
        }
        #endregion

        // GET: api/<TransferRequestsController>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<List<TransferRequests>> Get()
        {
            var requests = transferRequestRepository.AllRequests();
            return Ok(requests);
        }

        [HttpGet("status-request-post/{postId}")]
        public async Task<ActionResult<TransferRequests>> CheckStatusRequestPost(int postId)
        {
            var request = await transferRequestRepository.CheckStatusRequestPost(postId);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "This post does not have request"
                });
            }

            return Ok(request);
        }

        // GET api/<TransferRequestsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        #region Mark Received
        [Authorize(Roles = "Admin")]
        [HttpPost("mark-received")]
        public async Task<ActionResult> MarkReceived([FromBody] UserHandOverAdminDTO info)
        {
            var requestMarked = await transferRequestRepository.MarkReceived(info.RequestId);
            if (requestMarked == null)
            {
                return BadRequest("Mark failed or post not found");
            }

            var isHandedOver = await postRepository.HandOverAdmin(info.PostId, info.OldUserId);
            if (!isHandedOver)
            {
                return BadRequest("Hand over failed or post not found");
            }

            return Ok(new
            {
                message = "Marked successfully"
            });
        }
        #endregion

        #region Mark Received
        [Authorize(Roles = "Admin")]
        [HttpPost("cancel-handover")]
        public async Task<ActionResult> CancelHandover([FromBody] UserHandOverAdminDTO info)
        {
            var requestMarked = await transferRequestRepository.CancelRequest(info.RequestId);
            if (requestMarked == null)
            {
                return BadRequest("Cancel failed or post not found");
            }

            return Ok(new
            {
                message = "Cancelled successfully"
            });
        }
        #endregion

        #region Create Request
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateRequet([FromBody] UserHandOverAdminDTO info)
        {
            var request = new TransferRequests
            {
                PostId = info.PostId,
                UserId = info.OldUserId,
                ToUserId = userRepository.GetAdmin().UserId,
            };

            var isAddRequest = await transferRequestRepository.CreateRequest(request);

            if (isAddRequest)
            {
                return Ok(new
                {
                    Message = "Created request successfully"
                });
            }

            return BadRequest("Create request failed or post not found");
        }
        #endregion

        #region Search Transfer Request for Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("search-request")]
        public async Task<ActionResult<List<TransferRequests>>> SearchFirstNameRequest([FromQuery] string query)
        {
            var posts = await transferRequestRepository.SearchRequest(query).ToListAsync();
            return Ok(posts);
        }
        #endregion

        // PUT api/<TransferRequestsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TransferRequestsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
