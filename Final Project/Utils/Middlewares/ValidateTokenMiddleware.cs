using Final_Project.Services;
using Final_Project.Utils.Resources.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Web.Http;

namespace Final_Project.Utils.Middlewares
{
    public class ValidateTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public ValidateTokenMiddleware(RequestDelegate next, IConfiguration configuration, TokenService tokenService)
        {
            _next = next;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is object)
            {
                await _next(context);
                return;
            }
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (token != null)
                await _tokenService.ValidateTokenAsync(token);

            await _next(context);
        }
    }
}
