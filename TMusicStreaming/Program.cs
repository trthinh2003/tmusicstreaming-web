using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TMusicStreaming.Data;
using TMusicStreaming.Middleware;
using TMusicStreaming.Repositories;
using TMusicStreaming.Repositories.Implementations;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Implementations;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // DBContext
            builder.Services.AddDbContext<TMusicStreamingContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("StrConnection"))
             );

            // Add CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVue", policy =>
                {
                    policy.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:3000")
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            //Cloudinary
            var cloudinarySettings = builder.Configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinarySettings["CloudName"],
                cloudinarySettings["ApiKey"],
                cloudinarySettings["ApiSecret"]
            );
            builder.Services.AddSingleton(new Cloudinary(account));
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(Program));

            // Repositories & Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IGenreRepository, GenreRepository>();
            builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
            builder.Services.AddScoped<ISongRepository, SongRepository>();
            builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
            builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
            builder.Services.AddScoped<IDownloadRepository, DownloadRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
            builder.Services.AddHostedService<RecommendationBackgroundService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IArtistService, ArtistService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();
            builder.Services.AddScoped<IDownloadService, DownloadService>();
            builder.Services.AddScoped<IShareService, ShareService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            // Add Jwt Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            // Cấu hình Data Protection
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
                .SetApplicationName("TMusicStreaming")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                        ForwardedHeaders.XForwardedProto | 
                                        ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = ClaimTypes.Role
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "external_auth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = false;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            })
            .AddGoogle(options =>
            {
                IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
                options.ClientId = googleAuthNSection["ClientId"] ?? "GOOGLE_CLIENT_ID";
                options.ClientSecret = googleAuthNSection["ClientSecret"] ?? "GOOGLE_CLIENT_SECRET";

                options.CallbackPath = "/api/socialauth/google-callback";

                options.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Google OAuth redirect URI: {RedirectUri}", context.RedirectUri);
                        if (context.HttpContext.Request.Headers.ContainsKey("X-Forwarded-Proto"))
                        {
                            var redirectUri = context.RedirectUri;
                            if (redirectUri.StartsWith("http://") &&
                                context.HttpContext.Request.Headers["X-Forwarded-Proto"] == "https")
                            {
                                redirectUri = redirectUri.Replace("http://", "https://");
                                context.RedirectUri = redirectUri;
                                logger.LogInformation("Updated redirect URI to HTTPS: {RedirectUri}", redirectUri);
                            }
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },

                    OnCreatingTicket = async context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Google OAuth ticket created successfully");
                    },

                    OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Google OAuth failed: {Error}", context.Failure?.Message);

                        var errorMessage = context.Failure?.Message ?? "Authentication failed";
                        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
                        context.Response.Redirect($"{frontendUrl}/auth/callback?status=error&message={Uri.EscapeDataString(errorMessage)}");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.SaveTokens = true;

                // Cấu hình cookie
                options.CorrelationCookie.Name = "google_correlation";
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            })
            .AddFacebook(options =>
            {
                IConfigurationSection facebookAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
                options.ClientId = facebookAuthNSection["AppId"] ?? "FACEBOOK_CLIENT_ID";
                options.ClientSecret = facebookAuthNSection["AppSecret"] ?? "FACEBOOK_CLIENT_SECRET";
                options.CallbackPath = "/api/SocialAuth/facebook-callback";

                options.Scope.Add("email");
                options.Scope.Add("public_profile");
                options.SaveTokens = true;

                // Cấu hình cookie cho Facebook OAuth
                options.CorrelationCookie.Name = "facebook_correlation";
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;

                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Facebook OAuth failed: {Error}", context.Failure?.Message);

                        var errorMessage = context.Failure?.Message ?? "Facebook authentication failed";
                        context.Response.Redirect($"{builder.Configuration["FrontendUrl"]}/auth/callback?status=error&message={Uri.EscapeDataString(errorMessage)}");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Add Controllers
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseRequestLogging();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });
            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Use CORS
            app.UseCors("AllowVue");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

            // Run the application
            app.Run();
        }
    }
}