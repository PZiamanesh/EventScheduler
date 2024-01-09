using EventScheduler.Services.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EventScheduler
{
    public class ErrHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrHandleMiddleware> _logger;

        public ErrHandleMiddleware(RequestDelegate next, ILogger<ErrHandleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceprion(ex, httpContext);
            }
        }

        private async Task HandleExceprion(Exception ex, HttpContext http)
        {
            //var exeption = http.Features.Get<IExceptionHandlerFeature>();
            http.Response.ContentType = "application/json";

            if (/*exeption.Error*/ ex is IAppExceptionHandler exp)
            {
                await http.Response.WriteAsync(exp.ToJson());
                _logger.LogError("baaad things!");
            }
            else
            {
                await http.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = http.Response.StatusCode,
                    Message = "somthing went wrong while processing"
                }));
            }
        }
    }

    public static class ErrHandleMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrHandleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrHandleMiddleware>();
        }
    }
}
