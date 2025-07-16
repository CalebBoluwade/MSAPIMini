using System.Net;
using Asp.Versioning;
using MS.API.Mini.Data;

namespace MS.API.Mini.Extensions;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
public abstract class ControllerBaseExtension
    : ControllerBase
{
    // Helper method for handling successful responses
    protected ActionResult Success<T>(T data, string message, Metadata? metadata)
    {
        return Ok(data);
    }

    // Helper method for handling pagination
    protected ActionResult PaginatedSuccessResult<T>(
        List<T> items,
        int pageNumber,
        int pageSize,
        string message) where T: class
    {
        var metadata = new Metadata
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = items.Count,
            TotalPages = (int)Math.Ceiling(items.Count / (double)pageSize),
            HasNextPage = pageNumber < (int)Math.Ceiling(items.Count / (double)pageSize),
            HasPreviousPage = pageNumber > 1
        };

        return Success(items, message, metadata);
    }

    // Helper method for handling model validation
    protected ActionResult? ValidateModel()
    {
        if (ModelState.IsValid) return null;

        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return Error(new Exception("Validation Failed"), errors.ToString());
    }

    // Helper method for handling error responses
    protected ActionResult Error(Exception ex, string? errorMessage = ResponseMessages.Error, HttpStatusCode? statusCode = HttpStatusCode.BadRequest)
    {
        var response = new APIResponse<object> { Data = new {}, Cause = ex.Message, Message = errorMessage! };

        return StatusCode((int)statusCode!, response);
    }
}