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
using StudentProj.Repository_Interface;

// Configure Serilog with timestamped file name (brand new file every time the app starts)
var logFileName = System.IO.Path.Combine("logs", $"log-{DateTime.Now:yyyyMMdd_HHmmss}.txt");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    // Sub-logger A: Write EVERYTHING to the Console (terminal)
    .WriteTo.Logger(lc => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
    // Sub-logger B: Filter and write ONLY custom audit logs to the File
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(logEvent => 
            logEvent.MessageTemplate.Text.StartsWith("Name:") || 
            logEvent.Properties.ContainsKey("Name"))
        .WriteTo.File(logFileName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<StudentProj.Mapping.MappingProfile>());

// Add validator services to the container.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<StudentDTO>, StudentValidator>();
builder.Services.AddScoped<IValidator<LoginDTO>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegisterDTO>, RegisterValidator>();
builder.Services.AddScoped<IValidator<AssignRoleDTO>, AssignRoleValidator>();
builder.Services.AddScoped<IValidator<RoleDTO>, RoleValidator>();
builder.Services.AddScoped<IValidator<PermissionDTO>, PermissionValidator>();
builder.Services.AddScoped<IValidator<RoutePermissionDTO>, RoutePermissionValidator>();

//redis connection
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
    builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "StudentProj_";
});


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
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IRoutePermissionRepository, RoutePermissionRepository>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IAttendenceRepository, AttendanceRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILogsRepository, LogsRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

var app = builder.Build();

app.UseMiddleware<StudentProj.Middleware.ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<StudentProj.Middleware.DynamicRbacMiddleware>();

app.UseAuthorization();

app.UseMiddleware<StudentProj.Middleware.RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
