using System.Text.Encodings.Web;

namespace Training.Observability.Web;

public class TMDbApiClient(HttpClient httpClient)
{
    public async Task<Movie[]> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
    {
        List<Movie>? movies = null;

        await foreach (var movie in httpClient.GetFromJsonAsAsyncEnumerable<Movie>($"/movies/search?query={UrlEncoder.Default.Encode(query)}", cancellationToken))
        {
            if (movie is not null)
            {
                movies ??= [];
                movies.Add(movie);
            }
        }

        return movies?.ToArray() ?? [];
    }
}

public record Movie(string OriginalTitle, string OriginalLanguage, DateTime? ReleaseDate, string? Title, string PosterPath, string Overview, double VoteAverage, int VoteCount);