using LiteDB;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Threading.Tasks;

namespace movie_search_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[EnableCors("http://localhost:8080")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly ElasticClient _client;
        private readonly ILiteCollection<Movie> _col;

        public MoviesController(ILogger<MoviesController> logger, ElasticClient client, ILiteCollection<Movie> col)
        {
            _logger = logger;
            _client = client;
            _col = col;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Get([FromQuery(Name = "term")] string term)
        {
            //.Index("mytweetindex") //or specify index via settings.DefaultIndex("mytweetindex");
            var response = Movie.Search(_client, term);

            // turn movies into movienames
            string[] movieNames = new string[10];
            var movies = response.Documents;
            int i = 0;

            foreach (var m in movies)
            {
                movieNames[i] = m.MovieName;
                i++;
            }

            Console.WriteLine(term);
            Movie.AddView(_col, movieNames);
            return Ok(response.Documents);
        }
        [HttpGet]
        [Route("{name}")]
        public IActionResult GetID(string name)
        {
            // Console.WriteLine(name);
            var resp = Movie.AddClick(_col, name);
            if (resp != null)
                return Ok(resp);
            else
                return NotFound("no movie with this name exists");
        }
    }
}
