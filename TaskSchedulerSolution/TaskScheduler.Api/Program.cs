using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskScheduler.Api.Data;
using TaskScheduler.Shared;
using TaskScheduler.Api.Services;
using TaskScheduler.Infrastructure.Data;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Repositories;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Application.Services.Implementation;
using TaskScheduler.Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<TaskScheduler.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity (if using the simple ApplicationDbContext in Api/Data)
// Note: You may need to decide which ApplicationDbContext to use
// builder.Services.AddDbContext<TaskScheduler.Api.Data.ApplicationDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();

// Register application services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// Optional: Use SmtpEmailService for production email sending
// builder.Services.AddScoped<IEmailService, SmtpEmailService>();

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

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskScheduler.Infrastructure.Data.ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Creates database and tables if they don't exist
}

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