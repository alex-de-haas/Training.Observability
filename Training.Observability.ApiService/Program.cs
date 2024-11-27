using Training.Observability.ApiService;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<MoviesService>();

var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/movies/search", async (string query, MoviesService moviesService) => await moviesService.SearchMoviesAsync(query));
app.MapDefaultEndpoints();
app.Run();