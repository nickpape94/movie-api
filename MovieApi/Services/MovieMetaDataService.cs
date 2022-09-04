using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MovieApi.Models;

namespace MovieApi.Services
{
    public class MovieMetaDataService : IMovieMetaDataService
    {
        private static readonly List<MovieMetaData> Database = new List<MovieMetaData>();
        private static int LinesInDatabase = 0;

        private readonly string _metadataPath = $"{Directory.GetCurrentDirectory()}\\Resources\\metadata.csv";

        public MovieMetaDataService()
        {
            using (var reader = new StreamReader(_metadataPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line != null)
                    {
                        LinesInDatabase++;
                    }
                }
            }
        }

        public void PostMovieMetaData(MovieMetaData movieMetaData)
        {
            movieMetaData.Id = LinesInDatabase++;

            Database.Add(movieMetaData);
        }

        public IEnumerable<MovieMetaData> ReadMoviesMetaData(int movieId)
        {
            var movieMetaDataList = new List<MovieMetaData>();

            using (var reader = new StreamReader(_metadataPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    var splitLine = line.Replace(@", ", "- ").Replace("\\", "").Replace("\"", "").Split(',');

                    if (int.TryParse(splitLine.ElementAtOrDefault(1), out var result) && result == movieId)
                    {
                        var movieMetaData = new MovieMetaData
                        {
                            Id = int.TryParse(splitLine.ElementAtOrDefault(0), out var id) ? id : (int?)null,
                            MovieId = movieId,
                            Title = splitLine.ElementAtOrDefault(2),
                            Language = splitLine.ElementAtOrDefault(3),
                            Duration = splitLine.ElementAtOrDefault(4),
                            ReleaseYear = int.TryParse(splitLine.ElementAtOrDefault(5), out var releaseYear) ? releaseYear : (int?)null
                        };

                        movieMetaDataList.Add(movieMetaData);
                    }
                }
            }

            // Check Database list for matching records and append to result
            var relatedRecordsInAddedMoviesList = Database.Where(movie => movie.MovieId == movieId);
            movieMetaDataList = movieMetaDataList.Concat(relatedRecordsInAddedMoviesList).ToList();

            // Filter only latest piece of metadata where there are multiple metadata records for a given language, then order by language
            movieMetaDataList = FilterMetaData(movieMetaDataList);

            return movieMetaDataList;
        }

        private List<MovieMetaData> FilterMetaData(List<MovieMetaData> movieMetaDataList)
        {
            var languageOccurences = new Dictionary<string, int>();

            foreach (var movieMetaData in movieMetaDataList)
            {
                if (languageOccurences.Select(x => x.Key.ToLowerInvariant()).Contains(movieMetaData.Language.ToLowerInvariant()))
                {
                    languageOccurences[movieMetaData.Language]++;
                }
                else
                {
                    languageOccurences.Add(movieMetaData.Language, 1);
                }
            }

            foreach (var languageOccurence in languageOccurences)
            {
                if (languageOccurence.Value > 1)
                {
                    var latestMetaData = movieMetaDataList
                        .Where(x => string.Equals(x.Language, languageOccurence.Key, StringComparison.InvariantCultureIgnoreCase))
                        .OrderByDescending(x => x.Id)
                        .First();

                    movieMetaDataList = movieMetaDataList.Where(x => !string.Equals(x.Language, languageOccurence.Key, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    movieMetaDataList.Add(latestMetaData);
                }
            }

            return movieMetaDataList.Where(x => MovieDoesNotHaveNullFields(x)).OrderBy(x => x.Language).ToList();
        }

        private bool MovieDoesNotHaveNullFields(MovieMetaData movieMetaData)
        {
            if (movieMetaData.Title == null || movieMetaData.Language == null || movieMetaData.Duration == null || movieMetaData.ReleaseYear == null)
            {
                return false;
            }

            return true;
        }
    }
}