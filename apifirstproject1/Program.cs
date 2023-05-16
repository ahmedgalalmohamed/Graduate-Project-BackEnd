using Graduate_Project_BackEnd.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBCONTEXT>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = builder.Configuration.GetSection("JWT:Issuer").Value,
                       ValidAudience = builder.Configuration.GetSection("JWT:Audience").Value,
                       ClockSkew = TimeSpan.Zero,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Key").Value))
                   };
               });

builder.Services.AddAuthorization();

var MyCorsApi = "MyCorsApi";
builder.Services.AddCors(option =>
{
    option.AddPolicy(name: MyCorsApi, builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});


var app = builder.Build();





if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    try
    {

        var flag = app.Services.CreateScope().ServiceProvider.GetRequiredService<DBCONTEXT>().Database.EnsureCreated();
        if (flag)
        {
            Console.WriteLine("**************************************");
            Console.WriteLine("\t\tDataBase Created :)");
            Console.WriteLine("**************************************");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("**************************************");
        Console.WriteLine(ex.Message);
        Console.WriteLine("**************************************");
    }
}

app.UseHttpsRedirection();

app.UseCors(MyCorsApi);

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.Run();
