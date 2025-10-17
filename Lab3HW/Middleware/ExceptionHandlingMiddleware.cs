using Lab3.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Lab3.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;
                
                case Lab3.Exceptions.ValidationException:
                    statusCode = HttpStatusCode.UnprocessableEntity;
                    message = ex.Message;
                    break;

                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
