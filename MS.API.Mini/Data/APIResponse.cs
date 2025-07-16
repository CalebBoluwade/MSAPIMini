namespace MS.API.Mini.Data
{
    public record Response
    {
        public bool IsSuccess { get; init; }
        public bool IsExisting { get; init; }
        public required string Message { get; init; }
    }
    
    public class APIResponse<T>
    {
        [JsonPropertyName("Message")] public string Message { get; set; } = string.Empty;


        [JsonPropertyName("Data")] public required T Data { get; set; }

        [JsonPropertyName("Cause")] public string Cause { get; set; } = string.Empty;

        [JsonPropertyName("MetaData")] 
        public Metadata? MetaData { get; set; }
    }

    public record Metadata
    {
        // [JsonPropertyName("TotalPages")]
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
    
    public struct ResponseMessages
    {
        public const string Success = "Successful";
        public const string ExistingAccount = "Email is already registered.";
        public const string UserError = "Invalid Username and / or Password";
        public const string ExpiredLicense = "License Expired";
        public const string UnrecognizedDomain = "Email domain is not associated with any organization.";
        public const string Error = "Unexpected Error Occured";
    }
}