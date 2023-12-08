using fume.api.Data;
using fume.api.Helpers;
using fume.shared.Enttities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer("name=DockerConnection"));
builder.Services.AddTransient<SeedBd>();
builder.Services.AddScoped<IUserHelper, UserHelper>();

builder.Services.AddIdentity<User, IdentityRole>(x =>
{
    x.User.RequireUniqueEmail = true;
    x.Password.RequireDigit = false;
    x.Password.RequiredUniqueChars = 0;
    x.Password.RequireLowercase = false;
    x.Password.RequireNonAlphanumeric = false;
    x.Password.RequireUppercase = false;
   
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();




var app = builder.Build();
SeedeDataDb(app);

void SeedeDataDb(WebApplication app)
{
    IServiceScopeFactory? scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (IServiceScope? scope = scopedFactory!.CreateScope())
    {
        SeedBd? service = scope.ServiceProvider.GetService<SeedBd>();
        service!.seedAsync().Wait();


    }
}

  if (app.Environment.IsDevelopment())
{
        app.UseSwagger();
        app.UseSwaggerUI();
  }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseCors(x => x
       .AllowAnyMethod()
       .AllowAnyHeader()
       .SetIsOriginAllowed(Origin => true)
       .AllowCredentials()
        );
    app.Run();
