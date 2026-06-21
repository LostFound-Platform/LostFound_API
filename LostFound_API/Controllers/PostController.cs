using DataAccess;
using FBLA_API.DTOs.Match;
using Microsoft.AspNetCore.Mvc;
using ObjectBusiness;
using Repository;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FBLA_API.DTOs.Users;
using Microsoft.Extensions.Hosting;
using FBLA_API.DTOs.Posts;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        #region Variables
        private readonly IPostRepository postRepository;
        private readonly ICategoryPostRepository categoryPostRepository;
        private readonly IUsersRepository userRepository;
        private readonly IStudentRepository studentRepository;
        private readonly ITransferRequestRepository transferRequestRepository;
        private readonly IWebHostEnvironment webHost;
        #endregion

        #region Constructor
        public PostController(IPostRepository postRepository,
                               IUsersRepository userRepository,
                               ICategoryPostRepository categoryPostRepository,
                               IWebHostEnvironment webHost,
                               ITransferRequestRepository transferRequestRepository,
                               IStudentRepository studentRepository)
        {
            this.postRepository = postRepository;
            this.userRepository = userRepository;
            this.webHost = webHost;
            this.transferRequestRepository = transferRequestRepository;
            this.studentRepository = studentRepository;
            this.categoryPostRepository = categoryPostRepository;
        }
        #endregion

        #region Newest Posts
        // GET: api/<PostController>
        [HttpGet("newest-posts")]
        public async Task<ActionResult<List<object>>> GetNewestPosts()
        {
            var newestPosts = await postRepository.GetNewestPosts();

            var results = newestPosts.Select(p => new
            {
                PostId = p.PostId,
                CategoryPostId = p.CategoryPostId,
                CreatedAt = p.CreatedAt,
                Description = p.Description,
                Image = p.Image,
                Title = p.Title,
                TypePost = p.TypePost,
                Vector = p.Vector,
                Code = p.Code,
                IsReceived = p.IsReceived,
                UrlImage = p.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{p.Image}",
                User = new Users
                {
                    UserId = p.User.UserId,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Email = p.User.Email,
                    Avatar = p.User.Avatar,
                    UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{p.User.Avatar}"
                }
            });

            return Ok(results);
        }
        #endregion

        #region Get Pick 60 Lost Posts
        // GET: api/<PostController>
        [HttpGet("quick-60-lost-posts")]
        public async Task<ActionResult<List<object>>> GetPick60LostPosts()
        {
            var posts = await postRepository.GetPick60LostPosts();
            return Ok(posts);
        }
        #endregion

        #region Get Pick 60 Lost Posts
        // GET: api/<PostController>
        [HttpGet("quick-60-received-posts")]
        public async Task<ActionResult<List<object>>> GetPick60ReceivedPosts()
        {
            var posts = await postRepository.GetPick60ReceivedPosts();
            return Ok(posts);
        }
        #endregion

        #region Sort by status
        // GET: api/<PostController>
        [Authorize]
        [HttpGet("sort-status")]
        public async Task<ActionResult<List<Posts>>> SortByStatus([FromQuery] TypePost typePost)
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

            var results = await postRepository.SortByStatus(typePost, user.UserId).ToListAsync();

            foreach (var post in results)
            {
                if (!string.IsNullOrEmpty(post.Image))
                {
                    post.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{post.Image}";
                }
            }

            return Ok(results);
        }
        #endregion

        #region All Lost Post Codes
        // GET: api/<PostController>
        [Authorize(Roles = "Admin")]
        [HttpGet("lost-post-codes")]
        public async Task<ActionResult<List<object>>> AllLostPostCodes()
        {
            var codes = await postRepository.AllLostPostCodes().ToListAsync();

            try
            {
                var results = codes.Select(p => new
                {
                    PostId = p.PostId,
                    CategoryPostId = p.CategoryPostId,
                    CreatedAt = p.CreatedAt,
                    Description = p.Description,
                    Image = p.Image,
                    Title = p.Title,
                    TypePost = p.TypePost,
                    Vector = p.Vector,
                    Code = p.Code,
                    IsReceived = p.IsReceived,
                    UrlImage = p.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{p.Image}",
                    User = new Users
                    {
                        UserId = p.User.UserId,
                        FirstName = p.User.FirstName,
                        LastName = p.User.LastName,
                        Email = p.User.Email,
                        UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{p.User.Avatar}"
                    },
                    StudentId = studentRepository.GetStudentByUserId(p.UserId).StudentId,
                    CategoryName = categoryPostRepository.GetCategoryPostById(p.CategoryPostId).CategoryPostName
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Something went wrong!" });
            }
        }
        #endregion

        #region My Posts
        [Authorize]
        [HttpGet("my-posts")]
        public async Task<ActionResult<List<Posts>>> GetAllPostsByUserId()
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

            var posts = await postRepository.AllPostsByUserId(user.UserId).ToListAsync();

            foreach (var post in posts)
            {
                if (!string.IsNullOrEmpty(post.Image))
                {
                    post.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{post.Image}";
                }
            }

            return Ok(posts);
        }
        #endregion

        #region My Posts
        [Authorize(Roles = "Admin")]
        [HttpGet("user-posts/{userId}")]
        public async Task<ActionResult<List<Posts>>> GetAllPostsByUserId(int userId)
        {
            var user = await userRepository.GetUserByID(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var posts = await postRepository.AllPostsByUserId(userId).ToListAsync();

            foreach (var post in posts)
            {
                if (!string.IsNullOrEmpty(post.Image))
                {
                    post.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{post.Image}";
                }
            }

            return Ok(posts);
        }
        #endregion

        #region Get Lost Posts Per Month
        [Authorize(Roles = "Admin")]
        [HttpGet("lost-posts-per-month")]
        public async Task<List<object>> GetLostPostsPerMonth()
        {
            var listPosts = await postRepository.GetLostPostsPerMonth().ToListAsync();
            return listPosts;
        }
        #endregion

        #region Get Found Posts Per Month
        [Authorize(Roles = "Admin")]
        [HttpGet("found-posts-per-month")]
        public async Task<List<object>> GetFoundPostsPerMonth()
        {
            var listPosts = await postRepository.GetFoundPostsPerMonth().ToListAsync();
            return listPosts;
        }
        #endregion

        #region Get Received Posts Per Month
        [Authorize(Roles = "Admin")]
        [HttpGet("received-posts-per-month")]
        public async Task<List<object>> GetReceivedPostsPerMonth()
        {
            var listPosts = await postRepository.GetReceivedPostsPerMonth().ToListAsync();
            return listPosts;
        }
        #endregion

        #region Get Found Posts Not Received
        [Authorize(Roles = "Admin")]
        [HttpGet("found-posts-not-received")]
        public async Task<List<object>> GetFoundPostsNotReceived()
        {
            var listPosts = await postRepository.GetFoundPostsNotReceived().ToListAsync();
            return listPosts;
        }
        #endregion

        #region Suggest Post
        [Authorize]
        [HttpGet("suggest-post/{postId}")]
        public async Task<ActionResult<List<MatchSuggestionDTO>>> SuggestPost(int postId)
        {
            var post = await postRepository.GetPostById(postId);
            if (post == null)
            {
                return NotFound("This post does not found");
            }

            if (post.TypePost != TypePost.Lost)
            {
                return Ok("Only lost posts can suggest found posts");
            }

            var suggestion = new List<MatchSuggestionDTO>();
            var allFoundPost = await postRepository.GetFoundPosts().Where(p => p.CategoryPostId == post.CategoryPostId).ToListAsync();

            // Check similar post (Suggest post)
            foreach (var found in allFoundPost)
            {
                // Calculate score each found post
                var score = CalculateSimilarity(post.Description, found.Description);

                if (score >= 65)
                {
                    suggestion.Add(new MatchSuggestionDTO
                    {
                        PostId = found.PostId,
                        Score = score,
                        CategoryPostId = found.CategoryPostId,
                        CreatedAt = found.CreatedAt,
                        Description = found.Description,
                        Image = found.Image,
                        Title = found.Title,
                        TypePost = found.TypePost,
                        Vector = found.Vector,
                        Code = found.Code,
                        UrlImage = found.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{found.Image}",
                        User = new Users
                        {
                            UserId = found.User.UserId,
                            FirstName = found.User.FirstName,
                            LastName = found.User.LastName,
                            Email = found.User.Email,
                            UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{found.User.Avatar}"
                        }
                    });
                }
            }

            return Ok(suggestion.OrderByDescending(x => x.Score).Take(5).ToList());
        }
        #endregion

        #region All Posts
        [HttpGet]
        public async Task<ActionResult<List<Posts>>> AllPosts()
        {
            var newestPosts = await postRepository.AllPosts().ToListAsync();

            var results = newestPosts.Select(p => new
            {
                PostId = p.PostId,
                CategoryPostId = p.CategoryPostId,
                CreatedAt = p.CreatedAt,
                Description = p.Description,
                Image = p.Image,
                Title = p.Title,
                TypePost = p.TypePost,
                Vector = p.Vector,
                Code = p.Code,
                IsReceived = p.IsReceived,
                UrlImage = p.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{p.Image}",
                User = new Users
                {
                    UserId = p.User.UserId,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Email = p.User.Email,
                    Avatar = p.User.Avatar,
                    UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{p.User.Avatar}"
                }
            });

            return Ok(results);
        }
        #endregion

        #region Get Post By Id
        // GET api/<PostController>/5
        [HttpGet("{postId}")]
        public async Task<ActionResult<Posts>> Get(int postId)
        {
            var post = await postRepository.GetPostById(postId);

            if (post == null)
            {
                return NotFound("This post does not found");
            }

            post.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{post.Image}";
            post.User = new Users
            {
                UserId = post.UserId,
                FirstName = post.User.FirstName,
                LastName = post.User.LastName,
                Email = post.User.Email,
                Avatar = post.User.Avatar,
                UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{post.User.Avatar}"
            };
            return Ok(post);
        }
        #endregion

        #region Create Post
        // POST api/<PostController>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Post([FromForm] Posts post)
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

            if (post.ImageUpload != null)
            {
                post.Image = await UploadFiles(post.ImageUpload);
            }

            var isAdded = await postRepository.CreatePost(post);

            if (isAdded)
            {
                return Ok("Created post successfully");
            }

            return BadRequest("Create post failed");
        }
        #endregion

        #region Mark Received
        [Authorize]
        [HttpPost("mark-received/{postId}")]
        public async Task<ActionResult> MarkReceived(int postId)
        {
            var checkPostMarked = await postRepository.GetPostById(postId);
            if (checkPostMarked != null && checkPostMarked.IsReceived == true)
            {
                return Ok(new
                {
                    message = "Your lost post is already marked"
                });
            }

            var postMarked = await postRepository.MarkReceived(postId);
            if (postMarked == null)
            {
                return BadRequest("Mark failed or post not found");
            }

            return Ok(new
            {
                message = "Marked successfully"
            });
        }
        #endregion

        #region Search Code Lost for Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("search-codes")]
        public async Task<ActionResult<List<Posts>>> SearchCodes([FromQuery] string query)
        {
            var posts = await postRepository.SearchCodes(query).ToListAsync();
            return Ok(posts);
        }
        #endregion

        #region Regular Search
        [HttpGet("regular-search")]
        public async Task<ActionResult<List<Posts>>> RegularSearch([FromQuery] string? status,
                                                      [FromQuery] int? categoryId,
                                                      [FromQuery] string? nameItem)
        {
            var posts = await postRepository.RegularSearch(status, categoryId, nameItem);
            var results = posts.Select(p => new
            {
                PostId = p.PostId,
                CategoryPostId = p.CategoryPostId,
                CreatedAt = p.CreatedAt,
                Description = p.Description,
                Image = p.Image,
                Title = p.Title,
                TypePost = p.TypePost,
                Vector = p.Vector,
                Code = p.Code,
                UrlImage = p.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{p.Image}",
                User = new Users
                {
                    UserId = p.User.UserId,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Email = p.User.Email,
                    Avatar = p.User.Avatar,
                    UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{p.User.Avatar}"
                }
            });

            return Ok(results);
        }
        #endregion

        #region Search Image Similarity
        // POST Search Image Similarity api/<PostController>
        [HttpPost("search-image-similarity")]
        public ActionResult<List<SearchResult>> SearchImageSimilarity([FromBody] List<double> vector)
        {
            var post = postRepository.SearchImageSimilarity(vector).ToList();

            foreach (var item in post)
            {
                item.Post.Vector = "";
                item.Post.UrlImage = $"{Request.Scheme}://{Request.Host}/Uploads/{item.Post.Image}";
                item.Post.User = new Users
                {
                    UserId = item.Post.User.UserId,
                    FirstName = item.Post.User.FirstName,
                    LastName = item.Post.User.LastName,
                    Email = item.Post.User.Email,
                    UrlAvatar = $"{Request.Scheme}://{Request.Host}/Uploads/{item.Post.User.Avatar}"
                };
            }

            return Ok(post);
        }
        #endregion

        #region Update Post
        // PUT api/<PostController>/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<bool>> Put(int id, [FromForm] PostDTO postDTO)
        {
            var post = await postRepository.GetPostById(id);

            if (post == null)
            {
                return NotFound(new
                {
                    Message = "Post does not found"
                });
            }

            post.CategoryPostId = postDTO.CategoryPostId;
            post.Title = postDTO.Title;
            post.Description = postDTO.Description;
            post.UpdatedAt = DateTime.Now;
            post.Vector = postDTO.Vector;

            if (postDTO.ImageUpload != null)
            {
                post.Image = await UploadFiles(postDTO.ImageUpload);
            }

            var isUpdated = await postRepository.UpdatePost(post);

            if (!isUpdated)
            {
                return BadRequest(new
                {
                    Message = "Update post failed"
                });
            }

            return Ok(new
            {
                Message = "Updated post successfully"
            });
        }
        #endregion

        #region Delete Post for Admin
        // DELETE api/<PostController>/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{postId}")]
        public async Task<ActionResult<bool>> Delete(int postId)
        {
            var isDeleted = await postRepository.DeletePost(postId);

            if (!isDeleted)
            {
                return BadRequest(new
                {
                    Message = "Delete post failed"
                });
            }

            return Ok(new
            {
                Message = "Deleted post successfully"
            });
        }
        #endregion

        // Funtion not related to Entity
        #region Function calculation similarity
        private int CalculateSimilarity(string lostDesc, string foundDesc)
        {
            var lostWords = NormalizeText(lostDesc);
            var foundWords = new HashSet<string>(NormalizeText(foundDesc)); // Faster search

            int matchCount = 0;

            foreach (var word in lostWords)
            {
                if (foundWords.Contains(word))
                {
                    matchCount++;
                }
            }

            if (lostWords.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)matchCount / lostWords.Count * 100);
        }
        #endregion

        #region Normalize Text
        private List<string> NormalizeText(string text)
        {
            text = text.ToLower();
            text = RemoveVietnameseTones(text);

            text = Regex.Replace(text, "<.*?>", ""); // Remove all html tags
            text = Regex.Replace(text, @"[^a-z0-9\s]", " ");

            #region Stop Words
            var stopWords = new HashSet<string>()
            {
                "la",
                "va",
                "tai",
                "toi",
                "bi",
                "lost",
                "found",
                "the",
                "am",
                "think",
                "are",
                "and",
                "holding",
                "was",
                "were",
                "is",
                "a",
                "an",
                "in",
                "on",
                "at",
                "of",
                "for",
                "with",
                "to",
                "by",
                "from",
                "cua",
                "co",
                "trong",
                "duoc",
                "mot",
                "nhung",
                "item",
                "object",
                "it",
                "thing",
                "they"
            };
            #endregion

            return text
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1 && !stopWords.Contains(w))
                .ToList();
        }
        #endregion

        #region Remove Vietnamese Tones
        private string RemoveVietnameseTones(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        #endregion

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
