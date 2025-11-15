using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ServiceStack.Text;
using System.Runtime.InteropServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("defaultstring") ?? throw new InvalidOperationException("Connection string 'defaultstring' not found.");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddCors();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddSignalR();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));//This for cloudinary and first CloudinarySettings is helper class and second one from appsetting.json

//This is for Identity Configuration
builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<AppRoles>()
    .AddRoleManager<RoleManager<AppRoles>>()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("TokenKey Not Found");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
    .AddPolicy("RequirePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"));
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PresenseHub>("hubs/presence");//Mapp incomming requests with the specified path
//seed data code
using var scope = app.Services.CreateScope(); //Return A IserviceScope to resolved scope services
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>(); //return a service object of datacontext
    var usermanager = services.GetRequiredService<UserManager<AppUser>>();
    var rolemanager = services.GetRequiredService<RoleManager<AppRoles>>();
    await context.Database.MigrateAsync();//will create the database if the database not exist // Automatically creates the database if it doesn’t exist.
    if (!context.Users.Any()) //If The users have no data then you can seed the data
    {
        await Seed.SeedUsers(usermanager,rolemanager);
    }

}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");

}


app.Run();
