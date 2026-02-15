using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamsForgeAPI.Domain.Entities;
using TeamsForgeAPI.Infrastructure.Data;
using TeamsForgeAPI.Infrastructure.Extensions;
using TeamsForgeAPI.Infrastructure.Options;
using TeamsForgeAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
      //cors 
  builder.Services.AddCors(options => {
      options.AddPolicy("MyCors", builder =>
      {
          builder.WithOrigins("http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader();
      });
  });
  // Identity setup  
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
     .AddEntityFrameworkStores<AppDbContext>()
     .AddDefaultTokenProviders();
     
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtTokenService>();
builder.RegisterAuthentication();
builder.Services.AddSwaggerGen();
builder.Services.AddSwagger();
builder.Services.AddScoped<IAuthService, AuthService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
 app.UseCors("MyCors");
app.MapControllers();

app.Run();
