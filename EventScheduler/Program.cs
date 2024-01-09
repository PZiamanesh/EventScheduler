using EventScheduler;
using EventScheduler.Config;
using EventScheduler.Data;
using EventScheduler.Data.Model;
using EventScheduler.Filters;
using EventScheduler.Services;
using EventScheduler.Services.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExternalDependencyExceptionFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.ClientErrorMapping[500].Link = "https://codes.com/500";
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("EventSchedulerDB")));
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("EventSchedulerApp"));// Use in memory db

builder.Services.RegisterEventSchedulerServices();

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
});

// swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Events API",
        Description = "An ASP.NET Core Web API for managing events",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });

    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xmlFilePath);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddLogging();






var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/ErrorHandler/error-development");
}
else
{
    app.UseErrHandleMiddleware();

    //app.UseExceptionHandler(options =>
    //{
    //    options.Run(async http =>
    //    {
    //        var exeption = http.Features.Get<IExceptionHandlerFeature>();
    //        if (exeption is null)
    //        {
    //            return;
    //        }

    //        if (exeption.Error is IAppExceptionHandler exp)
    //        {
    //            await http.Response.WriteAsync(exp.ToJson());
    //        }
    //        else
    //        {
    //            await http.Response.WriteAsync(JsonConvert.SerializeObject(new
    //            {
    //                StatusCode = http.Response.StatusCode,
    //                Message = "somthing went wrong while processing"
    //            }));
    //        }
    //    });
    //});
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.SeedUsers();
app.MapControllers();

app.Run();
