using Sdk_EC_Backend.Configuration;
using Sdk_EC_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure Supabase
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(SupabaseSettings.SectionName));
// If the Key is not present in configuration, allow providing it via environment variable SUPABASE_KEY
builder.Services.PostConfigure<SupabaseSettings>(opts =>
{
    if (string.IsNullOrWhiteSpace(opts.Key))
    {
        var envKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            opts.Key = envKey;
        }
    }
});
builder.Services.AddScoped<ISupabaseService, SupabaseService>();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers (migrate minimal endpoints to controllers)
builder.Services.AddControllers();

var app = builder.Build();

// Enable CORS - add this before other middleware
app.UseCors("AllowAngularDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger JSON and the Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI();

    // Keep existing OpenAPI mapping (if used by other tooling)
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

// Map controllers (ProductController handles /api/products)
app.MapControllers();

app.Run();
