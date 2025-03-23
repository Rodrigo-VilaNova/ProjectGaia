using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Environment: {environment}");

            SetupCredentials();
            Console.WriteLine($"Using email: {Environment.GetEnvironmentVariable("email")}");

            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsProduction())
            {
                builder.Configuration["Kestrel:Certificates:Default:Path"] = "./Certificates/domain.cert.pem";
                builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = "./Certificates/private.key.pem";
            }

            builder.Services.AddScoped<ConfirmationService>();
            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddScoped<TokenService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    if (builder.Environment.IsProduction()) policy.WithOrigins(["https://projectgaia.azurewebsites.net", "https://gaia.pombos.net:443"]);
                    else policy.WithOrigins(["http://127.0.0.1:5002", "http://localhost:5002"]);

                    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
            builder.Services.AddDbContext<AppDbContext>(options => options
                    .UseSqlServer(connectionString)
                    .ConfigureWarnings(b => b.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS))
                    );
            //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllers();/*.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });*/
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectGaia", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }

        private static void SetupCredentials()
        {
            string filePath = "./credentials.json";
            Dictionary<string, string>? credentials = null;

            bool invalidCredentials = false;

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Credentials file found. Reading...");

                try
                {
                    string json = File.ReadAllText(filePath);
                    credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (credentials != null)
                    {
                        if (credentials.Count == 2 && credentials.ContainsKey("email") && credentials.ContainsKey("password"))
                        {
                            string email = credentials.GetValueOrDefault("email") ?? "";
                            string password = credentials.GetValueOrDefault("password") ?? "";
                            invalidCredentials = !CheckCredentials(email, password);

                            if (invalidCredentials) Console.WriteLine("Invalid credentials stored. Recreating...");
                        }
                        else Console.WriteLine("Invalid credentials file. Recreating...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading credentials file: {ex.Message}");
                }
            }
            else Console.WriteLine("Credentials file not found. Creating...");

            if (credentials == null || credentials.Count != 2 || !credentials.ContainsKey("email") || !credentials.ContainsKey("password") || invalidCredentials)
            {
                credentials = new Dictionary<string, string>();

                while (true)
                {
                    string email;
                    while (true)
                    {
                        Console.Write("Email: ");
                        email = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(email)) break;
                        Console.WriteLine("Invalid email");
                    }

                    string password;
                    while (true)
                    {
                        Console.Write("Password: ");
                        password = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(password)) break;
                        Console.WriteLine("Invalid password");
                    }

                    if (CheckCredentials(email, password))
                    {
                        Console.WriteLine("Credentials validated");
                        credentials.Add("email", email);
                        credentials.Add("password", password);
                        break;
                    }

                    Console.WriteLine("Credentials incorrect");
                }

                Console.WriteLine("Writing credentials to file...");
                SaveCredentials(filePath, credentials);
                Console.WriteLine($"Credentials saved to \"{filePath}\"");
            }

            Environment.SetEnvironmentVariable("email", credentials["email"]);
            Environment.SetEnvironmentVariable("password", credentials["password"]);
        }

        private static bool CheckCredentials(string email, string password)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate(email, password);
                    client.Disconnect(true);
                    return true;
                }
            }
            catch (AuthenticationException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking credentials: " + ex.Message);
                return false;
            }
        }

        private static void SaveCredentials(string filePath, Dictionary<string, string> credentials)
        {
            string json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}
