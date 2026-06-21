using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using Repository;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryPostController : ControllerBase
    {
        #region Variables
        private readonly IPostRepository postRepository;
        private readonly ICategoryPostRepository categoryPostRepository;
        #endregion

        #region Constructor
        public CategoryPostController(IPostRepository postRepository,
                               ICategoryPostRepository categoryPostRepository)
        {
            this.postRepository = postRepository;
            this.categoryPostRepository = categoryPostRepository;
        }
        #endregion

        // GET: api/<CategoryPostController>
        [HttpGet]
        public async Task<ActionResult<List<CategoryPost>>> Get()
        {
            var categories = await categoryPostRepository.AllCategoryPosts().ToListAsync();
            return Ok(categories);
        }

        [HttpGet("search-category-post")]
        public async Task<ActionResult<List<CategoryPost>>> Get(string query)
        {
            var categories = await categoryPostRepository.SearchCategoryPost(query).ToListAsync();
            return Ok(categories);
        }

        // GET api/<CategoryPostController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CategoryPostController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CategoryPostController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CategoryPostController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
