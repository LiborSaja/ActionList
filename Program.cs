using ActionList.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Initialization a connection parameters
string connectionString = "Host=localhost;Database=ActionListDB;Username=postgres;Password=QaY123321WsX";
string databaseName = "ActionListDB";
builder.Services.AddSingleton(new DatabaseService(connectionString, databaseName));

builder.Services.AddScoped<TodoService>();

builder.Services.AddControllers();
//builder.Services.AddControllers(options => {
//    options.Filters.Add<ActionList.Filters.ValidateModelFilter>();
//});

builder.Services.AddOpenApi();

var app = builder.Build();

// DB Initalization
using (var scope = app.Services.CreateScope()) {
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    dbService.InitializeDatabase();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
