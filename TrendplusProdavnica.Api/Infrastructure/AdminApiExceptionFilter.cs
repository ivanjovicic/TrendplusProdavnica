#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Admin.Common;

namespace TrendplusProdavnica.Api.Infrastructure
{
    public class AdminApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case AdminValidationException validationException:
                    var modelState = new ModelStateDictionary();
                    foreach (var error in validationException.Errors)
                    {
                        foreach (var message in error.Value)
                        {
                            modelState.AddModelError(error.Key, message);
                        }
                    }
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(modelState)
                    {
                        Title = "Validation failed.",
                        Detail = validationException.Message,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://httpstatuses.com/400"
                    });
                    context.ExceptionHandled = true;
                    break;

                case AdminNotFoundException notFoundException:
                    context.Result = new NotFoundObjectResult(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = notFoundException.Message,
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://httpstatuses.com/404"
                    });
                    context.ExceptionHandled = true;
                    break;

                case AdminConflictException conflictException:
                    context.Result = new ConflictObjectResult(new ProblemDetails
                    {
                        Title = "Conflict.",
                        Detail = conflictException.Message,
                        Status = StatusCodes.Status409Conflict,
                        Type = "https://httpstatuses.com/409"
                    });
                    context.ExceptionHandled = true;
                    break;

                case DbUpdateConcurrencyException:
                    context.Result = new ConflictObjectResult(new ProblemDetails
                    {
                        Title = "Concurrency conflict.",
                        Detail = "The resource was changed by another operation. Refresh and retry.",
                        Status = StatusCodes.Status409Conflict,
                        Type = "https://httpstatuses.com/409"
                    });
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}
