using AutoMapper;
using Final_Project.Services;
using Final_Project.Utils.Middlewares;
using Final_Project.Utils.Resources.Commons;
using Final_Project.Utils.Resources.Exceptions;
using Final_Project.Utils.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "Enter Bearer {token} in the text input below.\r\n\r\nExample: \"Bearer BHAWQEASD\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
                Name = "Authorization",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenKey").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SendSMSService>();
builder.Services.AddSingleton<OTPService>();
builder.Services.AddSingleton<RoleService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHostedService<MongoDBIndexesService>();
builder.Services.AddSingleton<ItemService>();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<ItemTypeService>();
builder.Services.AddSingleton<OrderService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*app.ConfigureExceptionHandler();*/

app.UseMiddleware<ValidateTokenMiddleware>();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        throw new HttpReturnException(HttpStatusCode.Unauthorized, "CHUA DANG NHAP KIA TL");
    }
    if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
    {
        throw new HttpReturnException(HttpStatusCode.Forbidden, "AI CHO MAY ACCESS CAI NAY?");
    }
});

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials());

app.UseHttpLogging();
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

//ad