using Listing_Queue_BackgroundServices.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace Listing_Queue_BackgroundServices.Controllers
{

    public class PagingQueryParameter
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Keyword { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly SocialNetworkDbContext _context;


        //public IActionResult Index()
        //{

        //    var userProfiles = from p in _context.UserProfiles
        //                       select p;

        //    return (IActionResult)userProfiles;
        //}
        public WeatherForecastController(ILogger<WeatherForecastController> logger, SocialNetworkDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("Users")]
        public IActionResult Get([FromQuery]PagingQueryParameter paging)
        {

            IQueryable<UserProfile> query = _context.UserProfiles.AsQueryable();

            if(!string.IsNullOrWhiteSpace(paging.Keyword))
            {
                query = _context.UserProfiles.Where(x => x.UserName.Contains(paging.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(paging.Keyword))
            {
                query = _context.UserProfiles.Where(x => x.UserSurname.Contains(paging.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(paging.Keyword))
            {
                query = _context.UserProfiles.Where(x => x.Country.Contains(paging.Keyword));
            }

            if(paging.Start.HasValue && paging.End.HasValue)
            {
                query = query.Where(x=>x.DateOfBirth <=paging.End.Value && x.DateOfBirth >=paging.Start.Value);
            }

            //var userProfiles = (from p in _context.UserProfiles
            //                   select p).ToList();

            var totalCount = query.Count();
            var totalPage = (int)Math.Ceiling(totalCount / (double)paging.Limit);

            var skip = (paging.Page -1) * paging.Limit;
            var data = query.Skip(skip).Take(paging.Limit).ToList();

            bool hasNextPage = paging.Page < totalPage;
            bool hasPreviousPage = paging.Page > 1;

            var metaData = new
            {
                totalCount,
                paging.Limit,
                paging.Page,
                totalPage,
                hasNextPage,
                hasPreviousPage,
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(data);

            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = Random.Shared.Next(-20, 55),
            //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            //})
            //.ToArray();
        }
    }
}