using Npgsql;
using Stock.RealTime.Api;
using Stock.RealTime.Api.Services.Realtime;
using Stock.RealTime.Api.Services.Stocks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// CORS: allow all (origins, headers, methods)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
        .WithOrigins("http://127.0.0.1:8000", "http://localhost:8000")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());

});

// Postgres data source
builder.Services.AddSingleton(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("StockDb")
        ?? throw new InvalidOperationException("Connection string 'StockDb' not found.");
    return NpgsqlDataSource.Create(cs);
});

builder.Services.AddHostedService<DatabaseInitializer>();

builder.Services.AddHttpClient<StocksClient>("StockPriceClient", httpClient =>
{
    var apiUrl = builder.Configuration["Stocks:ApiUrl"] ??
        throw new InvalidOperationException("Config 'Stocks:ApiUrl' is missing.");
            
    if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseUri))
        throw new InvalidOperationException("Config 'Stocks:ApiUrl' must be a valid absolute URI.");
    httpClient.BaseAddress = baseUri;
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<IStocksClient>(sp => sp.GetRequiredService<StocksClient>());

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IStockService, StockService>();
//builder.Services.AddScoped<IStocksClient, StocksClient>();
builder.Services.AddSingleton<ActiveTickerManager>();
builder.Services.AddHostedService<StockFeedUpdater>();

builder.Services.Configure<StockUpdateOptions>(builder.Configuration.GetSection("StockUpdateOptions"));


var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    // Built-in OpenAPI JSON (served at /openapi/v1.json)
    app.MapOpenApi();

    // Swagger UI consuming the built-in OpenAPI JSON
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Stock RealTime API v1");
        options.RoutePrefix = "swagger"; // UI at /swagger
    });
}

// Enable CORS before auth and endpoints
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers().RequireCors("AllowAll");
app.MapHub<StocksFeedHub>("/stocks-feed").RequireCors("AllowAll");

//app.UseHttpsRedirection();
app.Run();
