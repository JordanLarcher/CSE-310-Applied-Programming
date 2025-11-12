using TaskScheduler.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// Background Service Registration
builder.Services.AddHostedService<ReminderService>();

// Add CORS services to allow our Blazor app to call the API
// This is critical for connecting frontend and backend.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5158") // <-- IMPORTANT: Update this to your Blazor app's URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add Swagger/OpenAPI for API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// **************************************************
// ** ADD THIS LINE to enable the CORS policy **
// **************************************************

app.UseCors("AllowBlazorApp");
app.UseAuthorization();
app.MapControllers();
app.Run();