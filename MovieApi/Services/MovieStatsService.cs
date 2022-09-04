using System.Collections.Generic;
using System.IO;
using System.Linq;
using MovieApi.Models;

namespace MovieApi.Services
{
    public class MovieStatsService : IMovieStatsService
    {
        private readonly string _statsPath = $"{Directory.GetCurrentDirectory()}\\Resources\\stats.csv";
        private readonly string _metadataPath = $"{Directory.GetCurrentDirectory()}\\Resources\\metadata.csv";

        public IEnumerable<AggregatedMovieStats> ReadMovieStats()
        {
            var movieStatsList = new List<MovieStats>();

            using (var reader = new StreamReader(_statsPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var splitLine = line.Split(',');

                    if (int.TryParse(splitLine.ElementAtOrDefault(0), out var movieId))
                    {
                        var movieStats = new MovieStats
                        {
                            MovieId = movieId,
                            WatchDurationMs = long.Parse(splitLine.ElementAtOrDefault(1))
                        };

                        movieStatsList.Add(movieStats);
                    }
                }
            }

            return AggregateData(movieStatsList);
        }

        private List<AggregatedMovieStats> AggregateData(List<MovieStats> movieStatsList)
        {
            var aggregatedData = new Dictionary<int, WatchMetrics>();
            var aggregatedMovieStatsList = new List<AggregatedMovieStats>();

            foreach (var movieStats in movieStatsList)
            {
                if (aggregatedData.Select(x => x.Key).Any(x => x == movieStats.MovieId))
                {
                    var watchMetrics = aggregatedData[movieStats.MovieId];
                    var count = watchMetrics.Count += 1;
                    var watchDurationMs = watchMetrics.WatchDurationMs + movieStats.WatchDurationMs;

                    aggregatedData[movieStats.MovieId] = new WatchMetrics { Count = count, WatchDurationMs = watchDurationMs };
                }
                else
                {
                    aggregatedData.Add(movieStats.MovieId, new WatchMetrics { Count = 1, WatchDurationMs = movieStats.WatchDurationMs });
                }
            }

            foreach (var data in aggregatedData)
            {
                var movieId = data.Key;
                var averageWatchDurationMs = data.Value.WatchDurationMs / data.Value.Count;
                var averageWatchDurationS = averageWatchDurationMs / 1000;

                var movieMetaData = ReadMovieMetaData(movieId);

                if (movieMetaData.Title != null && movieMetaData.ReleaseYear != null)
                {
                    aggregatedMovieStatsList.Add(new AggregatedMovieStats
                    {
                        MovieId = movieId,
                        Title = movieMetaData.Title,
                        AverageWatchDurationS = (int)averageWatchDurationS,
                        Watches = data.Value.Count,
                        ReleaseYear = movieMetaData.ReleaseYear
                    });
                }
               
            }

            return aggregatedMovieStatsList.OrderByDescending(x => x.Watches).ThenByDescending(x => x.ReleaseYear).ToList();
        }

        private MovieMetaData ReadMovieMetaData(int movieId)
        {
            var movieMetadata = new MovieMetaData();

            using (var reader = new StreamReader(_metadataPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var splitLine = line.Split(',');

                    if (int.TryParse(splitLine.ElementAtOrDefault(1), out var result) && result == movieId)
                    {
                        movieMetadata.MovieId = movieId;
                        movieMetadata.Title = splitLine.ElementAtOrDefault(2);
                        movieMetadata.ReleaseYear = int.TryParse(splitLine.ElementAtOrDefault(5), out var releaseYear) ? releaseYear : (int?)null;

                        break;
                    }
                }
            }

            return movieMetadata;
        }
    }
}