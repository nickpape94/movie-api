using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieApi.Services;

namespace MovieApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieStatsService _movieStatsService;

        public MoviesController(IMovieStatsService movieStatsService)
        {
            _movieStatsService = movieStatsService;
        }

        [HttpGet]
        [Route("stats")]
        public IActionResult GetMovieStats()
        {
            var movieMetaData = _movieStatsService.ReadMovieStats();

            return Ok(movieMetaData);
        }
    }
}