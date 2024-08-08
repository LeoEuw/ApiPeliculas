using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConnexionSql")));

//soporte para autentificacion con .NET Identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//Soporte para versionamiento
var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
});

apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

//add repository here
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//soporte para caché
builder.Services.AddResponseCaching();

//soporte de versionamiento
builder.Services.AddApiVersioning();

//add AutoMapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

//Aqui se configura la autentificación
builder.Services.AddAuthentication
    (
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddControllers(option => 
{
    option.CacheProfiles.Add("PorDefecto20Segundos" , new CacheProfile() {Duration = 20});
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = 
            "Autenticación JWT usando el esquema Bearer. \r\n\r\n" +
            "Ingresa la palabra 'bearer' seguido de un [espacio] y despues su token en el campo de abajo. \r\n\r\n" +
            "Ejemplo: \"Bearer tklj256gekk\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
        //Detalle de la version 1
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0",
            Title = "Peliculas Api V1",
            Description = "Api de peliculas Version 1",
            TermsOfService = new Uri("https://www.linkedin.com/in/leoeuw/"),
            Contact = new OpenApiContact
            {
                Name = "Leonel Euward",
                Url = new Uri("https://www.linkedin.com/in/leoeuw/")
            },
            License = new OpenApiLicense
            {
                Name = "Licencia Personal",
                Url = new Uri("https://www.linkedin.com/in/leoeuw/")
            }
        }
        );

        //Detalle de la version 2
        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "2.0",
            Title = "Peliculas Api V2",
            Description = "Api de peliculas Version 1",
            TermsOfService = new Uri("https://www.linkedin.com/in/leoeuw/"),
            Contact = new OpenApiContact
            {
                Name = "Leonel Euward",
                Url = new Uri("https://www.linkedin.com/in/leoeuw/")
            },
            License = new OpenApiLicense
            {
                Name = "Licencia Personal",
                Url = new Uri("https://www.linkedin.com/in/leoeuw/")
            }
        }
        );
    }
);

//Añadir las politicas para CORS
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build => 
{
    build.WithOrigins("https://localhost:3223").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //aqui es para agregar las versiones en de la API
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiPeliculasV1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "ApiPeliculasV2");
    });
}

//soporte de archivos estaticos como imagen
app.UseStaticFiles();

app.UseHttpsRedirection();

//Agregar el CORS 
app.UseCors("PoliticaCors");

//Soporte para autentificacion
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
