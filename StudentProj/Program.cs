using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Repository;
using StudentProj.Validator;
using System.Text;
using Microsoft.OpenApi.Models;
using StudentProj.Services;
using StudentProj.Validators;
using Serilog;

// Configure Serilog with timestamped file name (brand new file every time the app starts)
var logFileName = System.IO.Path.Combine("logs", $"log-{DateTime.Now:yyyyMMdd_HHmmss}.txt");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logFileName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add validator services to the container.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<StudentDTO>, StudentValidator>();
builder.Services.AddScoped<IValidator<LoginDTO>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegisterDTO>, RegisterValidator>();
builder.Services.AddScoped<IValidator<AssignRoleDTO>, AssignRoleValidator>();
builder.Services.AddScoped<IValidator<RoleDTO>, RoleValidator>();
builder.Services.AddScoped<IValidator<PrivilegeDTO>, PrivilegeValidator>();


builder.Services.AddControllers().AddNewtonsoftJson();

//json token 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT-Token"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthentication();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StudentProj",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer your_token_here"
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
            Array.Empty<string>()
        }
    });
});

//Dbcontext Configuration
builder.Services.AddDbContext<StudentDbcontext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentDb"));
});

//Repository Dependency Injection
builder.Services.AddScoped<IStudent, StudentRepository>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();

var app = builder.Build();

app.UseMiddleware<StudentProj.Middleware.ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<StudentProj.Middleware.RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
