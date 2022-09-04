using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieApi.Models;
using MovieApi.Services;

namespace MovieApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {
        private readonly IMovieMetaDataService _movieMetaDataService;

        public MetaDataController(IMovieMetaDataService movieMetaDataService)
        {
            _movieMetaDataService = movieMetaDataService;
        }

        [HttpGet("{movieId}")]
        public IActionResult GetMovieMetaData(int movieId)
        {
            var movieMetaData = _movieMetaDataService.ReadMoviesMetaData(movieId);

            if (!movieMetaData.Any())
            {
                return NotFound();
            }

            return Ok(movieMetaData);
        }

        [HttpPost]
        public IActionResult CreateMetaDataForMovie(MovieMetaData movieMetaData)
        {
            _movieMetaDataService.PostMovieMetaData(movieMetaData);

            return Ok(movieMetaData);
        }
    }
}