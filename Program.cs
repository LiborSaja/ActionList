using ActionList.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Initialization a connection parameters
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
var databaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "ActionListDB";

// Vytvoøení connection stringu
var connectionString = $"Host={host};Port={port};Username={username};Password={password}";

// Registrace DatabaseService s connection stringem a názvem databáze
builder.Services.AddSingleton(new DatabaseService(connectionString, databaseName));

builder.Services.AddScoped<TodoService>();

builder.Services.AddControllers();

// Pøidání služby CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:5173") // Adresa frontendu
              .AllowAnyHeader() // Povolit všechny hlavièky
              .AllowAnyMethod(); // Povolit všechny HTTP metody
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// DB Initialization
using (var scope = app.Services.CreateScope()) {
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Aktivace CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
