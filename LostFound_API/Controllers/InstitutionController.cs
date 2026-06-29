using Azure;
using LostFound_API.DTOs.Institution;
using LostFound_API.DTOs.Users;
using LostFound_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObjectBusiness;
using Repository;
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
        private readonly StateService stateService;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        #endregion

        #region Constructor
        public InstitutionController(IInstitutionRepository institutionRepository,
                               StateService stateService,
                               IConfiguration configuration,
                               IHttpClientFactory httpClientFactory)
        {
            this.institutionRepository = institutionRepository;
            this.stateService = stateService;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }
        #endregion

        [HttpGet]
        public IActionResult Test()
        {
            var state = stateService.ByCode["GA"];

            return Ok(state.Name);
        }

        // GET: api/<InstitutionController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

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

        // POST api/<InstitutionController>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateInstitution([FromBody] Institution institution)
        {
            if (institution == null || string.IsNullOrWhiteSpace(institution.InstitutionName) || string.IsNullOrWhiteSpace(institution.InstitutionAddress))
            {
                return BadRequest("Invalid institution data");
            }

            var isAddedInstitution = await institutionRepository.CreateInstitution(institution);

            if (isAddedInstitution)
            {
                return Ok(new
                {
                    Message = "Created institution successfully"
                });
            }

            return BadRequest("Create institution failed");
        }

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
