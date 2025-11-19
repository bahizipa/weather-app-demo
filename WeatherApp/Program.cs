var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHealthChecks();          // Adds /health endpoint
builder.Services.AddEndpointsApiExplorer();  // Enables endpoint discovery for Swagger
builder.Services.AddSwaggerGen();            // Generates Swagger UI

// Configure Kestrel to listen on port 5038 (matching launch settings)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5038);  // Allows traffic from any network interface
});

var app = builder.Build();

// Enable Swagger UI in Dev & Prod
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
        c.RoutePrefix = "swagger";  // URL: /swagger
    });
}

// Welcome endpoint
app.MapGet("/", () => new
{
    Message = "Welcome to the Weather App!",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
})
.WithName("GetWelcome")
.WithTags("General");

// Weather forecast endpoint
app.MapGet("/weather", () =>
{
    var summaries = new[] { 
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching" 
    };

    var forecast = Enumerable.Range(1, 5).Select(index => new
    {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)), // Next 5 days
        TemperatureC = Random.Shared.Next(-20, 55),                 // Random °C
        TemperatureF = 0,                                           // Placeholder
        Summary = summaries[Random.Shared.Next(summaries.Length)]
    })
    .Select(temp => new
    {
        temp.Date,
        temp.TemperatureC,
        TemperatureF = 32 + (int)(temp.TemperatureC / 0.5556), // Formula °C → °F
        temp.Summary
    });

    return forecast;
})
.WithName("GetWeatherForecast")
.WithTags("Weather");

// Health check endpoint (important for Kubernetes probes)
app.MapHealthChecks("/health")
.WithTags("Health");

app.Run();  // Starts the web server