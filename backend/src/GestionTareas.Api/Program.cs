using System.Text;
using System.Text.Json.Serialization;
using GestionTareas.Api.Middleware;
using GestionTareas.Aplicacion;
using GestionTareas.Infraestructura;
using GestionTareas.Infraestructura.Persistencia;
using GestionTareas.Infraestructura.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    // Los enums (estado, prioridad) viajan como texto en el JSON: "Pending", "Alta"...
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestión de tareas",
        Version = "v1",
        Description = "API para gestionar usuarios y tareas. Para probar: use POST /api/auth/login, copie el token y péguelo en el botón Authorize."
    });

    // Lee los comentarios /// de los controladores y de los DTOs para mostrarlos en Swagger.
    foreach (var archivo in Directory.GetFiles(AppContext.BaseDirectory, "GestionTareas.*.xml"))
        o.IncludeXmlComments(archivo);

    // Agrega el botón "Authorize" a Swagger para poder probar los endpoints protegidos con el token.
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue solo el token que devuelve /api/auth/login."
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);

// Autenticación con JWT: el token se valida con la misma clave, emisor y audiencia con los que se genera.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// CORS para que el Angular (por defecto en localhost:4200) pueda llamar a la API.
var origenes = builder.Configuration.GetSection("CorsOrigenes").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Al arrancar se aplican las migraciones pendientes: si la base no existe, se crea sola en el SQL Server local.
using (var alcance = app.Services.CreateScope())
{
    var contexto = alcance.ServiceProvider.GetRequiredService<GestionTareasDbContext>();
    contexto.Database.Migrate();
}

app.UseMiddleware<ManejoErroresMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
