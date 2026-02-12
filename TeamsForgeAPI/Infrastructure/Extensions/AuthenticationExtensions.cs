using System;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TeamsForgeAPI.Domain.Entities;
using TeamsForgeAPI.Infrastructure.Data;
using TeamsForgeAPI.Infrastructure.Options;

namespace TeamsForgeAPI.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
     public static WebApplicationBuilder RegisterAuthentication(this WebApplicationBuilder builder)
 {
     // Bind JWT settings from configuration
     var jwtSettings = new JwtSettings();
     builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);

     var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
     builder.Services.Configure<JwtSettings>(jwtSection);

     // Configure JWT Bearer authentication
     builder.Services.AddAuthentication(options =>
     {
         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
     })
     .AddJwtBearer(jwt =>
     {
         jwt.SaveToken = true;
         jwt.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SigningKey
                 ?? throw new InvalidOperationException())),
             ValidateIssuer = true,
             ValidIssuer = jwtSettings.Issuer,
             ValidateAudience = true,
             ValidAudiences = jwtSettings.Audiences,
             RequireExpirationTime = true,
             ClockSkew = TimeSpan.Zero, // No tolerance for expired tokens
             NameClaimType = ClaimTypes.NameIdentifier,   // Specifies the claim type to be used as the user's unique identifier
             ValidateLifetime = true // Ensures token has not expired
         };

         jwt.Audience = jwtSettings.Audiences?[0];
         jwt.ClaimsIssuer = jwtSettings.Issuer;
     });

     // Configure Identity options
     builder.Services.AddIdentityCore<ApplicationUser>(options =>
     {
         options.Password.RequireDigit = false;
         options.Password.RequiredLength = 5;
         options.Password.RequireLowercase = false;
         options.Password.RequireUppercase = false;
         options.Password.RequireNonAlphanumeric = false;
     })
     .AddRoles<IdentityRole>() // Add role support
     .AddSignInManager() // Add sign-in manager
     .AddEntityFrameworkStores<AppDbContext>(); // Use EF Core for identity storage

     return builder;
 }

 

 public static IServiceCollection AddSwagger(this IServiceCollection services)
 {
     // Add Swagger for API documentation
     services.AddSwaggerGen(options =>
     {
         options.SwaggerDoc("v1", new OpenApiInfo { Title = "SupportTaskAPI", Version = "v1" });
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
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                     }
                 },
                 new string[]{}
             }
         });
     });

     return services;
 }

}
