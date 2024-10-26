using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using TaskBlaster.TaskManagement.Models.Exceptions;

namespace TaskBlaster.TaskManagement.API.ExceptionHandlerExtension;

public static class ExceptionHandlerExtensions
{
    private const string ContentType = "application/json";
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {

        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature == null) return;

                Exception exception = exceptionHandlerFeature.Error;
                context.Response.ContentType = ContentType;

                int statusCode = exception switch
                {
                    ResourceNotFoundException => (int)HttpStatusCode.NotFound,
                    ArgumentOutOfRangeException => (int)HttpStatusCode.BadRequest,
                    BadRequestException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = statusCode;
                var response = new { exception.Message };

                await context.Response.WriteAsJsonAsync(response);

            });
        });
    }
}
