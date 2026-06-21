using FBLA_API.DTOs.Match;
using FBLA_API.DTOs.VerificationCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ObjectBusiness;
using Repository;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        #region Variables
        private readonly IMatchRepository matchRepository;
        private readonly IUsersRepository userRepository;
        private readonly IPostRepository postRepository;
        private readonly IVerificationCodeRepository verificationCodeRepository;
        #endregion

        #region Constructor
        public MatchController(IMatchRepository matchRepository,
                               IVerificationCodeRepository verificationCodeRepository,
                               IPostRepository postRepository,
                               IUsersRepository userRepository)
        {
            this.matchRepository = matchRepository;
            this.userRepository = userRepository;
            this.verificationCodeRepository = verificationCodeRepository;
            this.postRepository = postRepository;
        }
        #endregion

        // GET: api/<MatchController>
        [Authorize]
        [HttpGet("match-user")]
        public async Task<ActionResult<List<MatchDTO>>> Get()
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

            var matches = await matchRepository.GetMatchesByUserId(user.UserId).ToListAsync();

            return Ok(matches);
        }

        [Authorize]
        [HttpGet("check-matched-post/{postId}")]
        public async Task<ActionResult<Match>> CheckMatchedPost(int postId)
        {
            var match = await matchRepository.GetMatchByPostId(postId);

            if (match == null)
            {
                return NotFound(new
                {
                    message = "This post does not have any matches"
                });
            }

            return Ok(match);
        }

        // GET api/<MatchController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<MatchController>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Match match)
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // User email currently signed in

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    Message = "User unauthorized"
                });
            }

            var user = await userRepository.GetUserByEmail(userEmail);

            // Check user if existed
            if (user == null)
            {
                return NotFound(new
                {
                    Message = "User does not found"
                });
            }

            // Check user if verified email
            if (!user.IsVerifiedEmail)
            {
                return Forbid();
            }

            var postExists = matchRepository.AllMatches().Any(m => m.LostPostId == match.LostPostId);
            if (postExists)
            {
                return Conflict(new
                {
                    message = "Your lost post is already matched"
                });
            }

            var isAddedMatch = await matchRepository.CreateMatch(match);
            if (isAddedMatch)
            {
                var isAddedVerificationCode = await verificationCodeRepository.CreateVerificationCode(new VerificationCode
                {
                    MatchId = match.MatchId,
                    Code = match.Code
                });
                return Ok("Create match successfully");
            }

            return BadRequest("Create match failed");
        }

        // PUT api/<MatchController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MatchController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
