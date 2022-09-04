using System.Collections.Generic;
using MovieApi.Models;

namespace MovieApi.Services
{
    public interface IMovieMetaDataService
    {
        IEnumerable<MovieMetaData> ReadMoviesMetaData(int movieId);

        void PostMovieMetaData(MovieMetaData movieMetaData);
    }
}