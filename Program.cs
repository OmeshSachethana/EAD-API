var builder = WebApplication.CreateBuilder(args);

// Register JwtHelper as a singleton or scoped service
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtHelper>();

// Add other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Context (add this if not already present)
builder.Services.AddSingleton<MongoDbContext>();

var app = builder.Build();

// Configure middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
