using CP5.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace CP5.Controllers;

internal static class OperationResultHttpExtensions
{
    public static IActionResult ToHttpResult<T>(
        this ControllerBase controller,
        OperationResult<T> result) =>
        result.Status switch
        {
            OperationStatus.Success =>
                controller.Ok(result),

            OperationStatus.NotFound =>
                controller.NotFound(result),

            OperationStatus.ValidationError =>
                controller.BadRequest(result),

            OperationStatus.Conflict =>
                controller.Conflict(result),

            OperationStatus.ExternalServiceFailure =>
                controller.StatusCode(503, result),

            _ => controller.StatusCode(500, result)
        };
}