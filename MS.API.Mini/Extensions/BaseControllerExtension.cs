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
    protected ActionResult Success<T>(T data, string message, PagedResult<T>? metadata)
    {
        return Ok(data);
    }

    // Helper method for handling pagination
    // protected ActionResult PaginatedSuccessResult<T>(
    //     List<T> items,
    //     int pageNumber,
    //     int pageSize,
    //     string message) where T: class
    // {
    //     var metadata = new PagedResult<T>
    //     {
    //         Data = items,
    //         Page = pageNumber,
    //         PageSize = pageSize,
    //         TotalCount = items.Count,
    //         TotalPages = (int)Math.Ceiling(items.Count / (double)pageSize),
    //         
    //         // HasNextPage = pageNumber < (int)Math.Ceiling(items.Count / (double)pageSize),
    //         // HasPreviousPage = pageNumber > 1
    //     };
    //
    //     return Success(items, message, metadata);
    // }
}