using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using StackExchange.Redis;
using TheShoesShop_BackEnd.Auth;
using TheShoesShop_BackEnd.Models;
using TheShoesShop_BackEnd.Services;
using TheShoesShop_BackEnd.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration["ConnectionStrings:MySqlConnection"];
builder.Services.AddDbContext<TheShoesShopDbContext>(options => options.UseMySQL(connectionString));

// Configure Redis connection
var connectionStringRedis = builder.Configuration["ConnectionStrings:Redis"];
var redisOptions = ConfigurationOptions.Parse(connectionStringRedis);
var redis = ConnectionMultiplexer.Connect(redisOptions);

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddTransient<RedisTokenBlacklist>();

//
builder.Services.AddControllers();

// Add services to config authentication with jwt bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Validate token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
        
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Check blacklist
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var Blacklist = context.HttpContext.RequestServices.GetRequiredService<RedisTokenBlacklist>();
            var BearerToken = context.Request.Headers["Authorization"].ToString();
            var Token = BearerToken.Substring(BearerToken.IndexOf(" ") + 1);

            if (await Blacklist.IsBlacklistedAsync(Token))
            {
                context.Fail("Token has been revoked");
            }
        }
    };
});

builder.Services.AddAuthorization();

// Add services to use inject
builder.Services.AddSingleton<JWTService>();
builder.Services.AddScoped<TheShoesShopServices>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ShoesModelService>();
builder.Services.AddScoped<SendingEmail>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Add automapper config
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Use public folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Public")),
    RequestPath = ""
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
