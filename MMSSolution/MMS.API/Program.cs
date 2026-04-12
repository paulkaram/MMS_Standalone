using Intalio.Tools.Common.AsposeWrapper;
using Intalio.Tools.Common.Encryptions;
using Intalio.Tools.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMS.DAL;
using MMS.BLL;
using System.Text;
using MMS.API.Common.Hubs;
using MMS.BLL.Constants;
using MMS.BLL.Storage;
using StackExchange.Redis;
using Newtonsoft.Json;
using MMS.API.Common.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

AsposeLicense.SetLicense();


var builder = WebApplication.CreateBuilder(args);

string[]? allowedHosts = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
string? dbConfigurationConnectionString = builder.Configuration.GetConnectionString("Configuration");

if (!string.IsNullOrWhiteSpace(dbConfigurationConnectionString))
{
    //load configuration from database
    //builder.Configuration.Sources.Clear();
    builder.Configuration.AddDbConfiguration(dbConfigurationConnectionString);
}
builder.Services.AddSingleton<ConfigurationManager>(builder.Configuration); 
StorageSettings storageSettings = builder.Configuration.GetSection(AppSettingsConstants.StorageSectionName).Get<StorageSettings>() ?? new();

// NCA Compliance: Initialize encryption service with keys from secure configuration (NCS-1:2020 Section 8)
// Keys should be stored in environment variables or a secure vault, not in appsettings.json
EncryptionSettings encryptionSettings = builder.Configuration.GetSection(AppSettingsConstants.EncryptionSectionName).Get<EncryptionSettings>() ?? new();
#pragma warning disable CS0618 // Obsolete warning - static EncryptionService is used for backwards compatibility
EncryptionService.Initialize(encryptionSettings);
#pragma warning restore CS0618

builder.Services.AddSingleton(storageSettings);
builder.Services.AddSingleton(encryptionSettings);
builder.Services.AddSingleton<IEncryptionService, EncryptionServiceImpl>();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLayer();

// DCC Compliance: Add rate limiting for API protection
builder.Services.AddRateLimiting(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = Convert.ToBoolean(builder.Configuration["JWT:HttpsOnly"]);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
		ValidIssuers =
		[
			builder.Configuration["JWT:Issuer"],
        ],
		ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? ""))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR sends token via query string for WebSocket/SSE connections
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();

            // Fallback to Authorization header if not in query string
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            }

            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/api/intaliohub") && !string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

var redisConnection = builder.Configuration["RedisConnection"];
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
	 {
	 });

var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// DCC Compliance: Session limit settings for concurrent session management
var sessionLimitSettings = builder.Configuration.GetSection("SessionLimit").Get<SessionLimitSettings>() ?? new SessionLimitSettings();
builder.Services.AddSingleton(sessionLimitSettings);

builder.Services.AddScoped<IMainHub, IntalioHub>();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "API V1", Version = "v1" });

	// Add security definition for Bearer token
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please enter your Bearer token",
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
				new string[] {}
			}
		});
});
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers(options =>
{
	options.Filters.Add<ValidateModelFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.SuppressModelStateInvalidFilter = true;  // Disables automatic validation errors with problem details
});
var app = builder.Build();

// Build frame-ancestors from allowed origins for CSP
var frameAncestors = allowedHosts != null && allowedHosts.Length > 0
    ? "frame-ancestors 'self' " + string.Join(" ", allowedHosts) + "; "
    : "frame-ancestors 'self'; ";

app.Use(async (context, next) =>
{
	// Prevent IFrame and set other security headers
	if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
	{
        context.Response.Headers.Append("Content-Security-Policy",
            frameAncestors +                    // Allow framing from allowed origins
            "object-src 'none'; " +             // Blocks Flash, Java, and other plugin-based content
            "base-uri 'self'; "                 // Prevents base URL modifications
        );
    }

	if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
	{
		context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
	}

	if (!context.Response.Headers.ContainsKey("Strict-Transport-Security"))
	{
		context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
	}

	if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
	{
		context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
	}

	// Allow framing from allowed origins instead of DENY
	if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
	{
		context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
	}
	if (context.Response.Headers.ContainsKey("X-Powered-By"))
	{
		context.Response.Headers.Remove("X-Powered-By"); 
	}
	if (context.Response.Headers.ContainsKey("Server"))
	{
		context.Response.Headers.Remove("Server");
	}
	await next();
});
app.UseMiddleware<HttpMethodRestrictionMiddleware>();

// DCC Compliance: Rate limiting middleware to prevent brute force attacks
app.UseRateLimiting();

app.Use(async (context, next) =>
{
	if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
	{
		context.Request.EnableBuffering();

		try
		{
			var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
			context.Request.Body.Position = 0;  // Reset body stream for further processing
		}
		catch (JsonException)
		{
			// Handle invalid JSON request
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			context.Response.ContentType = "application/json";
			var errorResponse = new { message = "Invalid JSON format. Please check your request body." };
			await context.Response.WriteAsJsonAsync(errorResponse);
			return;  // Stop further processing
		}
	}
	await next();
});
// Middleware to catch unhandled exceptions and return a generic error message
app.Use(async (context, next) =>
{
	try
	{
		await next.Invoke(); 
	}
	catch (Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		context.Response.ContentType = "application/json";

		var errorResponse = new { message = "An internal server error occurred. Please try again later." };
		await context.Response.WriteAsJsonAsync(errorResponse);
	}
});
app.UseMiddleware<InputSanitizationMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger(c => c.SerializeAsV2 = true);
	app.UseSwaggerUI(c => {
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
	});
}
if (allowedHosts != null && allowedHosts.Length > 0)
{
    app.UseCors(options => options.WithOrigins(allowedHosts).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
   
}

app.UseAuthentication();
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/intaliohub"), appBuilder =>
{
	appBuilder.UseMiddleware<TokenValidationMiddleware>();
}); app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<IntalioHub>("/api/intaliohub");

app.Run();
