using System.Collections.Generic;
using MovieApi.Models;

namespace MovieApi.Services
{
    public interface IMovieStatsService
    {
        IEnumerable<AggregatedMovieStats> ReadMovieStats();
    }
}