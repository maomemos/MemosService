using MemosService.Data;
using MemosService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace MemosService
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    policy.WithOrigins(builder.Configuration["Cors:domain"]!)
                        .AllowAnyHeader()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyMethod();
                });
            });
            
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Memos API",
                    Description = "An ASP.NET Core Web API for managing memos",
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("https://mit-license.org/")
                    }
                });
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "MemosService.xml");
                option.IncludeXmlComments(xmlPath, true);

                // 文档页添加标头
                var scheme = new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Description = "Authorization header \r\n 输入 JWT 值",
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Authorization",
                    },
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                };
                option.AddSecurityDefinition("Authorization", scheme);
                var requirement = new OpenApiSecurityRequirement();
                requirement[scheme] = new List<string>();
                option.AddSecurityRequirement(requirement);
            });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
#pragma warning disable CS8604
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
#pragma warning restore CS8604
                        // Token 过期时间为 1 年
                        LifetimeValidator = (before, expires, token, param) => expires > DateTime.UtcNow,
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            builder.Services.AddDbContext<MemosContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("WebApiDatabase"));
            });
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IMemoService, MemoService>();
            builder.Services.AddControllers();

            var app = builder.Build();

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");

            // app.UseDeveloperExceptionPage();

            app.UseDefaultFiles(defaultFilesOptions);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
