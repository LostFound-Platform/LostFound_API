using DataAccess;
using LostFound_API.DTOs.Chat;
using LostFound_API.DTOs.Match;
using LostFound_API.DTOs.Posts;
using LostFound_API.DTOs.Users;
using LostFound_API.DTOs.VerificationCodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ObjectBusiness;
using Repository;
using Services;
using SignalRLayer;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactWeb", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
                           "https://back2me.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Memorycache
builder.Services.AddMemoryCache();

// Register HTTP Client
builder.Services.AddHttpClient();

// Add services to the container and allow automatic handle enum
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]))
    };

    // Configure JWT to read token from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var accessToken = ctx.Request.Query["access_token"]; // For use accessTokenFactory
            var path = ctx.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/SystemHub"))
            {
                ctx.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Add SignalR
builder.Services.AddSignalR()
       //.AddAzureSignalR() // Use azure signalR service
       .AddJsonProtocol(options =>
       {
           options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // To handle enum serialization
       });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Dependency Injection for DAOs and Repositories
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<UsersDAO>();
builder.Services.AddScoped<StudentDAO>();
builder.Services.AddScoped<PostDAO>();
builder.Services.AddScoped<CategoryPostDAO>();
builder.Services.AddScoped<MatchDAO>();
builder.Services.AddScoped<PickUpRequestDAO>();
builder.Services.AddScoped<VerificationCodeDAO>();
builder.Services.AddScoped<TransferRequestDAO>();
builder.Services.AddScoped<ChatDAO>();
builder.Services.AddScoped<NotificationsDAO>();
builder.Services.AddScoped<MessageChatDAO>();
builder.Services.AddScoped<InstitutionDAO>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICategoryPostRepository, CategoryPostRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
builder.Services.AddScoped<IPickUpRequestRepository, PickUpRequestRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageChatRepository, MessageChatRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IInstitutionRepository, InstitutionRepository>();

// Register BackgroundService
builder.Services.AddHostedService<HolderReminderService>();

// Connect to SQL Server
builder.Services.AddDbContext<BackToMeDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection"));
});

// Register for send email
//builder.Services.AddTransient<EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Allow frontend to fetch images via HTTP
app.UseStaticFiles();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowReactWeb");

app.UseAuthentication();
app.UseAuthorization();

// Use SignalR
app.MapHub<SystemHub>("/SystemHub");

app.MapControllers();

app.Run();
