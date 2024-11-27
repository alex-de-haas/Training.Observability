using System.Diagnostics;
using System.Diagnostics.Metrics;
using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace Training.Observability.ApiService
{
    public partial class MoviesService
    {
        private static ActivitySource ActivitySource = new("Training.Observability.ApiService", "1.0.0");
        private static Meter Meter = new("Training.Observability.ApiService", "1.0.0");

        private static Histogram<double> MoviesSearchHistogram = Meter.CreateHistogram<double>("movies.search", "Avg. search operation duration (include handlers)", "ms");
        private static Counter<int> MoviesSearchCounter = Meter.CreateCounter<int>("movies.search.counter", "Read operation counter");

        private readonly ILogger<MoviesService> _logger;

        public MoviesService(ILogger<MoviesService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<SearchMovie>> SearchMoviesAsync(string query)
        {
            using (var activity = ActivitySource.StartActivity($"SEARCH MOVIES for '{query}'", ActivityKind.Client))
            {
                var client = new TMDbClient("e4c50756f8e355bf6d02422fc6a2a535");
                var stopwatch = Stopwatch.StartNew();
                var movies = await client.SearchMovieAsync(query);
                stopwatch.Stop();

                //throw new ApplicationException("Test exception.");
                //_logger.LogInformation("Found {TotalResults} movies for '{Query}'", movies.TotalResults, query);
                LogMovie(_logger, movies.TotalResults, query);

                activity?.AddTag("search.Query", query);
                activity?.AddTag("search.TotalResults", movies.TotalResults);

                if (movies.TotalResults == 1)
                {
                    activity?.AddEvent(new ActivityEvent("Exact match!"));

                    var exactMovie = movies.Results.First();
                    LogExactMovie(_logger, exactMovie.OriginalTitle, exactMovie);
                }
                else if (movies.TotalResults == 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, $"No movies found for query '{query}'.");
                }

                MoviesSearchCounter.Add(movies.TotalResults);
                MoviesSearchHistogram.Record(stopwatch.Elapsed.TotalMilliseconds);

                SomeLogic();

                return movies.Results;
            }
        }

        private void SomeLogic()
        {
            var activity = Activity.Current;
            activity?.AddTag("some.tag", "some value");
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Found {TotalResults} movies for '{Query}'")]
        private static partial void LogMovie(ILogger logger, int totalResults, string query);

        [LoggerMessage(Level = LogLevel.Information, Message = "Found exact '{OriginalTitle}' movie")]
        private static partial void LogExactMovie(
            ILogger logger,
            string originalTitle,
            [TagProvider(typeof(SearchMovieTagProvider), nameof(SearchMovieTagProvider.RecordTags))] SearchMovie movie);
    }

    internal static class SearchMovieTagProvider
    {
        public static void RecordTags(ITagCollector collector, SearchMovie movie)
        {
            collector.Add(nameof(movie.ReleaseDate), movie.ReleaseDate);
            collector.Add(nameof(movie.OriginalLanguage), movie.OriginalLanguage);
            collector.Add(nameof(movie.OriginalTitle), movie.OriginalTitle);
            collector.Add(nameof(movie.Title), movie.Title);
            collector.Add(nameof(movie.Overview), movie.Overview);
        }
    }
}
